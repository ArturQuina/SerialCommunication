using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SerialCommunication
{
    // Partial class to hold helper methods for Form1
    public partial class Form1 : Form
    {
        // Read digital inputs d5..d7 once and update radio buttons.
        // Uses a single local variable 'antwoord' reused to avoid duplicate declarations.
        private void RefreshDigitalInputsOnce()
        {
            if (serialPortArduino == null || !serialPortArduino.IsOpen) return;

            try { serialPortArduino.ReadExisting(); } catch { }

            try
            {
                string commando;
                string antwoord = string.Empty;

                commando = "get d5";
                serialPortArduino.WriteLine(commando);
                antwoord = serialPortArduino.ReadLine().TrimEnd();
                if (antwoord.Length >= 4) antwoord = antwoord.Substring(4);
                radioButtonDigital5.Checked = (antwoord == "1");

                commando = "get d6";
                serialPortArduino.WriteLine(commando);
                antwoord = serialPortArduino.ReadLine().TrimEnd();
                if (antwoord.Length >= 4) antwoord = antwoord.Substring(4);
                radioButtonDigital6.Checked = (antwoord == "1");

                commando = "get d7";
                serialPortArduino.WriteLine(commando);
                antwoord = serialPortArduino.ReadLine().TrimEnd();
                if (antwoord.Length >= 4) antwoord = antwoord.Substring(4);
                radioButtonDigital7.Checked = (antwoord == "1");
            }
            catch (TimeoutException)
            {
                // If any read times out, ignore and continue
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij lezen digitale ingangen: " + ex.Message);
            }
        }
    }
}
