using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }

    public UserData CurrentUser { get; private set; }
    private const string PlayerPrefRememberKey = "RememberedUserId";

    private static string userDataDir = Path.Combine(Application.dataPath, "UserData");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(userDataDir))
                Directory.CreateDirectory(userDataDir);

            LoadRememberedSession();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Registro y Autenticación
    public bool RegisterNewUser(string userName, string email, string rawPassword, string userType, int age, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(rawPassword))
        {
            error = "Complete todos los campos.";
            return false;
        }

        // Validar duplicados
        if (FindUserByEmailOrName(email) != null || FindUserByEmailOrName(userName) != null)
        {
            error = "El usuario o correo ya existe.";
            return false;
        }

        UserData ud = new UserData()
        {
            UserId = Guid.NewGuid().ToString(),
            UserName = userName.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = ComputeSHA256(rawPassword),
            UserType = userType,
            Age = age
        };

        // Inicializar progresos
        var baseModules = new string[]
        {
            "Modulo_Phishing",
            "Modulo_CSeguras",
            "Modulo_Privacidad",
            "Modulo_RedesSociales",
            "Modulo_IAResponsable",
            "Modulo_BienestarDigital"
        };

        foreach (var m in baseModules)
            ud.ProgressList.Add(new ModuleProgress() { ModuleID = m, Progress = 0f });

        // Guardar en archivo JSON
        SaveUserToFile(ud);

        CurrentUser = ud;
        EventLogger.Log($"Usuario registrado: {ud.UserName} ({ud.Email})");
        return true;
    }

    public bool Authenticate(string emailOrUsername, string rawPassword, bool remember, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(rawPassword))
        {
            error = "Complete los campos de acceso.";
            return false;
        }

        var user = FindUserByEmailOrName(emailOrUsername);
        if (user == null)
        {
            error = "Usuario o correo no encontrado.";
            EventLogger.Log($"Login fallido: no encontrado {emailOrUsername}");
            return false;
        }

        string hashed = ComputeSHA256(rawPassword);
        if (!string.Equals(hashed, user.PasswordHash, StringComparison.Ordinal))
        {
            error = "Contraseña incorrecta.";
            EventLogger.Log($"Login fallido: contraseña incorrecta para {emailOrUsername}");
            return false;
        }

        CurrentUser = user;

        if (remember) PlayerPrefs.SetString(PlayerPrefRememberKey, CurrentUser.UserId);
        else PlayerPrefs.DeleteKey(PlayerPrefRememberKey);

        EventLogger.Log($"Login exitoso: {CurrentUser.UserName}");
        return true;
    }

    public void Logout()
    {
        if (CurrentUser != null)
            EventLogger.Log($"Logout: {CurrentUser.UserName}");

        CurrentUser = null;
        PlayerPrefs.DeleteKey(PlayerPrefRememberKey);
    }

    private void LoadRememberedSession()
    {
        if (PlayerPrefs.HasKey(PlayerPrefRememberKey))
        {
            string id = PlayerPrefs.GetString(PlayerPrefRememberKey);
            var u = LoadUserFromFile(id);
            if (u != null)
            {
                CurrentUser = u;
                EventLogger.Log($"Sesión recordada cargada: {u.UserName}");
            }
        }
    }
    #endregion

    #region Progreso y Guardado
    public void UpdateProgress(string moduleId, float newProgress)
    {
        if (CurrentUser == null) return;

        var mp = CurrentUser.ProgressList.Find(x => x.ModuleID == moduleId);
        if (mp == null)
        {
            mp = new ModuleProgress() { ModuleID = moduleId, Progress = Mathf.Clamp(newProgress, 0f, 100f) };
            CurrentUser.ProgressList.Add(mp);
        }
        else
        {
            mp.Progress = Mathf.Clamp(newProgress, 0f, 100f);
        }

        SaveUserToFile(CurrentUser);
        EventLogger.Log($"Progreso actualizado: {CurrentUser.UserName} - {moduleId} -> {mp.Progress}%");
    }
    #endregion

    #region Archivo JSON
    private void SaveUserToFile(UserData user)
    {
        try
        {
            string path = Path.Combine(userDataDir, $"{user.UserId}.json");
            string json = JsonUtility.ToJson(user, true);
            File.WriteAllText(path, json);
            EventLogger.Log($"Datos de usuario guardados: {user.UserName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando usuario: {ex}");
        }
    }

    private UserData LoadUserFromFile(string id)
    {
        string path = Path.Combine(userDataDir, $"{id}.json");
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<UserData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cargando usuario: {ex}");
            return null;
        }
    }

    private UserData FindUserByEmailOrName(string query)
    {
        if (!Directory.Exists(userDataDir)) return null;

        foreach (string file in Directory.GetFiles(userDataDir, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                var user = JsonUtility.FromJson<UserData>(json);
                if (user.Email.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                    user.UserName.Equals(query, StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }
            catch { }
        }
        return null;
    }
    #endregion

    #region Utilidades
    private string ComputeSHA256(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    #endregion
}