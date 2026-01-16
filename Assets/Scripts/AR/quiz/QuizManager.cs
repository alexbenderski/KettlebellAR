using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    #region Data Structures
    
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)]
        public string question;
        public string[] answers = new string[4];
        public int correctIndex; // 0=A, 1=B, 2=C, 3=D
        public DifficultyLevel difficulty = DifficultyLevel.Easy;
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    [System.Serializable]
    public class SensorCategory
    {
        public string sensorName; // "Distance Sensor", "Force Sensor", "Hub"
        public string sensorDescription; // תיאור קצר
        public Color sensorColor = Color.cyan; // צבע לכרטיס
        public Sprite sensorIcon;
        public QuizQuestion[] questions;
        public Sprite badgeSprite;
        public AudioClip successSound;
    }

    #endregion

    #region Inspector References
    
    [Header("=== SENSOR CATEGORIES ===")]
    public SensorCategory[] sensorCategories;

    [Header("=== LEVEL SELECTION PANEL ===")]
    public GameObject levelSelectionPanel;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public TMP_Text levelTitleText;

    [Header("=== SENSOR SELECTION PANEL ===")]
    public GameObject sensorSelectionPanel;
    public TMP_Text sensorSelectionTitle;
    
    // 3 כפתורי חיישן פשוטים + טקסטים
    public Button distanceSensorButton;
    public TMP_Text distanceSensorText;
    public Button hubSensorButton;
    public TMP_Text hubSensorText;
    public Button forceSensorButton;
    public TMP_Text forceSensorText;

    [Header("=== OVERALL PROGRESS BAR ===")]
    public GameObject overallProgressPanel;
    public Slider overallProgressBar;
    public TMP_Text overallProgressText;
    public TMP_Text overallProgressPercent;
    public Transform sensorProgressContainer; // מכולת לפסי התקדמות של חיישנים בודדים (מתחת לפס הכללי)
    public GameObject sensorProgressPrefab; // פריפאב של פס התקדמות חיישן בודד (עם שם, סליידר ואחוז)

    [Header("=== BADGE DISPLAY (Above Progress) ===")]
    public Transform earnedBadgesContainer;
    public GameObject earnedBadgePrefab;

    [Header("=== QUESTION PANEL ===")]
    public GameObject questionPanel;
    public TMP_Text questionText;
    public TMP_Text questionCounterText; // "Question 3/5"
    public Button answerA;
    public Button answerB;
    public Button answerC;
    public Button answerD;
    public TMP_Text textA;
    public TMP_Text textB;
    public TMP_Text textC;
    public TMP_Text textD;
    public Slider questionProgressBar;
    public Image currentSensorIcon;
    public TMP_Text currentSensorName;

    [Header("=== BUTTON SPRITES ===")]
    public Sprite normalSprite;
    public Sprite correctSprite;
    public Sprite wrongSprite;

    [Header("=== SENSOR SUCCESS PANEL ===")]
    public GameObject sensorSuccessPanel;
    public Image successBadge;
    public TMP_Text successMessage;
    public TMP_Text successSubMessage;
    public Animator successAnimator;
    public Button continueButton;

    [Header("=== FINAL COMPLETION PANEL ===")]
    public GameObject finalCompletionPanel;
    public Transform finalBadgeContainer;
    public GameObject finalBadgePrefab;
    public TMP_Text finalTitleText;
    public TMP_Text finalMessageText;
    public Button restartButton;
    public Button backToMenuButton;

    [Header("=== GENERAL UI ===")]
    public GameObject mainMenuPanel;
    public GameObject ExploreButton;
    public GameObject quizButton;
    public GameObject BackButton;
    public GameObject quizRoot;
    public GameObject background_main_menu;

    [Header("=== AUDIO ===")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip badgeEarnedSound;
    public AudioClip badgeDisplaySound; // Sound when badge appears in EarnedBadgesContainer
    public AudioClip allCompletedSound;
    public AudioClip buttonClickSound;

    [Header("=== LEGACY (Backward Compatibility) ===")] 
    public GameObject scoreImage;
    public TMP_Text scoreText;

    #endregion

    #region Private State
    
    private DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
    private int currentSensorIndex = -1;
    private int currentQuestion = 0;
    private int currentSensorScore = 0; // ניקוד בחיישן הנוכחי
    
    // מעקב לכל חיישן
    private bool[] sensorCompleted;
    private bool[] sensorBadgeEarned; // האם קיבל badge (3+ תשובות נכונות)
    private int[] sensorCorrectAnswers; // כמה תשובות נכונות בכל חיישן
    private int[] sensorTotalAnswered; // כמה שאלות נענו בכל חיישן
    
    private List<int> availableSensors; // חיישנים שעדיין לא הושלמו
    private bool answerLocked = false;
    
    // רשימת שאלות מסוננות לפי קושי
    private List<QuizQuestion> filteredQuestions;

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        Debug.Log("[QuizManager] Start() called");
        
        // Check if sensorCategories is set up
        if (sensorCategories == null || sensorCategories.Length == 0)
        {
            Debug.LogError("[QuizManager] ERROR: sensorCategories is not set up in Inspector! Must have 3 sensors.");
            return;
        }
        
        Debug.Log($"[QuizManager] Found {sensorCategories.Length} sensors");
        
        InitializeState();
        SetupButtonListeners();
        SetupSensorButtonListeners();
        SetupBackButton();
        ShowLevelSelection();
    }
    
    void SetupSensorButtonListeners()
    {
        Debug.Log("[QuizManager] === SetupSensorButtonListeners START ===");
        
        // חיבור כפתורי חיישנים אוטומטית
        if (distanceSensorButton != null)
        {
            Debug.Log($"[QuizManager] Distance button found: {distanceSensorButton.gameObject.name}");
            Debug.Log($"[QuizManager] Distance button onClick listener count BEFORE: {distanceSensorButton.onClick.GetPersistentEventCount()}");
            
            distanceSensorButton.onClick.RemoveAllListeners();
            distanceSensorButton.onClick.AddListener(() => 
            {
                Debug.Log("[QuizManager] *** DISTANCE BUTTON CLICKED! ***");
                OnSensorSelected(0);
            });
            
            Debug.Log($"[QuizManager] Distance button interactable: {distanceSensorButton.interactable}");
            Debug.Log("[QuizManager] ✓ Distance button listener added");
        }
        else
        {
            Debug.LogError("[QuizManager] ❌ Distance button is NULL! Not assigned in Inspector!");
        }
        
        if (hubSensorButton != null)
        {
            hubSensorButton.onClick.RemoveAllListeners();
            hubSensorButton.onClick.AddListener(() => 
            {
                Debug.Log("[QuizManager] *** HUB BUTTON CLICKED! ***");
                OnSensorSelected(1);
            });
            Debug.Log("[QuizManager] ✓ Hub button listener added");
        }
        else
        {
            Debug.LogError("[QuizManager] ❌ Hub button is NULL! Not assigned in Inspector!");
        }
        
        if (forceSensorButton != null)
        {
            forceSensorButton.onClick.RemoveAllListeners();
            forceSensorButton.onClick.AddListener(() => 
            {
                Debug.Log("[QuizManager] *** FORCE BUTTON CLICKED! ***");
                OnSensorSelected(2);
            });
            Debug.Log("[QuizManager] ✓ Force button listener added");
        }
        else
        {
            Debug.LogError("[QuizManager] ❌ Force button is NULL! Not assigned in Inspector!");
        }
        
        Debug.Log("[QuizManager] Sensor buttons listeners setup complete");
        Debug.Log("[QuizManager] === SetupSensorButtonListeners END ===");
    }
    
    void SetupBackButton()
    {
        // חיבור כפתור Back אוטומטית
        if (BackButton != null)
        {
            var backBtn = BackButton.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(BackButtonPressed);
                Debug.Log("[QuizManager] Back button listener setup complete");
            }
        }
    }

    void InitializeState()
    {
        int sensorCount = sensorCategories.Length;
        Debug.Log($"[QuizManager] InitializeState - sensorCount = {sensorCount}");
        
        sensorCompleted = new bool[sensorCount];
        sensorBadgeEarned = new bool[sensorCount];
        sensorCorrectAnswers = new int[sensorCount];
        sensorTotalAnswered = new int[sensorCount];
        availableSensors = new List<int>();
        
        for (int i = 0; i < sensorCount; i++)
            availableSensors.Add(i);
    }

    void SetupButtonListeners()// הגדרת מאזיני לחיצות לכפתורים השונים
    {
        // תשובות
        answerA.onClick.AddListener(() => OnAnswerSelected(0));
        answerB.onClick.AddListener(() => OnAnswerSelected(1));
        answerC.onClick.AddListener(() => OnAnswerSelected(2));
        answerD.onClick.AddListener(() => OnAnswerSelected(3));

        // רמות קושי
        if (easyButton != null)
            easyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Easy));
        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Medium));
        if (hardButton != null)
            hardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Hard));

        // כפתורי סיום
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackButtonPressed);
    }


    #endregion

    #region Level Selection

    void ShowLevelSelection()
    {
        Debug.Log("[QuizManager] ShowLevelSelection");
        
        HideAllPanels();
        levelSelectionPanel.SetActive(true);
        BackButton.SetActive(true);
        
        if (levelTitleText != null)
            levelTitleText.text = "Select Difficulty Level";
        
        // עדכון טקסט כפתורים
        UpdateLevelButtonsUI();
    }

    void UpdateLevelButtonsUI()
    {
        // ניתן להוסיף כאן סימון של רמה נוכחית או הצגת סטטיסטיקות
    }

    void OnDifficultySelected(DifficultyLevel difficulty)
    {
        Debug.Log($"[QuizManager] OnDifficultySelected: {difficulty}");
        
        PlayButtonClick();
        currentDifficulty = difficulty;
        ShowSensorSelection();
    }

    #endregion

    #region Sensor Selection

    void ShowSensorSelection()
    {
        Debug.Log("[QuizManager] ShowSensorSelection");
        
        HideAllPanels();
        sensorSelectionPanel.SetActive(true);
        
        // CRITICAL FIX: Disable raycast on background image if it exists
        Transform background = sensorSelectionPanel.transform.Find("background");
        if (background != null)
        {
            Image bgImage = background.GetComponent<Image>();
            if (bgImage != null)
            {
                if (bgImage.raycastTarget)
                {
                    Debug.LogWarning("[QuizManager] *** FOUND BLOCKING BACKGROUND! Disabling raycastTarget ***");
                    bgImage.raycastTarget = false;
                }
                else
                {
                    Debug.Log("[QuizManager] Background raycastTarget already disabled (OK)");
                }
            }
        }
        
        // Check if EventSystem exists
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
            Debug.LogError("[QuizManager] *** NO EVENTSYSTEM FOUND! UI clicks will NOT work! ***");
        else
            Debug.Log($"[QuizManager] EventSystem found: {eventSystem.name}, enabled: {eventSystem.enabled}");
        
        // Check for UI blocking elements in panel
        CheckForBlockingUI();
        
        // הצגת Progress bar רק אחרי שבחרו חיישן ראשון
        bool hasStarted = sensorCompleted[0] || sensorCompleted[1] || sensorCompleted[2];
        if (overallProgressPanel != null)
            overallProgressPanel.SetActive(hasStarted);
        
        BackButton.SetActive(true);
        
        if (sensorSelectionTitle != null)
            sensorSelectionTitle.text = "Choose a Sensor you want to start with :";
        
        // עדכון פס התקדמות כללי
        if (hasStarted)
        {
            UpdateOverallProgress();
            UpdateEarnedBadgesDisplay();
        }
        
        // הצגת/הסתרת כפתורים לפי מה שהושלם
        UpdateSensorButtons();
    }
    
    void CheckForBlockingUI()
    {
        Debug.Log("[QuizManager] === CHECKING FOR UI BLOCKERS ===");
        
        // Check all Image components in sensor panel that might block clicks
        if (sensorSelectionPanel != null)
        {
            Image[] allImages = sensorSelectionPanel.GetComponentsInChildren<Image>(true);
            Debug.Log($"[QuizManager] Found {allImages.Length} Image components in chooseSensorPanel");
            
            foreach (Image img in allImages)
            {
                if (img.raycastTarget && img.gameObject != distanceSensorButton?.gameObject 
                    && img.gameObject != hubSensorButton?.gameObject 
                    && img.gameObject != forceSensorButton?.gameObject)
                {
                    Debug.LogWarning($"[QuizManager] *** BLOCKING IMAGE FOUND: {img.name} (raycastTarget=true) ***", img.gameObject);
                    Debug.LogWarning($"    → To fix: Select '{img.name}' in hierarchy, uncheck 'Raycast Target' in Image component");
                }
            }
        }
    }
    
