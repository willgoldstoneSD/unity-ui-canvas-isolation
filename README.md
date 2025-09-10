# Unity UI (UGUI) Canvas Isolation

A Unity Editor tool that automatically isolates UI canvases in the Scene View for better UI development workflow.

## Features

- **Automatic UI Detection**: Detects when you select Canvas objects or UI elements
- **Smart Canvas Framing**: Frames the nearest canvas to show UI elements in proper context
- **Smart Layer Isolation**: Shows only UI layer for screen space canvases, shows canvas layer for world space canvases
- **Smart View Mode**: Switches to 2D orthographic mode for screen space canvases, keeps 3D perspective for world space canvases
- **Configurable Settings**: Support for different canvas types (Screen Space Overlay, World Space)
- **Performance Optimized**: Component caching and debouncing for smooth operation
- **Undo Support**: All changes can be undone with Ctrl+Z
- **Multi-Scene View Support**: Works with multiple scene views open

## Installation

### Via Git URL

Add this line to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.willgoldstone.ui-canvas-isolation": "https://github.com/willgoldstoneSD/unity-ui-canvas-isolation.git"
  }
}
```

### Via Package Manager UI

1. Open Unity's Package Manager (Window → Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/willgoldstoneSD/unity-ui-canvas-isolation.git`

## Usage

### Basic Usage

1. **Enable the tool**: Look for the "Edit UI in Isolation" toggle in the Scene View overlay
2. **Select a UI element**: Click on any Canvas or UI element (Button, Image, Text, etc.)
3. **Automatic isolation**: The tool will:
   - Switch to 2D orthographic mode
   - Show only the UI layer
   - Frame the nearest canvas
   - Disable skybox and post-processing for cleaner UI view
4. **Select a non-UI object**: The tool automatically restores the original view

### Configuration

The tool includes configurable settings accessible through the `UICanvasIsolation.Settings` property:

```csharp
// Example: Configure the tool programmatically
var settings = UICanvasIsolation.Settings;
settings.handleWorldSpaceCanvases = true;
settings.handleScreenSpaceCameraCanvases = true;
settings.framingPadding = 1.5f;
settings.affectAllSceneViews = true;
```

#### Available Settings

- **Canvas Types**:
  - `handleScreenSpaceOverlayCanvases` (default: true)
  - `handleScreenSpaceCameraCanvases` (default: false)
  - `handleWorldSpaceCanvases` (default: true)

- **Scene View Options**:
  - `affectAllSceneViews` (default: false)
  - `framingPadding` (default: 1.2f)

- **Performance**:
  - `enableComponentCaching` (default: true)
  - `enableDebouncing` (default: true)
  - `debounceTime` (default: 0.1 seconds)

### API Reference

#### Public Methods

```csharp
// Force restore UI isolation state (safety mechanism)
UICanvasIsolation.ForceRestoreUIState();

// Clear component cache
UICanvasIsolation.ClearCache();

// Access settings
var settings = UICanvasIsolation.Settings;
```

## How It Works

1. **Selection Detection**: Monitors Unity's selection changes
2. **UI Element Identification**: Uses component caching to efficiently detect Canvas components
3. **Context Switching**: 
   - Saves current layer visibility and scene view settings
   - Switches to UI-only view with 2D mode
   - Frames the nearest canvas for context
4. **State Restoration**: Restores original settings when selecting non-UI objects

## Performance Features

- **Component Caching**: Avoids repeated `GetComponent` calls
- **Debouncing**: Prevents excessive operations during rapid selection changes
- **Memory Management**: Automatic cache cleanup when objects are destroyed
- **Error Handling**: Safe scene view operations with try-catch blocks

## Troubleshooting

### Scene Stuck in 2D Mode

If the scene gets stuck in 2D isometric mode:

```csharp
UICanvasIsolation.ForceRestoreUIState();
```

### Performance Issues

Disable caching or debouncing if needed:

```csharp
var settings = UICanvasIsolation.Settings;
settings.enableComponentCaching = false;
settings.enableDebouncing = false;
```

### Multiple Scene Views

Enable multi-scene view support:

```csharp
var settings = UICanvasIsolation.Settings;
settings.affectAllSceneViews = true;
```

## Requirements

- Unity 2022.3.0f1 or later
- No additional dependencies required

## License

MIT License - see LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and feature requests, please use the [GitHub Issues](https://github.com/willgoldstone/unity-ui-canvas-isolation/issues) page.
