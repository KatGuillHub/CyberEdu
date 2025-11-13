using UnityEngine;

public class PanelController : MonoBehaviour
{
    [Header("Configuración del Panel")]
    [Tooltip("El panel UI que se activará/desactivará")]
    public GameObject panelToControl;

    [Tooltip("Si está marcado, el panel empezará desactivado")]
    public bool startDisabled = true;

    private void Start()
    {
        // Configuración inicial del panel
        if (panelToControl != null && startDisabled)
        {
            panelToControl.SetActive(false);
        }
    }

    // Método para abrir el panel (usado por los botones)
    public void OpenPanel()
    {
        if (panelToControl != null)
        {
            panelToControl.SetActive(true);
        }
    }

    // Método para cerrar el panel (usado por el botón de cerrar)
    public void ClosePanel()
    {
        if (panelToControl != null)
        {
            panelToControl.SetActive(false);
        }
    }

    // Método toggle para alternar entre abrir/cerrar
    public void TogglePanel()
    {
        if (panelToControl != null)
        {
            panelToControl.SetActive(!panelToControl.activeSelf);
        }
    }
}