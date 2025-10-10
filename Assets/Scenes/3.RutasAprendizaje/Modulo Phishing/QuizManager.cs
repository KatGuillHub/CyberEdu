using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI TextoPregunta;
    public Image ImagenPregunta;
    public Button BotonA;
    public Button BotonB;
    public Button BotonC;
    public Button BotonD;

    public GameObject PanelRetroalimentacion;
    public TextMeshProUGUI TextoRetroalimentacion;
    public Button BotonRepetir;
    public Button BotonContinuar;

    public GameObject PanelResultados;
    public TextMeshProUGUI TextoResultados;
    public Button BotonTerminar;

    [System.Serializable]
    public class Pregunta
    {
        public string textoPregunta;
        public Sprite imagen;
        public string[] opciones;
        public int respuestaCorrecta; // índice correcto (0-3)
        public string retroCorrecta;
        public string retroIncorrecta;
    }

    [Header("Banco de preguntas")]
    public List<Pregunta> preguntas;
    private int indiceActual = 0;
    private int puntaje = 0;

    void Start()
    {
        MostrarPregunta();
        PanelRetroalimentacion.SetActive(false);
        PanelResultados.SetActive(false);

        BotonA.onClick.AddListener(() => SeleccionarRespuesta(0));
        BotonB.onClick.AddListener(() => SeleccionarRespuesta(1));
        BotonC.onClick.AddListener(() => SeleccionarRespuesta(2));
        BotonD.onClick.AddListener(() => SeleccionarRespuesta(3));

        BotonRepetir.onClick.AddListener(RepetirPregunta);
        BotonContinuar.onClick.AddListener(SiguientePregunta);
        BotonTerminar.onClick.AddListener(TerminarPrueba);
    }

    void MostrarPregunta()
    {
        if (indiceActual >= preguntas.Count)
        {
            MostrarResultados();
            return;
        }

        var p = preguntas[indiceActual];
        TextoPregunta.text = p.textoPregunta;
        ImagenPregunta.sprite = p.imagen;

        BotonA.GetComponentInChildren<TextMeshProUGUI>().text = "A) " + p.opciones[0];
        BotonB.GetComponentInChildren<TextMeshProUGUI>().text = "B) " + p.opciones[1];
        BotonC.GetComponentInChildren<TextMeshProUGUI>().text = "C) " + p.opciones[2];
        BotonD.GetComponentInChildren<TextMeshProUGUI>().text = "D) " + p.opciones[3];

        PanelRetroalimentacion.SetActive(false);
    }

    void SeleccionarRespuesta(int indice)
    {
        var p = preguntas[indiceActual];

        PanelRetroalimentacion.SetActive(true);
        bool esCorrecta = indice == p.respuestaCorrecta;

        if (esCorrecta)
        {
            TextoRetroalimentacion.text = "✅ " + p.retroCorrecta;
            puntaje++;
            BotonRepetir.gameObject.SetActive(false);
            BotonContinuar.gameObject.SetActive(true);
        }
        else
        {
            TextoRetroalimentacion.text = "❌ " + p.retroIncorrecta;
            BotonRepetir.gameObject.SetActive(true);
            BotonContinuar.gameObject.SetActive(false);
        }
    }

    void RepetirPregunta()
    {
        PanelRetroalimentacion.SetActive(false);
    }

    void SiguientePregunta()
    {
        indiceActual++;
        if (indiceActual < preguntas.Count)
        {
            MostrarPregunta();
        }
        else
        {
            MostrarResultados();
        }
    }

    void MostrarResultados()
    {
        PanelQuizActivo(false);
        PanelResultados.SetActive(true);
        TextoResultados.text = $"Has completado la prueba.\nPuntaje: {puntaje}/{preguntas.Count}";
    }

    void TerminarPrueba()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Modulo_Phishing_Descripcion");
    }

    void PanelQuizActivo(bool estado)
    {
        TextoPregunta.transform.parent.gameObject.SetActive(estado);
    }
}

