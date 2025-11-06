using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text.RegularExpressions;

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
    public Sprite spriteEyeOpen;
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
        if (inputName != null) inputName.contentType = TMP_InputField.ContentType.Standard;
        if (inputEmail != null) inputEmail.contentType = TMP_InputField.ContentType.Standard;
        if (inputPass1 != null) inputPass1.contentType = TMP_InputField.ContentType.Password;
        if (inputPass2 != null) inputPass2.contentType = TMP_InputField.ContentType.Password;

        showPass1 = showPass2 = false;
        if (imgShowPass1 != null) imgShowPass1.sprite = spriteEyeClosed;
        if (imgShowPass2 != null) imgShowPass2.sprite = spriteEyeClosed;

        if (btnShowPass1 != null) btnShowPass1.onClick.AddListener(() => TogglePassword(inputPass1, imgShowPass1, ref showPass1));
        if (btnShowPass2 != null) btnShowPass2.onClick.AddListener(() => TogglePassword(inputPass2, imgShowPass2, ref showPass2));

        if (btnJoven != null) btnJoven.onClick.AddListener(() => SelectRole("Joven"));
        if (btnAdulto != null) btnAdulto.onClick.AddListener(() => SelectRole("AdultoMayor"));
        if (btnRegister != null) btnRegister.onClick.AddListener(OnRegisterClicked);
        if (btnHaveAccount != null) btnHaveAccount.onClick.AddListener(() => LoadScene(sceneIndex_Login));

        if (imgJoven != null) imgJoven.sprite = spriteUnselected;
        if (imgAdulto != null) imgAdulto.sprite = spriteUnselected;

        if (messageText != null) messageText.gameObject.SetActive(false);

        if (dropdownAge != null)
        {
            dropdownAge.options.Clear();
            dropdownAge.captionText.text = "Edad";
        }
    }

    // --- Mostrar / Ocultar contraseñas ---
    private void TogglePassword(TMP_InputField field, Image eyeImg, ref bool state)
    {
        state = !state;
        field.contentType = state ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        field.ForceLabelUpdate();
        if (eyeImg != null)
            eyeImg.sprite = state ? spriteEyeOpen : spriteEyeClosed;
    }

    // --- Roles y edades ---
    private void SelectRole(string role)
    {
        selectedRole = role;
        if (imgJoven != null) imgJoven.sprite = (role == "Joven") ? spriteSelected : spriteUnselected;
        if (imgAdulto != null) imgAdulto.sprite = (role == "AdultoMayor") ? spriteSelected : spriteUnselected;
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

    // --- Registro ---
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

        if (!ValidateUserName(name))
        {
            ShowMessage("Nombre inválido. Use entre 3-20 caracteres (letras, números, guiones o guion bajo).");
            return;
        }

        if (!ValidateEmail(email))
        {
            ShowMessage("Correo electrónico inválido.");
            return;
        }

        if (!ValidatePassword(p1, out string passError))
        {
            ShowMessage(passError);
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

        if (!IsPasswordSecure(p1, out string pwdError))
        {
            ShowMessage(pwdError);
            return;
        }

        EventLogger.Log($"Register: nuevo usuario {name} ({selectedRole}, {age} años)");
        LoadScene(sceneIndex_Menu);
    }

    // --- Validaciones de ciberseguridad ---
    private bool ValidateUserName(string name)
    {
        return Regex.IsMatch(name, @"^[a-zA-Z0-9_-]{3,20}$");
    }

    private bool ValidateEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool ValidatePassword(string password, out string error)
    {
        error = null;
        if (password.Length < 8 || password.Length > 32)
        {
            error = "La contraseña debe tener entre 8 y 32 caracteres.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            error = "La contraseña debe tener al menos una letra mayúscula.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            error = "La contraseña debe tener al menos una letra minúscula.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            error = "La contraseña debe incluir al menos un número.";
            return false;
        }
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>/?]"))
        {
            error = "Debe incluir al menos un carácter especial.";
            return false;
        }
        if (password.Contains(" "))
        {
            error = "La contraseña no debe contener espacios.";
            return false;
        }
        return true;
    }

    // --- Utilidades ---
    private void LoadScene(int index)
    {
        var loader = FindObjectOfType<SceneLoader>();
        if (loader != null) loader.Sceneloader(index);
        else SceneManager.LoadScene(index);
    }

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

    private bool IsPasswordSecure(string password, out string error)
    {
        error = null;

        if (string.IsNullOrEmpty(password))
        {
            error = "La contraseña no puede estar vacía.";
            return false;
        }

        if (password.Length < 8 || password.Length > 16)
        {
            error = "La contraseña debe tener entre 8 y 16 caracteres.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
        {
            error = "Debe contener al menos una letra mayúscula.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
        {
            error = "Debe contener al menos una letra minúscula.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
        {
            error = "Debe contener al menos un número.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[@#$%&*!?\-_.]"))
        {
            error = "Debe contener al menos un carácter especial permitido: @ # $ % & * ! ? - _ .";
            return false;
        }

        if (password.Contains(" "))
        {
            error = "No se permiten espacios en blanco en la contraseña.";
            return false;
        }

        return true;
    }

}