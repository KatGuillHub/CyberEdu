using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public string nombreZona;
    private float offsetY = -150f;  // espacio vertical entre iconos (aumenta para más espacio)
    private float startY = 250f;   // punto inicial (ajusta según necesites)

    public void AgregarActividad(GameObject actividad)
    {
        RectTransform rect = actividad.GetComponent<RectTransform>();

        if (rect != null)
        {
            // Guarda posición y tamaño en world space antes de cambiar parent
            Vector3 posicionWorld = rect.position;
            Vector2 tamañoOriginal = rect.rect.size;

            // Cambia el padre al panel de la zona con world position mantiene = true
            rect.SetParent(transform, true);

            // Restaura el tamaño original
            rect.sizeDelta = tamañoOriginal;

            // Calcula la nueva posición local dentro de la zona
            int index = transform.childCount - 1;
            Vector2 newPos = new Vector2(0, startY + (index * offsetY));
            rect.anchoredPosition = newPos;

            Debug.Log($"[{nombreZona}] recibió {actividad.name} - Tamaño: {rect.sizeDelta}");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Este método es llamado por el sistema de eventos de UI
        // pero ahora DragDropHandler se encarga de la lógica
    }
}