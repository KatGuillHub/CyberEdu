using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gestiona la navegación entre paneles de información y de juego dentro de cada módulo.
/// Incluye persistencia del progreso, control del botón "Completar módulo"
/// y restauración completa del estado incluso tras cerrar la app o sesión.
/// Ahora soporta múltiples usuarios (cada usuario tiene su propio progreso guardado).
/// </summary>
public class ModulePanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelData
    {
        public GameObject panelObject;
        public bool isGamePanel;
    }

    [Header("Paneles del módulo (en orden)")]
    public List<PanelData> panels = new List<PanelData>();

    [Header("Botones de navegación")]
    public Button nextButton;
    public Button previousButton;

    [Header("Botón para completar el módulo")]
    public Button completeModuleButton;

    [Header("Texto de felicitación (opcional)")]
    public TMP_Text completionMessage;
    public float messageDuration = 4f;

    [Header("Integración opcional con progreso")]
    public ModuleProgressTracker progressTracker;

    private int currentPanelIndex = 0;
    private string moduleKey;
    private string completionKey;
    private bool moduleCompleted = false;

    private void Start()
    {
        // Obtener nombre o ID único del usuario actual
        string userId = "Guest";
        if (UserManager.Instance != null && UserManager.Instance.CurrentUser != null)
            userId = UserManager.Instance.CurrentUser.UserName;

        // Crear claves únicas por módulo y usuario
        moduleKey = $"{userId}_{gameObject.scene.name}_PanelIndex";
        completionKey = $"{userId}_{gameObject.scene.name}_Completed";

        // Recuperar último panel y estado de completado
        currentPanelIndex = PlayerPrefs.GetInt(moduleKey, 0);
        moduleCompleted = PlayerPrefs.GetInt(completionKey, 0) == 1;

        // Asegurar valores válidos
        currentPanelIndex = Mathf.Clamp(currentPanelIndex, 0, panels.Count - 1);

        // Activar el panel guardado
        for (int i = 0; i < panels.Count; i++)
            panels[i].panelObject.SetActive(i == currentPanelIndex);

        // Asignar listeners
        if (nextButton != null) nextButton.onClick.AddListener(NextPanel);
        if (previousButton != null) previousButton.onClick.AddListener(PreviousPanel);
        if (completeModuleButton != null) completeModuleButton.onClick.AddListener(CompleteModule);

        // Ocultar mensaje inicial
        if (completionMessage != null)
            completionMessage.gameObject.SetActive(false);

        // Si el módulo ya estaba completado, asegurar estado final
        if (moduleCompleted)
        {
            Debug.Log($"[ModulePanelManager] Módulo ya completado previamente por {userId}: {gameObject.scene.name}");
            currentPanelIndex = panels.Count - 1;
            UpdateProgress(true); // Forzar 100%
            if (completeModuleButton != null)
                completeModuleButton.gameObject.SetActive(false);
        }

        UpdateButtonStates();
        UpdateProgress();
    }

    public void NextPanel()
    {
        if (currentPanelIndex < panels.Count - 1)
        {
            panels[currentPanelIndex].panelObject.SetActive(false);
            currentPanelIndex++;
            panels[currentPanelIndex].panelObject.SetActive(true);

            SavePanelProgress();
            UpdateButtonStates();
            UpdateProgress();
        }
    }

    public void PreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            panels[currentPanelIndex].panelObject.SetActive(false);
            currentPanelIndex--;
            panels[currentPanelIndex].panelObject.SetActive(true);

            SavePanelProgress();
            UpdateButtonStates();
            UpdateProgress();
        }
    }

    private void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = currentPanelIndex > 0;

        if (nextButton != null)
        {
            bool isGamePanel = panels[currentPanelIndex].isGamePanel;
            nextButton.interactable = !isGamePanel;
        }

        if (completeModuleButton != null)
        {
            bool isLastPanel = currentPanelIndex == panels.Count - 1;
            completeModuleButton.gameObject.SetActive(isLastPanel && !moduleCompleted);
        }
    }

    private void UpdateProgress(bool forceFull = false)
    {
        if (progressTracker != null)
        {
            float progress;

            if (moduleCompleted || forceFull)
                progress = 100f;
            else
            {
                progress = ((float)(currentPanelIndex + 1) / panels.Count) * 100f;
                if (currentPanelIndex == panels.Count - 1)
                    progress = Mathf.Min(progress, 99f);
            }

            progressTracker.SetProgress(progress);
        }
    }

    private void SavePanelProgress()
    {
        PlayerPrefs.SetInt(moduleKey, currentPanelIndex);
        PlayerPrefs.Save();
    }

    public void OnMiniGamePhaseComplete()
    {
        Debug.Log("Mini-juego completado, botón Siguiente desbloqueado.");
        if (nextButton != null)
            nextButton.interactable = true;

        UpdateProgress();
    }

    private void CompleteModule()
    {
        Debug.Log("Módulo completado por el usuario.");

        moduleCompleted = true;

        // Guardar estado completado
        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.SetInt(moduleKey, currentPanelIndex);
        PlayerPrefs.Save();

        // Actualizar progreso
        if (progressTracker != null)
            progressTracker.SetProgress(100f);

        // Mostrar mensaje de felicitación
        if (completionMessage != null)
        {
            completionMessage.gameObject.SetActive(true);
            completionMessage.text = "¡Felicidades! Has completado este módulo con éxito ";
            CancelInvoke(nameof(HideCompletionMessage));
            Invoke(nameof(HideCompletionMessage), messageDuration);
        }

        // Desactivar botón de completar
        if (completeModuleButton != null)
            completeModuleButton.gameObject.SetActive(false);
    }

    private void HideCompletionMessage()
    {
        if (completionMessage != null)
            completionMessage.gameObject.SetActive(false);
    }

    public int GetCurrentPanelIndex() => currentPanelIndex;
}