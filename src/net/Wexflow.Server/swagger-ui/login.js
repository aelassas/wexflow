window.Login = function () {

    var uri = window.Common.trimEnd(window.Settings.Uri, "/");
    var loginBtn = document.getElementById("btn-login");
    var usernameTxt = document.getElementById("txt-username");
    var passwordTxt = document.getElementById("txt-password");

    loginBtn.onclick = async function () {
        await login();
    };

    passwordTxt.onkeyup = async function (event) {
        event.preventDefault();

        if (event.key === 'Enter') {
            await login();
        }
    };

    async function login() {

        let username = usernameTxt.value;
        let password = passwordTxt.value;
        let stayConnected = true;

        if (username === "" || password === "") {
            window.Common.toastInfo(language.get("valid-username"));
        } else {
            try {
                const res = await fetch(uri + "/login",
                    {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({ username, password, stayConnected }),
                    })
                if (!res.ok) {
                    throw new Error(`HTTP ${res.status} - ${res.statusText}`)
                }
                const data = await res.json()
                if (data.access_token) {
                    window.authorize(username);
                    window.location.replace("doc.html");
                }
            } catch (err) {
                console.error("Login failed:", err);
                alert("Wrong Credentials");
            }
        }
    }

}