using UnityEngine;
using TMPro;

public class GameManagerBienestar : MonoBehaviour
{
    public TMP_Text resultadoTexto;
    public Transform zonaMañana, zonaTarde, zonaNoche;
    private float tiempoMensaje = 3f; // segundos que dura el mensaje
    private float tiempoActual = 0f;

    private void Start()
    {
        resultadoTexto.text = "";
    }

    private void Update()
    {
        // Limpia el mensaje después del tiempo especificado
        if (tiempoActual > 0)
        {
            tiempoActual -= Time.deltaTime;
            if (tiempoActual <= 0)
            {
                resultadoTexto.text = "";
            }
        }
    }

    public void EvaluarDia()
    {
        int puntaje = 0;
        string[] actividadesSaludables = { "Ejercicio", "Dormir", "Descanso", "Estudiar" };
        string[] actividadesDigitales = { "Videojuegos", "Redes Sociales" };

        // Calcula puntaje total de todas las zonas
        puntaje += CalcularPuntos(zonaMañana, actividadesSaludables, actividadesDigitales);
        puntaje += CalcularPuntos(zonaTarde, actividadesSaludables, actividadesDigitales);
        puntaje += CalcularPuntos(zonaNoche, actividadesSaludables, actividadesDigitales);

        // Muestra nuevo resultado según puntaje
        if (puntaje >= 5)
            resultadoTexto.text = "Excelente equilibrio digital";
        else if (puntaje >= 3)
            resultadoTexto.text = "Buen equilibrio, pero puedes mejorar";
        else
            resultadoTexto.text = "Demasiado tiempo en lo digital";

        // Reinicia el temporizador
        tiempoActual = tiempoMensaje;

        Debug.Log($"Evaluación completa: puntaje = {puntaje}");
    }

    private int CalcularPuntos(Transform zona, string[] saludables, string[] digitales)
    {
        int puntos = 0;
        foreach (Transform hijo in zona)
        {
            string nombre = hijo.name;
            if (System.Array.Exists(saludables, s => nombre.Contains(s))) puntos++; //la cantidad de puntos maximos que se pueden conseguir es 4
            if (System.Array.Exists(digitales, s => nombre.Contains(s))) puntos--;
        }
        return puntos;
    }
}