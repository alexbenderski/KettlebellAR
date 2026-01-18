using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages explanation steps with optional video/image and text
/// Supports swipe navigation with back/forward buttons
/// </summary>
public class SimulationExplanation : MonoBehaviour
{
    [System.Serializable]
    public class ExplanationStep
    {
        [Tooltip("Optional video for this step")]
        public VideoClip video;
        
        [Tooltip("Optional image for this step (shown if no video)")]
        public Sprite image;
        
        [Tooltip("Text content for this step")]
        [TextArea(5, 10)]
        public string text;
    }

    [Header("Steps Content")]
    [SerializeField] private List<ExplanationStep> steps = new List<ExplanationStep>();
    
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI stepText;           // Text display
    [SerializeField] private TextMeshProUGUI stepCounter;        // Shows "Step X/Y"
    [SerializeField] private VideoPlayer videoPlayer;           // For video steps
    [SerializeField] private RawImage videoDisplay;             // Display for video
    [SerializeField] private Image imageDisplay;                // Display for image steps
    
    [Header("Navigation Buttons")]
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button backButton;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swipeSound;
    
    private int currentStepIndex = 0;

    private void Start()
    {
        SetupButtons();
        UpdateStepDisplay();
    }

    private void SetupButtons()
    {
        if (forwardButton != null)
            forwardButton.onClick.AddListener(NextStep);
            
        if (backButton != null)
            backButton.onClick.AddListener(PreviousStep);
    }

    /// <summary>
    /// Opens explanation and shows first step
    /// </summary>
    public void OpenExplanation()
    {
        currentStepIndex = 0;
        gameObject.SetActive(true);
        UpdateStepDisplay();
        Debug.Log("[SimulationExplanation] Opened at step 1");
    }

    /// <summary>
    /// Closes explanation panel
    /// </summary>
    public void CloseExplanation()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
            
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Navigate to next step
    /// </summary>
    public void NextStep()
    {
        if (currentStepIndex < steps.Count - 1)
        {
            PlaySwipeSound();
            currentStepIndex++;
            UpdateStepDisplay();
            Debug.Log($"[SimulationExplanation] Next → Step {currentStepIndex + 1}");
        }
    }

    /// <summary>
    /// Navigate to previous step
    /// </summary>
    public void PreviousStep()
    {
        if (currentStepIndex > 0)
        {
            PlaySwipeSound();
            currentStepIndex--;
            UpdateStepDisplay();
            Debug.Log($"[SimulationExplanation] Back ← Step {currentStepIndex + 1}");
        }
    }

    /// <summary>
    /// Updates the display to show current step content
    /// </summary>
    private void UpdateStepDisplay()
    {
        if (steps == null || steps.Count == 0 || currentStepIndex < 0 || currentStepIndex >= steps.Count)
        {
            Debug.LogWarning("[SimulationExplanation] Invalid step index or no steps available");
            return;
        }

        ExplanationStep currentStep = steps[currentStepIndex];

        // Update text content
        if (stepText != null)
            stepText.text = currentStep.text;

        // Update step counter (e.g., "Step 1/7")
        if (stepCounter != null)
            stepCounter.text = $"Step {currentStepIndex + 1}/{steps.Count}";

        // Handle video or image display
        bool hasVideo = currentStep.video != null;
        bool hasImage = currentStep.image != null;

        // Show video if available
        if (hasVideo && videoPlayer != null && videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            if (imageDisplay != null)
                imageDisplay.gameObject.SetActive(false);
                
            videoPlayer.clip = currentStep.video;
            videoPlayer.Play();
        }
        // Otherwise show image if available
        else if (hasImage && imageDisplay != null)
        {
            if (videoDisplay != null)
                videoDisplay.gameObject.SetActive(false);
                
            imageDisplay.gameObject.SetActive(true);
            imageDisplay.sprite = currentStep.image;
            
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Stop();
        }
        // Hide both if neither available
        else
        {
            if (videoDisplay != null)
                videoDisplay.gameObject.SetActive(false);
            if (imageDisplay != null)
                imageDisplay.gameObject.SetActive(false);
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Stop();
        }

        // Update button interactability
        if (backButton != null)
            backButton.interactable = currentStepIndex > 0;
            
        if (forwardButton != null)
            forwardButton.interactable = currentStepIndex < steps.Count - 1;
    }

    private void PlaySwipeSound()
    {
        if (audioSource != null && swipeSound != null)
            audioSource.PlayOneShot(swipeSound);
    }

    private void OnDestroy()
    {
        if (forwardButton != null)
            forwardButton.onClick.RemoveListener(NextStep);
            
        if (backButton != null)
            backButton.onClick.RemoveListener(PreviousStep);
    }
}
