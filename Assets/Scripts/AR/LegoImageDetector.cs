using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LegoImageDetector : MonoBehaviour
{
    public ARTrackedImageManager manager;
    public GameObject modelSpawned;

    public InstructionSwipe instructionStage0;
    public InstructionSwipe instructionStage1;
    public InstructionSwipe instructionStage2;

    public PlaceBytouch placeByTouch;

    private bool imageDetected = false;
    private bool planePlaced = false;
    private bool stage0Completed = false;  // Guard flag: block detection until stage 0 finishes

    public AudioSource audioSource;
    public AudioClip model_spawned_sound;
    public AudioClip photo_scanned_sound;

    void Awake()// אתחול משתנים
    {
  
        if (manager == null)
            manager = FindFirstObjectByType<ARTrackedImageManager>();

        if (placeByTouch == null)
            placeByTouch = FindFirstObjectByType<PlaceBytouch>();
    }

    void OnEnable()// הרשמה לאירועים
    {
        if (manager != null)
            manager.trackedImagesChanged += OnImageChanged;//   טיפול בשינויי תמונות במעקב

        if (placeByTouch != null)
            placeByTouch.OnModelPlaced += HandleModelPlaced;// טיפול באירוע הנחת מודל
    }

    void OnDisable()// ביטול הרשמה לאירועים
    {
        if (manager != null)
            manager.trackedImagesChanged -= OnImageChanged; // ביטול טיפול בשינויי תמונות במעקב

        if (placeByTouch != null)
            placeByTouch.OnModelPlaced -= HandleModelPlaced;    // ביטול טיפול באירוע הנחת מודל
    }

       //נקרא מתוך 
    // MainMenuManager
    // Explore כשנכנסים ל-
    // הפונקציה מאתחלת את המעקב אחרי תמונות ומכינה את הסצנה לזיהוי תמונה
    public void EnableTracking()//מכינים את הסצנה לזיהוי תמונה
    {
        Debug.Log("[LegoImageDetector] EnableTracking");
        // Reset states:
        imageDetected = false;
        planePlaced = false;
        stage0Completed = false;  // Reset flag when entering explore

        if (manager != null)
            manager.enabled = true;// הפעלת מעקב תמונות

        if (placeByTouch != null)
        { 
            //placed=false תמיד מתחילים מצב נקי – בלי מודל ועם 
            placeByTouch.ResetPlacement();
            placeByTouch.DisablePlaneDetection();  // Explicitly disable planes before stage 0
            placeByTouch.enabled = false;   // נדליק אותו רק אחרי זיהוי תמונה
        }

        // Subscribe to stage 0 completion
        if (instructionStage0 != null)
            instructionStage0.OnInstructionsClosed += HandleStage0Completed;//watch for stage 0 close event
    }

    // נקרא מתוך BackButtonExplore כשחוזרים לתפריט
    public void ResetDetector()//set everything back to initial state before returning to main menu
    {
        Debug.Log("[LegoImageDetector] ResetDetector");

        imageDetected = false;
        planePlaced = false;
        stage0Completed = false;  // Reset flag on return to menu

        // Unsubscribe from stage 0
        if (instructionStage0 != null)
            instructionStage0.OnInstructionsClosed -= HandleStage0Completed;

        // Unsubscribe from stage 1
        if (instructionStage1 != null)
            instructionStage1.OnInstructionsClosed -= HandleStage1Completed;

        if (placeByTouch != null)
        {
            placeByTouch.ResetPlacement();
            placeByTouch.DisablePlaneDetection();  // Explicitly disable planes on reset
            placeByTouch.enabled = false;
        }

        if (modelSpawned != null)// הסתרת המודל אם קיים
        {
            modelSpawned.SetActive(false);
            modelSpawned = null;
        }

        // "ריסט" עדין ל-ImageManager – בלי ARSession.Reset
        if (manager != null)
        {
            manager.enabled = false;
            manager.enabled = true;
        }
    }

    // ----------------------------------------------------------
    private void OnImageChanged(ARTrackedImagesChangedEventArgs args)//    כשמתרחשים שינויים במעקב התמונות אז קוראים לפונקציה הזו כדי שתטפל בהם
    {
        foreach (var img in args.added)
            CheckImage(img);// בדיקת תמונה חדשה במעקב לאיתור התמונה הרצויה 

        foreach (var img in args.updated)
            CheckImage(img);// בדיקת תמונה מעודכנת במעקב לאיתור התמונה הרצויה
    }

    private void CheckImage(ARTrackedImage img) //בודקים אם התמונה במעקב היא התמונה הרצויה
    {
        if (imageDetected || !stage0Completed)  // Block until stage 0 done
            return;

        // מוודאים שהתמונה באמת במעקב
        if (img.referenceImage.name == "hub_for_detection" && 
            img.trackingState == TrackingState.Tracking)// בדיקה אם התמונה שזוהתה היא התמונה הרצויה על ידי השוואת השם שלה
        {
            Debug.Log("[LegoImageDetector] IMAGE DETECTED: hub_for_detection");
            imageDetected = true;

            if (audioSource != null && photo_scanned_sound != null)
                audioSource.PlayOneShot(photo_scanned_sound);

            // שלב 0 → סגור, שלב 1 → נפתח
            if (instructionStage0 != null)
                instructionStage0.CloseInstructions();

            if (instructionStage1 != null)
            {
                instructionStage1.OpenInstructions();
                // Subscribe to stage 1 closing to enable planes
                instructionStage1.OnInstructionsClosed += HandleStage1Completed;
            }

            // עכשיו מותר להניח מודל על משטח
            if (placeByTouch != null)
            {
                placeByTouch.ResetPlacement();
                placeByTouch.enabled = true;
                // Planes will be enabled when Stage 1 instructions close
            }
        }
    }

    private void HandleStage1Completed()
    {
        Debug.Log("[LegoImageDetector] Stage 1 closed — enabling plane detection");
        
        if (instructionStage1 != null)
            instructionStage1.OnInstructionsClosed -= HandleStage1Completed;
        
        if (placeByTouch != null)
            placeByTouch.EnablePlaneDetection();
    }

    private void HandleModelPlaced()
    {
        Debug.Log("[LegoImageDetector] HandleModelPlaced() called.");

        if (planePlaced)
            return;

        planePlaced = true;

        if (placeByTouch != null)
            modelSpawned = placeByTouch.Lego3D;

        if (audioSource != null && model_spawned_sound != null)
            audioSource.PlayOneShot(model_spawned_sound);

        Debug.Log("[LegoImageDetector] MODEL PLACED — showing stage 2 instructions");

        if (instructionStage1 != null)
            instructionStage1.CloseInstructions();

        if (instructionStage2 != null)
            instructionStage2.OpenInstructions();
    }

    // Called when stage 0 closes
    private void HandleStage0Completed()
    {
        Debug.Log("[LegoImageDetector] Stage 0 completed — enabling image detection");
        stage0Completed = true;
    }
}
