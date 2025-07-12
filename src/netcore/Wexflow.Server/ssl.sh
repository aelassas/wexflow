#!/bin/bash

# ssl.sh - Generate self-signed SSL certificate (key, crt, pfx)
# Prerequisite: OpenSSL installed (https://www.openssl.org/)

# Config
KEY="/opt/wexflow/Wexflow/wexflow.key"
CRT="/opt/wexflow/Wexflow/wexflow.crt"
PFX="/opt/wexflow/Wexflow/wexflow.pfx"
PASSWORD="wexflow2018"
DAYS=6935 # 19 years

# Clean up old files
rm -f "$KEY" "$CRT" "$PFX"

# Step 1: Generate a 2048-bit RSA private key in PEM format
openssl genpkey -algorithm RSA -out "$KEY" -pkeyopt rsa_keygen_bits:2048

# Step 2: Generate a self-signed certificate
openssl req -new -x509 -key "$KEY" -out "$CRT" -days "$DAYS" -subj "/CN=localhost"

# Step 3: Export to PKCS#12 (.pfx)
openssl pkcs12 -export -out "$PFX" -inkey "$KEY" -in "$CRT" -password pass:"$PASSWORD"

# Step 4: Confirm
if [ -f "$PFX" ]; then
  echo "PFX generated: $PFX"
else
  echo "Failed to generate PFX"
  exit 1
fi
