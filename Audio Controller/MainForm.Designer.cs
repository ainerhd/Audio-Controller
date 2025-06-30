using System.Windows.Forms;

namespace Audio_Controller
{
    partial class MainForm
    {
        private DataGridView dgvMapping;
        private DataGridViewTextBoxColumn channelColumn;
        private DataGridViewComboBoxColumn deviceColumn;
        private DataGridViewTextBoxColumn volumeColumn;
        private NumericUpDown numChannels;
        private TextBox txtComPort;
        private Button btnStart;
        private Label lblChannels;
        private Label lblComPort;

        private void InitializeComponent()
        {
            this.dgvMapping = new DataGridView();
            this.channelColumn = new DataGridViewTextBoxColumn();
            this.deviceColumn = new DataGridViewComboBoxColumn();
            this.volumeColumn = new DataGridViewTextBoxColumn();
            this.numChannels = new NumericUpDown();
            this.txtComPort = new TextBox();
            this.btnStart = new Button();
            this.lblChannels = new Label();
            this.lblComPort = new Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numChannels)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvMapping
            // 
            this.dgvMapping.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMapping.Columns.AddRange(new DataGridViewColumn[] {
                this.channelColumn,
                this.deviceColumn,
                this.volumeColumn});
            this.dgvMapping.Location = new System.Drawing.Point(12, 41);
            this.dgvMapping.Name = "dgvMapping";
            this.dgvMapping.RowHeadersVisible = false;
            this.dgvMapping.Size = new System.Drawing.Size(360, 200);
            this.dgvMapping.TabIndex = 0;
            // 
            // channelColumn
            // 
            this.channelColumn.HeaderText = "Kanal";
            this.channelColumn.ReadOnly = true;
            this.channelColumn.Width = 60;
            // 
            // deviceColumn
            // 
            this.deviceColumn.HeaderText = "Gerät";
            this.deviceColumn.Width = 180;
            // 
            // volumeColumn
            // 
            this.volumeColumn.HeaderText = "Lautstärke";
            this.volumeColumn.ReadOnly = true;
            this.volumeColumn.Width = 80;
            // 
            // numChannels
            // 
            this.numChannels.Location = new System.Drawing.Point(90, 12);
            this.numChannels.Minimum = 1;
            this.numChannels.Maximum = 8;
            this.numChannels.Name = "numChannels";
            this.numChannels.Size = new System.Drawing.Size(50, 20);
            this.numChannels.Value = 1;
            this.numChannels.ValueChanged += new System.EventHandler(this.numChannels_ValueChanged);
            // 
            // txtComPort
            // 
            this.txtComPort.Location = new System.Drawing.Point(242, 12);
            this.txtComPort.Name = "txtComPort";
            this.txtComPort.Size = new System.Drawing.Size(60, 20);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(308, 11);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(64, 23);
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblChannels
            // 
            this.lblChannels.AutoSize = true;
            this.lblChannels.Location = new System.Drawing.Point(12, 14);
            this.lblChannels.Name = "lblChannels";
            this.lblChannels.Size = new System.Drawing.Size(72, 13);
            this.lblChannels.Text = "Kanäle:";
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new System.Drawing.Point(160, 14);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(76, 13);
            this.lblComPort.Text = "COM-Port:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.lblComPort);
            this.Controls.Add(this.lblChannels);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtComPort);
            this.Controls.Add(this.numChannels);
            this.Controls.Add(this.dgvMapping);
            this.Name = "MainForm";
            this.Text = "Audio Controller";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numChannels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
