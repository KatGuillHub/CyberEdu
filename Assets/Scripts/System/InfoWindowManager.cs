using UnityEngine;
using UnityEngine.UI;

public class InfoWindowManager : MonoBehaviour
{
    [Header("Botones principales")]
    public Button btnInfoTeorica;
    public Button btnInfoJuego;

    [Header("Ventanas de información")]
    public GameObject windowTeorica;
    public GameObject windowJuego;

    [Header("Botones de cierre dentro de las ventanas")]
    public Button btnCloseTeorica;
    public Button btnCloseJuego;

    [Header("Opcional: bloqueo de interacción externa")]
    public CanvasGroup sceneCanvasGroup;

    private void Start()
    {
        // Ocultamos todas las ventanas al inicio
        if (windowTeorica != null) windowTeorica.SetActive(false);
        if (windowJuego != null) windowJuego.SetActive(false);

        // Asignamos los listeners a los botones principales
        if (btnInfoTeorica != null) btnInfoTeorica.onClick.AddListener(OpenTeorica);
        if (btnInfoJuego != null) btnInfoJuego.onClick.AddListener(OpenJuego);

        // Asignamos los listeners a los botones de cierre
        if (btnCloseTeorica != null) btnCloseTeorica.onClick.AddListener(CloseTeorica);
        if (btnCloseJuego != null) btnCloseJuego.onClick.AddListener(CloseJuego);
    }

    // ---------------------------------------------------------
    // MÉTODOS DE APERTURA
    // ---------------------------------------------------------

    public void OpenTeorica()
    {
        CloseAll();
        if (windowTeorica != null)
        {
            windowTeorica.SetActive(true);
            LogEvent("open_theory_window");
        }
        SetSceneInteraction(false);
    }

    public void OpenJuego()
    {
        CloseAll();
        if (windowJuego != null)
        {
            windowJuego.SetActive(true);
            LogEvent("open_game_window");
        }
        SetSceneInteraction(false);
    }

    // ---------------------------------------------------------
    // MÉTODOS DE CIERRE
    // ---------------------------------------------------------

    public void CloseTeorica()
    {
        if (windowTeorica != null) windowTeorica.SetActive(false);
        SetSceneInteraction(true);
        LogEvent("close_theory_window");
    }

    public void CloseJuego()
    {
        if (windowJuego != null) windowJuego.SetActive(false);
        SetSceneInteraction(true);
        LogEvent("close_game_window");
    }

    // Cierra cualquier ventana abierta
    private void CloseAll()
    {
        if (windowTeorica != null) windowTeorica.SetActive(false);
        if (windowJuego != null) windowJuego.SetActive(false);
    }

    // ---------------------------------------------------------
    // UTILIDADES
    // ---------------------------------------------------------

    private void SetSceneInteraction(bool enabled)
    {
        if (sceneCanvasGroup == null) return;

        sceneCanvasGroup.interactable = enabled;
        sceneCanvasGroup.blocksRaycasts = enabled;
        sceneCanvasGroup.alpha = enabled ? 1f : 0.95f;
    }

    private void LogEvent(string action)
    {
        EventLogger.Log($"InfoWindowManager: {action} | User: {(UserManager.Instance?.CurrentUser?.UserName ?? "N/A")}");
    }
}