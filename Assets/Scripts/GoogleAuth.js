function SignInWithGoogle() {
  google.accounts.id.initialize({
      client_id: "1083895502867-da72s70b24uaihnk5nmktmimc7d8ht8k.apps.googleusercontent.com",
      callback: (response) => {
          const user = parseJwt(response.credential); // Decodifique o JWT do Google
          const userJson = JSON.stringify(user);

          // Envia as informações para Unity
          if (typeof unityInstance !== "undefined" && unityInstance) {
              unityInstance.SendMessage("GoogleAuthentication", "OnGoogleSignInWebGL", userJson);
          }
      }
  });

  // Mostra o prompt de login do Google
  google.accounts.id.prompt();
}

function parseJwt(token) {
  const base64Url = token.split('.')[1];
  const base64 = decodeURIComponent(
      atob(base64Url)
          .split('')
          .map(c => `%${('00' + c.charCodeAt(0).toString(16)).slice(-2)}`)
          .join('')
  );
  return JSON.parse(base64);
}
