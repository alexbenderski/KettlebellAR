//InstructionSwipe
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.EventSystems;


public class InstructionSwipe : MonoBehaviour
{
    
    [Header("UI Controller")]
    public UIController uiController;// Reference to UIController to open animation panel where located in the scene instructions
    public int animationStepIndex = 16; // Step index to open animation panel

    [Header("Canvas Dragging")]
    public RectTransform explanationCanvas;  // ExplanationCanvas0 to be dragged
    public Canvas canvas;                    // Canvas parent (needed for raycaster)
    public Camera eventCamera;               // Camera for World Space canvas conversion
    public float centerDistance = 10f;       // Distance from camera when centering
    private bool isDragging = false;
    private Vector2 dragOffset;
    private GraphicRaycaster graphicRaycaster;
    private Vector2 dragStartPosition;       // Initial touch position when drag begins
    private Vector3 dragStartCanvasPos;      // Canvas position when drag begins
    
    [Header("Pinch Zoom")]
    public float minScale = 0.5f;            // Minimum scale (50% of original)
    public float maxScale = 1.5f;            // Maximum scale (150% of original)
    private float initialPinchDistance = 0f; // Distance between fingers at start
    private Vector3 initialScale;            // Canvas scale at pinch start
    private Vector3 originalScale;           // Original canvas scale (set once)


    [System.Serializable]
    public class Step
    {
         public VideoClip video;    // נגן הוידאו
        public Sprite image;
        [TextArea(5, 10)]
        public string text;
    }

    public List<Step> steps;

    public GameObject fullScreenPopup;   // Full screen UI panel
    public Image stepImage;              // StepImage (UI)
    public RTLTextMeshPro stepText;      // StepText (UI)
    public RawImage stepVideo;          
    public VideoPlayer videoPlayer;
    public Button centerButton;          // Button to center canvas in front of camera     

    public AudioSource audioSource;
    public AudioClip swipeSound;

    public System.Action OnInstructionsClosed;  // Event fired when instructions close

    private int currentStep = 0;
    private bool isActive = false;

    private Vector2 swipeStart;
    private bool isSwiping = false;

    // Sensitivity
    private float swipeThreshold = 120f;

    void Start()
    {
        // Get the GraphicRaycaster from the canvas
        if (canvas == null && explanationCanvas != null)
            canvas = explanationCanvas.GetComponentInParent<Canvas>();
        
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"[InstructionSwipe] Canvas render mode: {canvas.renderMode}");
            
