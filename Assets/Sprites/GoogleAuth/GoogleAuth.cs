using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class GoogleAuth : MonoBehaviour{

    public string username, email;

    public GameObject LoginPanel;

     public string webClientId = "1083895502867-da72s70b24uaihnk5nmktmimc7d8ht8k.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

    // Defer the configuration creation until Awake so the web Client ID
    // Can be set via the property inspector in the Editor.
    void Awake() {
      configuration = new GoogleSignInConfiguration {
            WebClientId = webClientId,
            RequestIdToken = true
      };
    }

    public void OnSignIn() {
      GoogleSignIn.Configuration = configuration;
      GoogleSignIn.Configuration.UseGameSignIn = false;
      GoogleSignIn.Configuration.RequestIdToken = true;
      GoogleSignIn.Configuration.RequestEmail = true;
      Debug.LogError("Calling SignIn");

      GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
        OnAuthenticationFinished,TaskScheduler.Default);
    }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task) {
      if (task.IsFaulted) {
        using (IEnumerator<System.Exception> enumerator =
                task.Exception.InnerExceptions.GetEnumerator()) {
          if (enumerator.MoveNext()) {
            GoogleSignIn.SignInException error =
                    (GoogleSignIn.SignInException)enumerator.Current;
            Debug.LogError("Got Error: " + error.Status + " " + error.Message);
          } else {
            Debug.LogError("Got Unexpected Exception?!?" + task.Exception);
          }
        }
      } else if(task.IsCanceled) {
        Debug.LogError("Canceled");
      } else  {
        Debug.LogError("Welcome: " + task.Result.DisplayName + "!");
        LoginPanel.SetActive(false);
      }
    }

        public void OnSignOut() {
            LoginPanel.SetActive(true);
      Debug.LogError("Calling SignOut");
      GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect() {
      Debug.LogError("Calling Disconnect");
      GoogleSignIn.DefaultInstance.Disconnect();
    }
}
