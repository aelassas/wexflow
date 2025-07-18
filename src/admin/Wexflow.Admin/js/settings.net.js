/**
 * Configuration file for the Wexflow admin panel (.NET 4.8 version).
 *
 * Note:
 * - Server-Sent Events (SSE) are not supported in this version.
 * - Polling is used instead for real-time updates.
 * - The SSE option **must always remain set to false** in this version.
 */
window.Settings = (function () {
    const hostname = (window.location.hostname === "" ? "localhost" : window.location.hostname);
    const port = 8000;

    return {
        Hostname: hostname,
        Port: port,
        Uri: "http://" + hostname + ":" + port + "/api/v1/",
        SSE: false,
        Version: "net48",
    };
})();