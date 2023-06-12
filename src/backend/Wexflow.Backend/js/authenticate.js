window.authorize = function (username, password, userProfile) {
    set("wf-authorize", '{"Username": "' + username + '", "Password":"' + password + '","UserProfile":' + userProfile + '}');
}

window.getUser = function () {
    return get("wf-authorize");
}

window.deleteUser = function () {
    remove("wf-authorize");
}

window.logout = function (callback) {
    window.deleteUser();
    if (callback) {
        callback();
    } else {
        window.location.replace("index.html");
    }
}

function set(key, value) {
    if (isIE() || isFirefox()) {
        setCookie(key, value, 365);
    } else {
        window.localStorage.setItem(key, value);
    }
}

function get(key) {
    if (isIE() || isFirefox()) {
        return getCookie(key);
    } else {
        return window.localStorage.getItem(key);
    }
}

function remove(key) {
    if (isIE() || isFirefox()) {
        setCookie(key, "", -365);
    } else {
        window.localStorage.removeItem(key);
    }
}

function setCookie(cname, cvalue, exdays) {
    let d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    let name = cname + "=";
    let ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function isIE() {
    let ua = navigator.userAgent;
    /* MSIE used to detect old browsers and Trident used to newer ones*/
    let is_ie = ua.indexOf("MSIE") > -1 || ua.indexOf("Trident/") > -1;

    return is_ie;
}

function isFirefox() {
    let ua = navigator.userAgent;
    let is_firefox = ua.toLowerCase().indexOf("firefox") > -1;

    return is_firefox;
}
