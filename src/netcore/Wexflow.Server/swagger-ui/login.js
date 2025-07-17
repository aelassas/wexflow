window.Login = function () {

    const uri = window.Common.trimEnd(window.Settings.Uri, "/");

    let suser = window.getUser();

    document.body.style.display = 'none';
    const load = () => document.body.style.display = 'block';

    if (suser) {
        let user = JSON.parse(suser);

        const _logout = () => {
            window.logout(() => {
                load();
            });
        };

        window.Common.post(uri + "/validate-token?u=" + encodeURIComponent(user.Username),
            function (res) {
                if (res.valid === true) {
                    window.location.replace("doc.html");
                } else {
                    _logout();
                }
            }, function () {
                _logout();
            }, null);
    } else {
        load();
    }

    const loginBtn = document.getElementById("btn-login");
    const usernameTxt = document.getElementById("txt-username");
    const passwordTxt = document.getElementById("txt-password");

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