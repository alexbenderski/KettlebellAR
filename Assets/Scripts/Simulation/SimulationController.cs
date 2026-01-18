using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the simulation panel and explanation panel
/// Manages switching between simulation view and explanation steps
/// </summary>
public class SimulationController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject mainPanel;           // Main panel with choice buttons
    [SerializeField] private GameObject simulationPanel;     // The working simulation part
    [SerializeField] private GameObject explanationPanel;    // The explanation steps panel
    [SerializeField] private GameObject mainMenuPanel;       // Main menu of the app (Canvas)
    [SerializeField] private GameObject simulationCanvas;    // The simulation canvas (parent)
    
    [Header("AR Components (for proper cleanup)")]
    [SerializeField] private LegoImageDetector detector;     // AR image detector to reset
    [SerializeField] private GameObject scannedModel;        // AR scanned model to hide
    
    [Header("UI Buttons")]
    [SerializeField] private Button showExplanationButton;   // Button to open explanation
    [SerializeField] private Button showSimulationButton;    // Button to open simulation
    [SerializeField] private Button exitButton;              // Button to exit to main menu
    [SerializeField] private Button returnFromSimulationButton;  // Return button in simulation panel
    [SerializeField] private Button returnFromExplanationButton; // Return button in explanation panel
    
    [Header("Components")]
    [SerializeField] private SimulationExplanation explanationComponent;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    private void Start()
    {
        // Start with main panel visible, others hidden
        ShowMainPanel();
        
        // Setup button listeners
        if (showExplanationButton != null)
            showExplanationButton.onClick.AddListener(ShowExplanation);
            
        if (showSimulationButton != null)
            showSimulationButton.onClick.AddListener(ShowSimulation);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMainMenu);
            
        if (returnFromSimulationButton != null)
            returnFromSimulationButton.onClick.AddListener(ReturnToSimulationMainPanel);
            
        if (returnFromExplanationButton != null)
            returnFromExplanationButton.onClick.AddListener(ReturnToSimulationMainPanel);
    }

    /// <summary>
    /// Show main panel with choice buttons
    /// </summary>
    public void ShowMainPanel()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);
            
        if (simulationPanel != null)
            simulationPanel.SetActive(false);
            
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
            if (explanationComponent != null)
                explanationComponent.CloseExplanation();
        }
        
        Debug.Log("[SimulationController] Main panel opened");
    }

    /// <summary>
    /// Return to simulation main panel from sub-panels (resets state)
    /// </summary>
    public void ReturnToSimulationMainPanel()
    {
        PlayButtonSound();
        
        // Reset explanation component if needed
        if (explanationComponent != null)
            explanationComponent.CloseExplanation();
        
        // Show main panel and hide others
        ShowMainPanel();
        
        Debug.Log("[SimulationController] Returned to simulation main panel");
    }

    /// <summary>
    /// Exit to main menu of the app
    /// </summary>
    public void ExitToMainMenu()
    {
        PlayButtonSound();
        
        Debug.Log("[SimulationController] === EXIT TO MAIN MENU START ===");
        
        // Reset AR detector (stop tracking and plane detection)
        if (detector != null)
        {
            detector.ResetDetector();
            Debug.Log("[SimulationController] ✓ AR detector reset");
        }
        else
        {
            Debug.LogWarning("[SimulationController] ⚠️ Detector is NULL - cannot reset AR");
        }
        
        // Hide scanned AR model if exists
        if (scannedModel != null)
        {
            scannedModel.SetActive(false);
            Debug.Log("[SimulationController] ✓ Scanned model hidden");
        }
        
        // Reset explanation state
        if (explanationComponent != null)
            explanationComponent.CloseExplanation();
        
        // Hide ALL simulation panels
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
            Debug.Log("[SimulationController] ✓ Main panel hidden");
        }
        
        if (simulationPanel != null)
        {
            simulationPanel.SetActive(false);
            Debug.Log("[SimulationController] ✓ Simulation panel hidden");
        }
        
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
            Debug.Log("[SimulationController] ✓ Explanation panel hidden");
        }
        
        // Hide simulation canvas LAST
        if (simulationCanvas != null)
        {
            simulationCanvas.SetActive(false);
            Debug.Log("[SimulationController] ✓ Simulation canvas hidden");
        }
        else
        {
            Debug.LogWarning("[SimulationController] ⚠️ SimulationCanvas is NULL");
        }
        
        // Show main menu panel
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            Debug.Log("[SimulationController] ✓✓✓ MAIN MENU PANEL ACTIVATED ✓✓✓");
            Debug.Log($"[SimulationController] MainMenuPanel state: activeSelf={mainMenuPanel.activeSelf}, inHierarchy={mainMenuPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[SimulationController] ❌❌❌ MAIN MENU PANEL IS NULL - CANNOT SHOW IT! ❌❌❌");
            Debug.LogError("[SimulationController] Please assign MainMenuPanel in the Inspector!");
        }
        
        Debug.Log("[SimulationController] === EXIT TO MAIN MENU COMPLETE ===");
    }

    /// <summary>
    /// Show the explanation panel with steps
    /// </summary>
    public void ShowExplanation()
    {
        PlayButtonSound();
        
        if (mainPanel != null)
            mainPanel.SetActive(false);
            
        if (simulationPanel != null)
            simulationPanel.SetActive(false);
            
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(true);
            if (explanationComponent != null)
                explanationComponent.OpenExplanation();
        }
        
        Debug.Log("[SimulationController] Explanation panel opened");
    }

    /// <summary>
    /// Show the simulation panel (working part)
    /// </summary>
    public void ShowSimulation()
    {
        PlayButtonSound();
        
        if (mainPanel != null)
            mainPanel.SetActive(false);
            
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
            if (explanationComponent != null)
                explanationComponent.CloseExplanation();
        }
            
        if (simulationPanel != null)
            simulationPanel.SetActive(true);
        
        Debug.Log("[SimulationController] Simulation panel opened");
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

    private void OnDestroy()
    {
        if (showExplanationButton != null)
            showExplanationButton.onClick.RemoveListener(ShowExplanation);
            
        if (showSimulationButton != null)
            showSimulationButton.onClick.RemoveListener(ShowSimulation);
            
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitToMainMenu);
            
        if (returnFromSimulationButton != null)
            returnFromSimulationButton.onClick.RemoveListener(ReturnToSimulationMainPanel);
            
        if (returnFromExplanationButton != null)
            returnFromExplanationButton.onClick.RemoveListener(ReturnToSimulationMainPanel);
    }
}
