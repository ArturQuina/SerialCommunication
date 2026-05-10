using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        private SerialPort serialPortArduino;

        public Form1()
        {
            InitializeComponent();
            trackBarPWM9.Scroll += trackBarPWM9_Scroll;
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (serialPortArduino.IsOpen)
            {
                try
                {
                    serialPortArduino.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fout bij sluiten van poort: " + ex.Message);
                }
                radioButtonVerbonden.Checked = false;
                buttonConnect.Text = "Connect";
                labelStatus.Text = "Niet verbonden";
            }
            else
            {
                try
                {
                    // Port en basis instellingen
                    serialPortArduino.PortName = (string)comboBoxPoort.SelectedItem;
                    serialPortArduino.BaudRate = int.Parse((string)comboBoxBaudrate.SelectedItem);

                    // Data bits
                    serialPortArduino.DataBits = (int)numericUpDownDatabits.Value;

                    // Parity
                    if (radioButtonParityEven.Checked) serialPortArduino.Parity = Parity.Even;
                    else if (radioButtonParityOdd.Checked) serialPortArduino.Parity = Parity.Odd;
                    else if (radioButtonParityMark.Checked) serialPortArduino.Parity = Parity.Mark;
                    else if (radioButtonParitySpace.Checked) serialPortArduino.Parity = Parity.Space;
                    else serialPortArduino.Parity = Parity.None;

                    // Stop bits
                    if (radioButtonStopbitsNone.Checked) serialPortArduino.StopBits = StopBits.None;
                    else if (radioButtonStopbitsOne.Checked) serialPortArduino.StopBits = StopBits.One;
                    else if (radioButtonStopbitsOnePointFive.Checked) serialPortArduino.StopBits = StopBits.OnePointFive;
                    else if (radioButtonStopbitsTwo.Checked) serialPortArduino.StopBits = StopBits.Two;
                    else serialPortArduino.StopBits = StopBits.One;

                    // Handshake
                    if (radioButtonHandshakeNone.Checked) serialPortArduino.Handshake = Handshake.None;
                    else if (radioButtonHandshakeRTS.Checked) serialPortArduino.Handshake = Handshake.RequestToSend;
                    else if (radioButtonHandshakeRTSXonXoff.Checked) serialPortArduino.Handshake = Handshake.RequestToSendXOnXOff;
                    else if (radioButtonHandshakeXonXoff.Checked) serialPortArduino.Handshake = Handshake.XOnXOff;
                    else serialPortArduino.Handshake = Handshake.None;

                    // RTS/DTR
                    serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked;
                    serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked;

                    // Open en controleer ping-pong
                    serialPortArduino.Open();

                    // Stuur ping en wacht op antwoord
                    serialPortArduino.DiscardInBuffer();
                    serialPortArduino.WriteLine("ping");
                    string reply = string.Empty;
                    try
                    {
                        reply = serialPortArduino.ReadLine().Trim();
                    }
                    catch (TimeoutException)
                    {
                        // geen antwoord binnen timeout
                        serialPortArduino.Close();
                        MessageBox.Show("Geen antwoord van Arduino (timeout).");
                        labelStatus.Text = "Niet verbonden";
                        return;
                    }

                    if (reply == "pong")
                    {
                        radioButtonVerbonden.Checked = true;
                        buttonConnect.Text = "Disconnect";
                        labelStatus.Text = "Verbonden";
                    }
                    else
                    {
                        serialPortArduino.Close();
                        MessageBox.Show("Onverwacht antwoord van Arduino: " + reply);
                        labelStatus.Text = "Niet verbonden";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fout bij verbinden: " + ex.Message);
                    try { if (serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                    labelStatus.Text = "Niet verbonden";
                }
            }
        }

        private void checkBoxDigital2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string command = checkBoxDigital2.Checked ? "set d2 high" : "set d2 low";
                    serialPortArduino.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
            }
        }

        private void checkBoxDigital3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string command = checkBoxDigital3.Checked ? "set d3 high" : "set d3 low";
                    serialPortArduino.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
            }
        }

        private void checkBoxDigital4_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string command = checkBoxDigital4.Checked ? "set d4 high" : "set d4 low";
                    serialPortArduino.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
            }
        }
        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string cmd = "set pwm9 " + trackBarPWM9.Value.ToString();
                    serialPortArduino.WriteLine(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
            }
        }
        private void trackBarPWM10_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string cmd = "set pwm10 " + trackBarPWM10.Value.ToString();
                    serialPortArduino.WriteLine(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
            }
        }
    }
}