            // Auto-detect camera for World Space canvas
            if (eventCamera == null)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    eventCamera = canvas.worldCamera ?? Camera.main;
                    Debug.Log($"[InstructionSwipe] World Space canvas detected, using camera: {eventCamera?.name}");
                }
                else
                {
                    eventCamera = null; // Screen Space doesn't need camera
                }
            }
        }
        
        if (explanationCanvas != null)
            Debug.Log($"[InstructionSwipe] ExplanationCanvas initialized: {explanationCanvas.name}, position: {explanationCanvas.position}");
        
        // Setup center button listener
        if (centerButton != null)
        {
            centerButton.onClick.RemoveAllListeners();
            centerButton.onClick.AddListener(CenterCanvasInFrontOfCamera);
            Debug.Log("[InstructionSwipe] Center button listener added");
        }
        else
        {
            Debug.LogWarning("[InstructionSwipe] Center button not assigned in Inspector");
        }
    }

    // --------------------------------------------------------
    // Helper: Check if touch hit the ExplanationCanvas or its children
    //----------------------------------------------------------
    private bool IsTouchOnCanvas(Vector2 touchPosition)
    {
        if (explanationCanvas == null)
            return false;

        // Simple approach: check if touch is within canvas bounds
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            explanationCanvas, 
            touchPosition, 
            null,  // For overlay canvas
            out localPoint
        );

        // Check if point is inside the rect
        bool isInside = explanationCanvas.rect.Contains(localPoint);
        
        Debug.Log($"[InstructionSwipe] Touch at screen {touchPosition}, local {localPoint}, inside: {isInside}, rect: {explanationCanvas.rect}");
        
        return isInside;
    }

    //----------------------------------------------------------
    // Helper: Clamp canvas position to stay within screen bounds
    //----------------------------------------------------------
    private void ClampCanvasPosition()
    {
        if (explanationCanvas == null)
            return;

        Rect canvasRect = explanationCanvas.rect;
        Vector3 canvasPos = explanationCanvas.position;
        
        // Get parent canvas rect
        Canvas parentCanvas = explanationCanvas.GetComponentInParent<Canvas>();
        RectTransform parentRect = parentCanvas.GetComponent<RectTransform>();
        
        // Clamp X
        float minX = -parentRect.rect.width / 2 + canvasRect.width / 2;
        float maxX = parentRect.rect.width / 2 - canvasRect.width / 2;
        canvasPos.x = Mathf.Clamp(canvasPos.x, minX, maxX);
        
        // Clamp Y
        float minY = -parentRect.rect.height / 2 + canvasRect.height / 2;
        float maxY = parentRect.rect.height / 2 - canvasRect.height / 2;
        canvasPos.y = Mathf.Clamp(canvasPos.y, minY, maxY);
        
        explanationCanvas.position = canvasPos;
    }

    // --------------------------------------------------------
    // OPEN INSTRUCTIONS
    // --------------------------------------------------------
    // public void OpenInstructions()
    // {
    //     currentStep = 0;
    //     UpdateStep();

    //     fullScreenPopup.SetActive(true);
    //     isActive = true;

    //     // Disable all PartClick scripts so they don't swallow touches
    //     foreach (var pc in FindObjectsOfType<PartClick>())
    //         pc.enabled = false;
    // }


    public void OpenInstructions()
{
    // כיבוי כל InstructionSwipe אחרים (חשוב!)
    foreach (var swipe in FindObjectsOfType<InstructionSwipe>())
    {
        if (swipe != this)
        {
            swipe.fullScreenPopup.SetActive(false);
            swipe.isActive = false;
        }
    }

    // איפוס הצעדים
    currentStep = 0;

    // עדכון UI
    UpdateStep();

    // הפעלת הפופאפ
    fullScreenPopup.SetActive(true);

    // הפעלת הסוויפים
    isActive = true;
    
    // Store original scale for zoom calculations
    if (explanationCanvas != null)
    {
        originalScale = explanationCanvas.localScale;
        Debug.Log($"[InstructionSwipe] Original scale stored: {originalScale}");
    }

    // כיבוי PartClick כדי שלא ישתלט על מגעים
    foreach (var pc in FindObjectsOfType<PartClick>())
        pc.enabled = false;

    // כיבוי FingerTrailFollow כדי שלא ישתלט על מגעים
    foreach (var ft in FindObjectsOfType<FingerTrailFollow>())
        ft.enabled = false;

    Debug.Log("InstructionSwipe [" + this.name + "] activated.");
}


// ------------------------------------------------------------------------
    // reset when click on back from the exploring screens. so we can go again through all the steps from the begening
