using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform parentBeforeDrag;
    private Vector2 posicionAnterior;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentBeforeDrag = transform.parent;
        posicionAnterior = rectTransform.anchoredPosition;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Detecta si fue soltado sobre una zona válida
        DropZone zonaTarget = DetectarZona(eventData);

        if (zonaTarget != null)
        {
            // La zona se encarga de posicionar el elemento
            zonaTarget.AgregarActividad(gameObject);
        }
        else
        {
            // Vuelve a la posición anterior
            rectTransform.anchoredPosition = posicionAnterior;
            transform.SetParent(parentBeforeDrag);
        }
    }

    private DropZone DetectarZona(PointerEventData eventData)
    {
        var resultados = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, resultados);

        foreach (RaycastResult resultado in resultados)
        {
            DropZone dropZone = resultado.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
                return dropZone;
        }

        return null;
    }
}