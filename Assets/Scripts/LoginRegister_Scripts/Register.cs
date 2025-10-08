using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Register : MonoBehaviour
{
    [Header("TMP Fields")]
    public TMP_InputField inputName;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPass1;
    public TMP_InputField inputPass2;

    [Header("Password visibility controls")]
    public Button btnShowPass1;
    public Button btnShowPass2;
    public Image imgShowPass1;
    public Image imgShowPass2;
    [Tooltip("Sprite para ojo abierto (contraseña visible)")]
    public Sprite spriteEyeOpen;
    [Tooltip("Sprite para ojo cerrado (contraseña oculta)")]
    public Sprite spriteEyeClosed;
    private bool showPass1 = false;
    private bool showPass2 = false;

    [Header("Role buttons")]
    public Button btnJoven;
    public Button btnAdulto;
    public Image imgJoven;
    public Image imgAdulto;
    public Sprite spriteSelected;
    public Sprite spriteUnselected;
    private string selectedRole = null;

    [Header("Age")]
    public TMP_Dropdown dropdownAge;

    [Header("Buttons")]
    public Button btnRegister;
    public Button btnHaveAccount;

    [Header("UI Feedback")]
    public TMP_Text messageText;
    public float messageDuration = 3f;

    [Header("Scenes")]
    public int sceneIndex_Login = 0;
    public int sceneIndex_Menu = 2;

    private void Start()
    {
        // Aceptar cualquier caracter en los campos básicos
        if (inputName != null) inputName.contentType = TMP_InputField.ContentType.Standard;
        if (inputEmail != null) inputEmail.contentType = TMP_InputField.ContentType.Standard;

        // Por defecto las contraseñas se muestran como ocultas (masked)
        if (inputPass1 != null) inputPass1.contentType = TMP_InputField.ContentType.Password;
        if (inputPass2 != null) inputPass2.contentType = TMP_InputField.ContentType.Password;

        // Inicializar imágenes de los botones de mostrar/ocultar
        showPass1 = false;
        showPass2 = false;
        if (imgShowPass1 != null && spriteEyeClosed != null) imgShowPass1.sprite = spriteEyeClosed;
        if (imgShowPass2 != null && spriteEyeClosed != null) imgShowPass2.sprite = spriteEyeClosed;

        // Listeners para botones de visibilidad
        if (btnShowPass1 != null) btnShowPass1.onClick.AddListener(TogglePass1);
        if (btnShowPass2 != null) btnShowPass2.onClick.AddListener(TogglePass2);

        // Asignar eventos de selección de rol
        if (btnJoven != null) btnJoven.onClick.AddListener(() => SelectRole("Joven"));
        if (btnAdulto != null) btnAdulto.onClick.AddListener(() => SelectRole("AdultoMayor"));
        if (btnRegister != null) btnRegister.onClick.AddListener(OnRegisterClicked);
        if (btnHaveAccount != null) btnHaveAccount.onClick.AddListener(OnHaveAccountClicked);

        // Estados iniciales de roles
        if (imgJoven != null) imgJoven.sprite = spriteUnselected;
        if (imgAdulto != null) imgAdulto.sprite = spriteUnselected;

        // Mensaje oculto
        if (messageText != null) messageText.gameObject.SetActive(false);

        // Inicializa dropdown vacío
        if (dropdownAge != null)
        {
            dropdownAge.options.Clear();
            dropdownAge.captionText.text = "Edad";
        }
    }

    // ------------------ Mostrar / ocultar contraseñas ------------------

    private void TogglePass1()
    {
        showPass1 = !showPass1;
        UpdatePasswordFieldVisibility(inputPass1, imgShowPass1, showPass1);
    }

    private void TogglePass2()
    {
        showPass2 = !showPass2;
        UpdatePasswordFieldVisibility(inputPass2, imgShowPass2, showPass2);
    }

    /// <summary>
    /// Actualiza la visibilidad de un campo de contraseña y el sprite del botón asociado.
    /// </summary>
    private void UpdatePasswordFieldVisibility(TMP_InputField field, Image eyeImage, bool visible)
    {
        if (field == null) return;

        // Guardar si el campo estaba enfocado para restaurarlo después
        bool wasFocused = field.isFocused;
        int caretPos = Mathf.Clamp(field.caretPosition, 0, field.text.Length);

        // Cambiar tipo de contenido (password <-> standard)
        field.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;

        // Forzar actualización visual
        field.ForceLabelUpdate();

        // Restaurar caret/foco si previamente estaba enfocado (no cambiamos foco innecesariamente)
        if (wasFocused)
        {
            field.ActivateInputField();
            // al reactivar, posicionamos el caret
            field.caretPosition = Mathf.Clamp(caretPos, 0, field.text.Length);
        }
        else
        {
            // asegurar caret no queda fuera
            field.caretPosition = Mathf.Clamp(caretPos, 0, field.text.Length);
        }

        // Actualizar sprite del botón (ojo abierto = visible, ojo cerrado = oculto)
        if (eyeImage != null)
        {
            if (visible && spriteEyeOpen != null) eyeImage.sprite = spriteEyeOpen;
            else if (!visible && spriteEyeClosed != null) eyeImage.sprite = spriteEyeClosed;
        }
    }

    // ------------------ Roles y edades ------------------

    private void SelectRole(string role)
    {
        selectedRole = role;

        if (imgJoven != null)
            imgJoven.sprite = (role == "Joven") ? spriteSelected : spriteUnselected;
        if (imgAdulto != null)
            imgAdulto.sprite = (role == "AdultoMayor") ? spriteSelected : spriteUnselected;

        UpdateAgeDropdown();
    }

    private void UpdateAgeDropdown()
    {
        if (dropdownAge == null) return;

        dropdownAge.options.Clear();

        int start = (selectedRole == "AdultoMayor") ? 30 : 12;
        int end = (selectedRole == "AdultoMayor") ? 60 : 30;

        for (int i = start; i <= end; i++)
            dropdownAge.options.Add(new TMP_Dropdown.OptionData(i.ToString()));

        dropdownAge.value = 0;
        dropdownAge.captionText.text = dropdownAge.options[0].text;
    }

    // ------------------ Registro ------------------

    public void OnRegisterClicked()
    {
        string name = inputName.text.Trim();
        string email = inputEmail.text.Trim().ToLower();
        string p1 = inputPass1.text;
        string p2 = inputPass2.text;

        if (string.IsNullOrEmpty(selectedRole))
        {
            ShowMessage("Seleccione si es Joven o Adulto Mayor.");
            return;
        }

        if (dropdownAge == null || dropdownAge.options.Count == 0)
        {
            ShowMessage("Seleccione su edad antes de continuar.");
            return;
        }

        if (!int.TryParse(dropdownAge.options[dropdownAge.value].text, out int age))
        {
            ShowMessage("Seleccione una edad válida.");
            return;
        }

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(p1) || string.IsNullOrEmpty(p2))
        {
            ShowMessage("Complete todos los campos.");
            return;
        }

        if (p1 != p2)
        {
            ShowMessage("Las contraseñas no coinciden.");
            return;
        }

        string error;
        bool ok = UserManager.Instance.RegisterNewUser(name, email, p1, selectedRole, age, out error);
        if (!ok)
        {
            ShowMessage(error);
            return;
        }

        EventLogger.Log($"Register: nuevo usuario {name} ({selectedRole}, {age} años)");
        LoadScene(sceneIndex_Menu);
    }

    public void OnHaveAccountClicked() => LoadScene(sceneIndex_Login);

    private void LoadScene(int index)
    {
        var loader = FindObjectOfType<SceneLoader>();
        if (loader != null) loader.Sceneloader(index);
        else SceneManager.LoadScene(index);
    }

    // ------------------ UI Feedback ------------------

    private void ShowMessage(string msg)
    {
        if (messageText == null) return;

        StopAllCoroutines();
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}