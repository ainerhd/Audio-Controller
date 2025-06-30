using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
        }

        private void LoadDevices()
        {
            var deviceNames = volumeController.GetDeviceNames();
            deviceColumn.Items.Clear();
            foreach (var name in deviceNames)
            {
                deviceColumn.Items.Add(name);
            }
            numChannels.Value = 1;
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
            dataProcessor = new DataProcessor(channelCount);
            serialConnection = new SerialConnection(txtComPort.Text);
            serialConnection.DataReceived += SerialDataReceived;
            try
            {
                serialConnection.Open();
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
    }
}
