# Installation Guide

## Option 1: Git URL (Recommended)

Add this line to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.willgoldstone.ui-canvas-isolation": "https://github.com/willgoldstone/unity-ui-canvas-isolation.git"
  }
}
```

## Option 2: Scoped Registry

1. Open Unity's Package Manager (Window → Package Manager)
2. Click the gear icon and select "Advanced Project Settings"
3. In the Scoped Registries section, click the "+" button
4. Add the following registry:
   - **Name**: Will Goldstone Packages
   - **URL**: `https://willgoldstone.github.io/unity-packages/`
   - **Scope(s)**: `com.willgoldstone`
5. Click "Apply"
6. In the Package Manager, select "My Registries" from the dropdown
7. Find "UI Canvas Isolation" and click "Install"

## Option 3: Package Manager UI

1. Open Unity's Package Manager (Window → Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/willgoldstone/unity-ui-canvas-isolation.git`
4. Click "Add"

## Verification

After installation, you should see:
- A toggle labeled "Edit UI in Isolation" in the Scene View overlay
- The package listed in your Package Manager under "In Project"

## Troubleshooting

### Package Not Found
- Ensure you have Unity 2022.3.0f1 or later
- Check your internet connection
- Verify the Git URL is correct

### Toggle Not Visible
- Make sure you have a Scene View open
- Check that the package is properly installed
- Try refreshing the Unity Editor

### Import Errors
- Clear your Library folder and reimport
- Check Unity Console for specific error messages

