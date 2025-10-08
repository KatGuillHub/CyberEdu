using System;
using System.IO;
using UnityEngine;

public static class EventLogger
{
    private static string logDirectory = Path.Combine(Application.dataPath, "Logs");
    private static string logFile => Path.Combine(logDirectory, "event_log.txt");

    // Tamaño máximo permitido antes de rotar (1 MB por defecto)
    private const long MaxLogSizeBytes = 1 * 1024 * 1024;

    // Máximo número de archivos de respaldo conservados
    private const int MaxBackupFiles = 5;

    /// <summary>
    /// Registra un mensaje en el log, rotando si es necesario.
    /// </summary>
    public static void Log(string message)
    {
        try
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            // Rotar si el archivo excede el tamaño permitido
            RotateIfTooLarge();

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logFile, line + Environment.NewLine);

            Debug.Log("<color=#00FFCC>[EventLogger]</color> " + line);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EventLogger] Error: {ex}");
        }
    }

    /// <summary>
    /// Comprueba el tamaño actual del archivo de log y lo rota si supera el límite.
    /// </summary>
    private static void RotateIfTooLarge()
    {
        if (!File.Exists(logFile))
            return;

        FileInfo info = new FileInfo(logFile);
        if (info.Length < MaxLogSizeBytes)
            return;

        // Generar nombre de respaldo con timestamp
        string backupName = $"event_log_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        string backupPath = Path.Combine(logDirectory, backupName);

        // Mover el log actual al archivo de respaldo
        File.Move(logFile, backupPath);

        // Registrar el evento en el nuevo archivo limpio
        File.WriteAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log rotado automáticamente ({info.Length / 1024f:F1} KB)\n");

        CleanupOldBackups();
    }

    /// <summary>
    /// Mantiene solo los últimos MaxBackupFiles respaldos, eliminando los más antiguos.
    /// </summary>
    private static void CleanupOldBackups()
    {
        string[] backups = Directory.GetFiles(logDirectory, "event_log_*.bak");
        if (backups.Length <= MaxBackupFiles)
            return;

        Array.Sort(backups, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

        int excess = backups.Length - MaxBackupFiles;
        for (int i = 0; i < excess; i++)
        {
            try
            {
                File.Delete(backups[i]);
                Debug.Log($"🧹 Backup antiguo eliminado: {Path.GetFileName(backups[i])}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EventLogger] No se pudo eliminar backup: {ex.Message}");
            }
        }
    }

    public static string GetLogFilePath()
    {
        return logFile;
    }
}