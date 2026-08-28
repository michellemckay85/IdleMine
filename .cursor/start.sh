#!/usr/bin/env bash
# Per-boot startup for the Gold and Goblins Unity environment.
# Activates the Unity Editor license from secrets, then warms the project by
# importing/compiling it once so the Library cache is ready for the agent.
#
# Provide ONE of the following via environment secrets:
#   * Unity Pro/Plus:  UNITY_SERIAL + UNITY_EMAIL + UNITY_PASSWORD
#   * Unity Personal:  UNITY_LICENSE  (the full contents of a .ulf file)
#
# start.sh never hard-fails when a license is missing so the VM still boots;
# it prints guidance instead. See .cursor/README.md for details.
set -uo pipefail

UNITY_VERSION="2022.3.50f1"
UNITY_BIN="/opt/unity/${UNITY_VERSION}/Editor/Unity"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ULF_SYSTEM_PATH="/usr/share/unity3d/Unity/Unity_lic.ulf"
ACT_LOG="/tmp/unity-activation.log"
IMPORT_LOG="/tmp/unity-import.log"

if [ ! -x "${UNITY_BIN}" ]; then
  echo "[start] Unity Editor not found at ${UNITY_BIN}. Run .cursor/install.sh first."
  exit 0
fi

run_unity() {
  # Headless Unity under a virtual framebuffer (some subsystems still probe X).
  xvfb-run -a "${UNITY_BIN}" "$@"
}

license_active=0

if [ -n "${UNITY_SERIAL:-}" ]; then
  echo "[start] Activating Unity (Pro/Plus serial)..."
  if run_unity -batchmode -nographics -quit -logFile "${ACT_LOG}" \
      -serial "${UNITY_SERIAL}" \
      -username "${UNITY_EMAIL:-}" -password "${UNITY_PASSWORD:-}"; then
    echo "[start] Serial activation succeeded."
    license_active=1
  else
    echo "[start] Serial activation failed -- see ${ACT_LOG}."
  fi
elif [ -n "${UNITY_LICENSE:-}" ]; then
  echo "[start] Installing Unity Personal license (.ulf)..."
  sudo mkdir -p "$(dirname "${ULF_SYSTEM_PATH}")"
  printf '%s' "${UNITY_LICENSE}" | sudo tee "${ULF_SYSTEM_PATH}" >/dev/null
  sudo chmod 644 "${ULF_SYSTEM_PATH}"
  echo "[start] .ulf written to ${ULF_SYSTEM_PATH}."
  license_active=1
else
  cat <<'EOF'
[start] No Unity license secret found.
[start] Unity cannot compile or run until a license is provided. Add ONE of:
[start]   * UNITY_SERIAL + UNITY_EMAIL + UNITY_PASSWORD   (Unity Pro/Plus)
[start]   * UNITY_LICENSE                                 (Unity Personal .ulf contents)
[start] See .cursor/README.md for how to obtain these.
EOF
fi

if [ "${license_active}" = "1" ]; then
  echo "[start] Warming project (import + script compile)... log: ${IMPORT_LOG}"
  if run_unity -batchmode -nographics -quit \
      -projectPath "${PROJECT_PATH}" -logFile "${IMPORT_LOG}"; then
    echo "[start] Project import/compile succeeded."
  else
    echo "[start] Project import/compile reported errors -- see ${IMPORT_LOG}."
  fi
fi

echo "[start] Startup complete."
