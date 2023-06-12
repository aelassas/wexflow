window.Login = function () {

    var uri = window.Common.trimEnd(window.Settings.Uri, "/");
    var loginBtn = document.getElementById("btn-login");
    var usernameTxt = document.getElementById("txt-username");
    var passwordTxt = document.getElementById("txt-password");
    var auth = "";

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

        var username = usernameTxt.value;
        var password = passwordTxt.value;
        var passwordHash = window.MD5(password);
        auth = "Basic " + btoa(username + ":" + passwordHash);

        if (username === "" || password === "") {
            alert("Enter a valid username and password.");
        } else {
            window.Common.get(uri + "/user?username=" + encodeURIComponent(username), function (user) {
                if (typeof user === "undefined" || user === null) {
                    alert("Wrong credentials.");
                } else {
                    if (passwordHash === user.Password) {
                        window.authorize(username, passwordHash, user.UserProfile);
                        window.location.replace("doc.html");
                    } else {
                        alert("The password is incorrect.");
                    }

                }
            }, function () { }, auth);
        }
    }

}