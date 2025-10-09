using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Índice de la escena de Login en el Build Settings (por defecto 0)")]
    public int loginSceneIndex = 0;

    [Header("Opciones de desarrollo")]
    [Tooltip("Si está marcado, al iniciar el juego se borrará cualquier sesión recordada (modo developer)")]
    public bool forceClearRememberedSession = false;

    private void Start()
    {
        // Limpieza opcional desde el inspector (modo desarrollador)
        if (forceClearRememberedSession)
        {
            PlayerPrefs.DeleteKey("RememberedUserId");
            PlayerPrefs.Save();
            Debug.Log("[LogoutManager] Sesión recordada eliminada por el modo desarrollador.");
        }
    }

    /// <summary>
    /// Cierra sesión, borra el 'recordarme' y regresa al login.
    /// </summary>
    public void LogoutUser()
    {
        if (UserManager.Instance != null && UserManager.Instance.CurrentUser != null)
        {
            string username = UserManager.Instance.CurrentUser.UserName;
            EventLogger.Log($"Usuario '{username}' cerró sesión manualmente desde MenuPrincipal.");
            UserManager.Instance.Logout();
        }
        else
        {
            EventLogger.Log("Cierre de sesión ejecutado sin usuario activo (posible test o sesión no iniciada).");
            Debug.LogWarning("[LogoutManager] No se encontró UserManager o CurrentUser al intentar cerrar sesión.");
        }

        // Limpia la sesión recordada
        PlayerPrefs.DeleteKey("RememberedUserId");
        PlayerPrefs.Save();

        // Redirige a la escena de login
        EventLogger.Log("Redirigiendo a la escena de Login (0.LoginScreen).");
        SceneManager.LoadScene(loginSceneIndex);
    }
}