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
    public UIController uiController;
    public int animationStepIndex = 16; // Step index to open animation panel

    [Header("Canvas Dragging")]
    public RectTransform explanationCanvas;  // ExplanationCanvas0 to be dragged
    public Canvas canvas;                    // Canvas parent (needed for raycaster)
    private bool isDragging = false;
    private Vector2 dragOffset;
    private GraphicRaycaster graphicRaycaster;
    private Vector2 dragStartPosition;       // Initial touch position when drag begins
    private Vector3 dragStartCanvasPos;      // Canvas position when drag begins


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
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
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

    // כיבוי PartClick כדי שלא ישתלט על מגעים
    foreach (var pc in FindObjectsOfType<PartClick>())
        pc.enabled = false;

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

        // Invoke the event to notify listeners that instructions are closed
        OnInstructionsClosed?.Invoke();
    }

    // --------------------------------------------------------
    // UPDATE LOOP
    // --------------------------------------------------------
    void Update()
    {
        if (!isActive)
            return;

        if (Input.touchCount == 0)
        {
            isDragging = false;
            return;
        }

        Touch t = Input.GetTouch(0);

        // Begin touch - check if dragging canvas
        if (t.phase == TouchPhase.Began)
        {
            swipeStart = t.position;
            isSwiping = true;
            isDragging = false;  // Don't start dragging yet
            
            // Check if touch is on canvas using GraphicRaycaster
            if (IsTouchOnCanvas(t.position))
            {
                // Mark as potential drag - will confirm on first move
                isDragging = true;
                dragStartPosition = t.position;
                dragStartCanvasPos = explanationCanvas.position;
                Debug.Log($"[InstructionSwipe] Touch detected on canvas at {dragStartPosition}");
            }
        }

        // Dragging canvas - only move if we detected initial touch on canvas
        if (t.phase == TouchPhase.Moved && isDragging && explanationCanvas != null)
        {
            // Calculate delta movement from where touch started
            Vector2 touchDelta = t.position - dragStartPosition;
            
            // Apply delta to canvas position (1:1 movement)
            Vector3 newPos = dragStartCanvasPos + new Vector3(touchDelta.x, touchDelta.y, 0);
            explanationCanvas.position = newPos;
            
            // Clamp canvas to stay within bounds
            ClampCanvasPosition();
            
            // Cancel the swipe action since we're dragging now
            isSwiping = false;
        }

        // End swipe/drag
        if (t.phase == TouchPhase.Ended)
        {
            if (isDragging)
            {
                // We were dragging, just stop
                isDragging = false;
                Debug.Log("[InstructionSwipe] Canvas drag ended");
            }
            else if (isSwiping)
            {
                // We were swiping (not dragging), handle navigation or close
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
            }

            isSwiping = false;
            isDragging = false;
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
