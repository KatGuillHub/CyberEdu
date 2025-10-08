using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Login : MonoBehaviour
{
    [Header("TMP Fields")]
    public TMP_InputField inputEmailOrUser;
    public TMP_InputField inputPassword;

    [Header("Remember toggle (button)")]
    public Button rememberButton;
    public Image rememberImage;
    public Sprite rememberOnSprite;
    public Sprite rememberOffSprite;
    private bool rememberMe = false;

    [Header("Buttons")]
    public Button btnLogin;
    public Button btnRegister;

    [Header("UI Feedback")]
    public TMP_Text messageText;
    public float messageDuration = 3f;

    [Header("Scenes")]
    public int sceneIndex_Register = 1;
    public int sceneIndex_Menu = 2;

    private void Start()
    {
        // Permitir cualquier caracter en InputFields
        inputEmailOrUser.contentType = TMP_InputField.ContentType.Standard;
        inputPassword.contentType = TMP_InputField.ContentType.Password;

        if (rememberButton != null) rememberButton.onClick.AddListener(ToggleRemember);
        if (btnLogin != null) btnLogin.onClick.AddListener(OnLoginClicked);
        if (btnRegister != null) btnRegister.onClick.AddListener(OnRegisterClicked);

        if (messageText != null) messageText.gameObject.SetActive(false);

        // Auto-login si hay usuario recordado
        if (UserManager.Instance != null && UserManager.Instance.CurrentUser != null)
        {
            EventLogger.Log("Auto-redirect: remembered session detected.");
            LoadScene(sceneIndex_Menu);
        }
    }

    private void ToggleRemember()
    {
        rememberMe = !rememberMe;
        if (rememberImage != null)
            rememberImage.sprite = rememberMe ? rememberOnSprite : rememberOffSprite;
    }

    public void OnLoginClicked()
    {
        string idOrEmail = inputEmailOrUser.text;
        string pwd = inputPassword.text;

        if (string.IsNullOrEmpty(idOrEmail) || string.IsNullOrEmpty(pwd))
        {
            ShowMessage("Complete los campos para iniciar sesión.");
            return;
        }

        string error;
        bool ok = UserManager.Instance.Authenticate(idOrEmail, pwd, rememberMe, out error);
        if (!ok)
        {
            ShowMessage(error);
            return;
        }

        EventLogger.Log($"Login: {UserManager.Instance.CurrentUser.UserName}");
        LoadScene(sceneIndex_Menu);
    }

    public void OnRegisterClicked() => LoadScene(sceneIndex_Register);

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
}