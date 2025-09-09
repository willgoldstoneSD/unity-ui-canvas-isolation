using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "UI Canvas Isolation", defaultDisplay = true)]
public class UICanvasIsolationSceneOverlay : Overlay
{
    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement();

        // Add a UI Toggle to the overlay
        var toggle = new Toggle("Edit UI in Isolation")
        {
            value = IsEnabled(),
            name = "UICanvasIsolationToggle"
        };
        toggle.RegisterValueChangedCallback(evt => OnToggleChanged(evt.newValue));
        root.Add(toggle);

        return root;
    }

    private void OnToggleChanged(bool newValue)
    {
        ToggleHandler(newValue);

        // Update the toggle text based on current state
        var toggle = rootVisualElement.Q<Toggle>("UICanvasIsolationToggle");
        if (toggle != null)
        {
            toggle.value = IsEnabled();
        }
    }

    private static bool isEnabled = true;

    public static void ToggleHandler(bool enabled)
    {
        isEnabled = enabled;

        Selection.selectionChanged -= UICanvasIsolation.OnSelectionChanged;

        if (isEnabled)
        {
            Selection.selectionChanged += UICanvasIsolation.OnSelectionChanged;
        }

        Debug.Log(isEnabled ? "UI Canvas Isolation Enabled" : "UI Canvas Isolation Disabled");
    }

    public static bool IsEnabled()
    {
        return isEnabled;
    }
}