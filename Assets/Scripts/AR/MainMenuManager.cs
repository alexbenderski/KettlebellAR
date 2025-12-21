using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public LegoImageDetector detector;

    [Header("Explore Windows for Debug")]

    public InstructionSwipe instructionOpening0;
    public GameObject quizButton;   // כדי לנעול אותו
    public GameObject ExploreButton;   // כדי לנעול אותו
    public GameObject quizRoot;
    public GameObject CanvasQuiz;

    public GameObject BackButton;

    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    public GameObject[] exploreWindows;
    private bool wasMainMenuActive = false;




    void Start()
    {
        // מסך פתיחה ON
        mainMenuPanel.SetActive(true);
    // כפתור חזור לא רלוונטי במסך הראשי
        if (BackButton != null)
            BackButton.SetActive(false);
        // כל המסכים של ההוראות OFF
        // instructionOpening0.gameObject.SetActive(false);
    }
    void Update()
    {
        DebugExploreWindows();

        // אם המסך הראשי נדלק עכשיו – והוא קודם היה כבוי
        if (mainMenuPanel.activeInHierarchy && !wasMainMenuActive)
        {
            wasMainMenuActive = true;
        }
        else if (!mainMenuPanel.activeInHierarchy)
        {
            wasMainMenuActive = false;
        }
    }
    void DebugExploreWindows()
    {
        Debug.Log("------ EXPLORE WINDOW STATE CHECK ------");

        if (exploreWindows == null || exploreWindows.Length == 0)
        {
            Debug.Log("No explore windows assigned!");
            return;
        }

        foreach (var w in exploreWindows)
        {
            if (w == null)
            {
                Debug.Log("Null window in list.");
                continue;
            }

            Debug.Log(w.name + " → " + (w.activeInHierarchy ? "ACTIVE" : "INACTIVE"));
        }

        Debug.Log("------ END CHECK ------");
    }


public void OnExplorePressed()
{
    Debug.Log(">>> OnExplorePressed fired");

    PlayClick();

    if (detector != null)
    {
        Debug.Log(">>> Calling ResetDetector()");
        detector.ResetDetector();

        Debug.Log(">>> Calling EnableTracking()");
        detector.EnableTracking();
    }

    Debug.Log(">>> Hiding main menu");
    mainMenuPanel.SetActive(false);

    // פותחים את שלב 0 (הפופאפס של ההסברים)
    instructionOpening0.OpenInstructions();

    // ורק אחרי זה מדליקים את כפתור החזור
    if (BackButton != null)
    {
        BackButton.SetActive(true);

        // דואג שהכפתור יהיה תמיד האחרון (מעל כל שאר האלמנטים בקאנבס)
        BackButton.transform.SetAsLastSibling();

        Debug.Log("BackButton: activeSelf=" + BackButton.activeSelf +
                  " inHierarchy=" + BackButton.activeInHierarchy);
    }
}


    // כפתור Quiz )
    public void OnQuizPressed()
    {
        PlayClick();
        // מכבים את המסך הראשי
        mainMenuPanel.SetActive(false);
        

        quizRoot.SetActive(true);
            // אתחול מלא של ה-Quiz
        quizRoot.GetComponent<QuizManager>().ResetQuiz();
    }

    void PlayClick()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
}
