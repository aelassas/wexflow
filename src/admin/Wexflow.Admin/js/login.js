window.Login = function () {

    let uri = window.Common.trimEnd(window.Settings.Uri, "/");
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
                    window.location.replace("dashboard.html");
                } else {
                    _logout();
                }
            }, function () {
                _logout();
            }, null);
    } else {
        load();
    }

    let updateLanguage = function (language) {
        document.getElementById("help").innerHTML = language.get("help");
        document.getElementById("about").innerHTML = language.get("about");
        document.getElementById("lbl-username").innerHTML = language.get("username");
        document.getElementById("lbl-password").innerHTML = language.get("password");
        document.getElementById("forgot-password").innerHTML = language.get("forgot-password");
        document.getElementById("lbl-stay-connected").innerHTML = language.get("stay-connected");
        document.getElementById("btn-login").value = language.get("login");
    };

    let language = new window.Language("lang", updateLanguage);
    language.init();

    let loginBtn = document.getElementById("btn-login");
    let usernameTxt = document.getElementById("txt-username");
    let passwordTxt = document.getElementById("txt-password");
    let divkStayConnected = document.getElementById("stay-connected");
    let chkStayConnected = document.getElementById("chk-stay-connected");

    loginBtn.onclick = async function () {
        await login();
    };

    passwordTxt.onkeyup = async function (event) {
        event.preventDefault();

        if (event.key === 'Enter') {
            await login();
        }
    };

    divkStayConnected.onclick = function () {
        chkStayConnected.checked = !chkStayConnected.checked;
    }

    chkStayConnected.onclick = function () {
        chkStayConnected.checked = !chkStayConnected.checked;
    }

    async function login() {

        let username = usernameTxt.value;
        let password = passwordTxt.value;
        let stayConnected = chkStayConnected.checked;

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
                    window.location.replace("dashboard.html");
                }
            } catch (err) {
                console.error("Login failed:", err);
                window.Common.toastError(language.get("wrong-credentials"));
            }
        }
    }

}