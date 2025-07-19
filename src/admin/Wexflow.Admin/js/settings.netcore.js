/**
 * Configuration file for the Wexflow admin panel (.NET 9.0+ version).
 *
 * Notes:
 * - Server-Sent Events (SSE) are supported and used for real-time updates
 * - Set the SSE option to true to enable SSE-based updates.
 * - This version runs on ASP.NET Core and replaces the older backend.
 * 
 * @namespace Settings
 * @property {string} Hostname - The backend server hostname (defaults to current hostname or 'localhost').
 * @property {number} Port - The backend server port (default is 8000).
 * @property {string} Uri - Full API base URI constructed from protocol, hostname, and port.
 * @property {boolean} SSE - Enable Server-Sent Events. Must be true when using the 'netcore' version.
 * @property {"net48"|"netcore"} Version - Indicates the Wexflow server version ('net48' for .NET Framework, 'netcore' for .NET Core/.NET 5+).
 */
window.Settings = (function () {
    // Get the current hostname or fallback to 'localhost'
    const hostname = window.location.hostname === "" ? "localhost" : window.location.hostname;

    // Default Wexflow backend port
    const port = 8000;

    // Use current protocol (http or https)
    const protocol = `${window.location.protocol}//`;

    return {
        Hostname: hostname,
        Port: port,
        Uri: `${protocol}${hostname}:${port}/api/v1/`,

        /**
         * To enable Server-Sent Events (SSE), set `SSE` to true
         * and ensure `Version` is set to "netcore"
         */
        SSE: true,

        /**
         * Version of the Wexflow server: "net48" for .NET Framework 4.8,
         * or "netcore" for .NET Core / .NET 5+.
         */
        Version: "netcore",
    };
})()
