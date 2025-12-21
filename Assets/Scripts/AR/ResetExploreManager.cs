using UnityEngine;

public class ResetExploreManager : MonoBehaviour
{
    [Header("All Instruction Windows")]
    public GameObject[] instructionWindows;   // כל InstructionSwipe_window0/1/2...
    
    [Header("All Fullscreen Popups")]
    public GameObject[] fullscreenPopups;     // כל FullScreenPopup_opening0/1/2...

    [Header("AR Components")]
    public GameObject arSessionOrigin;
    public GameObject scannedModel;           // האובייקט שנסרק (אם יש)

    [Header("Opening Instruction")]
    public InstructionSwipe openingWindow0;   // חלון מספר 0

    public void FullResetExplore()
    {
        // 1) כיבוי כל חלונות ההוראות
        foreach (var w in instructionWindows)
            if (w != null) w.SetActive(false);

        // 2) כיבוי כל הפופאפים
        foreach (var p in fullscreenPopups)
            if (p != null) p.SetActive(false);

        // 3) איפוס AR — מכבה הכול ושולף מודל סרוק
        if (arSessionOrigin != null)
            arSessionOrigin.SetActive(false);

        if (scannedModel != null)
            Destroy(scannedModel);

        // מפעילים מחדש AR (ריק)
        if (arSessionOrigin != null)
            arSessionOrigin.SetActive(true);

        // 4) איפוס ההתקדמות של חלון מספר 0
        if (openingWindow0 != null)
        {
            openingWindow0.ResetSteps();
            // openingWindow0.OpenInstructions();
        }
    }
}
