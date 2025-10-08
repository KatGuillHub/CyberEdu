using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIEventTracker : MonoBehaviour
{
    private static UIEventTracker instance;
    private List<Button> trackedButtons = new List<Button>();
    private DateTime sessionStart;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.quitting += OnApplicationQuit;

            sessionStart = DateTime.Now;
            EventLogger.Log("Sistema de seguimiento iniciado.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AttachToSceneButtons();
        LogUserSessionStart();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EventLogger.Log($"Escena cargada: {scene.name}");
        AttachToSceneButtons();
    }

    private void OnApplicationQuit()
    {
        double duration = (DateTime.Now - sessionStart).TotalMinutes;
        string user = UserManager.Instance?.CurrentUser?.UserName ?? "Invitado";
        EventLogger.Log($"Aplicación cerrada. Usuario: {user}. Duración de sesión: {duration:F1} min.");
    }

    private void LogUserSessionStart()
    {
        string user = UserManager.Instance?.CurrentUser?.UserName ?? "Invitado";
        EventLogger.Log($"Sesión iniciada por: {user}");
    }

    /// <summary>
    /// Detecta todos los botones de la escena actual y se suscribe a sus eventos.
    /// </summary>
    private void AttachToSceneButtons()
    {
        trackedButtons.Clear();
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button b in allButtons)
        {
            if (!trackedButtons.Contains(b))
            {
                b.onClick.AddListener(() => OnButtonClicked(b));
                trackedButtons.Add(b);
            }
        }
    }

    /// <summary>
    /// Se ejecuta cuando un botón es presionado por el usuario.
    /// </summary>
    private void OnButtonClicked(Button b)
    {
        string buttonName = b.name;
        string sceneName = SceneManager.GetActiveScene().name;
        string user = UserManager.Instance?.CurrentUser?.UserName ?? "Invitado";

        // Solo registrar interacciones reales (no logs de interfaz)
        EventLogger.Log($"Click en botón '{buttonName}' (escena: {sceneName}, usuario: {user})");
    }
}