void UpdateSensorButtons()
{
    Debug.Log("[QuizManager] === UpdateSensorButtons START ===");
    
    // Distance Sensor (Index 0)
    if (distanceSensorButton != null)
    {
        bool isCompleted = sensorCompleted[0];
        distanceSensorButton.gameObject.SetActive(!isCompleted);
        distanceSensorButton.interactable = !isCompleted; // Ensure clickable
        
        if (distanceSensorText != null)
            distanceSensorText.gameObject.SetActive(!isCompleted);
        
        Debug.Log($"[QuizManager] Distance: Active={!isCompleted}, Interactable={distanceSensorButton.interactable}, GameObject.name={distanceSensorButton.gameObject.name}");
    }
    else
    {
        Debug.LogError("[QuizManager] ❌ distanceSensorButton is NULL!");
    }
    
    // Hub (Index 1)
    if (hubSensorButton != null)
    {
        bool isCompleted = sensorCompleted[1];
        hubSensorButton.gameObject.SetActive(!isCompleted);
        hubSensorButton.interactable = !isCompleted; // Ensure clickable
        
        if (hubSensorText != null)
            hubSensorText.gameObject.SetActive(!isCompleted);
        
        Debug.Log($"[QuizManager] Hub: Active={!isCompleted}, Interactable={hubSensorButton.interactable}");
    }
    else
    {
        Debug.LogError("[QuizManager] ❌ hubSensorButton is NULL!");
    }
    
    // Force Sensor (Index 2)
    if (forceSensorButton != null)
    {
        bool isCompleted = sensorCompleted[2];
        forceSensorButton.gameObject.SetActive(!isCompleted);
        forceSensorButton.interactable = !isCompleted; // Ensure clickable
        
        if (forceSensorText != null)
            forceSensorText.gameObject.SetActive(!isCompleted);
        
        Debug.Log($"[QuizManager] Force: Active={!isCompleted}, Interactable={forceSensorButton.interactable}");
    }
    else
    {
        Debug.LogError("[QuizManager] ❌ forceSensorButton is NULL!");
    }
    
    Debug.Log($"[QuizManager] UpdateSensorButtons - D:{!sensorCompleted[0]}, H:{!sensorCompleted[1]}, F:{!sensorCompleted[2]}");
    Debug.Log("[QuizManager] === UpdateSensorButtons END ===");
}

    public void OnSensorSelected(int sensorIndex)
    {
        Debug.Log($"[QuizManager] OnSensorSelected CALLED with index: {sensorIndex}");
        
        if (sensorIndex < 0 || sensorIndex >= sensorCategories.Length)
        {
            Debug.LogError($"[QuizManager] Invalid sensor index: {sensorIndex}");
            return;
        }
        
        Debug.Log($"[QuizManager] Sensor: {sensorCategories[sensorIndex].sensorName}");
        
        PlayButtonClick();
        currentSensorIndex = sensorIndex;
        currentQuestion = 0;
        currentSensorScore = 0;
        answerLocked = false;
        
        Debug.Log($"[QuizManager] Before filter - Questions available: {sensorCategories[sensorIndex].questions.Length}");
        
        // סינון שאלות לפי רמת קושי
        FilterQuestionsForCurrentSensor();
        
        Debug.Log($"[QuizManager] After filter - Filtered questions: {filteredQuestions.Count}");
        
        if (filteredQuestions.Count == 0)
        {
            Debug.LogError($"[QuizManager] ERROR: No questions after filtering!");
            return;
        }
        
        // מעבר למסך שאלות
        ShowQuestionPanel();
    }

    void FilterQuestionsForCurrentSensor()
    {
        filteredQuestions = new List<QuizQuestion>();
        var allQuestions = sensorCategories[currentSensorIndex].questions;
        
        // סינון לפי רמת קושי - רק השאלות המתאימות
        foreach (var q in allQuestions)
        {
            bool include = currentDifficulty switch
            {
                DifficultyLevel.Easy => q.difficulty == DifficultyLevel.Easy,      // רק Easy
                DifficultyLevel.Medium => q.difficulty == DifficultyLevel.Medium,   // רק Medium
                DifficultyLevel.Hard => q.difficulty == DifficultyLevel.Hard,       // רק Hard
                _ => true
            };
            
            if (include)
                filteredQuestions.Add(q);
        }
        
        Debug.Log($"[QuizManager] Filtered {filteredQuestions.Count} questions for difficulty {currentDifficulty}");
        
        // אם אין שאלות ברמה זו, קח את כולן
        if (filteredQuestions.Count == 0)
        {
            Debug.LogWarning($"[QuizManager] No questions found for {currentDifficulty}, using all questions");
            filteredQuestions.Clear();
            filteredQuestions.AddRange(allQuestions);
        }
        
        // ערבוב
        ShuffleList(filteredQuestions);
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    #endregion

    #region Question Panel

    void ShowQuestionPanel()
    {
        Debug.Log("[QuizManager] ShowQuestionPanel() CALLED");
        
        HideAllPanels();
        Debug.Log("[QuizManager] All panels hidden");
        
        if (questionPanel == null)
        {
            Debug.LogError("[QuizManager] ERROR: questionPanel is NULL! Not assigned in Inspector!");
            return;
        }
        
        questionPanel.SetActive(true);
        Debug.Log("[QuizManager] Question panel activated");
        
        // תמיד להציג progress bar כשנמצאים בשאלות
        if (overallProgressPanel != null)
            overallProgressPanel.SetActive(true);
        
        BackButton.SetActive(true);
        
        // עדכון כותרת חיישן
        var sensor = sensorCategories[currentSensorIndex];
        if (currentSensorIcon != null) currentSensorIcon.sprite = sensor.sensorIcon;
        if (currentSensorName != null) currentSensorName.text = sensor.sensorName;
        
        Debug.Log($"[QuizManager] Loading first question for {sensor.sensorName}");
        LoadQuestion();
    }

    void LoadQuestion()
    {
        if (currentQuestion >= filteredQuestions.Count)
        {
            ShowSensorSuccess();
            return;
        }
        
        var q = filteredQuestions[currentQuestion];
        
        // עדכון פס התקדמות
        UpdateQuestionProgress();
        UpdateOverallProgress();
        
        // שמירת תשובה נכונה לפני ערבוב
        string correctAnswer = q.answers[q.correctIndex];
        
        // ערבוב תשובות
        string[] shuffledAnswers = (string[])q.answers.Clone();
        for (int i = 0; i < shuffledAnswers.Length; i++)
        {
            int r = Random.Range(0, shuffledAnswers.Length);
            (shuffledAnswers[i], shuffledAnswers[r]) = (shuffledAnswers[r], shuffledAnswers[i]);
        }
        
        // מציאת אינדקס חדש
        int newCorrectIndex = System.Array.IndexOf(shuffledAnswers, correctAnswer);
        q.correctIndex = newCorrectIndex;
        q.answers = shuffledAnswers;
        
        // הצגה על המסך
        questionText.text = q.question;
        textA.text = q.answers[0];
        textB.text = q.answers[1];
        textC.text = q.answers[2];
        textD.text = q.answers[3];
        
        ResetButtonSprites();
        answerLocked = false;
    }

    void UpdateQuestionProgress()
    {
        int total = filteredQuestions.Count;
        float progress = (float)(currentQuestion) / total;
        
        if (questionProgressBar != null)
            questionProgressBar.value = progress;
        
        if (questionCounterText != null)
            questionCounterText.text = $"Question {currentQuestion + 1}/{total}";
    }

    void ResetButtonSprites()
    {
        answerA.image.sprite = normalSprite;
        answerB.image.sprite = normalSprite;
        answerC.image.sprite = normalSprite;
        answerD.image.sprite = normalSprite;
        
        answerA.interactable = true;
        answerB.interactable = true;
        answerC.interactable = true;
        answerD.interactable = true;
    }

    void OnAnswerSelected(int index)
    {
        if (answerLocked) return;
        answerLocked = true;
        
        var q = filteredQuestions[currentQuestion];
        bool correct = index == q.correctIndex;
        
        // עדכון סטטיסטיקות
        sensorTotalAnswered[currentSensorIndex]++;
        
        if (correct)
        {
            if (audioSource != null && correctSound != null)
                audioSource.PlayOneShot(correctSound);
            currentSensorScore++;
            sensorCorrectAnswers[currentSensorIndex]++;
            HighlightCorrect(index);
            
            // בדיקה אם הגיע ל-3 תשובות נכונות - מקבל badge!
            if (sensorCorrectAnswers[currentSensorIndex] == 3 && !sensorBadgeEarned[currentSensorIndex])
            {
                sensorBadgeEarned[currentSensorIndex] = true;
                if (audioSource != null && badgeEarnedSound != null)
                    audioSource.PlayOneShot(badgeEarnedSound);
                // עדכון תצוגת badges
                UpdateEarnedBadgesDisplay();
            }
        }
        else
        {
            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);
            HighlightWrong(index);
            HighlightCorrect(q.correctIndex);
        }
        
        Invoke(nameof(NextQuestion), 1.2f);
    }

    void HighlightCorrect(int index) => GetButton(index).image.sprite = correctSprite;
    void HighlightWrong(int index) => GetButton(index).image.sprite = wrongSprite;

    Button GetButton(int index) => index switch
    {
        0 => answerA,
        1 => answerB,
        2 => answerC,
        3 => answerD,
        _ => answerA
    };

    void NextQuestion()
    {
        currentQuestion++;
        
        if (currentQuestion >= filteredQuestions.Count)
        {
            ShowSensorSuccess();
            return;
        }
        
        LoadQuestion();
    }

    #endregion

    #region Progress Tracking

    void UpdateOverallProgress()// עדכון התקדמות כללית
    {
        // חישוב התקדמות כללית - רק לפי רמת הקושי הנוכחית
        int totalSensors = sensorCategories.Length;
        int completedSensors = 0;
        
        foreach (bool c in sensorCompleted)
            if (c) completedSensors++;
        
        // אחוז לפי חיישנים שהושלמו (לא לפי שאלות)
        float overallPercent = totalSensors > 0 ? (float)completedSensors / totalSensors : 0f;
        
        if (overallProgressBar != null)
            overallProgressBar.value = overallPercent;
        
        if (overallProgressPercent != null)
            overallProgressPercent.text = $"{Mathf.RoundToInt(overallPercent * 100)}%";
        
        if (overallProgressText != null)
            overallProgressText.text = $"Completed {completedSensors}/{totalSensors} sensors - Keep learning!";
        
        Debug.Log($"[QuizManager] Progress: {completedSensors}/{totalSensors} sensors = {Mathf.RoundToInt(overallPercent * 100)}%");
        
        // עדכון פסי התקדמות של כל חיישן (אם יש)
        UpdateSensorProgressBars();
    }

    void UpdateSensorProgressBars()
    {
        if (sensorProgressContainer == null || sensorProgressPrefab == null) return;
        
        // מחיקת פסים קיימים
        foreach (Transform child in sensorProgressContainer)
            Destroy(child.gameObject);
        
        // יצירת פס לכל חיישן
        for (int i = 0; i < sensorCategories.Length; i++)
        {
            var sensor = sensorCategories[i];
            int total = sensor.questions.Length;
            int answered = sensorTotalAnswered[i];
            float percent = total > 0 ? (float)answered / total : 0f;
            
            GameObject progressObj = Instantiate(sensorProgressPrefab, sensorProgressContainer);
            
            var nameText = progressObj.transform.Find("Name")?.GetComponent<TMP_Text>();
            var slider = progressObj.GetComponentInChildren<Slider>();
            var percentText = progressObj.transform.Find("Percent")?.GetComponent<TMP_Text>();
            
            if (nameText != null) nameText.text = sensor.sensorName;
            if (slider != null) slider.value = percent;
            if (percentText != null) percentText.text = $"{Mathf.RoundToInt(percent * 100)}%";
        }
    }

    void UpdateEarnedBadgesDisplay()
    {
        if (earnedBadgesContainer == null || earnedBadgePrefab == null) return;
        
        // Count current badges before clearing
        int previousBadgeCount = earnedBadgesContainer.childCount;
        
        // מחיקת badges קיימים
        foreach (Transform child in earnedBadgesContainer)
            Destroy(child.gameObject);
        
        // הצגת badges שנצברו
        int newBadgeCount = 0;
        for (int i = 0; i < sensorCategories.Length; i++)
        {
            if (sensorBadgeEarned[i])
            {
                GameObject badge = Instantiate(earnedBadgePrefab, earnedBadgesContainer);
                var img = badge.GetComponent<Image>();
                if (img != null) img.sprite = sensorCategories[i].badgeSprite;
                newBadgeCount++;
            }
        }
        
        // Play sound if a new badge was added (not on initial display)
        if (newBadgeCount > previousBadgeCount && audioSource != null && badgeDisplaySound != null)
        {
            audioSource.PlayOneShot(badgeDisplaySound);
            Debug.Log("[QuizManager] Badge display sound played!");
        }
    }

    #endregion

    #region Sensor Success

    void ShowSensorSuccess()
    {
        var sensor = sensorCategories[currentSensorIndex];
        int total = filteredQuestions.Count;
        int correct = currentSensorScore;
        
        Debug.Log($"[QuizManager] ShowSensorSuccess: {sensor.sensorName} - Score: {correct}/{total}");
        
        // סימון החיישן כהושלם
        sensorCompleted[currentSensorIndex] = true;
        availableSensors.Remove(currentSensorIndex);
        
        // הצגת פאנל
        HideAllPanels();
        sensorSuccessPanel.SetActive(true);
        overallProgressPanel.SetActive(true);
        
        // עדכון תוכן
        if (successBadge != null)
            successBadge.sprite = sensor.badgeSprite;
        
        bool earnedBadge = sensorBadgeEarned[currentSensorIndex];
        
        if (successMessage != null)
        {
            if (earnedBadge)
                successMessage.text = $"Amazing!\nYou earned the\n{sensor.sensorName} Badge!";
            else
                successMessage.text = $"Sensor Completed!\n\n{sensor.sensorName}";
        }
        
        if (successSubMessage != null)
            successSubMessage.text = $"Score: {correct}/{total} correct answers";
        
        // אנימציה
        if (successAnimator != null)
            successAnimator.SetTrigger("Show");
        
        // צליל
        if (earnedBadge && audioSource != null && sensor.successSound != null)
            audioSource.PlayOneShot(sensor.successSound);
        
        // עדכון התקדמות
        UpdateOverallProgress();
        UpdateEarnedBadgesDisplay();
    }

    void OnContinuePressed()
    {
        PlayButtonClick();
        
        // בדיקה אם כל החיישנים הושלמו
        bool allCompleted = true;
        foreach (bool c in sensorCompleted)
            if (!c) { allCompleted = false; break; }
        
        if (allCompleted)
            ShowFinalCompletion();
        else
            ShowSensorSelection();
    }

    #endregion

    #region Final Completion

    void ShowFinalCompletion()
    {
        Debug.Log("[QuizManager] ShowFinalCompletion - All sensors completed!");
        
        HideAllPanels();
        finalCompletionPanel.SetActive(true);
        
        // צליל סיום
        if (audioSource != null && allCompletedSound != null)
            audioSource.PlayOneShot(allCompletedSound);
        
        // יצירת badges
        if (finalBadgeContainer != null && finalBadgePrefab != null)
        {
            foreach (Transform child in finalBadgeContainer)
                Destroy(child.gameObject);
            
            Debug.Log($"[QuizManager] Creating final badges - {sensorCategories.Length} sensors checked");
            
            for (int i = 0; i < sensorCategories.Length; i++)
            {
                if (sensorBadgeEarned[i])
                {
                    GameObject badge = Instantiate(finalBadgePrefab, finalBadgeContainer);
                    Debug.Log($"[QuizManager] Badge created for {sensorCategories[i].sensorName}");
                    
                    // Get ALL Image components (prefab might have background + badge image)
                    Image[] allImages = badge.GetComponentsInChildren<Image>(true);
                    
                    if (allImages.Length > 0)
                    {
                        Debug.Log($"[QuizManager] Found {allImages.Length} Image components in badge prefab");
                        
                        // Set sprite on ALL Images (this fixes white squares and ensures badge shows)
                        foreach (Image img in allImages)
                        {
                            img.sprite = sensorCategories[i].badgeSprite;
                            Debug.Log($"[QuizManager] ✓ Set sprite on '{img.name}': {sensorCategories[i].badgeSprite?.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[QuizManager] ⚠ No Image component found in finalBadgePrefab! Add Image component.");
                    }
                    
                    var text = badge.GetComponentInChildren<TMP_Text>();
                    if (text != null) text.text = sensorCategories[i].sensorName;
                }
            }
        }
        
        // הודעת סיום
        int badgesEarned = 0;
        int totalCorrect = 0;
        int totalQuestions = 0;
        
        for (int i = 0; i < sensorCategories.Length; i++)
        {
            if (sensorBadgeEarned[i]) badgesEarned++;
            totalCorrect += sensorCorrectAnswers[i];
            totalQuestions += sensorTotalAnswered[i];
        }
        
        if (finalTitleText != null)
        {
            if (badgesEarned == sensorCategories.Length)
                finalTitleText.text = "🏆 SPIKE Prime Expert! 🏆";
            else if (badgesEarned > 0)
                finalTitleText.text = $"🎉 Great Job! 🎉";
            else
                finalTitleText.text = "Quiz Completed!";
        }
        
        if (finalMessageText != null)
        {
            string message = $"You earned {badgesEarned}/{sensorCategories.Length} badges!\n";
            message += $"Total Score: {totalCorrect}/{totalQuestions}\n\n";
            
            if (badgesEarned == sensorCategories.Length)
                message += "Amazing! You mastered all SPIKE Prime sensors!\nYou're now a certified SPIKE Expert!";
            else if (badgesEarned >= 2)
                message += "Great work! Keep practicing to earn all badges!";
            else if (badgesEarned == 1)
                message += "Good start! Try again to earn more badges!";
            else
                message += "Keep learning! Answer 3+ questions correctly per sensor to earn badges.";
            
            finalMessageText.text = message;
        }
    }

    void OnRestartPressed()
    {
        Debug.Log("[QuizManager] OnRestartPressed");
        PlayButtonClick();
        ResetQuiz();
    }

    #endregion

    #region Quiz Reset & Navigation

    public void ResetQuiz()
    {
        Debug.Log("[QuizManager] ResetQuiz - Full reset");
        
        // Check if initialized
        if (sensorCategories == null || sensorCategories.Length == 0)
        {
            Debug.LogWarning("[QuizManager] ResetQuiz called before initialization - skipping");
            return;
        }
        
        currentQuestion = 0;
        currentSensorScore = 0;
        currentSensorIndex = -1;
        answerLocked = false;
        
        // איפוס כל המעקבים
        if (sensorCompleted != null)
        {
            for (int i = 0; i < sensorCategories.Length; i++)
            {
                sensorCompleted[i] = false;
                sensorBadgeEarned[i] = false;
                sensorCorrectAnswers[i] = 0;
                sensorTotalAnswered[i] = 0;
            }
        }
        
        if (availableSensors != null)
        {
            availableSensors.Clear();
            for (int i = 0; i < sensorCategories.Length; i++)
                availableSensors.Add(i);
        }
        
        filteredQuestions?.Clear();
        
        // ניקוי badges מהתצוגה
        ClearBadgesDisplay();
        
        ShowLevelSelection();
    }
    
    void ClearBadgesDisplay()
    {
        if (earnedBadgesContainer != null)
        {
            foreach (Transform child in earnedBadgesContainer)
                Destroy(child.gameObject);
            Debug.Log("[QuizManager] Badges cleared");
        }
    }

    public void BackButtonPressed()
    {
        Debug.Log("[QuizManager] BackButtonPressed");
        PlayButtonClick();
        
        // מסך בחירת רמה → תפריט ראשי (יציאה)
        if (levelSelectionPanel.activeSelf)
        {
            ResetQuiz(); // ריסט מלא כולל ניקוי badges
            HideAllPanels();
            quizRoot.SetActive(false);
            mainMenuPanel.SetActive(true);
            background_main_menu.SetActive(true);
            ExploreButton.SetActive(true);
            quizButton.SetActive(true);
            BackButton.SetActive(false);
            return;
        }
        
        // כל שאר המסכים → חזרה לבחירת רמה
        // (שאלות, בחירת חיישן, הצלחה, סיום)
        ResetQuiz(); // ריסט מלא כולל ניקוי badges
        ShowLevelSelection();
    }

    void HideAllPanels()
    {
        if (levelSelectionPanel != null) levelSelectionPanel.SetActive(false);
        if (sensorSelectionPanel != null) sensorSelectionPanel.SetActive(false);
        if (questionPanel != null) questionPanel.SetActive(false);
        if (sensorSuccessPanel != null) sensorSuccessPanel.SetActive(false);
        if (finalCompletionPanel != null) finalCompletionPanel.SetActive(false);
        if (overallProgressPanel != null) overallProgressPanel.SetActive(false);
        if (scoreImage != null) scoreImage.SetActive(false);
    }

    void PlayButtonClick()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
    
    /// <summary>
    /// DEBUG METHOD: Call this from Unity Inspector or another script to test if button logic works
    /// </summary>
    public void DEBUG_TestDistanceButtonDirect()
    {
        Debug.Log("[QuizManager] *** DEBUG_TestDistanceButtonDirect CALLED MANUALLY ***");
        OnSensorSelected(0);
    }
    
    /// <summary>
    /// DEBUG METHOD: Prints complete UI state for debugging blocked clicks
    /// </summary>
    public void DEBUG_PrintUIState()
    {
        Debug.Log("[QuizManager] === DEBUG UI STATE ===");
        Debug.Log($"sensorSelectionPanel: active={sensorSelectionPanel?.activeSelf}, activeInHierarchy={sensorSelectionPanel?.activeInHierarchy}");
        
        // Check button states
        Debug.Log($"distanceSensorButton: active={distanceSensorButton?.gameObject.activeSelf}, enabled={distanceSensorButton?.enabled}, interactable={distanceSensorButton?.interactable}");
        
        // Check button RectTransform positions
        if (distanceSensorButton != null)
        {
            RectTransform rt = distanceSensorButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                Debug.Log($"  distanceSensorButton position: {rt.anchoredPosition}, size: {rt.sizeDelta}");
                Debug.Log($"  distanceSensorButton worldCorners: {GetWorldCornerString(rt)}");
            }
        }
        
        // Check Canvas Raycaster
        var canvas = sensorSelectionPanel?.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
                Debug.LogError("*** Canvas missing GraphicRaycaster! UI clicks will NOT work! ***");
            else
                Debug.Log($"Canvas '{canvas.name}' has GraphicRaycaster: enabled={raycaster.enabled}, blockingObjects={raycaster.blockingObjects}");
        }
        
        // Check EventSystem
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
            Debug.LogError("*** NO EventSystem in scene! UI clicks will NOT work! ***");
        else
            Debug.Log($"EventSystem: {eventSystem.name}, enabled={eventSystem.enabled}");
        
        // Check for blocking Images
        if (sensorSelectionPanel != null)
        {
            Image[] allImages = sensorSelectionPanel.GetComponentsInChildren<Image>(true);
            Debug.Log($"Images in chooseSensorPanel: {allImages.Length}");
            foreach (Image img in allImages)
            {
                if (img.raycastTarget)
                {
                    string buttonStatus = "";
                    if (img.gameObject == distanceSensorButton?.gameObject) buttonStatus = " (DISTANCE BUTTON - OK)";
                    else if (img.gameObject == hubSensorButton?.gameObject) buttonStatus = " (HUB BUTTON - OK)";
                    else if (img.gameObject == forceSensorButton?.gameObject) buttonStatus = " (FORCE BUTTON - OK)";
                    else buttonStatus = " *** BLOCKING CLICKS! ***";
                    
                    Debug.Log($"  - {img.name}: raycastTarget=true{buttonStatus}");
                }
            }
        }
        
        // Check for blocking UI elements
        var allCanvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Active Canvases: {allCanvases.Length}");
        foreach (var c in allCanvases)
        {
            if (c.gameObject.activeInHierarchy)
                Debug.Log($"  Canvas: {c.name}, sortOrder={c.sortingOrder}, layer={LayerMask.LayerToName(c.gameObject.layer)}");
        }
    }
    
    string GetWorldCornerString(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return $"BL={corners[0]}, TR={corners[2]}";
    }

    #endregion

    #region Legacy Support

    void HideAllAnswers()
    {
        answerA.gameObject.SetActive(false);
        answerB.gameObject.SetActive(false);
        answerC.gameObject.SetActive(false);
        answerD.gameObject.SetActive(false);
    }

    void UnhideAllAnswers()
    {
        answerA.gameObject.SetActive(true);
        answerB.gameObject.SetActive(true);
        answerC.gameObject.SetActive(true);
        answerD.gameObject.SetActive(true);
    }

    #endregion
}
