using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Genera y guarda los reportes del usuario (TXT y JSON) con los progresos y respuestas.
/// Guarda una copia en el Escritorio o Descargas, y otra dentro de la carpeta local "Reports" del proyecto Unity.
/// </summary>
public class ReportManager : MonoBehaviour
{
    public static ReportManager Instance;

    [Header("Configuración de guardado")]
    [Tooltip("Si está activado, los archivos se guardarán en el Escritorio; si no, en la carpeta Descargas.")]
    public bool saveToDesktop = true;

    [Tooltip("Prefijo de nombre para los archivos de reporte.")]
    public string reportFilePrefix = "CyberEdu_Report";

    private List<ReportEntry> reportEntries = new List<ReportEntry>();

    [System.Serializable]
    public class ReportEntry
    {
        public string ModuleID;
        public int PhaseIndex;
        public int QuestionIndex;
        public string QuestionText;
        public string UserAnswer;
        public bool IsCorrect;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Registra una respuesta individual del usuario.
    /// </summary>
    public void RecordAnswer(string moduleId, int phaseIndex, int questionIndex, string question, string userAnswer, bool isCorrect)
    {
        ReportEntry entry = new ReportEntry()
        {
            ModuleID = moduleId,
            PhaseIndex = phaseIndex,
            QuestionIndex = questionIndex,
            QuestionText = question,
            UserAnswer = userAnswer,
            IsCorrect = isCorrect
        };

        reportEntries.Add(entry);
        Debug.Log($"[ReportManager] Respuesta registrada: {moduleId} | {question} | {userAnswer} | {(isCorrect ? "Correcta" : "Incorrecta")}");
    }

    /// <summary>
    /// Genera los archivos TXT y JSON del reporte.
    /// También guarda una copia dentro de la carpeta local 'Reports' en el proyecto.
    /// </summary>
    public string GenerateReport()
    {
        if (UserManager.Instance == null || UserManager.Instance.CurrentUser == null)
        {
            Debug.LogWarning("[ReportManager] No hay usuario activo, reporte no generado.");
            return null;
        }

        string userId = UserManager.Instance.CurrentUser.UserName;
        string basePath;

        // Ruta configurable (Escritorio o Descargas)
        if (saveToDesktop)
            basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        else
            basePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");

        // Carpeta local dentro del proyecto (para developers)
        string localReportsPath = Path.Combine(Application.dataPath, "Reports");
        if (!Directory.Exists(localReportsPath))
            Directory.CreateDirectory(localReportsPath);

        // Nombre base de los archivos
        string fileNameBase = $"{reportFilePrefix}_{userId}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        string txtPath = Path.Combine(basePath, fileNameBase + ".txt");
        string jsonPath = Path.Combine(basePath, fileNameBase + ".json");

        string localTxtPath = Path.Combine(localReportsPath, fileNameBase + ".txt");
        string localJsonPath = Path.Combine(localReportsPath, fileNameBase + ".json");

        // ---------------- TXT ----------------
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("====================================");
        sb.AppendLine("        REPORTE DE PROGRESOS        ");
        sb.AppendLine("====================================");
        sb.AppendLine($"Usuario: {userId}");
        sb.AppendLine($"Fecha: {System.DateTime.Now}");
        sb.AppendLine("------------------------------------");

        // Progresos
        if (UserManager.Instance.CurrentUser.ProgressList != null)
        {
            sb.AppendLine(">>> PROGRESOS POR MÓDULO <<<");
            foreach (var p in UserManager.Instance.CurrentUser.ProgressList)
                sb.AppendLine($"- {p.ModuleID}: {p.Progress:0}%");
            sb.AppendLine();
        }

        // Respuestas registradas
        sb.AppendLine(">>> RESPUESTAS DE QUIZ <<<");
        foreach (var entry in reportEntries)
        {
            sb.AppendLine($"[{entry.ModuleID}] Fase {entry.PhaseIndex + 1} - Pregunta {entry.QuestionIndex + 1}");
            sb.AppendLine($"Pregunta: {entry.QuestionText}");
            sb.AppendLine($"Respuesta: {entry.UserAnswer}");
            sb.AppendLine($"Resultado: {(entry.IsCorrect ? "Correcta" : "Incorrecta")}");
            sb.AppendLine("------------------------------------");
        }

        string txtContent = sb.ToString();
        File.WriteAllText(txtPath, txtContent, Encoding.UTF8);
        File.WriteAllText(localTxtPath, txtContent, Encoding.UTF8);

        // ---------------- JSON ----------------
        string json = JsonUtility.ToJson(new ReportCollection { entries = reportEntries }, true);
        File.WriteAllText(jsonPath, json, Encoding.UTF8);
        File.WriteAllText(localJsonPath, json, Encoding.UTF8);

        Debug.Log($"[ReportManager] Archivos generados:\n" +
                  $"TXT → {txtPath}\nJSON → {jsonPath}\n" +
                  $"Copia local → {localReportsPath}");

        return txtPath;
    }

    [System.Serializable]
    public class ReportCollection
    {
        public List<ReportEntry> entries;
    }
}