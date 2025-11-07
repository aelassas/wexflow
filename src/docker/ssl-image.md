You can override Wexflow's `appsettings.json` either by editing inside the container or, more cleanly, by mounting a local copy into the container.

### Option 1: Edit inside the running container (temporary)

If you only want to test changes:

```bash
docker exec -it wexflow /bin/bash
```

Then open or edit the config file inside:

```bash
vi /opt/wexflow/Wexflow.Server/appsettings.json
```

After saving, restart the container so the app reloads the config:

```bash
docker restart wexflow
```

**Drawback:** When you remove or recreate the container, your changes will be lost (since they live inside the container's filesystem).

### Option 2: Use a local copy (persistent & recommended)

This is the best approach.

1. Copy the original file out of the container (if you want a base to modify):

   ```bash
   docker cp wexflow:/opt/wexflow/Wexflow.Server/appsettings.json ./appsettings.json
   ```

2. Edit the local copy on your machine (e.g. `./appsettings.json`).

3. Run Wexflow using a bind mount so Docker uses your local file:

   ```bash
   docker run -d -p 8000:8000 \
     --name wexflow \
     -v "$(pwd)/appsettings.json:/opt/wexflow/Wexflow.Server/appsettings.json" \
     aelassas/wexflow:latest
   ```

Now, Wexflow will use your local version of the configuration file.
If you edit it locally, you can just restart the container to reload changes:

```bash
docker restart wexflow
```

4. If you want to set `HTTPS` option to `true`, you need to provide a `PfxFile` in `appsettings.json`:

#### Option 1: Mount the `.pfx` file directly

Assuming:

* Your local file: `./mycert.pfx`
* Inside the container, Wexflow expects the file at `/opt/wexflow/Wexflow.Server/mycert.pfx`

##### Step 1: Mount the `.pfx` file

You can do this along with your modified `appsettings.json`:

```bash
docker run -d -p 8000:8000 \
  --name wexflow \
  -v "$(pwd)/appsettings.json:/opt/wexflow/Wexflow.Server/appsettings.json" \
  -v "$(pwd)/mycert.pfx:/opt/wexflow/Wexflow.Server/mycert.pfx" \
  aelassas/wexflow:latest
```

---

##### Step 2: Set the path inside `appsettings.json`

Inside your local `appsettings.json`, make sure the path points to the file inside the container, for example:

```json
{
 "HTTPS": true,
 "PfxFile": "/opt/wexflow/Wexflow.Server/mycert.pfx",
 "PfxPassword": "your_password_here"
}
```

This way:

* Your local file `mycert.pfx` is available inside the container at `/opt/wexflow/Wexflow.Server/mycert.pfx`
* The app will read it normally.

---

#### Option 2: Mount a folder for multiple files (cleaner)

If you have several config-related files, mount the entire folder instead:

##### Directory structure:

```
project/
 ├─ wexflow-config/
 │   ├─ appsettings.json
 │   ├─ mycert.pfx
 └─ ...
```

Then:

```bash
docker run -d -p 8000:8000 \
  --name wexflow \
  -v "$(pwd)/wexflow-config:/opt/wexflow/Wexflow.Server" \
  aelassas/wexflow:latest
```

Now, everything in your `wexflow-config` folder (including both files) will replace the ones in `/opt/wexflow/Wexflow.Server`.
