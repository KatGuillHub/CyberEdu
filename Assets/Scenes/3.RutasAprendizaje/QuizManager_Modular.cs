using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Versión modular del Quiz Manager para CyberEdu+
/// Compatible con múltiples fases de preguntas, cada una vinculada a su panel correspondiente.
/// Mejora: se sincroniza automáticamente con ModulePanelManager cuando su panel se (re)activa,
/// y ahora solo muestra/configura preguntas cuando el panel activo es de juego.
/// Además incorpora un polling ligero para detectar cambios de panel en tiempo de ejecución.
/// </summary>
public class QuizManager_Modular : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        [TextArea(2, 5)] public string questionText;
        public string[] options;
        [Range(0, 10)] public int correctAnswerIndex;
    }

    [System.Serializable]
    public class QuizPhase
    {
        public string phaseName;
        public List<Question> questions = new List<Question>();
        public List<TMP_Text> questionTexts = new List<TMP_Text>();
        public List<Button> optionButtons = new List<Button>();
        public GameObject feedbackX;
        public RawImage feedbackCorrect;
    }

    [Header("Fases del Quiz (una por panel de juego)")]
    public List<QuizPhase> phases = new List<QuizPhase>();

    [Header("Integración con Panel Manager (opcional)")]
    public ModulePanelManager panelManager;

    // estado interno
    private int currentPhaseIndex = 0;
    private int currentQuestionIndex = 0;
    private int score = 0;

    // Control para evitar llamadas simultáneas
    private Coroutine initCoroutine = null;

    // Polling ligero para detectar cambios de panel
    private Coroutine panelWatcherCoroutine = null;
    private int lastKnownPanelIndex = -999;

    private void Start()
    {
        if (panelManager == null)
            panelManager = FindObjectOfType<ModulePanelManager>();

        // Ocultar feedbacks
        foreach (var phase in phases)
            if (phase.feedbackX != null)
                phase.feedbackX.SetActive(false);

        // Inicializar diferido para asegurar que ModulePanelManager ya activó el panel correcto.
        initCoroutine = StartCoroutine(InitializeAfterFrame());
    }

    private void OnEnable()
    {
        // Re-inicializamos si se vuelve a activar
        if (initCoroutine != null)
            StopCoroutine(initCoroutine);
        initCoroutine = StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        // Esperar un frame para asegurarnos que la jerarquía y ModulePanelManager hayan establecido el panel activo
        yield return null;

        // Si no hay panelManager, intentar encontrarlo
        if (panelManager == null)
            panelManager = FindObjectOfType<ModulePanelManager>();

        // Sincronizamos la fase con el panel activo solo si es un panel de juego
        // (SyncPhaseWithActivePanel internamente verifica si el panel activo es de juego)
        SyncPhaseWithActivePanel();

        // Obtener panelIndex actual y decidir si mostrar preguntas:
        int panelIndex = panelManager != null ? panelManager.GetCurrentPanelIndex() : -1;
        bool panelIsGame = (panelManager != null && panelIndex >= 0 && panelIndex < panelManager.panels.Count)
                           ? panelManager.panels[panelIndex].isGamePanel
                           : false;

        // Mostrar/configurar preguntas SÓLO si el panel activo actual es un panel de juego
        if (panelIsGame)
        {
            ShowQuestion();
        }
        else
        {
            // No estamos en un panel de juego: no mostrar preguntas.
            Debug.Log("[QuizManager] Panel activo no es de juego: no se muestran preguntas por ahora.");
        }

        // Guardamos panel inicial y arrancamos watcher (si no está corriendo) para detectar cambios posteriores
        lastKnownPanelIndex = panelIndex;
        if (panelWatcherCoroutine == null)
            panelWatcherCoroutine = StartCoroutine(PollPanelChanges());

        initCoroutine = null;
    }

    /// <summary>
    /// Polling ligero: cada 0.2s verifica si ModulePanelManager cambió de panel.
    /// Si se cambia a un panel de juego, sincroniza la fase y configura sus preguntas.
    /// </summary>
    private IEnumerator PollPanelChanges()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.18f); // frecuencia baja y suficiente

            if (panelManager == null) continue;

            int currentPanel = panelManager.GetCurrentPanelIndex();
            if (currentPanel != lastKnownPanelIndex)
            {
                // Panel cambiado
                lastKnownPanelIndex = currentPanel;
                Debug.Log($"[QuizManager] Detected panel change -> {currentPanel}");

                // Si el nuevo panel es de tipo juego, sincronizar fase y mostrar preguntas
                if (panelManager.panels != null && currentPanel >= 0 && currentPanel < panelManager.panels.Count)
                {
                    if (panelManager.panels[currentPanel].isGamePanel)
                    {
                        SyncPhaseWithActivePanel();
                        // mostrar/configurar preguntas de la fase correspondiente
                        ShowQuestion();
                    }
                    else
                    {
                        // Si es panel de info, no mostrar preguntas — dejamos estado interno tal cual
                        Debug.Log("[QuizManager] Nuevo panel es de información; no se configuran preguntas.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determina la fase actual basándose en el panel activo (solo si es un panel de juego).
    /// </summary>
    private void SyncPhaseWithActivePanel()
    {
        if (panelManager == null) return;

        int panelIndex = panelManager.GetCurrentPanelIndex();

        // Protecciones
        if (panelManager.panels == null || panelManager.panels.Count == 0) return;
        if (panelIndex < 0 || panelIndex >= panelManager.panels.Count) return;

        // Solo sincronizamos si el panel activo es de juego
        if (!panelManager.panels[panelIndex].isGamePanel) return;

        // Contar cuántos paneles de tipo 'isGamePanel' aparecen desde el inicio hasta panelIndex inclusive.
        int phaseCounter = -1;
        for (int i = 0; i <= panelIndex; i++)
            if (panelManager.panels[i].isGamePanel)
                phaseCounter++;

        if (phaseCounter < 0 || phaseCounter >= phases.Count)
        {
            Debug.LogWarning($"[QuizManager] No se encontró fase correspondiente al panel {panelIndex}. phases.Count={phases.Count}, phaseCounter={phaseCounter}");
            return;
        }

        // Ajustar la fase actual y resetear el índice de pregunta dentro de la fase
        currentPhaseIndex = phaseCounter;
        currentQuestionIndex = 0;

        Debug.Log($"[QuizManager] Sincronizado a fase {currentPhaseIndex} (panel {panelIndex}).");
    }

    // ---------------------------------------------------------------
    // Muestra la pregunta actual según la fase y el índice de pregunta
    // ---------------------------------------------------------------
    private void ShowQuestion()
    {
        if (phases == null || phases.Count == 0) return;

        // Comprueba que el panel actualmente activo sea de juego antes de intentar mostrar
        if (panelManager != null)
        {
            int panelIndex = panelManager.GetCurrentPanelIndex();
            if (panelIndex < 0 || panelIndex >= panelManager.panels.Count) return;
            if (!panelManager.panels[panelIndex].isGamePanel)
            {
                // No mostramos preguntas si el panel activo no es uno de juego
                Debug.Log("[QuizManager] ShowQuestion abortado: panel activo no es de juego.");
                return;
            }
        }

        if (currentPhaseIndex >= phases.Count)
        {
            Debug.Log("Todas las fases completadas.");
            OnQuizCompleted();
            return;
        }

        var currentPhase = phases[currentPhaseIndex];

        if (currentPhase.questions == null || currentPhase.questions.Count == 0)
        {
            Debug.LogWarning($"La fase '{currentPhase.phaseName}' no tiene preguntas configuradas.");
            OnPhaseCompleted();
            return;
        }

        if (currentQuestionIndex >= currentPhase.questions.Count)
        {
            Debug.Log($"Fase completada: {currentPhase.phaseName}");
            OnPhaseCompleted();
            return;
        }

        Question q = currentPhase.questions[currentQuestionIndex];

        if (q.options == null || q.options.Length == 0)
        {
            Debug.LogWarning($"La pregunta '{q.questionText}' no tiene opciones configuradas.");
            OnPhaseCompleted();
            return;
        }

        // Mostrar texto en los campos TMP configurados para esta fase
        foreach (var txt in currentPhase.questionTexts)
            if (txt != null) txt.text = q.questionText;

        // Configurar botones de esta fase (listeners y textos)
        for (int i = 0; i < currentPhase.optionButtons.Count; i++)
        {
            var btn = currentPhase.optionButtons[i];
            if (btn == null) continue;

            var label = btn.GetComponentInChildren<TMP_Text>();

            if (i < q.options.Length)
            {
                if (label != null) label.text = q.options[i];
                btn.interactable = true;

                int index = i; // captura local
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => CheckAnswer(index));
            }
            else
            {
                if (label != null) label.text = "-";
                btn.interactable = false;
            }
        }
    }

    private void CheckAnswer(int index)
    {
        if (currentPhaseIndex >= phases.Count) return;

        var currentPhase = phases[currentPhaseIndex];
        if (currentQuestionIndex >= currentPhase.questions.Count) return;

        var q = currentPhase.questions[currentQuestionIndex];

        if (index < 0 || index >= q.options.Length)
        {
            Debug.LogError($"Índice fuera de rango ({index}) en pregunta '{q.questionText}' — tiene {q.options.Length} opciones.");
            return;
        }

        if (q.correctAnswerIndex < 0 || q.correctAnswerIndex >= q.options.Length)
        {
            Debug.LogError($"Índice de respuesta correcta inválido ({q.correctAnswerIndex}) en pregunta '{q.questionText}'.");
            return;
        }

        if (ReportManager.Instance != null)
        {
            // moduleId: asigna aquí el id real de este módulo (puedes pasar por inspector desde QuizManager)
            string moduleId = this.gameObject.scene.name; // o un campo público moduleId
            ReportManager.Instance.RecordAnswer(moduleId, currentPhaseIndex, currentQuestionIndex, q.questionText, q.options[index], index == q.correctAnswerIndex);
        }

        if (index == q.correctAnswerIndex)
        {
            //Mostrar feedback positivo SI existe
            StartCoroutine(ShowFeedbackCorrect(currentPhase));

            score++;
            currentQuestionIndex++;

            if (currentQuestionIndex < currentPhase.questions.Count)
                ShowQuestion();
            else
                OnPhaseCompleted();
        }
        else
        {
            //Mostrar feedback negativo existente
            StartCoroutine(ShowFeedbackX(currentPhase));
        }
    }

    private IEnumerator ShowFeedbackX(QuizPhase phase)
    {
        if (phase.feedbackX == null) yield break;
        phase.feedbackX.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        phase.feedbackX.SetActive(false);
    }

    private IEnumerator ShowFeedbackCorrect(QuizPhase phase)
    {
        if (phase.feedbackCorrect == null) yield break;

        phase.feedbackCorrect.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.2f); // puedes ajustar el tiempo si quieres
        phase.feedbackCorrect.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Lógica cuando se completa una fase del quiz
    // ---------------------------------------------------------------
    private void OnPhaseCompleted()
    {
        Debug.Log($"Fase '{phases[currentPhaseIndex].phaseName}' completada. Puntuación: {score}");

        if (panelManager != null)
        {
            Debug.Log("Desbloqueando botón 'Siguiente' (fase completada)");
            panelManager.OnMiniGamePhaseComplete();
        }

        currentPhaseIndex++;
        currentQuestionIndex = 0;

        if (currentPhaseIndex < phases.Count)
        {
            StartCoroutine(WaitAndShowNextPhase());
        }
        else
        {
            OnQuizCompleted();
        }
    }

    private IEnumerator WaitAndShowNextPhase()
    {
        yield return new WaitForSeconds(0.5f);
        ShowQuestion();
    }

    // ---------------------------------------------------------------
    // Lógica cuando se completa todo el quiz
    // ---------------------------------------------------------------
    private void OnQuizCompleted()
    {
        Debug.Log("Quiz completo — notificando al PanelManager.");
        if (panelManager != null)
            panelManager.OnMiniGamePhaseComplete();
    }

    private void OnDestroy()
    {
        // Limpiar coroutines si se destruye el objeto
        if (initCoroutine != null) StopCoroutine(initCoroutine);
        if (panelWatcherCoroutine != null) StopCoroutine(panelWatcherCoroutine);
    }
}