using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] answers = new string[4];
        public int correctIndex; // 0=A, 1=B, 2=C, 3=D
    }

    public QuizQuestion[] questions;

    public GameObject mainMenuPanel;
    public GameObject ExploreButton;
    public GameObject quizButton;
    // UI References
    public TMP_Text questionText;
    public Button answerA;
    public Button answerB;
    public Button answerC;
    public Button answerD;
    public TMP_Text textA;
    public TMP_Text textB;
    public TMP_Text textC;
    public TMP_Text textD;
    // ⭐ NEW — Sprites for button backgrounds
    public Sprite normalSprite;
    public Sprite correctSprite;
    public Sprite wrongSprite;
    public GameObject BackButton;

    public GameObject scoreImage;
    public TMP_Text scoreText;
    public GameObject questionPanel;
    public GameObject background_main_menu;
    public GameObject quizRoot;
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip scoreSound;


    private int currentQuestion = 0;
    private int score = 0;
    private bool answerLocked = false;

    void Start()
    {
        // BackButton.SetActive(true);
        // questionPanel.SetActive(true);
        LoadQuestion();

        answerA.onClick.AddListener(() => OnAnswerSelected(0));
        answerB.onClick.AddListener(() => OnAnswerSelected(1));
        answerC.onClick.AddListener(() => OnAnswerSelected(2));
        answerD.onClick.AddListener(() => OnAnswerSelected(3));
    }

    void LoadQuestion()
{
    var q = questions[currentQuestion];

    // שמירת התשובה הנכונה לפני ערבוב
    string correctAnswer = q.answers[q.correctIndex];

    // ערבוב פשוט של המערך
    for (int i = 0; i < q.answers.Length; i++)
    {
        int r = Random.Range(0, q.answers.Length);
        (q.answers[i], q.answers[r]) = (q.answers[r], q.answers[i]);
    }

    // מציאת האינדקס החדש של התשובה הנכונה
    q.correctIndex = System.Array.IndexOf(q.answers, correctAnswer);

    // הצגה על המסך
    questionText.text = q.question;

    textA.text = q.answers[0];
    textB.text = q.answers[1];
    textC.text = q.answers[2];
    textD.text = q.answers[3];

    ResetButtonSprites();
}

    // ⭐ RESET using sprite instead of color
    void ResetButtonSprites()
    {
        answerA.image.sprite = normalSprite;
        answerB.image.sprite = normalSprite;
        answerC.image.sprite = normalSprite;
        answerD.image.sprite = normalSprite;
    }

    void OnAnswerSelected(int index)
    {

        //  אם כבר נלחצה תשובה, לא מאפשר ללחוץ שוב
        if (answerLocked) 
            return;
        //  נועל לחיצות נוספות
        answerLocked = true;
        bool correct = index == questions[currentQuestion].correctIndex;

        if (correct)
            {
                if (audioSource != null && correctSound != null)
                    audioSource.PlayOneShot(correctSound);
                score++;
                HighlightCorrect(index);
            }
        else
        {
            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);
            HighlightWrong(index);
            HighlightCorrect(questions[currentQuestion].correctIndex);
        }

        Invoke(nameof(NextQuestion), 1f); 
    }

    // ⭐ Change sprite instead of color
    void HighlightCorrect(int index)
    {
        GetButton(index).image.sprite = correctSprite;
    }

    void HighlightWrong(int index)
    {
        GetButton(index).image.sprite = wrongSprite;
    }

    Button GetButton(int index)
    {
        return index switch
        {
            0 => answerA,
            1 => answerB,
            2 => answerC,
            3 => answerD,
            _ => answerA
        };
    }


public void ResetQuiz()
{
    currentQuestion = 0;
    score = 0;
    answerLocked = false;



    // החזר פאנלים
    scoreImage.SetActive(false);
    scoreText.gameObject.SetActive(false);
    questionPanel.SetActive(true);

    answerA.onClick.AddListener(() => OnAnswerSelected(0));
    answerB.onClick.AddListener(() => OnAnswerSelected(1));
    answerC.onClick.AddListener(() => OnAnswerSelected(2));
    answerD.onClick.AddListener(() => OnAnswerSelected(3));
    UnhideAllAnswers();
    BackButton.SetActive(true);
    LoadQuestion();
}


public void BackButtonPressed()
{
    // כיבוי מלא של מסך המבחן
    quizRoot.SetActive(false);

    // הפעלה של התפריט הראשי
    mainMenuPanel.SetActive(true);
    background_main_menu.SetActive(true);
    ExploreButton.SetActive(true);
    quizButton.SetActive(true);

    BackButton.SetActive(false);
}

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

string BuildScoreMessage(int score, int total)
{
    float percent = (float)score / total;
    string feedback;

    if (percent >= 0.8f)
        feedback = "Excellent job! You really know your SPIKE modules!";
    else if (percent >= 0.5f)
        feedback = "Good work! You're learning well — keep practicing!";
    else
        feedback = "Don't worry! Try again — you'll improve next time!";

    return $"Congratulations!\nYour Score: {score} / {total}\n\n{feedback}";
}

void ShowScore()
{
    // מכבים את כל הפאנל של השאלות
    questionPanel.SetActive(false);
    scoreImage.SetActive(true);
    if (audioSource != null && scoreSound != null)
        audioSource.PlayOneShot(scoreSound);
    HideAllAnswers();
    // מציגים רק את הטקסט של הציון
    scoreText.gameObject.SetActive(true);
    //feedback text:
    scoreText.text = BuildScoreMessage(score, questions.Length);

}

void NextQuestion()
{
    currentQuestion++;
    // אם סיימנו את כל השאלות → הצג ציון
    if (currentQuestion >= questions.Length)
    {
        ShowScore();
        return;
    }
    answerLocked = false; // ← מאפשר לחיצה שוב
    LoadQuestion();
}
}
