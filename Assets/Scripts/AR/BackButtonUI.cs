using UnityEngine;

// public class BackButtonExplore : MonoBehaviour
// {
//     public GameObject mainMenuPanel;
//     public GameObject exploreButton;
//     public GameObject quizButton;

//     public GameObject[] allExploreObjects; // כל חלונות הסוויפ, פופאפים, מודל וכו'

//     public void OnBackPressed()
//     {
//         // כיבוי כל UI של Explore
//         foreach (var obj in allExploreObjects)
//         {
//             if (obj != null)
//                 obj.SetActive(false);
//         }

//         // החזרת מסך ראשי
//         mainMenuPanel.SetActive(true);
//         exploreButton.SetActive(true);
//         quizButton.SetActive(true);
//     }

// }

public class BackButtonExplore : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject exploreButton;
    public GameObject quizButton;

    public GameObject[] instructionSwipeObjects; // כל InstructionSwipe_opening_window0/1/2
    public GameObject[] fullScreenPopups;        // כל FullScreenPopup_opening0/1/2
    public GameObject scannedModel;              // אם יש AR Model
    public LegoImageDetector detector;
    public GameObject backButton;

    public void OnBackPressed()
    {
        // detector.DisableTracking(); //disable AR tracking for the image detection
        detector.ResetDetector();

        // 1) לכבות את כל הפופאפים של הסוויפים
        foreach (var p in fullScreenPopups)
            if (p != null)
                p.SetActive(false);

        // 2) להדליק את אובייקטי הסוויפ עצמם (חשוב!!)
        foreach (var s in instructionSwipeObjects)
            if (s != null)
                s.SetActive(true);

        // 3) לכבות מודל AR אם יש
        if (scannedModel != null)
            scannedModel.SetActive(false);

        // 4) להדליק את המסך הראשי
        mainMenuPanel.SetActive(true);
        exploreButton.SetActive(true);
        quizButton.SetActive(true);

        Debug.Log(">>>Back button set to false");
        backButton.SetActive(false);
    }
}