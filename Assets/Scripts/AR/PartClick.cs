// using UnityEngine;
// using RTLTMPro;   // חשוב!

// public class PartClick : MonoBehaviour
// {
//     public Camera Cam;
//     public GameObject PopupPanel;
//     public RTLTextMeshPro PopupText;

//     private bool popupOpen = false;

//     void Update()
//     {
//         // אם יש לחיצה
//         if (Input.touchCount == 0)
//             return;

//         Touch touch = Input.GetTouch(0);

//         // לחיצה לסגירת הפופאפ
//         if (popupOpen && touch.phase == TouchPhase.Ended)
//         {
//             PopupPanel.SetActive(false);
//             popupOpen = false;
//             return;
//         }

//         // אם הפופאפ סגור — בודקים על מה לחצו
//         if (!popupOpen && touch.phase == TouchPhase.Ended)
//         {
//             Ray ray = Cam.ScreenPointToRay(touch.position);

//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 if (hit.transform == transform)
//                 {
//                     ShowPopup();
//                 }
//             }
//         }
//     }

//         void ShowPopup()
//         {
//             // אם לחצו על ה-Hub → פותחים הוראות, לא פופאפ רגיל
//             if (name == "ModuleHub")
//             {
//                 FindObjectOfType<InstructionSwipe>().OpenInstructions();
//                 return; // יוצאים מהפונקציה – לא מפעילים Popup רגיל
//             }

//             // אם זה כל חלק אחר → מציגים את הפופאפ הרגיל
//             PopupPanel.SetActive(true);
//             PopupText.text = "You clicked on " + name + " this part is doing the follow:";

//             popupOpen = true; // חשוב שיהיה כאן!
//         }
// }




using UnityEngine;
using RTLTMPro;

public class PartClick : MonoBehaviour
{
    public Camera Cam;
    public AudioSource audioSource;
    public AudioClip part_selected_sound;

    [Header("InstructionSwipe for THIS specific part")]
    public InstructionSwipe instructionSwipe;   // רפרנס ישיר!

    private bool popupOpen = false;

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        // אם מסך הוראות פתוח — כל לחיצה סוגרת
        if (popupOpen && touch.phase == TouchPhase.Ended)
        {
            instructionSwipe.CloseInstructions();
            popupOpen = false;
            return;
        }

        // אם פאנל סגור — בדיקת ריי קסט
        if (!popupOpen && touch.phase == TouchPhase.Ended)
        {
            Ray ray = Cam.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    
                    OpenInstructions();
                }
            }
        }
    }

    void OpenInstructions()
    {
        if (instructionSwipe == null)
        {
            Debug.LogError("PartClick: Missing InstructionSwipe reference!");
            return;
        }
        if (audioSource != null && part_selected_sound != null)
            audioSource.PlayOneShot(part_selected_sound);

        instructionSwipe.OpenInstructions();
        popupOpen = true;
    }
}