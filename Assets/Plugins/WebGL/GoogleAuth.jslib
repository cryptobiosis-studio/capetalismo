mergeInto(LibraryManager.library, {
    SignInWithGoogle: function () {
        if (!window.googleAuthClientId) {
            console.error("Google Auth not initialized. Call LoadGoogleAuth first.");
            return;
        }

        google.accounts.id.initialize({
            client_id: window.googleAuthClientId,
            callback: function (response) {
                console.log("Google Sign-In Successful:", response);
                // Pass JSON data to Unity
                window.unityInstance.SendMessage(
                    "GoogleAuthentication",  // Nome do GameObject no Unity
                    "OnGoogleSignInWebGL",   // Nome do método no script Unity
                    JSON.stringify({
                        DisplayName: response.credential.name,
                        Email: response.credential.email,
                        ImageUrl: response.credential.picture
                    })
                );
            }
        });

        google.accounts.id.prompt();
    }
});
