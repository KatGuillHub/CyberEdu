using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script genérico para manejar el progreso de cada módulo en CyberEdu+.
/// Se comunica con UserManager para leer/actualizar el progreso del usuario actual.
/// </summary>
public class ModuleProgressTracker : MonoBehaviour
{
    [Header("Configuración del Módulo")]
    [Tooltip("Debe coincidir exactamente con el ID del módulo (por ejemplo: Modulo_Phishing)")]
    public string moduleId = "Modulo_Default";

    [Tooltip("Texto opcional que mostrará el porcentaje de progreso")]
    public TMP_Text progressText;

    [Tooltip("Valor inicial si no hay progreso guardado")]
    [Range(0, 100)] public float initialProgress = 0f;

    private float currentProgress;

    private void Start()
    {
        LoadProgress();
        UpdateUI();
    }

    /// <summary>
    /// Carga el progreso del usuario actual desde el UserManager.
    /// </summary>
    public void LoadProgress()
    {
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("ModuleProgressTracker: No hay usuario activo.");
            currentProgress = initialProgress;
            return;
        }

        var user = UserManager.Instance.CurrentUser;
        var mp = user.ProgressList.Find(x => x.ModuleID == moduleId);

        if (mp != null)
            currentProgress = mp.Progress;
        else
            currentProgress = initialProgress;

        Debug.Log($"[ProgressTracker] {moduleId} cargado: {currentProgress}%");
    }

    /// <summary>
    /// Actualiza el progreso del módulo (0-100) y guarda en el archivo del usuario.
    /// </summary>
    /// <param name="newProgress">Nuevo valor de progreso (porcentaje)</param>
    public void SetProgress(float newProgress)
    {
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("ModuleProgressTracker: No hay usuario activo.");
            return;
        }

        currentProgress = Mathf.Clamp(newProgress, 0f, 100f);
        UserManager.Instance.UpdateProgress(moduleId, currentProgress);

        Debug.Log($"[ProgressTracker] {moduleId} actualizado a {currentProgress}%");

        UpdateUI();
    }

    /// <summary>
    /// Devuelve el progreso actual (0-100)
    /// </summary>
    public float GetProgress()
    {
        return currentProgress;
    }

    /// <summary>
    /// Actualiza la UI (barra y texto)
    /// </summary>
    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"{currentProgress:0}%";
    }

    /// <summary>
    /// Aumenta el progreso relativo (por ejemplo, +10%)
    /// </summary>
    public void AddProgress(float delta)
    {
        SetProgress(currentProgress + delta);
    }
}