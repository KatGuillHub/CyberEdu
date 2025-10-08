using UnityEngine;

public class AppQuitManager : MonoBehaviour
{
    [Header("Configuración de eventos")]
    [Tooltip("Si está activo, se registrará un evento en EventLogger cuando la aplicación se cierre.")]
    public bool logExitEvent = true;

    /// <summary>
    /// Cierra la aplicación de manera controlada.
    /// </summary>
    public void QuitApplication()
    {
        if (logExitEvent)
        {
            if (UserManager.Instance != null && UserManager.Instance.CurrentUser != null)
            {
                string username = UserManager.Instance.CurrentUser.UserName;
                EventLogger.Log($"Usuario '{username}' cerró la aplicación manualmente desde la interfaz.");
            }
            else
            {
                EventLogger.Log("Aplicación cerrada manualmente (sin sesión activa).");
            }
        }

#if UNITY_EDITOR
        Debug.Log("[AppQuitManager] Cerrando aplicación (modo editor).");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("[AppQuitManager] Cerrando aplicación (build).");
        Application.Quit();
#endif
    }
}