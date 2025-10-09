using UnityEngine;
using TMPro;

public class UserDisplayUI : MonoBehaviour
{
    [Header("Campo de texto TMP para mostrar el nombre del usuario")]
    public TMP_Text userNameText;

    [Header("Formato de texto opcional")]
    [Tooltip("Puedes usar {name} para insertar el nombre del usuario en un texto más largo.")]
    public string displayFormat = "{name}";

    private void Start()
    {
        UpdateUserName();
    }

    private void OnEnable()
    {
        UpdateUserName();
    }

    /// <summary>
    /// Actualiza el texto mostrado según el usuario activo en el sistema.
    /// </summary>
    public void UpdateUserName()
    {
        if (userNameText == null) return;

        if (UserManager.Instance != null && UserManager.Instance.CurrentUser != null)
        {
            string userName = UserManager.Instance.CurrentUser.UserName;
            userNameText.text = displayFormat.Replace("{name}", userName);
        }
        else
        {
            userNameText.text = "Invitado";
        }
    }
}