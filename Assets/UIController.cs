using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject animationPanel;

    public void OpenAnimationPanel()
    {
        mainMenuPanel.SetActive(false);
        animationPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        animationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
