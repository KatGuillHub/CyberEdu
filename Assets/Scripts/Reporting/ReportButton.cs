using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controla el botón de generación de reportes desde el menú principal.
/// Muestra un mensaje TMP_Text confirmando al usuario la ruta del archivo.
/// Ahora el mensaje desaparece automáticamente después de unos segundos.
/// </summary>
public class ButtonReport : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Botón que generará el reporte.")]
    public Button generateButton;

    [Tooltip("Texto TMP para mostrar mensajes al usuario.")]
    public TMP_Text infoText;

    [Header("Duración del mensaje")]
    [Tooltip("Tiempo en segundos que el mensaje permanecerá visible antes de ocultarse.")]
    public float messageDuration = 6f;

    private Coroutine hideMessageRoutine;

    private void Start()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);

        if (infoText != null)
            infoText.gameObject.SetActive(false);
    }

    private void OnGenerateClicked()
    {
        if (ReportManager.Instance == null)
        {
            Debug.LogWarning("ReportManager no encontrado en la escena.");
            return;
        }

        string filePath = ReportManager.Instance.GenerateReport();

        if (infoText != null)
        {
            infoText.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(filePath))
                infoText.text = $"Reporte generado exitosamente en:\n<b>{filePath}</b>";
            else
                infoText.text = "No se pudo generar el reporte (usuario no activo).";

            // Reiniciar temporizador de ocultado si ya hay uno corriendo
            if (hideMessageRoutine != null)
                StopCoroutine(hideMessageRoutine);

            hideMessageRoutine = StartCoroutine(HideMessageAfterDelay());
        }

        Debug.Log("[ButtonReport] Reporte generado y mensaje mostrado al usuario.");
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);

        if (infoText != null)
            infoText.gameObject.SetActive(false);

        hideMessageRoutine = null;
    }
}