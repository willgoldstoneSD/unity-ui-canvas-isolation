using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class UICanvasIsolationSettings
{
    [Header("Canvas Types")]
    public bool handleWorldSpaceCanvases = true;
    public bool handleScreenSpaceCameraCanvases = false;
    public bool handleScreenSpaceOverlayCanvases = true;
    
    [Header("Scene View Options")]
    public bool affectAllSceneViews = false;
    public float framingPadding = 1.2f;
    
    [Header("Performance")]
    public bool enableComponentCaching = true;
    public bool enableDebouncing = true;
    public double debounceTime = 0.1;
}

[InitializeOnLoad]
public static class UICanvasIsolation
{
    private const int UILayer = 5;
    private static bool wasLastSelectionUI = false;
    private static int cachedLayerMask = ~0; // Initialize to show all layers
    private static bool isPlayModeActive = false;
    private static bool hasValidLayerCache = false;
    
    // Debouncing for selection changes
    private static double lastSelectionTime;
    private const double SELECTION_DEBOUNCE_TIME = 0.1; // 100ms
    
    // Component caching to avoid repeated lookups
    private static readonly Dictionary<GameObject, Canvas> canvasCache = new Dictionary<GameObject, Canvas>();
    
    // Store previous orthographic state per scene view
    private static readonly Dictionary<SceneView, bool> previousOrthographicStates = new Dictionary<SceneView, bool>();
    
    // Configuration settings
    private static UICanvasIsolationSettings settings = new UICanvasIsolationSettings();
    
    public static UICanvasIsolationSettings Settings
    {
        get { return settings; }
        set { settings = value ?? new UICanvasIsolationSettings(); }
    }

    static UICanvasIsolation()
    {
        if (UICanvasIsolationSceneOverlay.IsEnabled())
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }
    
    private static void OnHierarchyChanged()
    {
        // Clear cache when hierarchy changes to prevent stale references
        if (settings.enableComponentCaching)
        {
            canvasCache.Clear();
        }
        
        // Clean up orthographic states for destroyed scene views
        var sceneViewsToRemove = new List<SceneView>();
        foreach (var kvp in previousOrthographicStates)
        {
            if (kvp.Key == null) // Scene view was destroyed
            {
                sceneViewsToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var sceneView in sceneViewsToRemove)
        {
            previousOrthographicStates.Remove(sceneView);
        }
    }

    public static void OnSelectionChanged()
    {
        if (!UICanvasIsolationSceneOverlay.IsEnabled())
            return;

        var selected = Selection.activeGameObject;
        var isUIElement = selected != null && IsUIElement(selected);

        // CRITICAL: Always restore UI isolation state when switching to non-UI objects
        // This must happen regardless of debouncing to prevent leaving scene in 2D mode
        if (!isUIElement && wasLastSelectionUI)
        {
            RestoreCachedLayerMask();
            EnableSceneViewSkyboxAndPostProcessing(true);
            Disable2DMode();
            wasLastSelectionUI = false;

            // Frame the non-UI object
            FrameSelectedObject();
            return;
        }

        // Debounce selection changes to prevent excessive calls (only for UI elements)
        if (settings.enableDebouncing && isUIElement)
        {
            var currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastSelectionTime < settings.debounceTime)
                return;
            
            lastSelectionTime = currentTime;
        }

        // Handle UI object selection
        if (isUIElement)
        {
            if (IsSupportedCanvas(selected, out Canvas canvas))
            {
                SwitchToUIContext(canvas);
                wasLastSelectionUI = true;
            }
            else if (IsChildOfSupportedCanvas(selected, out Canvas parentCanvas))
            {
                SwitchToChildUIContext(selected, parentCanvas);
                wasLastSelectionUI = true;
            }
        }
    }

