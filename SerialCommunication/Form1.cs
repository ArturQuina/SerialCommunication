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
        private int toestand = 0;

        public Form1()
        {
            InitializeComponent();
            trackBarPWM9.Scroll += trackBarPWM9_Scroll;
            serialPortArduino = new SerialPort();
            serialPortArduino.ReadTimeout = 1000;
            serialPortArduino.WriteTimeout = 1000;

            // timer for Oefening 3 (initialized in designer)
            timerOefening3.Tick += timerOefening3_Tick;
            timerOefening3.Enabled = false;

            // timer for Oefening 4 (initialized in designer)
            timerOefening4.Tick += timerOefening4_Tick;
            timerOefening4.Enabled = false;

            // timer for Oefening 5 (initialized in designer)
            timerOefening5.Tick += timerOefening5_Tick;
            timerOefening5.Enabled = false;

            // timer for TemperatuurAlarm (initialized in designer)
            timerTemperatuurAlarm.Tick += timerTemperatuurAlarm_Tick;
            timerTemperatuurAlarm.Enabled = false;

            // hook tab control selection changed to enable/disable timer
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
        }

        private void ResetConnectionUI()
        {
            try { labelStatus.Text = "Niet verbonden"; } catch { }
            try { radioButtonVerbonden.Checked = false; } catch { }
            try { buttonConnect.Text = "Connect"; } catch { }
            try { radioButtonVerbonden.Location = new System.Drawing.Point(968, 17); } catch { }
            try { radioButtonVerbonden.Text = "verbonden"; } catch { }
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
                        try { serialPortArduino.Close(); } catch { }
                        MessageBox.Show("Geen antwoord van Arduino (timeout).");
                        ResetConnectionUI();
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
                        try { serialPortArduino.Close(); } catch { }
                        MessageBox.Show("Onverwacht antwoord van Arduino: " + reply);
                        ResetConnectionUI();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fout bij verbinden: " + ex.Message);
                    try { if (serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                    ResetConnectionUI();
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
                ResetConnectionUI();
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
                ResetConnectionUI();
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
                ResetConnectionUI();
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
                ResetConnectionUI();
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
                ResetConnectionUI();
            }
        }

        private void trackBarPWM11_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string cmd = "set pwm11 " + trackBarPWM11.Value.ToString();
                    serialPortArduino.WriteLine(cmd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij verzenden: " + ex.Message);
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                ResetConnectionUI();
            }
        }


        

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            timerOefening3.Enabled = tabControl.SelectedIndex == 3;
            if (timerOefening4 != null) timerOefening4.Enabled = tabControl.SelectedIndex == 4;
            if (timerOefening5 != null) timerOefening5.Enabled = tabControl.SelectedIndex == 5;
            if (timerTemperatuurAlarm != null)
                timerTemperatuurAlarm.Enabled = tabControl.SelectedTab == tabPageTemperatuurAlarm;
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    serialPortArduino.ReadExisting();
                    string commando = "get d5";
                    serialPortArduino.WriteLine(commando);
                    string antwoord = serialPortArduino.ReadLine();
                    antwoord = antwoord.TrimEnd();
                    antwoord = antwoord.Substring(4);
                    radioButtonDigital5.Checked = (antwoord == "1");

                    commando = "get d7";
                    serialPortArduino.WriteLine(commando);
                    antwoord = serialPortArduino.ReadLine();
                    antwoord = antwoord.TrimEnd();
                    antwoord = antwoord.Substring(4);
                    radioButtonDigital6.Checked = (antwoord == "1");

                    commando = "get d6";
                    serialPortArduino.WriteLine(commando);
                    antwoord = serialPortArduino.ReadLine();
                    antwoord = antwoord.TrimEnd();
                    antwoord = antwoord.Substring(4);
                    radioButtonDigital7.Checked = (antwoord == "1");
                }
            }
            catch (Exception exception)
            {
                labelStatus.Text = "Error: " + exception.Message;
                serialPortArduino.Close();
                radioButtonVerbonden.Checked = false;
                buttonConnect.Text = "Connect";
                // Zorg dat de status en indicator terug naar beginwaarden gaan
                labelStatus.Text = "Niet verbonden";
                try { radioButtonVerbonden.Location = new System.Drawing.Point(968, 17); } catch { }
                radioButtonVerbonden.Text = "verbonden";
            }
        }

        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    // remove any previous unread data from Arduino
                    serialPortArduino.ReadExisting();

                    // request analog 0 value
                    string commando = "get a0";
                    serialPortArduino.WriteLine(commando);

                    string antwoord = serialPortArduino.ReadLine();
                    antwoord = antwoord.TrimEnd();
                    // expected format: "a0: 123", trim prefix if present
                    if (antwoord.Length > 4) antwoord = antwoord.Substring(4);

                    labelAnalog0.Text = antwoord;
                }
            }
            catch (Exception exception)
            {
                labelStatus.Text = "Error: " + exception.Message;
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                radioButtonVerbonden.Checked = false;
                buttonConnect.Text = "Connect";
                // Reset status and radio indicator to initial state
                labelStatus.Text = "Niet verbonden";
                try { radioButtonVerbonden.Location = new System.Drawing.Point(968, 17); } catch { }
                radioButtonVerbonden.Text = "verbonden";
            }
        }

        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    // remove any previous unread data from Arduino
                    serialPortArduino.ReadExisting();

                    // --- Read desired temperature from A0 (0..1023 -> 5..45 °C)
                    serialPortArduino.WriteLine("get a0");
                    string antwoordA0 = serialPortArduino.ReadLine().TrimEnd();
                    if (antwoordA0.Length > 4) antwoordA0 = antwoordA0.Substring(4);
                    string digitsA0 = new string(antwoordA0.Where(c => char.IsDigit(c)).ToArray());
                    double desiredTemp = double.NaN;
                    if (int.TryParse(digitsA0, out int rawA0))
                    {
                        double mA0 = 40.0 / 1023.0; // (45-5)/1023
                        double bA0 = 5.0;
                        desiredTemp = mA0 * rawA0 + bA0;
                        labelGewensteTemp.Text = Math.Round(desiredTemp, 1).ToString("0.0") + " °C";
                    }
                    else
                    {
                        labelStatus.Text = "Onverwacht antwoord A0: " + antwoordA0;
                    }

                    // --- Read current temperature from A1 (0..1023 -> 0..500 °C)
                    serialPortArduino.WriteLine("get a1");
                    string antwoordA1 = serialPortArduino.ReadLine().TrimEnd();
                    if (antwoordA1.Length > 4) antwoordA1 = antwoordA1.Substring(4);
                    string digitsA1 = new string(antwoordA1.Where(c => char.IsDigit(c)).ToArray());
                    double currentTemp = double.NaN;
                    if (int.TryParse(digitsA1, out int rawA1))
                    {
                        double mA1 = 500.0 / 1023.0; // slope for 0..1023 -> 0..500
                        double bA1 = 0.0;
                        currentTemp = mA1 * rawA1 + bA1;
                        labelHuidigeTemp.Text = Math.Round(currentTemp, 1).ToString("0.0") + " °C";
                    }
                    else
                    {
                        labelStatus.Text = "Onverwacht antwoord A1: " + antwoordA1;
                    }

                    // --- Control LED on digital pin 2: on when current < desired
                    if (!double.IsNaN(currentTemp) && !double.IsNaN(desiredTemp))
                    {
                        try
                        {
                            if (currentTemp < desiredTemp)
                            {
                                serialPortArduino.WriteLine("set d2 high");
                                try { checkBoxDigital2.Checked = true; } catch { }
                            }
                            else
                            {
                                serialPortArduino.WriteLine("set d2 low");
                                try { checkBoxDigital2.Checked = false; } catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            labelStatus.Text = "Fout bij LED-aansturing: " + ex.Message;
                            try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                            radioButtonVerbonden.Checked = false;
                            buttonConnect.Text = "Checked";
                            // Reset status and radio indicator to initial state
                            labelStatus.Text = "Niet verbonden";
                            try { radioButtonVerbonden.Location = new System.Drawing.Point(968, 17); } catch { }
                            radioButtonVerbonden.Text = "verbonden";
                        }
                    }
                }
                else
                {
                    // no connection
                    labelStatus.Text = "Niet verbonden";
                    radioButtonVerbonden.Checked = false; //x
                    buttonConnect.Text = "conected"; //x
                    timerOefening5.Enabled = false;
                }
            }
            catch (Exception exception)
            {
                labelStatus.Text = "Error: " + exception.Message;
                try { if (serialPortArduino != null && serialPortArduino.IsOpen) serialPortArduino.Close(); } catch { }
                radioButtonVerbonden.Checked = false; //x
                buttonConnect.Text = "conected"; //x
                // Reset status and radio indicator to initial state
                labelStatus.Text = "Niet verbonden";
                try { radioButtonVerbonden.Location = new System.Drawing.Point(968, 17); } catch { }
                radioButtonVerbonden.Text = "verbonden";
            }
        }

        
    }
}
