using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public string nombreZona;
    private float offsetY = -150f;  // espacio vertical entre iconos (aumenta para m�s espacio)
    private float startY = 250f;   // punto inicial (ajusta seg�n necesites)

    public void AgregarActividad(GameObject actividad)
    {
        RectTransform rect = actividad.GetComponent<RectTransform>();

        if (rect != null)
        {
            // Guarda posicin y tama�o en world space antes de cambiar parent
            Vector3 posicionWorld = rect.position;
            Vector2 tamaoOriginal = rect.rect.size;

            // Cambia el padre al panel de la zona con world position mantiene = true
            rect.SetParent(transform, true);

            // Restaura el tamao original
            rect.sizeDelta = tamaoOriginal;

            // Calcula la nueva posicin local dentro de la zona
            int index = transform.childCount - 1;
            Vector2 newPos = new Vector2(0, startY + (index * offsetY));
            rect.anchoredPosition = newPos;

            Debug.Log($"[{nombreZona}] recibi� {actividad.name} - Tama�o: {rect.sizeDelta}");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Este m�todo es llamado por el sistema de eventos de UI
        // pero ahora DragDropHandler se encarga de la l�gica
    }
}