If your reverse proxy or gateway already handles SSL termination, then keeping Wexflow running over plain HTTP inside Docker is totally fine — that’s a clean setup.

**Why `https://yourdomain.com:8000/api/v1/` fails**

When using the current config (`:8000`), the browser is told to connect directly to port 8000 — bypassing the gateway. But:

* The gateway doesn’t expose port 8000 publicly.
* Wexflow’s container only speaks HTTP, not HTTPS.
* So the browser can’t connect or gets a mixed-content error.

Make sure your gateway forwards all requests under `/api/v1/` to Wexflow’s container. For example, an Nginx rule might look like:

```nginx
location /api/v1/ {
    proxy_pass http://wexflow:8000/api/v1/;
    proxy_set_header X-Forwarded-Proto https;
}
```

Then update `/opt/wexflow/Admin/js/settings.js` to:

```js
window.Settings = (function () {
    return {
        // Use the browser’s current origin (will be https://yourdomain.com if accessed via the gateway)
        Uri: `${window.location.origin}/api/v1/`,

        SSE: true,
        Version: "netcore",
        DebounceDelay: 300,
    };
})();
```

Finally, create a local copy of `settings.js` and load it with your Docker Compose setup:

```yaml
version: "3.9"

services:
  wexflow:
    image: aelassas/wexflow:latest
    container_name: wexflow
    ports:
      - "8000:8000"  # HTTP
    volumes:
      - ./wexflow-config/settings.js:/opt/wexflow/Admin/js/settings.js:ro
    restart: unless-stopped
```

When you make changes, restart:
```
docker compose restart wexflow
```

Let me know if this works for you.
