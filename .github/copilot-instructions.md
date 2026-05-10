# Copilot Instructions for SerialCommunication

## Project Overview

**SerialCommunication** is a Windows Forms (.NET Framework 4.7.2) desktop application for communicating with Arduino/embedded devices via serial ports. The codebase includes:
- **C# GUI**: Main Windows Forms UI in `SerialCommunication/` directory
- **Arduino/C code**: Supporting device firmware in `.ino` and `.c` files

## Build & Run

### Build
```bash
# Using Visual Studio or MSBuild
msbuild SerialCommunication.slnx /p:Configuration=Debug
# Or in Visual Studio: Build > Build Solution
```

### Output
- **Debug**: `SerialCommunication\bin\Debug\SerialCommunication.exe`
- **Release**: `SerialCommunication\bin\Release\SerialCommunication.exe`

### Run
Execute the `.exe` directly or press F5 in Visual Studio to run with debugger.

**Note**: No test suite exists. Manual testing via the GUI is the current approach.

## Architecture

### Main Components

**Form1.cs** - Primary serial communication interface:
- Port discovery and selection (auto-refreshed on dropdown)
- Baudrate selection (defaults to 115200)
- Serial port connection/disconnection logic
- Device communication handler

**Supporting Files**:
- `Form1.Designer.cs` - Auto-generated UI layout (do not manually edit)
- `Form1.resx` - UI resources (icons for analog/digital in/out, thermostat)
- `Program.cs` - WinForms application entry point
- `App.config` - Application configuration

### Serial Communication Pattern

1. Form loads → discovers available COM ports via `SerialPort.GetPortNames()`
2. User selects port and baudrate
3. Click "Connect" → opens `System.IO.Ports.SerialPort` connection
4. Device communication via serial read/write operations

## Key Conventions

### Windows Forms Conventions
- Designer-generated files (`*.Designer.cs`) should not be manually edited
- UI controls follow naming pattern: `controlTypeControlName` (e.g., `comboBoxPoort`, `buttonConnect`)
- Exception handling in UI load/dropdown events uses empty catch blocks (suppress non-critical errors like port enumeration failures)

### Serial Communication
- Uses `System.IO.Ports.SerialPort` from System namespace
- Port enumeration uses `SerialPort.GetPortNames().Distinct()` to avoid duplicates
- Refresh port list on dropdown open to detect newly connected devices

### Namespacing
- All classes use `SerialCommunication` namespace

## Arduino Firmware

The Arduino sketch (`SerialCommunication.ino`) implements a command parser that receives text-based commands from the desktop application over serial. The protocol is case-sensitive and space-delimited.

### Command Structure

The `SerialCommand` library handles parsing. Commands and responses flow as plain text:

| Command | Arguments | Valid Pins | Response |
|---------|-----------|-----------|----------|
| `set d<pin> <value>` | pin: 2-4, value: 0/1/on/off/high/low | Digital Out | `set done` or error |
| `set pwm<pin> <value>` | pin: 9-11, value: 0-255 | PWM Out | `set done` or error |
| `toggle d<pin>` | pin: 2-4 | Digital Out | `toggle done` or error |
| `get d<pin>` | pin: 2-7 | Digital In | `d<pin>: <0 or 1>` |
| `get a<pin>` | pin: 0-5 | Analog In | `a<pin>: <0-1023>` |
| `ping` | — | — | `pong` |
| `help` | — | — | Shows all commands |
| `debug` | — | — | Displays ping counter |

### Pin Configuration

- **Digital Output**: 2, 3, 4 (digitalWrite / toggle)
- **PWM Output**: 9, 10, 11 (analogWrite, 0-255 values)
- **Digital Input**: 5, 6, 7 (digitalRead)
- **Analog Input**: A0-A5 (analogRead via `analogReadDelay` with 50ms delay for signal stabilization)

### Key Files

- **SerialCommunication.ino** - Main Arduino sketch with command handlers (onSet, onToggle, onGet, etc.)
- **SerialCommand.h/cpp** - External library for serial command parsing (tokenization & callbacks)
- **analog.c** - Low-level analog read implementation with configurable microsecond delay for weak signal stabilization

### Building & Uploading

Use Arduino IDE (or Arduino CLI):
```bash
# Arduino IDE: Sketch > Upload (or Ctrl+U)
# Arduino CLI:
arduino-cli compile --fqbn arduino:avr:uno SerialCommunication.ino
arduino-cli upload -p COM3 --fqbn arduino:avr:uno SerialCommunication.ino
```

**Target**: Arduino Uno or compatible ATmega328P board

### Validation Rules

- `isValidNumber()` - Validates numeric arguments (1-3 characters, digits only)
- `startsWith()` - Used to match prefixed commands (d, pwm, a) without full parsing
- Invalid arguments → error message; valid command structure is strict

## Communication Protocol

The desktop application sends **command strings** (e.g., `"set d2 high\r"`) to the Arduino. The Arduino responds with status/data, also as plain text. Timeouts default to 1000ms in Form1.cs. All commands must be explicitly implemented by `addCommand()` in the Arduino setup(); unknown commands trigger `onUnknownCommand()`.

## Recommended MCP Servers

For local Copilot CLI sessions, these MCP servers enhance development workflow:

- **Filesystem MCP** - Enables efficient file operations and exploration across the C# and Arduino code
- **Git MCP** - Helps with version control, commit history, and branch management

## Development Notes

- **Target Framework**: .NET Framework 4.7.2 (Windows-only, uses WinForms)
- **Output Type**: WinExe (Windows Forms executable)
- **Architectures**: AnyCPU (runs on both x86 and x64)
- **Assembly**: `System.IO.Ports` for serial communication (built-in, no NuGet dependency)
- **Arduino Target**: ATmega328P (Arduino Uno compatible)
- **License**: MIT
