# Static Assets

This folder contains static assets for the Runner application.

## Files

### `icon.ico`
Sample system tray icon for Runner. This is a ready-to-use icon that you can:
- Use as-is in your deployment
- Replace with your own custom icon
- Reference in `TrayMasterConfig.json` configuration

### `TrayMasterConfig.json.template`
**Complete configuration template** with all available options documented inline.

This is the **definitive reference** for Runner configuration, including:
- All top-level options (name, trayTitle)
- Icon configuration (file-based and text-generated)
- HTTP server settings (enabled, port, localOnly)
- Logging configuration (enabled, level, maxLines)
- Complete menu item options:
  - `label` - Menu item text
  - `command` - Command to execute
  - `action` - Special actions (quit)
  - `workingDir` - Working directory
  - `longRunning` - Track process lifecycle
  - `default` - Execute on tray icon click
  - `enabledWhen` - Conditional enablement (always/running/stopped)
  - `showWhen` - Conditional visibility (always/running/stopped)
  - `path` - HTTP endpoint path
  - `webOnly` - Hide from tray menu

## Usage

### For Deployment

Copy the template to your deployment directory and rename it:

```bash
cp static/TrayMasterConfig.json.template ./TrayMasterConfig.json
cp static/icon.ico ./icon.ico
```

Then edit `TrayMasterConfig.json` to configure your processes.

### For Development

The template is heavily commented to explain every option. Use it as a reference when creating or modifying Runner configurations.

### Related Documentation

- `../README.md` - Main Runner documentation
- `../examples/HTTP-REFERENCE.md` - HTTP webhook guide
- `../examples/` - Working example handlers
