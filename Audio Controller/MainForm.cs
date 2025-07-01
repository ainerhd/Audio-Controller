using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using NAudio.CoreAudioApi;
using Audio_Controller.Audio_Controller;

namespace Audio_Controller
{
    public partial class MainForm : Form
    {
        private VolumeController volumeController;
        private SerialConnection serialConnection;
        private DataProcessor dataProcessor;
        private Dictionary<int, MMDevice> deviceMap = new Dictionary<int, MMDevice>();

        public MainForm()
        {
            InitializeComponent();
            volumeController = new VolumeController();
            LoadDevices();
            LoadSerialPorts();
            cmbComPort.DropDown += (s, e) => LoadSerialPorts();
            dgvMapping.EditingControlShowing += dgvMapping_EditingControlShowing;
            LoadConfig();
        }

        private void LoadDevices()
        {
            volumeController.RefreshDevices();
            var deviceNames = volumeController.GetDeviceNames();
            deviceColumn.Items.Clear();
            foreach (var name in deviceNames)
            {
                deviceColumn.Items.Add(name);
            }
            numChannels.Value = 1;
        }

        private void LoadSerialPorts()
        {
            cmbComPort.Items.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                cmbComPort.Items.Add(port);
            }
        }

        private void dgvMapping_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvMapping.CurrentCell.ColumnIndex == 1 && e.Control is ComboBox combo)
            {
                volumeController.RefreshDevices();
                combo.Items.Clear();
                foreach (var name in volumeController.GetDeviceNames())
                {
                    combo.Items.Add(name);
                }
            }
        }

        private void LoadConfig()
        {
            var config = ConfigManager.LoadConfig();
            if (config == null) return;

            if (!string.IsNullOrEmpty(config.ComPort))
            {
                cmbComPort.Text = config.ComPort;
            }

            if (config.BufferSize > 0) numBufferSize.Value = config.BufferSize;
            if (config.DeadZone >= 0) numDeadZone.Value = config.DeadZone;

            if (config.ChannelDeviceMap != null && config.ChannelDeviceMap.Count > 0)
            {
                int count = Math.Min(config.ChannelDeviceMap.Count, (int)numChannels.Maximum);
                numChannels.Value = count;
                foreach (var kvp in config.ChannelDeviceMap)
                {
                    int index = kvp.Key - 1;
                    if (index < dgvMapping.Rows.Count)
                    {
                        dgvMapping.Rows[index].Cells[1].Value = kvp.Value;
                    }
                }
            }
        }

        private void numChannels_ValueChanged(object sender, EventArgs e)
        {
            int count = (int)numChannels.Value;
            dgvMapping.Rows.Clear();
            for (int i = 0; i < count; i++)
            {
                int rowIndex = dgvMapping.Rows.Add();
                dgvMapping.Rows[rowIndex].Cells[0].Value = (i + 1).ToString();
                dgvMapping.Rows[rowIndex].Cells[1].Value = deviceColumn.Items.Count > 0 ? deviceColumn.Items[0] : null;
                dgvMapping.Rows[rowIndex].Cells[2].Value = "0";
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int channelCount = (int)numChannels.Value;
            deviceMap.Clear();
            for (int i = 0; i < channelCount; i++)
            {
                var deviceName = dgvMapping.Rows[i].Cells[1].Value as string;
                var device = volumeController.GetDeviceByName(deviceName);
                if (device != null)
                {
                    deviceMap[i + 1] = device;
                }
            }
            string port = cmbComPort.Text;
            if (string.IsNullOrWhiteSpace(port))
            {
                port = MixerIdentifier.FindDeviceByMessage("HELLO_MIXER", "MIXER_READY");
                if (string.IsNullOrEmpty(port))
                {
                    MessageBox.Show("Kein Gerät gefunden.");
                    return;
                }
                cmbComPort.Text = port;
            }

            int bufferSize = (int)numBufferSize.Value;
            int deadZone = (int)numDeadZone.Value;

            dataProcessor = new DataProcessor(channelCount, bufferSize, deadZone);
            serialConnection = new SerialConnection(port);
            serialConnection.DataReceived += SerialDataReceived;
            try
            {
                serialConnection.Open();
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                SaveCurrentConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen des Ports: {ex.Message}");
            }
        }

        private void SerialDataReceived(string raw)
        {
            int[] values = dataProcessor.Process(raw);
            if (values.Length == 0) return;

            BeginInvoke((MethodInvoker)(() =>
            {
                for (int i = 0; i < values.Length; i++)
                {
                    int channel = i + 1;
                    if (i < dgvMapping.Rows.Count)
                    {
                        dgvMapping.Rows[i].Cells[2].Value = values[i].ToString();
                    }
                    if (deviceMap.TryGetValue(channel, out var device))
                    {
                        volumeController.SetVolume(device, values[i]);
                    }
                }
            }));
        }

        private void SaveCurrentConfig()
        {
            var config = new AppConfig
            {
                ComPort = cmbComPort.Text,
                BufferSize = (int)numBufferSize.Value,
                DeadZone = (int)numDeadZone.Value,
                ChannelDeviceMap = new Dictionary<int, string>()
            };

            int count = (int)numChannels.Value;
            for (int i = 0; i < count; i++)
            {
                var name = dgvMapping.Rows[i].Cells[1].Value as string;
                config.ChannelDeviceMap[i + 1] = name;
            }

            ConfigManager.SaveConfig(config);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (serialConnection != null)
            {
                serialConnection.Close();
                serialConnection = null;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            SaveCurrentConfig();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialConnection != null)
            {
                serialConnection.Close();
                serialConnection = null;
            }
            SaveCurrentConfig();
        }
    }
}
