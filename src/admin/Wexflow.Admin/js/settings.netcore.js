/**
 * Configuration file for the Wexflow admin panel (.NET 9.0+ version).
 *
 * Notes:
 * - Server-Sent Events (SSE) are supported and used for real-time updates
 * - Set the SSE option to true to enable SSE-based updates.
 * - This version runs on ASP.NET Core and replaces the older backend.
 */
window.Settings = (function () {
    const hostname = (window.location.hostname === "" ? "localhost" : window.location.hostname);
    const port = 8000;

    return {
        Hostname: hostname,
        Port: port,
        Uri: "http://" + hostname + ":" + port + "/api/v1/",
        SSE: true,
        Version: "netcore",
    };
})();