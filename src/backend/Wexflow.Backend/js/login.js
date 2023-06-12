window.Login = function () {

    let auth = "";
    let uri = window.Common.trimEnd(window.Settings.Uri, "/");
    let suser = window.getUser();

    document.body.style.display = 'none';
    const load = () => document.body.style.display = 'block';

    if (suser) {
        let user = JSON.parse(suser);

        qusername = user.Username;
        qpassword = user.Password;
        auth = "Basic " + btoa(qusername + ":" + qpassword);

        const _logout = () => {
            window.logout(() => {
                load();
            });
        };

        window.Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (u && user.Password === u.Password) {
                    window.location.replace("dashboard.html");
                } else {
                    _logout();
                }
            }, function () {
                _logout();
            }, auth);
    } else {
        load();
    }

    let updateLanguage = function (language) {
        document.getElementById("help").innerHTML = language.get("help");
        document.getElementById("about").innerHTML = language.get("about");
        document.getElementById("lbl-username").innerHTML = language.get("username");
        document.getElementById("lbl-password").innerHTML = language.get("password");
        document.getElementById("forgot-password").innerHTML = language.get("forgot-password");
        document.getElementById("btn-login").value = language.get("login");
    };

    let language = new window.Language("lang", updateLanguage);
    language.init();

    let loginBtn = document.getElementById("btn-login");
    let usernameTxt = document.getElementById("txt-username");
    let passwordTxt = document.getElementById("txt-password");

    loginBtn.onclick = function () {
        login();
    };

    passwordTxt.onkeyup = function (event) {
        event.preventDefault();

        if (event.keyCode === 13) {
            login();
        }
    };

    function login() {

        let username = usernameTxt.value;
        let password = passwordTxt.value;
        let passwordHash = window.MD5(password);
        auth = "Basic " + btoa(username + ":" + passwordHash);

        if (username === "" || password === "") {
            window.Common.toastInfo(language.get("valid-username"));
        } else {
            window.Common.get(uri + "/user?username=" + encodeURIComponent(username), function (user) {
                if (typeof user === "undefined" || user === null) {
                    window.Common.toastError(language.get("wrong-credentials"));
                } else {
                    if (passwordHash === user.Password) {
                        window.authorize(username, passwordHash, user.UserProfile);
                        window.location.replace("dashboard.html");
                    } else {
                        window.Common.toastError(language.get("wrong-password"));
                    }

                }
            }, function () { }, auth);
        }
    }

}