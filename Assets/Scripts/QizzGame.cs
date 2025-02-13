using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QuizGame : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    private int maxScore = 100;
    private float timeLimit = 30f;
    private float elapsedTime = 0f;
    private bool isAnswering = true;
    private int score;

    private List<Question> questions;
    private int currentQuestionIndex = 0;

    void Start()
    {
        StartCoroutine(GetQuestionsFromServer("http://localhost/Unity/get_questions.php"));
    }

    IEnumerator GetQuestionsFromServer(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            QuestionsList questionList = JsonUtility.FromJson<QuestionsList>("{\"questions\":" + jsonResponse + "}");
            questions = questionList.questions;
            DisplayNextQuestion();
        }
    }

    void DisplayNextQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            Question question = questions[currentQuestionIndex];
            questionText.text = question.question;
            answerTexts[0].text = question.answer1;
            answerTexts[1].text = question.answer2;
            answerTexts[2].text = question.answer3;
            answerTexts[3].text = question.answer4;
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
            }
            elapsedTime = 0f;
            isAnswering = true;
        }
        else
        {
            EndQuiz();
        }
    }

    void Update()
    {
        if (isAnswering)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimeText();

            if (elapsedTime >= timeLimit)
            {
                isAnswering = false;
                DisplayNextQuestion();
            }
        }
    }

    void OnAnswerSelected(int index)
    {
        if (isAnswering)
        {
            isAnswering = false;
            if (index == questions[currentQuestionIndex].correct_answer - 1)
            {
                score += CalculateScore(maxScore, timeLimit, elapsedTime);
            }
            UpdateScoreText();
            currentQuestionIndex++;
            DisplayNextQuestion();
        }
    }

    void EndQuiz()
    {
        Debug.Log("Quiz Ended! Final Score: " + score);
        // You can add more logic here, such as showing a result screen or restarting the quiz.
    }

    int CalculateScore(int maxScore, float timeLimit, float elapsedTime)
    {
        if (elapsedTime > timeLimit)
        {
            return 0;
        }
        float scorePercentage = (timeLimit - elapsedTime) / timeLimit;
        int calculatedScore = Mathf.RoundToInt(maxScore * scorePercentage);
        return calculatedScore;
    }

    void UpdateScoreText()
    {
        scoreText.text = "Điểm: " + score;
    }

    void UpdateTimeText()
    {
        float remainingTime = Mathf.Clamp(timeLimit - elapsedTime, 0, timeLimit);
        timeText.text = "Thời gian " + remainingTime.ToString("F2") + "s";
    }
}

[System.Serializable]
public class Question
{
    public string question;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public int correct_answer;
}

[System.Serializable]
public class QuestionsList
{
    public List<Question> questions;
}
