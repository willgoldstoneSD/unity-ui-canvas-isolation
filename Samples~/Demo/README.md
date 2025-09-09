# UI Canvas Isolation Demo

This sample demonstrates the UI Canvas Isolation tool with various canvas types and UI elements.

## What's Included

- **Screen Space Overlay Canvas**: Main UI canvas with buttons, images, and text
- **Screen Space Camera Canvas**: UI canvas rendered to a specific camera
- **World Space Canvas**: 3D UI canvas positioned in world space
- **Nested UI Elements**: Various UI components to test the isolation behavior

## How to Use

1. Open the demo scene
2. Enable "Edit UI in Isolation" in the Scene View overlay
3. Click on different UI elements to see the isolation in action
4. Try selecting 3D objects to see the restoration behavior

## Testing Different Canvas Types

- **Screen Space Overlay**: Select any UI element on the main canvas
- **Screen Space Camera**: Select UI elements on the camera canvas
- **World Space**: Select UI elements on the 3D canvas (if enabled in settings)

## Configuration Testing

Try modifying the settings programmatically to test different behaviors:

```csharp
// Enable all canvas types
var settings = UICanvasIsolation.Settings;
settings.handleScreenSpaceCameraCanvases = true;
settings.handleWorldSpaceCanvases = true;
```
