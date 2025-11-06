using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controla los textos y sliders de la escena "4.PROGRESOS".
/// Lee el progreso de cada módulo desde UserManager y actualiza los valores visuales.
/// Ahora los sliders están bloqueados para evitar interacción del usuario.
/// </summary>
public class ProgressSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class ModuleUI
    {
        public string moduleId;              // Debe coincidir con el ID usado en ModuleProgressTracker
        public TMP_Text progressText;        // Texto que muestra el porcentaje del módulo
        public Slider progressSlider;        // Slider que muestra el avance
    }

    [Header("Progreso global del usuario")]
    public TMP_Text globalProgressText;
    public Slider globalProgressSlider;

    [Header("Módulos individuales")]
    public List<ModuleUI> modules = new List<ModuleUI>();

    private void Start()
    {
        // Bloquear interacción con los sliders para evitar que el usuario los mueva
        if (globalProgressSlider != null)
            globalProgressSlider.interactable = false;

        foreach (var moduleUI in modules)
        {
            if (moduleUI.progressSlider != null)
                moduleUI.progressSlider.interactable = false;
        }

        UpdateAllProgress();
    }

    /// <summary>
    /// Actualiza el progreso de todos los módulos y el global.
    /// </summary>
    public void UpdateAllProgress()
    {
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("ProgressSceneManager: No hay usuario activo.");
            return;
        }

        var user = UserManager.Instance.CurrentUser;

        float totalProgress = 0f;
        int count = 0;

        // Actualizar cada módulo
        foreach (var moduleUI in modules)
        {
            var module = user.ProgressList.Find(x => x.ModuleID == moduleUI.moduleId);
            float progress = module != null ? module.Progress : 0f;

            if (moduleUI.progressText != null)
                moduleUI.progressText.text = $"{progress:0}%";

            if (moduleUI.progressSlider != null)
            {
                moduleUI.progressSlider.value = progress / 100f;
                moduleUI.progressSlider.interactable = false; // aseguramos nuevamente
            }

            totalProgress += progress;
            count++;
        }

        // Calcular promedio general
        float globalProgress = count > 0 ? totalProgress / count : 0f;

        if (globalProgressText != null)
            globalProgressText.text = $"{globalProgress:0}%";

        if (globalProgressSlider != null)
        {
            globalProgressSlider.value = globalProgress / 100f;
            globalProgressSlider.interactable = false; // aseguramos nuevamente
        }

        Debug.Log($"[ProgressSceneManager] Progreso global actualizado: {globalProgress}%");
    }
}