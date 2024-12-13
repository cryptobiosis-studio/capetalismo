using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using Unity.VisualScripting;

public class GoogleSignInHandler : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject ProfilePanel;
    public Button LoginButton;
    public Button LogoutButton;
    private bool isLoggedIn = false;
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

        // Exibe a área de login (já está ativo)
        isLoggedIn = false;
        ShowLoginPanel();

        // Chama a função de login no JavaScript
        CallJSFunction("initGoogleSignIn");
    }

    // Método chamado quando o botão de logout é clicado
    public void OnLogoutButtonClicked()
    {
        Debug.Log("LogoutButton clicado");

        // Chama a função de logout no JavaScript
        CallJSFunction("googleLogout");

        // Exibe o painel de login
        ShowLoginPanel();
    }

    // Método chamado quando o login é bem-sucedido (do JavaScript)
    public void GoogleLoginCallback(string token)
    {
        if (string.IsNullOrEmpty(token)){

            Debug.Log("Usuário deslogado");
            // Lidar com o logout do usuário
            googleToken = ""; // Limpa o token de autenticação
            HandleLogout();
            isLoggedIn = false;
        }
        else{

            Debug.Log("Google Token Received: " + token);
            googleToken = token; // Armazena o token de login

            // Valide o token (opcional)
            StartCoroutine(ValidateGoogleToken(token));
        }
        UpdateUI();
    }

    private IEnumerator ValidateGoogleToken(string token){
    
        string url = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token validated: " + request.downloadHandler.text);
                ShowProfilePanel();
            }
            else
            {
                Debug.LogError("Token validation failed: " + request.error);
            }
        }
    }

    // Função chamada no logout
    private void HandleLogout(){
        
        
        Debug.Log("Logout realizado. Usuário desconectado.");

    }

    private void ShowLoginPanel(){

        LoginPanel.SetActive(true);   // Mostra o painel de login
        ProfilePanel.SetActive(false); // Esconde o painel de perfil
    }

    private void ShowProfilePanel(){
        LoginPanel.SetActive(false);
        ProfilePanel.SetActive(true);
    }

    private void UpdateUI()
    {
        if (isLoggedIn)
        {
            // Exibe painel de perfil e esconde o painel de login
            ShowProfilePanel();
        }
        else
        {
            // Exibe painel de login e esconde o painel de perfil
            ShowLoginPanel();
        }
    }

    private void OnApplicationQuit(){
    
        // Limpeza adicional quando o jogo é fechado
        googleToken = "";
    }
    private void CallJSFunction(string functionName)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("initGoogleSignIn", "googleLogout" ); // Chama o método JavaScript
        #endif
    }
}

