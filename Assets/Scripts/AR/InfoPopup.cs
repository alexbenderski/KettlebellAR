using TMPro;
using UnityEngine;

public class InfoPopup : MonoBehaviour
{
    public static InfoPopup Instance;
    public TextMeshProUGUI popupText;
    public GameObject background;

    private void Awake()
    {
        Instance = this;
        background.SetActive(false);
    }

    public static void Show(string message)
    {
        Instance.popupText.text = message;
        Instance.background.SetActive(true);

        // Optional: hide after 2 seconds
        Instance.CancelInvoke(nameof(Hide));
        Instance.Invoke(nameof(Hide), 2f);
    }

    void Hide()
    {
        background.SetActive(false);
    }
}
