function authorize(username, password, userProfile) {
    set("authorize", '{"Username": "' + username + '", "Password":"' + password + '","UserProfile":' + userProfile + '}');
}

function getUser() {
    return get("authorize");
}

function deleteUser() {
    remove("authorize");
}

function set(key, value) {
    if (isIE()) {
        setCookie(key, value, 365);
    } else {
        window.localStorage.setItem(key, value);
    }
}

function get(key) {
    if (isIE()) {
        return getCookie(key);
    } else {
        return window.localStorage.getItem(key);
    }
}

function remove(key) {
    if (isIE()) {
        setCookie(key, "", -365);
    } else {
        window.localStorage.removeItem(key);
    }
}

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
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
    ua = navigator.userAgent;
    /* MSIE used to detect old browsers and Trident used to newer ones*/
    var is_ie = ua.indexOf("MSIE ") > -1 || ua.indexOf("Trident/") > -1;

    return is_ie;
}
