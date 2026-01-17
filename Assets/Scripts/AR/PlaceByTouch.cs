using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceBytouch : MonoBehaviour
{
    public GameObject virtualGround;
    public GameObject Lego3D;
    public event System.Action OnModelPlaced;
    
    [Header("Sensor Highlighting")]
    public GameObject[] sensorPartsToHighlight;  // Manually assign 3 sensor parts in Inspector
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 2f;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private List<GameObject> sensorParts = new List<GameObject>();

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private bool placed = false;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Plane selection state
    private ARPlane selectedPlane = null;
    private bool planeSelected = false;
    private bool isDragging = false;
    private float lastTouchX;

    void Start()
    {
        raycastManager = FindFirstObjectByType<ARRaycastManager>();
        planeManager = FindFirstObjectByType<ARPlaneManager>();
        virtualGround.SetActive(false);
        Lego3D.SetActive(false);
    }

    void OnEnable()
    {
        // Plane manager will be explicitly enabled after image detection
        // Do NOT enable planeManager here - let LegoImageDetector control it
    }
    
    // Called by LegoImageDetector after image detection to enable plane selection
    public void EnablePlaneDetection()
    {
        if (planeManager != null)
            planeManager.enabled = true;
        ShowAllPlanes();
    }
    
    // Called by LegoImageDetector to ensure planes are hidden
    public void DisablePlaneDetection()
    {
        if (planeManager != null)
        {
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(false);
            planeManager.enabled = false;
        }
    }

    void Update()
    {
        HandleRotationDrag();
        if (placed) return;
        if (!planeSelected)
            HandlePlaneSelection();
        else
            HandleModelPlacementOnPlane();
    }

    // Show all planes for selection
    void ShowAllPlanes()
    {
        if (planeManager == null) return;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(true);
    }

    // Let user tap to select a plane
    void HandlePlaneSelection() // טיפול בבחירת מישור על ידי המשתמש
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Ended)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                if (hits.Count > 0)
                {
                    selectedPlane = planeManager.GetPlane(hits[0].trackableId);
                    if (selectedPlane != null)
                    {
                        planeSelected = true;
                        HighlightSelectedPlane();
                        Debug.Log("[PlaceByTouch] Plane selected: " + selectedPlane.trackableId);
                    }
                }
            }
        }
    }

    // Highlight only the selected plane
    void HighlightSelectedPlane()// הדגשת המישור הנבחר
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(plane == selectedPlane);
    }

    // Let user tap on selected plane to place model
    void HandleModelPlacementOnPlane()// טיפול במיקום המודל על המישור הנבחר
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Ended)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon)) // בדיקת ריי קסט על המישור
            {
                if (hits.Count > 0)
                {
                    ARPlane hitPlane = planeManager.GetPlane(hits[0].trackableId); // קבלת המישור מהתוצאה
                    if (hitPlane == selectedPlane)
                    {
                        PlaceModelOnPlane(hits[0].pose);//hits[0].pose = הפוזיציה והסיבוב של נקודת המגע על המישור
                    }
                }
            }
        }
    }

    void PlaceModelOnPlane(Pose hitPose)// הצבת המודל על המישור
    {
        virtualGround.transform.position = hitPose.position;
        virtualGround.transform.rotation = hitPose.rotation;
        Vector2 planeSize = selectedPlane.size;
        virtualGround.transform.localScale = new Vector3(planeSize.x, 1, planeSize.y);
        virtualGround.SetActive(true);
        Lego3D.transform.position = hitPose.position;
        Lego3D.transform.rotation = Quaternion.identity;
        Lego3D.SetActive(true);
        placed = true;
        
        // FresnelPulse component on Lego3D handles the glowing effect automatically on OnEnable()
        
        OnModelPlaced?.Invoke();
        DisablePlanes();
    }

    void DisablePlanes()// השבתת כל המישורים
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
        planeManager.enabled = false;
    }

    void HandleRotationDrag()// טיפול בסיבוב המודל באמצעות גרירה - רק כשנוגעים במודל עצמו
    {
        if (!placed) return;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                // Check if touch hits the 3D model
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if we hit the Lego3D model or any of its children
                    if (hit.transform.IsChildOf(Lego3D.transform) || hit.transform == Lego3D.transform)
                    {
                        isDragging = true;
                        lastTouchX = touch.position.x;
                        Debug.Log($"[PlaceByTouch] Started rotation on: {hit.transform.name}");
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = touch.position.x - lastTouchX;
                Lego3D.transform.Rotate(0, deltaX * 0.3f, 0);
                lastTouchX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
    }

    public void ResetPlacement()
    {
        placed = false;
        planeSelected = false;
        selectedPlane = null;
        
        // FresnelPulse component handles cleanup on OnDisable() automatically
        
        if (planeManager != null)
        {
            planeManager.enabled = false;  // Disable plane manager on reset
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(false);
        }
        // Reset transforms to default state
        if (Lego3D != null)
        {
            Lego3D.SetActive(false);
            Lego3D.transform.position = Vector3.zero;
            Lego3D.transform.rotation = Quaternion.identity;
        }
        if (virtualGround != null)
        {
            virtualGround.SetActive(false);
            virtualGround.transform.position = Vector3.zero;
            virtualGround.transform.rotation = Quaternion.identity;
            virtualGround.transform.localScale = Vector3.one;
        }
    }
    
    // ---- SENSOR HIGHLIGHTING METHODS ----
    void FindSensorParts()
    {
        if (Lego3D == null) 
        {
            Debug.LogWarning("[PlaceByTouch] Lego3D is null, cannot find sensor parts");
            return;
        }
        
        sensorParts.Clear();
        originalMaterials.Clear();
        
        // Use manually assigned sensor parts from Inspector
        if (sensorPartsToHighlight != null && sensorPartsToHighlight.Length > 0)
        {
            foreach (var sensorObj in sensorPartsToHighlight)
            {
                if (sensorObj == null) continue;
                
                sensorParts.Add(sensorObj);
                
                // Store original materials
                Renderer renderer = sensorObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[renderer] = renderer.materials;
                }
                else
                {
                    Debug.LogWarning($"[PlaceByTouch] Sensor part '{sensorObj.name}' has no Renderer component");
                }
            }
            
            Debug.Log($"[PlaceByTouch] Found {sensorParts.Count} manually assigned sensor parts to highlight");
        }
        else
        {
            Debug.LogWarning("[PlaceByTouch] No sensor parts assigned in 'Sensor Parts To Highlight' array! Please assign 3 sensor GameObjects in Inspector.");
        }
    }
    
    void HighlightSensorParts()
    {
        if (sensorParts.Count == 0)
        {
            Debug.LogError("[PlaceByTouch] No sensor parts found to highlight! Check if sensor parts are assigned in Inspector.");
            return;
        }
        
        int highlightedCount = 0;
        foreach (var sensorObj in sensorParts)
        {
            if (sensorObj == null)
            {
                Debug.LogWarning("[PlaceByTouch] Sensor part is null, skipping");
                continue;
            }
            
            Debug.Log($"[PlaceByTouch] Attempting to highlight: {sensorObj.name}, active: {sensorObj.activeInHierarchy}");
            
            // Try to get Renderer on the object itself first
            Renderer renderer = sensorObj.GetComponent<Renderer>();
            
            // If not found, look for renderers in children
            if (renderer == null)
            {
                Debug.Log($"[PlaceByTouch] No Renderer on {sensorObj.name}, searching children...");
                Renderer[] childRenderers = sensorObj.GetComponentsInChildren<Renderer>();
                
                if (childRenderers.Length > 0)
                {
                    Debug.Log($"[PlaceByTouch] Found {childRenderers.Length} renderers in children of {sensorObj.name}");
                    
                    // Highlight all child renderers
                    foreach (var childRenderer in childRenderers)
                    {
                        if (childRenderer != null && childRenderer.gameObject.activeInHierarchy)
                        {
                            HighlightRenderer(childRenderer, sensorObj.name);
                            highlightedCount++;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlaceByTouch] No Renderer found on {sensorObj.name} or its children");
                }
            }
            else
            {
                // Highlight the renderer on the object
                HighlightRenderer(renderer, sensorObj.name);
                highlightedCount++;
            }
        }
        
        Debug.Log($"[PlaceByTouch] Highlighting complete: {highlightedCount} total renderers highlighted");
    }
    
    void HighlightRenderer(Renderer renderer, string partName)
    {
        if (renderer == null) return;
        
        Debug.Log($"[PlaceByTouch] Highlighting renderer on {partName}, material count: {renderer.materials.Length}");
        
        // Create highlighted materials
        Material[] highlightedMats = new Material[renderer.materials.Length];
        
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            // Create a copy of the material
            highlightedMats[i] = new Material(renderer.materials[i]);
            
            // For URP Lit shader, use different property names
            // Try both common emission property names
            
            // Method 1: Set _EmissionColor (standard)
            highlightedMats[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
            
            // Method 2: Enable emission keywords
            highlightedMats[i].EnableKeyword("_EMISSION");
            highlightedMats[i].SetFloat("_EmissionEnabled", 1f);
            
            // Method 3: Boost the base color significantly to ensure visibility
            Color baseColor = highlightedMats[i].GetColor("_BaseColor");
            if (baseColor == Color.clear)
            {
                baseColor = Color.white; // Fallback
            }
            
            // Make it brighter and more yellow
            Color boostedColor = Color.Lerp(baseColor, highlightColor, 0.6f);
            highlightedMats[i].SetColor("_BaseColor", boostedColor);
            
            // Also try the legacy _Color property
            highlightedMats[i].SetColor("_Color", boostedColor);
            
            Debug.Log($"[PlaceByTouch] Material setup: BaseColor={boostedColor}, EmissionColor={highlightColor * highlightIntensity}, Shader={highlightedMats[i].shader.name}");
        }
        
        renderer.materials = highlightedMats;
        Debug.Log($"[PlaceByTouch] ✓ Successfully highlighted renderer on {partName}");
    }
    
    void RemoveHighlighting()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value;
            }
        }
    }
}