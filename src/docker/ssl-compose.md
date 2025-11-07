You can override Wexflow's `appsettings.json` by mounting a local copy into the container.

If you want to set `HTTPS` option to `true`, you need to provide a `PfxFile` in `appsettings.json`. See the following [docs](https://github.com/aelassas/wexflow/wiki/SSL#net-90-1) for generating a PFX certificate. 

Here's how you can handle both your `appsettings.json` and `.pfx` certificate when running Wexflow via Compose.

## Directory layout example

Let's say your project folder looks like this:

```
project/
├─ docker-compose.yml
└─ wexflow-config/
   ├─ appsettings.json
   └─ mycert.pfx
```

## Step 1: Update `appsettings.json`

Here's the default `appsettings.json`:
```json
{
  "WexflowSettingsFile": "/opt/wexflow/Wexflow/Wexflow.xml",
  "LogLevel": "All",
  "WexflowServicePort": 8000,
  "SuperAdminUsername": "admin",
  "EnableWorkflowsHotFolder": false,
  "EnableRecordsHotFolder": true,
  "EnableEmailNotifications": false,
  "DateTimeFormat": "dd-MM-yyyy HH:mm:ss",
  "Smtp.Host": "smtp.gmail.com",
  "Smtp.Port": 587,
  "Smtp.EnableSsl": true,
  "Smtp.User": "user",
  "Smtp.Password": "password",
  "Smtp.From": "user",
  "AdminFolder": "/opt/wexflow/Admin",
  "HTTPS": false,
  "PfxFile": "/opt/wexflow/Wexflow/wexflow.pfx",
  "PfxPassword": "wexflow2018",
  "JwtSecret": "b7a3c04f10e84c3f95a3f3497bda8e32",
  "JwtExpireAtMinutes": 1440,
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

Update you local `appsettings.json`:

```json
{
  "HTTPS": true,
  "PfxFile": "/opt/wexflow/Wexflow.Server/mycert.pfx",
  "PfxPassword": "your_password_here"
}
```

## Step 2: `docker-compose.yml`

Here's a minimal working example:

```yaml
version: "3.9"

services:
  wexflow:
    image: aelassas/wexflow:latest
    container_name: wexflow
    ports:
      - "8000:8000"  # HTTPS
    volumes:
      - ./wexflow-config/appsettings.json:/opt/wexflow/Wexflow.Server/appsettings.json:ro
      - ./wexflow-config/mycert.pfx:/opt/wexflow/Wexflow.Server/mycert.pfx:ro
    restart: unless-stopped
```

Explanation:

* `volumes:` mounts your local configuration and certificate into the container.
* `:ro` makes them read-only (optional but good practice).
* You can easily restart or rebuild Wexflow without losing config.

## Step 3: Run it

Start your stack:

```bash
docker compose up -d
```

Then check logs:

```bash
docker compose logs -f wexflow
```

If everything's configured properly, you'll see Wexflow start and bind on HTTPS.

## When you make changes

Just edit your local files and restart:

```bash
docker compose restart wexflow
```
