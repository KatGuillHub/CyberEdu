using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuizManagerIA : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] options;
        public int correctAnswerIndex;
    }

    public TMP_Text questionText;
    public Button[] optionButtons;
    public Question[] questions;
    public GameObject feedbackX; // Asigna aquí el objeto con la "X"

    private int currentQuestionIndex = 0;
    private int score = 0;

    void Start()
    {
        if (feedbackX != null)
            feedbackX.SetActive(false); // Se asegura que esté oculta al inicio

        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex < questions.Length)
        {
            Question q = questions[currentQuestionIndex];
            questionText.text = q.questionText;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                optionButtons[i].GetComponentInChildren<TMP_Text>().text = q.options[i];
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => CheckAnswer(index));
            }
        }
        else
        {
            EvaluateResults();
        }
    }

    void CheckAnswer(int index)
    {
        if (index == questions[currentQuestionIndex].correctAnswerIndex)
        {
            score++;
            Debug.Log("Correcto!");

            currentQuestionIndex++;

            if (currentQuestionIndex >= questions.Length)
                EvaluateResults();
            else
                ShowQuestion();
        }
        else
        {
            Debug.Log("Incorrecto!");
            StartCoroutine(ShowFeedbackX()); // Muestra la X temporalmente
        }
    }

    IEnumerator ShowFeedbackX()
    {
        feedbackX.SetActive(true);
        yield return new WaitForSeconds(1f); // visible durante 1 segundo
        feedbackX.SetActive(false);
    }

    void EvaluateResults()
    {
        if (score == questions.Length)
        {
            Debug.Log("Todas correctas. Avanzando a la siguiente escena...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.Log("No todas correctas. Revisa tus respuestas.");
        }
    }
}
