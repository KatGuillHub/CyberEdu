using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManagerBienestar : MonoBehaviour
{
    [Header("Referencias principales")]
    public TMP_Text resultadoTexto;
    public Transform zonaMaana, zonaTarde, zonaNoche;

    [Header("Integración con CiberEdu+")]
    public ModulePanelManager panelManager;
    public string moduleId = "BienestarDigital"; // ID único del módulo

    private float tiempoMensaje = 3f;
    private float tiempoActual = 0f;
    private bool juegoCompletado = false;

    // Diccionario para guardar el nombre del ítem y la zona donde se encuentra
    private Dictionary<string, string> actividadUbicacion = new Dictionary<string, string>();

    private void Start()
    {
        resultadoTexto.text = "";

        if (panelManager == null)
            panelManager = FindObjectOfType<ModulePanelManager>();

        // Restaurar estado anterior si existe
        CargarEstado();
    }

    private void Update()
    {
        if (tiempoActual > 0)
        {
            tiempoActual -= Time.deltaTime;
            if (tiempoActual <= 0)
                resultadoTexto.text = "";
        }
    }

    // ============================================================
    // Evaluación del día: calcula equilibrio y notifica progreso
    // ============================================================
    public void EvaluarDia()
    {
        int puntaje = 0;
        string[] actividadesSaludables = { "Ejercicio", "Dormir", "Descanso", "Estudiar" };
        string[] actividadesDigitales = { "Videojuegos", "Redes Sociales" };

        // Calcular puntaje total
        puntaje += CalcularPuntos(zonaMaana, actividadesSaludables, actividadesDigitales);
        puntaje += CalcularPuntos(zonaTarde, actividadesSaludables, actividadesDigitales);
        puntaje += CalcularPuntos(zonaNoche, actividadesSaludables, actividadesDigitales);

        string resultado;
        if (puntaje >= 5)
            resultado = "Excelente equilibrio digital";
        else if (puntaje >= 3)
            resultado = "Buen equilibrio, pero puedes mejorar";
        else
            resultado = "Demasiado tiempo en lo digital";

        resultadoTexto.text = resultado;
        tiempoActual = tiempoMensaje;

        Debug.Log($"[BienestarDigital] Evaluación completa: puntaje={puntaje} → {resultado}");

        // Registrar en ReportManager
        if (ReportManager.Instance != null)
        {
            ReportManager.Instance.RecordAnswer(
                moduleId,
                0,
                0,
                "Resultado del minijuego Bienestar Digital",
                resultado,
                puntaje >= 3
            );
        }

        // Si el resultado es bueno o excelente, marcar módulo como completado
        if (!juegoCompletado && puntaje >= 3)
        {
            juegoCompletado = true;

            if (panelManager != null)
            {
                // Desbloquear el siguiente panel o completar el módulo al 100 %
                panelManager.OnMiniGamePhaseComplete();
                // Forzar progreso completo (porque es el último panel)
                var tracker = panelManager.progressTracker;
                if (tracker != null)
                    tracker.SetProgress(100f);

                Debug.Log("[BienestarDigital] Módulo completado al 100 %");
            }
        }

        // Guardar estado actual de las actividades
        GuardarEstado();
    }

    // ============================================================
    // Cálculo de puntos según las actividades en cada zona
    // ============================================================
    private int CalcularPuntos(Transform zona, string[] saludables, string[] digitales)
    {
        int puntos = 0;
        foreach (Transform hijo in zona)
        {
            string nombre = hijo.name;
            if (System.Array.Exists(saludables, s => nombre.Contains(s))) puntos++;
            if (System.Array.Exists(digitales, s => nombre.Contains(s))) puntos--;
        }
        return puntos;
    }

    // ============================================================
    // Guardar y cargar estado de las actividades
    // ============================================================
    private void GuardarEstado()
    {
        actividadUbicacion.Clear();

        // Registrar en qué zona está cada actividad
        RegistrarActividadesEnZona(zonaMaana, "Mañana");
        RegistrarActividadesEnZona(zonaTarde, "Tarde");
        RegistrarActividadesEnZona(zonaNoche, "Noche");

        // Convertir a JSON y guardar en PlayerPrefs
        string json = JsonUtility.ToJson(new SerializableDictionary<string, string>(actividadUbicacion));
        string key = $"{moduleId}_EstadoActividades";
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        Debug.Log("[BienestarDigital] Estado guardado.");
    }

    private void RegistrarActividadesEnZona(Transform zona, string nombreZona)
    {
        foreach (Transform hijo in zona)
        {
            if (!actividadUbicacion.ContainsKey(hijo.name))
                actividadUbicacion.Add(hijo.name, nombreZona);
        }
    }

    private void CargarEstado()
    {
        string key = $"{moduleId}_EstadoActividades";
        if (!PlayerPrefs.HasKey(key))
            return;

        string json = PlayerPrefs.GetString(key);
        SerializableDictionary<string, string> data = JsonUtility.FromJson<SerializableDictionary<string, string>>(json);

        if (data == null || data.dictionary == null) return;

        foreach (var kvp in data.dictionary)
        {
            string actividad = kvp.Key;
            string zonaNombre = kvp.Value;

            Transform zonaDestino = null;
            if (zonaNombre == "Mañana") zonaDestino = zonaMaana;
            else if (zonaNombre == "Tarde") zonaDestino = zonaTarde;
            else if (zonaNombre == "Noche") zonaDestino = zonaNoche;

            if (zonaDestino != null)
            {
                Transform actividadTransform = BuscarActividadPorNombre(actividad);
                if (actividadTransform != null)
                {
                    RectTransform rect = actividadTransform.GetComponent<RectTransform>();
                    rect.SetParent(zonaDestino, true);

                    // Ajustar posición dentro de la zona (igual que DropZone)
                    int index = zonaDestino.childCount - 1;
                    Vector2 newPos = new Vector2(0, 250f + (index * -150f));
                    rect.anchoredPosition = newPos;
                }
            }
        }

        Debug.Log("[BienestarDigital] Estado cargado y restaurado.");
    }

    private Transform BuscarActividadPorNombre(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        return obj != null ? obj.transform : null;
    }

    // ============================================================
    // Estructura auxiliar para serializar diccionario
    // ============================================================
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();
        public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        public SerializableDictionary(Dictionary<TKey, TValue> dict)
        {
            keys = new List<TKey>(dict.Keys);
            values = new List<TValue>(dict.Values);
            dictionary = dict;
        }

        public SerializableDictionary() { }

        // Método requerido por JsonUtility
        public void OnAfterDeserialize()
        {
            dictionary.Clear();
            for (int i = 0; i < keys.Count; i++)
                dictionary[keys[i]] = values[i];
        }

        public void OnBeforeSerialize() { }
    }
}