using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private string usersFile => Path.Combine(Application.persistentDataPath, "users.json");
    private List<UserData> users = new List<UserData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllUsers();
        }
        else Destroy(gameObject);
    }

    #region Users CRUD (Local JSON)
    [Serializable]
    private class UsersWrapper { public List<UserData> users = new List<UserData>(); }

    private void LoadAllUsers()
    {
        try
        {
            if (File.Exists(usersFile))
            {
                string json = File.ReadAllText(usersFile);
                if (!string.IsNullOrEmpty(json))
                {
                    UsersWrapper wrapper = JsonUtility.FromJson<UsersWrapper>(json);
                    users = wrapper?.users ?? new List<UserData>();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"DataManager: Error loading users: {ex}");
        }
    }

    private void SaveAllUsers()
    {
        try
        {
            UsersWrapper wrapper = new UsersWrapper() { users = users };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(usersFile, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"DataManager: Error saving users: {ex}");
        }
    }

    public List<UserData> GetAllUsers()
    {
        return users;
    }

    public UserData GetUserById(string id)
    {
        return users.FirstOrDefault(u => u.UserId == id);
    }

    public UserData FindByEmailOrUserName(string emailOrUser)
    {
        if (string.IsNullOrEmpty(emailOrUser)) return null;
        return users.FirstOrDefault(u =>
            string.Equals(u.Email, emailOrUser, StringComparison.OrdinalIgnoreCase)
            || string.Equals(u.UserName, emailOrUser, StringComparison.OrdinalIgnoreCase));
    }

    public bool AddUser(UserData newUser)
    {
        if (newUser == null) return false;
        if (FindByEmailOrUserName(newUser.Email) != null) return false; // email/username already exists
        users.Add(newUser);
        SaveAllUsers();
        return true;
    }

    public void UpdateUser(UserData user)
    {
        if (user == null) return;
        var existing = GetUserById(user.UserId);
        if (existing != null)
        {
            users.Remove(existing);
            users.Add(user);
            SaveAllUsers();
        }
    }
    #endregion
}