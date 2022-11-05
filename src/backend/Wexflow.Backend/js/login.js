function Login() {

    let auth = "";
    let uri = Common.trimEnd(Settings.Uri, "/");
    let suser = getUser();

    document.body.style.display = 'none';
    const load = () => document.body.style.display = 'block';
    
    if (suser) {
        let user = JSON.parse(suser);

        qusername = user.Username;
        qpassword = user.Password;
        auth = "Basic " + btoa(qusername + ":" + qpassword);

        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                console.log('user.Password', user.Password)
                console.log('u.Password', u.Password)
                if (user.Password === u.Password) {
                    window.location.replace("dashboard.html");
                } else {
                    deleteUser();
                    load();
                }
            }, function () { }, auth);
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

    let language = new Language("lang", updateLanguage);
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
        let passwordHash = MD5(password);
        auth = "Basic " + btoa(username + ":" + passwordHash);

        if (username === "" || password === "") {
            Common.toastInfo(language.get("valid-username"));
        } else {
            Common.get(uri + "/user?username=" + encodeURIComponent(username), function (user) {
                if (typeof user === "undefined" || user === null) {
                    Common.toastError(language.get("wrong-credentials"));
                } else {
                    if (passwordHash === user.Password) {
                        authorize(username, passwordHash, user.UserProfile);
                        window.location.replace("dashboard.html");
                    } else {
                        Common.toastError(language.get("wrong-password"));
                    }

                }
            }, function () { }, auth);
        }
    }

}