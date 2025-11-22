If you want to change the port from `8000` to `8338` for example you need to edit `/opt/wexflow/Admin/js/settings.js`.

Here is the `docker-compose.yml`:
```yml
services:
  wexflow:
    image: aelassas/wexflow:latest
    container_name: wexflow
    ports:
      - "8338:8000"
    volumes:
      - ./wexflow-config/settings.js:/opt/wexflow/Admin/js/settings.js
    restart: unless-stopped
```

Create `/wexflow-config/settings.js` next to your `docker-compose.yml` with the following content:
```js
window.Settings = (function () {
    const hostname = window.location.hostname === "" ? "localhost" : window.location.hostname;

    const port = 8338;

    const protocol = `${window.location.protocol}//`;

    return {
        Hostname: hostname,
        Port: port,
        Uri: `${protocol}${hostname}:${port}/api/v1/`,
        SSE: true,
        Version: "netcore",
        DebounceDelay: 300,
    };
})();
```
