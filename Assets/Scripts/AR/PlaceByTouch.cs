using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceBytouch : MonoBehaviour
{
    public GameObject virtualGround;
    public GameObject Lego3D;
    public event System.Action OnModelPlaced;

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
    void HandlePlaneSelection()
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
    void HighlightSelectedPlane()
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(plane == selectedPlane);
    }

    // Let user tap on selected plane to place model
    void HandleModelPlacementOnPlane()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Ended)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                if (hits.Count > 0)
                {
                    ARPlane hitPlane = planeManager.GetPlane(hits[0].trackableId);
                    if (hitPlane == selectedPlane)
                    {
                        PlaceModelOnPlane(hits[0].pose);
                    }
                }
            }
        }
    }

    void PlaceModelOnPlane(Pose hitPose)
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
        OnModelPlaced?.Invoke();
        DisablePlanes();
    }

    void DisablePlanes()
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
        planeManager.enabled = false;
    }

    void HandleRotationDrag()
    {
        if (!placed) return;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastTouchX = touch.position.x;
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
}