Problem
Voeg een timer timerOefening3 toe die elke 1000 ms de status van digital5/6/7 opvraagt en de bijhorende radio buttons bijwerkt.

Approach
- Voeg een private System.Windows.Forms.Timer timerOefening3 toe in Form1.cs en stel Interval = 1000.
- Sluit aan op tabControl.SelectedIndexChanged (eventhandler toevoegen in de constructor of Form1_Load). Wanneer tabPageOefening3 geselecteerd is, enable de timer; anders disable de timer.
- Implementeer timerOefening3_Tick:
  - Wrap alles in try-catch.
  - Controleer serialPortArduino != null && serialPortArduino.IsOpen.
  - Roep serialPortArduino.ReadExisting() aan en negeer de return (cleart antwoord).
  - Voor elk pin (5,6,7): stuur "get d{pin}" met WriteLine, lees antwoord (ReadLine()), extraheren deel na ':' en Trim(), vergelijk met "1" en zet radioButtonDigital{pin}.Checked = (waarde=="1").
  - Vang TimeoutException en andere exceptions en behandel (MessageBox of log).

Files to change
- SerialCommunication\\Form1.cs
  - Declare timer field
  - Instantiatie en interval instelling (constructor of Form1_Load)
  - Hook tabControl.SelectedIndexChanged
  - Implementatie timer Tick handler

Notes / Decisions
- Vermijd direct bewerken van Form1.Designer.cs; voeg handlers en timer in Form1.cs zodat designer blijft onaangeroerd.
- Gebruik ReadLine() met bestaande ReadTimeout (1000 ms). ReadExisting() wordt gebruikt om oude antwoorden te verwijderen.
- Alleen enable timer wanneer tabPageOefening3 geselecteerd; disable bij andere tabs.

Todos
- add-timer-oefening3: Declare+init timer, set Interval=1000, add Tick handler in Form1.cs.
- add-tabcontrol-eventhandler: Attach SelectedIndexChanged and implement enabling logic.
- add-timer-tick-handler: Implement tick logic: serial checks, ReadExisting, get d5/d6/d7, parse and set radio buttons, try-catch.
- testing-manual: Manual test with Arduino connected; verify radio buttons update when tab selected.
