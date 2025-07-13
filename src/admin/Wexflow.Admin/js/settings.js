window.Settings = (function () {
    const hostname = (window.location.hostname === "" ? "localhost" : window.location.hostname);
    const port = 8000;

    return {
        Hostname: hostname,
        Port: port,
        Uri: "http://" + hostname + ":" + port + "/api/v1/"
    };
})();