// ------------------------------------------------------------------------

    // public void ResetSteps()
    // {
    //     currentStep = 0;
    //     isActive = false;

    //     // סגירה מלאה של פופאפ במקרה והוא נשאר דלוק
    //     if (fullScreenPopup != null)
    //         fullScreenPopup.SetActive(false);
    // }

    public void ResetSteps()
    {
        currentStep = 0;
        isActive = false;

        if (fullScreenPopup != null)
            fullScreenPopup.SetActive(false);

        // עצירת וידאו
        if (videoPlayer != null)
            videoPlayer.Stop();
    }

    
    // --------------------------------------------------------
    // CLOSE INSTRUCTIONS
    // --------------------------------------------------------
    public void CloseInstructions()
    {
        fullScreenPopup.SetActive(false);
        isActive = false;

        // Enable PartClick scripts again
        foreach (var pc in FindObjectsOfType<PartClick>())
            pc.enabled = true;

        // Enable FingerTrailFollow scripts again
        foreach (var ft in FindObjectsOfType<FingerTrailFollow>())
            ft.enabled = true;

        // Invoke the event to notify listeners that instructions are closed
        OnInstructionsClosed?.Invoke();
    }

    // --------------------------------------------------------
    // CENTER CANVAS IN FRONT OF CAMERA
    // --------------------------------------------------------
    public void CenterCanvasInFrontOfCamera()
    {
        if (explanationCanvas == null)
        {
            Debug.LogWarning("[InstructionSwipe] Cannot center - explanationCanvas is null");
            return;
        }
        
        Camera cam = eventCamera ?? Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[InstructionSwipe] Cannot center - no camera found");
            return;
        }
        
        // Calculate position directly in front of camera at specified distance
        Vector3 centerPosition = cam.transform.position + cam.transform.forward * centerDistance;
        explanationCanvas.position = centerPosition;
        
        // Reset rotation to face camera (optional - makes canvas face user)
        explanationCanvas.rotation = Quaternion.LookRotation(cam.transform.forward);
        
        // Scale is NOT changed - keeps current zoom level
        
        Debug.Log($"[InstructionSwipe] Canvas centered at {centerPosition}, distance: {centerDistance}m from camera");
    }

    // --------------------------------------------------------
    // UPDATE LOOP
    // --------------------------------------------------------
    void Update()
    {
        if (!isActive)
            return;

        // ---- 2-FINGER DRAG + PINCH ZOOM ----
        // Works from anywhere on screen, converts screen delta to world space for World Space canvas
        if (Input.touchCount >= 2 && explanationCanvas != null)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Start 2-finger interaction (only on first detection)
            if (!isDragging && (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began))
            {
                isDragging = true;
                // Calculate midpoint between two fingers
                dragStartPosition = (touch0.position + touch1.position) / 2f;
                dragStartCanvasPos = explanationCanvas.position;
                
                // Initialize pinch zoom
                initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = explanationCanvas.localScale;
                
                isSwiping = false;  // Cancel any swipe in progress
                Debug.Log($"[InstructionSwipe] 2-finger interaction started at screen {dragStartPosition}, canvas world pos: {dragStartCanvasPos}, pinch distance: {initialPinchDistance}");
            }

            // Continue 2-finger drag + pinch zoom
            if (isDragging)
            {
                // ---- PINCH ZOOM ----
                float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                float pinchDelta = currentPinchDistance - initialPinchDistance;
                
                // Calculate scale factor relative to ORIGINAL scale (not current)
                // This prevents compounding scale on repeated pinches
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                Vector3 targetScale = originalScale * scaleFactor;
                
                // Clamp scale to min/max relative to original scale
                targetScale.x = Mathf.Clamp(targetScale.x, originalScale.x * minScale, originalScale.x * maxScale);
                targetScale.y = Mathf.Clamp(targetScale.y, originalScale.y * minScale, originalScale.y * maxScale);
                targetScale.z = Mathf.Clamp(targetScale.z, originalScale.z * minScale, originalScale.z * maxScale);
                
                explanationCanvas.localScale = targetScale;
                
                // Calculate percentage of original for display
                float scalePercent = (targetScale.x / originalScale.x) * 100f;
                
                // ---- DRAG ----
                // Calculate current midpoint and delta in screen space
                Vector2 currentMidpoint = (touch0.position + touch1.position) / 2f;
                Vector2 screenDelta = currentMidpoint - dragStartPosition;
                
                // Convert screen delta to world delta for World Space canvas
                Vector3 worldDelta;
                if (canvas.renderMode == RenderMode.WorldSpace && eventCamera != null)
                {
                    // Get canvas plane distance from camera
                    float distanceToCanvas = Vector3.Distance(eventCamera.transform.position, explanationCanvas.position);
                    
                    // Convert screen delta to world units
                    Vector3 screenStart = new Vector3(dragStartPosition.x, dragStartPosition.y, distanceToCanvas);
                    Vector3 screenCurrent = new Vector3(currentMidpoint.x, currentMidpoint.y, distanceToCanvas);
                    
                    Vector3 worldStart = eventCamera.ScreenToWorldPoint(screenStart);
                    Vector3 worldCurrent = eventCamera.ScreenToWorldPoint(screenCurrent);
                    
                    worldDelta = worldCurrent - worldStart;
                }
                else
                {
                    // Screen Space canvas - use pixel delta directly
                    worldDelta = new Vector3(screenDelta.x, screenDelta.y, 0);
                }
                
                // Apply delta to canvas position
                Vector3 newPos = dragStartCanvasPos + worldDelta;
                explanationCanvas.position = newPos;
                
                Debug.Log($"[InstructionSwipe] Scale: {scalePercent:F0}% of original, Pos: {newPos}");
            }

            // End 2-finger interaction (when finger count drops below 2)
            if (Input.touchCount < 2 && isDragging)
            {
                isDragging = false;
                Debug.Log("[InstructionSwipe] 2-finger interaction ended");
            }

            return;  // Skip single-finger swipe logic when 2 fingers detected
        }

        // ---- 1-FINGER SWIPE (Navigation) ----
        if (Input.touchCount == 0)
        {
            isDragging = false;
            return;
        }

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            // Begin touch - start swipe tracking
            if (t.phase == TouchPhase.Began)
            {
                swipeStart = t.position;
                isSwiping = true;
                isDragging = false;
            }

            // Touch moved - update swipe but don't trigger yet
            if (t.phase == TouchPhase.Moved && isSwiping)
            {
                // Swipe detection happens on TouchPhase.Ended
            }

            // End swipe
            if (t.phase == TouchPhase.Ended && isSwiping)
            {
                float dx = t.position.x - swipeStart.x;

                // If swipe is long enough → change step
                if (Mathf.Abs(dx) > swipeThreshold)
                {
                    if (dx < 0) NextStep();
                    else PrevStep();
                }
                else
                {
                    // Tap → close only if last step
                    if (currentStep == steps.Count - 1)
                        CloseInstructions();
                }

                isSwiping = false;
            }
        }
    }

    // --------------------------------------------------------
    // STEP NAVIGATION
    // --------------------------------------------------------
    void NextStep()
    {
        if (currentStep < steps.Count - 1)
        {
            currentStep++;
            UpdateStep();

            // 🔊 play swipe sound
            if (audioSource != null && swipeSound != null)
                audioSource.PlayOneShot(swipeSound);
        }
    }

