#!/usr/bin/env bash
# Idempotent environment bootstrap for the Gold and Goblins Unity project.
# Installs the headless-Unity system dependencies and the pinned Unity Editor.
# Safe to run repeatedly: existing installs are detected and skipped.
#
# Licensing is NOT done here (it needs per-run secrets) -- see start.sh.
set -euo pipefail

UNITY_VERSION="2022.3.50f1"
UNITY_CHANGESET="c3db7f8bf9b1"
UNITY_ROOT="/opt/unity/${UNITY_VERSION}"
UNITY_BIN="${UNITY_ROOT}/Editor/Unity"

echo "[install] Installing system dependencies for headless Unity..."
export DEBIAN_FRONTEND=noninteractive
sudo apt-get update -qq
# Package names are for Ubuntu 24.04 (noble); the *t64 variants replace the
# pre-24.04 names (libgtk-3-0, libasound2).
sudo apt-get install -y -qq \
  ca-certificates curl xz-utils \
  libgtk-3-0t64 libnss3 libxtst6 libxss1 libasound2t64 libglu1-mesa \
  xvfb libgbm1 libnotify4 libgl1 libcap2 libunwind8

if [ -x "${UNITY_BIN}" ]; then
  echo "[install] Unity ${UNITY_VERSION} already installed at ${UNITY_ROOT} -- skipping download."
else
  echo "[install] Downloading Unity ${UNITY_VERSION} Linux Editor (~4 GB, changeset ${UNITY_CHANGESET})..."
  sudo mkdir -p "${UNITY_ROOT}"
  sudo chown -R "$(id -un):$(id -gn)" /opt/unity
  TMP_DIR="$(mktemp -d)"
  trap 'rm -rf "${TMP_DIR}"' EXIT
  curl -fL --retry 4 --retry-delay 4 \
    -o "${TMP_DIR}/Unity.tar.xz" \
    "https://download.unity3d.com/download_unity/${UNITY_CHANGESET}/LinuxEditorInstaller/Unity.tar.xz"
  echo "[install] Extracting Unity Editor..."
  tar -xJf "${TMP_DIR}/Unity.tar.xz" -C "${UNITY_ROOT}"
fi

echo "[install] Unity Editor version: $(${UNITY_BIN} -version 2>/dev/null || echo 'unknown')"
echo "[install] Done. Run start.sh (per boot) to activate the license."
