using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Cambia a una escena por nombre
    public void CambiarEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    // Cambia a una escena por índice
    public void CambiarEscenaPorIndice(int indice)
    {
        SceneManager.LoadScene(indice);
    }

    // Recarga la escena actual
    public void RecargarEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Vuelve a la escena anterior (si tienes control de eso)
    public void SalirJuego()
    {
        Application.Quit();
    }
}