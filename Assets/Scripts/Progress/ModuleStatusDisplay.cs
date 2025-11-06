using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Actualiza los textos de estado de cada módulo en la escena "3.RutasAprendizaje".
/// Muestra el estado según el progreso del usuario: No iniciado, En progreso o Completado.
/// </summary>
public class ModuleStatusDisplay : MonoBehaviour
{
    [System.Serializable]
    public class ModuleStatusUI
    {
        public string moduleId;       // ID del módulo (debe coincidir con ModuleProgressTracker)
        public TMP_Text statusText;   // Texto que mostrará el estado
    }

    [Header("Estados de los módulos")]
    public List<ModuleStatusUI> moduleStatuses = new List<ModuleStatusUI>();

    private void Start()
    {
        UpdateModuleStatuses();
    }

    public void UpdateModuleStatuses()
    {
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("ModuleStatusDisplay: No hay usuario activo.");
            return;
        }

        var user = UserManager.Instance.CurrentUser;

        foreach (var moduleUI in moduleStatuses)
        {
            var module = user.ProgressList.Find(x => x.ModuleID == moduleUI.moduleId);
            float progress = module != null ? module.Progress : 0f;
            string estado;

            if (progress <= 0f)
                estado = "No iniciado";
            else if (progress >= 100f)
                estado = "Completado";
            else
                estado = $"En progreso ({progress:0}%)";

            if (moduleUI.statusText != null)
                moduleUI.statusText.text = estado;
        }

        Debug.Log("[ModuleStatusDisplay] Estados de módulos actualizados.");
    }
}