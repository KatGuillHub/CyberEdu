using System;
using System.Collections.Generic;

[Serializable]
public class ModuleProgress
{
    public string ModuleID;
    public float Progress; // 0 - 100
}

[Serializable]
public class UserSettings
{
    public string Language = "es";
    public bool LargeFonts = false;
    public bool Narration = false;
    public bool OfflineMode = false;
}

[Serializable]
public class UserData
{
    public string UserId;
    public string UserName;
    public string Email;
    public string PasswordHash; // hashed password (SHA256)
    public string UserType; // "Joven" o "AdultoMayor"
    public int Age;
    public List<ModuleProgress> ProgressList = new List<ModuleProgress>();
    public UserSettings Settings = new UserSettings();
}