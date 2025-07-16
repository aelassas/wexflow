window.authorize = function (username) {
    set("wf-swagger-authorize", '{"Username": "' + username + '"}');
}

window.getUser = function () {
    return get("wf-swagger-authorize");
}

window.deleteUser = function () {
    remove("wf-swagger-authorize");
}

window.logout = function (callback) {
    let uri = window.Common.trimEnd(window.Settings.Uri, "/");
    const _logout = () => {
        window.deleteUser();
        if (callback) {
            callback();
        } else {
            window.location.replace("index.html");
        }
    }
    window.Common.post(`${uri}/logout`, _logout, _logout);
}

function set(key, value) {
    window.localStorage.setItem(key, value);

}

function get(key) {
    return window.localStorage.getItem(key);
}

function remove(key) {
    window.localStorage.removeItem(key);

}