void PrevStep()
{
    if (currentStep > 0)
    {
        currentStep--;
        UpdateStep();

        // 🔊 play swipe sound
        if (audioSource != null && swipeSound != null)
            audioSource.PlayOneShot(swipeSound);
    }
}

    // --------------------------------------------------------
    // UPDATE UI
    // --------------------------------------------------------
    void UpdateStep()
    {
        Step s = steps[currentStep];// קבלת הצעד הנוכחי

        // ---- TEXT ----
        stepText.text = s.text;

        // ---- IMAGE ----
        if (s.image != null)
        {
            stepImage.sprite = s.image;
            stepImage.gameObject.SetActive(true);
        }
        else
        {
            stepImage.gameObject.SetActive(false);
        }


        // ---- VIDEO ----
        if (s.video != null)
        {
            stepVideo.gameObject.SetActive(true);

            videoPlayer.clip = s.video;   
            videoPlayer.Play();

            stepImage.gameObject.SetActive(false);
        }
        else
        {
            stepVideo.gameObject.SetActive(false);
            videoPlayer.Stop();
        }

        // ---- OPEN ANIMATION PANEL VIA UIController ----
        if (uiController != null && currentStep == animationStepIndex)
        {
            uiController.OpenAnimationPanel();
        }


    }

    public bool IsActive()
{
    return isActive;
}
}
