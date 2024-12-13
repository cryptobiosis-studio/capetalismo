using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GoogleSignInHandler : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject ProfilePanel;
    public Button LoginButton;
    public Button LogoutButton;

    private string googleToken = "";

    void Start()
    {
        // Configura os botões
        LoginButton.onClick.AddListener(OnLoginButtonClicked);
        LogoutButton.onClick.AddListener(OnLogoutButtonClicked);

        // Inicializa mostrando o painel de login
        ShowLoginPanel();
    }

    // Método chamado quando o botão de login é clicado
    public void OnLoginButtonClicked()
    {
        Debug.Log("LoginButton clicado");
        // O JavaScript gerencia o login agora, nenhuma outra ação necessária aqui.
    }

    // Método chamado quando o botão de logout é clicado
    public void OnLogoutButtonClicked()
    {
        Debug.Log("LogoutButton clicado");
        // O JavaScript gerencia o logout agora, nenhuma outra ação necessária aqui.
    }

    // Callback chamado pelo JavaScript
    public void GoogleLoginCallback(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("Usuário deslogado.");
            HandleLogout();
        }
        else
        {
            Debug.Log("Google Token Recebido: " + token);
            googleToken = token; // Armazena o token de login

            // Opcional: Valide o token
            StartCoroutine(ValidateGoogleToken(token));
        }
    }

    // Validação opcional do token
    private IEnumerator ValidateGoogleToken(string token)
    {
        string url = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token validado: " + request.downloadHandler.text);
                ShowProfilePanel();
            }
            else
            {
                Debug.LogError("Falha na validação do token: " + request.error);
            }
        }
    }

    // Lógica de logout
    private void HandleLogout()
    {
        googleToken = ""; // Limpa o token armazenado
        Debug.Log("Logout realizado.");
        ShowLoginPanel();
    }

    // Exibe o painel de login
    private void ShowLoginPanel()
    {
        LoginPanel.SetActive(true);
        ProfilePanel.SetActive(false);
    }

    // Exibe o painel de perfil
    private void ShowProfilePanel()
    {
        LoginPanel.SetActive(false);
        ProfilePanel.SetActive(true);
    }
}
