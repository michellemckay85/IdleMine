# Cloud Agent environment — Gold and Goblins (Unity 2022.3.50f1)

This folder configures the Cursor Cloud Agent development environment for the
Unity project.

| File | Purpose |
| --- | --- |
| `environment.json` | Declares the `install` and `start` commands. |
| `install.sh` | Installs headless-Unity system libraries and the pinned Unity `2022.3.50f1` Editor into `/opt/unity`. Idempotent. |
| `start.sh` | Activates the Unity license from secrets (per boot) and warms the project by importing/compiling it. |

## Required secret: a Unity license

Unity refuses to run — even in `-batchmode -nographics` — without an activated
license (`No valid Unity Editor license found`). Add **one** of the following in
the Cloud Agent **Secrets** panel:

### Option A — Unity Pro / Plus (recommended for CI)

Set all three secrets:

- `UNITY_SERIAL` — your Pro/Plus serial (`XX-XXXX-XXXX-XXXX-XXXX-XXXX`)
- `UNITY_EMAIL` — Unity account email
- `UNITY_PASSWORD` — Unity account password

Serial activation works on ephemeral machines. Note it consumes a seat; return
it from a licensed machine (`Unity -returnlicense`) if you need to free it.

### Option B — Unity Personal (free)

Set:

- `UNITY_LICENSE` — the full contents of a `.ulf` license file

To obtain the `.ulf`:

1. Download the manual activation request file produced by this environment:
   `Unity_v2022.3.50f1.alf` (attached to this agent run), or generate a fresh
   one with
   `/opt/unity/2022.3.50f1/Editor/Unity -batchmode -nographics -quit -createManualActivationFile`.
2. Go to <https://license.unity3d.com/manual>, upload the `.alf`, and complete
   the (free) Personal activation.
3. Download the resulting `Unity_v2022.x.ulf` and paste its **entire contents**
   into the `UNITY_LICENSE` secret.

## Manual commands

```bash
# Compile scripts / import the project headlessly:
/opt/unity/2022.3.50f1/Editor/Unity -batchmode -nographics -quit \
  -projectPath . -logFile /tmp/unity.log

# Regenerate the starter scene (menu: Gold And Goblins → Bootstrap Starter Scene):
/opt/unity/2022.3.50f1/Editor/Unity -batchmode -nographics -quit \
  -projectPath . -executeMethod GoldAndGoblins.EditorTools.ProjectBootstrapper.BootstrapScene \
  -logFile /tmp/unity.log
```
