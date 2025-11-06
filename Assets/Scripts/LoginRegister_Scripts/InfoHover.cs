using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InfoHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Panel de información a mostrar")]
    public GameObject infoPanel;

    [Header("Duración visible tras salir del cursor (segundos)")]
    [Tooltip("Tiempo que el panel permanece visible después de que el cursor sale del área.")]
    public float visibleTime = 2f;

    private Coroutine hideCoroutine;
    private bool isHovering = false;

    private void Start()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        if (infoPanel == null)
            return;

        // Si hay una corrutina activa de ocultar, la detenemos
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        // Mostramos el panel inmediatamente
        infoPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Iniciamos el temporizador para ocultar el panel después del tiempo definido
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleTime);

        // Solo se oculta si el usuario no volvió a colocar el mouse
        if (!isHovering && infoPanel != null)
            infoPanel.SetActive(false);
    }
}
