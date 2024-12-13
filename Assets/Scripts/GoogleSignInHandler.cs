using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GoogleSignInHandler : MonoBehaviour
{
    public void GoogleLoginCallback(string token)
    {
        Debug.Log("Google Token Received: " + token);

        // Valide o token (opcional)
        StartCoroutine(ValidateGoogleToken(token));
    }

    private IEnumerator ValidateGoogleToken(string token)
    {
        string url = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token validated: " + request.downloadHandler.text);
                // Use as informações do usuário
            }
            else
            {
                Debug.LogError("Token validation failed: " + request.error);
            }
        }
    }
}
