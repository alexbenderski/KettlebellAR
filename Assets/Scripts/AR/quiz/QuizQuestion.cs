using System;

[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers;   // 4 answers
    public int correctIndex;   // 0–3
}