    private static void FrameSelectedObject()
    {
        SafeSceneViewOperation(sceneView =>
        {
            var selected = Selection.activeGameObject;
            if (selected == null) return;
            
            // For UI elements, frame the nearest canvas for context
            var nearestCanvas = GetCanvas(selected);
            if (nearestCanvas != null)
            {
                // Frame the nearest canvas to show the UI element in context
                var rectTransform = nearestCanvas.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    var bounds = RectTransformToBounds(rectTransform);
                    // Apply padding to the bounds
                    var center = bounds.center;
                    var size = bounds.size * settings.framingPadding;
                    bounds = new Bounds(center, size);
                    sceneView.Frame(bounds, true);
                    Debug.Log($"Framing nearest canvas: {nearestCanvas.name} (for UI element: {selected.name})");
                }
            }
            else
            {
                // Fallback to standard frame selection for non-UI objects
                sceneView.FrameSelected();
                Debug.Log($"Framing selected object: {selected.name}");
            }
        });
    }
    
    private static void SafeSceneViewOperation(System.Action<SceneView> operation)
    {
        if (settings.affectAllSceneViews)
        {
            // SceneView.sceneViews returns ArrayList, so we need to cast each item
            foreach (SceneView sceneView in SceneView.sceneViews)
            {
                if (sceneView == null) continue;
                
                try
                {
                    operation(sceneView);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in scene view operation: {e.Message}");
                }
            }
        }
        else
        {
            // Only affect the last active scene view
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                try
                {
                    operation(sceneView);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in scene view operation: {e.Message}");
                }
            }
        }
    }

    private static void SwitchToUIContext(Canvas canvas)
    {
        CacheCurrentLayerMask();
        
        // For world space canvases, show the canvas layer instead of UI layer only
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            EnableCanvasLayerOnly(canvas);
        }
        else
        {
            EnableUILayerOnly();
        }
        
        EnableSceneViewSkyboxAndPostProcessing(false);
        SetSceneViewTo2DAndFrameSelection(canvas);
        
        // Frame the selected canvas again after mode is set up for better zoom
        FrameSelectedObject();
    }

    private static void SwitchToChildUIContext(GameObject child, Canvas parentCanvas)
    {
        CacheCurrentLayerMask();
        
        // For world space canvases, show the canvas layer instead of UI layer only
        if (parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            EnableCanvasLayerOnly(parentCanvas);
        }
        else
        {
            EnableUILayerOnly();
        }
        
        EnableSceneViewSkyboxAndPostProcessing(false);

        // Temporarily focus on the parent canvas for framing
        Selection.activeGameObject = parentCanvas.gameObject;
        SetSceneViewTo2DAndFrameSelection(parentCanvas);

        // Restore selection to the child object
        Selection.activeGameObject = child;
        
        // Frame the selected child object after mode is set up
        FrameSelectedObject();
    }

    private static bool IsUIElement(GameObject obj)
    {
        return GetCanvas(obj) != null;
    }
    
    private static Canvas GetCanvas(GameObject obj)
    {
        if (obj == null) return null;
        
        Canvas canvas = null;
        
        if (settings.enableComponentCaching && !canvasCache.TryGetValue(obj, out canvas))
        {
            canvas = GetNearestCanvas(obj);
            canvasCache[obj] = canvas;
        }
        else if (!settings.enableComponentCaching)
        {
            canvas = GetNearestCanvas(obj);
        }
        else if (canvas != null && canvas.gameObject == null)
        {
            // Handle case where cached canvas was destroyed
            canvasCache.Remove(obj);
            canvas = GetNearestCanvas(obj);
            canvasCache[obj] = canvas;
        }
        return canvas;
    }
    
    private static Canvas GetNearestCanvas(GameObject obj)
    {
        // First check if the object itself has a Canvas component
        var canvas = obj.GetComponent<Canvas>();
        if (canvas != null) return canvas;
        
        // For UI elements, we want to find the nearest canvas that actually contains this UI element
        // This is typically the canvas that has a RectTransform and is the immediate parent canvas
        var current = obj.transform.parent;
        while (current != null)
        {
            canvas = current.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Found a canvas - this is the nearest one containing our UI element
                return canvas;
            }
            current = current.parent;
        }
        
        return null;
    }
    
    public static void ClearCache()
    {
        canvasCache.Clear();
        Debug.Log("UI Canvas Isolation cache cleared");
    }
    
    /// <summary>
    /// Force restoration of UI isolation state. Use this as a safety mechanism
    /// to ensure the scene view is never left in an inconsistent state.
    /// </summary>
    public static void ForceRestoreUIState()
    {
        if (wasLastSelectionUI)
        {
            RestoreCachedLayerMask();
            EnableSceneViewSkyboxAndPostProcessing(true);
            Disable2DMode();
            wasLastSelectionUI = false;
            Debug.Log("UI Canvas Isolation state force restored");
        }
    }
    
    /// <summary>
    /// Reset the layer mask cache to show all layers. Useful for debugging layer issues.
    /// </summary>
    public static void ResetLayerMaskCache()
    {
        cachedLayerMask = ~0; // Show all layers
        hasValidLayerCache = false;
        Tools.visibleLayers = cachedLayerMask;
        SceneView.RepaintAll();
        Debug.Log("Layer mask cache reset to show all layers");
    }

    private static bool IsScreenSpaceOverlayCanvas(GameObject obj, out Canvas canvas)
    {
        canvas = GetCanvas(obj);
        return canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay && settings.handleScreenSpaceOverlayCanvases;
    }

    private static bool IsChildOfScreenSpaceOverlayCanvas(GameObject obj, out Canvas parentCanvas)
    {
        parentCanvas = GetCanvas(obj);
        return parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay && settings.handleScreenSpaceOverlayCanvases;
    }
    
    private static bool IsSupportedCanvas(GameObject obj, out Canvas canvas)
    {
        canvas = GetCanvas(obj);
        if (canvas == null) return false;
        
        return (canvas.renderMode == RenderMode.ScreenSpaceOverlay && settings.handleScreenSpaceOverlayCanvases) ||
               (canvas.renderMode == RenderMode.ScreenSpaceCamera && settings.handleScreenSpaceCameraCanvases) ||
               (canvas.renderMode == RenderMode.WorldSpace && settings.handleWorldSpaceCanvases);
    }
    
    private static bool IsChildOfSupportedCanvas(GameObject obj, out Canvas parentCanvas)
    {
        parentCanvas = GetCanvas(obj);
        if (parentCanvas == null) return false;
        
        return (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay && settings.handleScreenSpaceOverlayCanvases) ||
               (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera && settings.handleScreenSpaceCameraCanvases) ||
               (parentCanvas.renderMode == RenderMode.WorldSpace && settings.handleWorldSpaceCanvases);
    }

    private static void SetSceneViewTo2DAndFrameSelection(Canvas canvas)
    {
        SafeSceneViewOperation(sceneView =>
        {
            // Only enable 2D mode for screen space canvases
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Enable2DMode(sceneView);
            }

            if (canvas.TryGetComponent<RectTransform>(out var rectTransform))
            {
                var bounds = RectTransformToBounds(rectTransform);
                // Apply padding to the bounds
                var center = bounds.center;
                var size = bounds.size * settings.framingPadding;
                bounds = new Bounds(center, size);
                sceneView.Frame(bounds, true);
            }
        });
    }

    private static void Enable2DMode(SceneView sceneView)
    {
        // Register undo operation for 2D mode changes
        Undo.RegisterCompleteObjectUndo(sceneView, "UI Canvas Isolation - Enable 2D Mode");
        
        // Store the current orthographic state for this specific scene view
        previousOrthographicStates[sceneView] = sceneView.orthographic;
        sceneView.orthographic = true;
        sceneView.in2DMode = true;
    }
    private static void Disable2DMode()
    {
        SafeSceneViewOperation(sceneView =>
        {
            if (sceneView.in2DMode)
            {
                // Register undo operation for 2D mode changes
                Undo.RegisterCompleteObjectUndo(sceneView, "UI Canvas Isolation - Disable 2D Mode");
                
                // Restore the original orthographic state for this specific scene view
                if (previousOrthographicStates.TryGetValue(sceneView, out bool previousOrthographic))
                {
                    sceneView.orthographic = previousOrthographic;
                    previousOrthographicStates.Remove(sceneView); // Clean up
                }
                else
                {
                    // Fallback: assume it was perspective mode (false)
                    sceneView.orthographic = false;
                }
                
                sceneView.in2DMode = false;
            }
        });
    }

    private static void EnableUILayerOnly()
    {
        // Register undo operation for layer changes
        Undo.RegisterCompleteObjectUndo(SceneView.lastActiveSceneView, "UI Canvas Isolation - Show UI Layer Only");
        
        Tools.visibleLayers = 1 << UILayer;
        SceneView.RepaintAll();
    }
    
    private static void EnableCanvasLayerOnly(Canvas canvas)
    {
        // Register undo operation for layer changes
        Undo.RegisterCompleteObjectUndo(SceneView.lastActiveSceneView, "UI Canvas Isolation - Show Canvas Layer Only");
        
        // Show only the layer that the world space canvas is on
        int canvasLayer = canvas.gameObject.layer;
        Tools.visibleLayers = 1 << canvasLayer;
        SceneView.RepaintAll();
        Debug.Log($"Showing only canvas layer: {canvasLayer} for world space canvas: {canvas.name}");
    }

    private static void CacheCurrentLayerMask()
    {
        // Always cache the current layer mask when entering UI mode
        // This ensures we capture the state right before switching to UI isolation
        cachedLayerMask = Tools.visibleLayers;
        hasValidLayerCache = true;
        Debug.Log($"Cached layer mask: {cachedLayerMask} (wasLastSelectionUI: {wasLastSelectionUI})");
    }

    private static void RestoreCachedLayerMask()
    {
        // Register undo operation for layer changes
        Undo.RegisterCompleteObjectUndo(SceneView.lastActiveSceneView, "UI Canvas Isolation - Restore Layer Mask");
        
        // Safety check: if we don't have a valid cache or it's invalid, use a sensible default
        if (!hasValidLayerCache || cachedLayerMask == (1 << UILayer))
        {
            cachedLayerMask = ~0; // Show all layers as fallback
            Debug.LogWarning($"Invalid or missing cached layer mask, using fallback: {cachedLayerMask}");
        }
        
        Tools.visibleLayers = cachedLayerMask;
        SceneView.RepaintAll();
        hasValidLayerCache = false; // Reset the cache flag
        Debug.Log($"Restored layer mask: {cachedLayerMask}");
    }

    private static void EnableSceneViewSkyboxAndPostProcessing(bool enable)
    {
        SafeSceneViewOperation(sceneView =>
        {
            sceneView.sceneViewState.showSkybox = enable;
            sceneView.sceneViewState.showImageEffects = enable;
        });

        SceneView.RepaintAll();
        Debug.Log($"Scene View Skybox and Post Processing set to: {enable}");
    }

    private static Bounds RectTransformToBounds(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        var min = corners[0];
        var max = corners[0];
        foreach (var corner in corners)
        {
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        return new Bounds((min + max) / 2, max - min);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            isPlayModeActive = true;

            if (wasLastSelectionUI)
            {
                CacheCurrentLayerMask();
                EnableUILayerOnly();
                EnableSceneViewSkyboxAndPostProcessing(false);
            }
        }

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            isPlayModeActive = false;

            // Always restore UI state when exiting play mode to prevent leaving scene in 2D mode
            if (wasLastSelectionUI)
            {
                RestoreCachedLayerMask();
                EnableSceneViewSkyboxAndPostProcessing(true);
                Disable2DMode();
                wasLastSelectionUI = false; // Reset the flag
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            OnSelectionChanged();
        }
    }
}