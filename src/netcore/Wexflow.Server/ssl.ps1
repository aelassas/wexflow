# Prerequisites:
# 1. Install Win64 OpenSSL: https://slproweb.com/products/Win32OpenSSL.html
# 2. Add "C:\Program Files\OpenSSL-Win64\bin" to your Path environment variable

# Define certificate file paths
$KEY = "C:\Wexflow-netcore\wexflow.key"
$CRT = "C:\Wexflow-netcore\wexflow.crt"
$PFX = "C:\Wexflow-netcore\wexflow.pfx"
$PASSWORD = "wexflow2018"
$DAYS = 6935  # ~19 years

# Ensure OpenSSL is available
if (-not (Get-Command "openssl.exe" -ErrorAction SilentlyContinue)) {
    Write-Error "OpenSSL is not found in PATH. Please install and add it to your environment variables."
    exit 1
}

# Remove any existing files
Remove-Item -Force $KEY, $CRT, $PFX -ErrorAction SilentlyContinue

# Step 1: Generate a 2048-bit RSA private key
openssl genpkey -algorithm RSA -out $KEY -pkeyopt rsa_keygen_bits:2048

# Step 2: Generate a self-signed certificate
openssl req -new -x509 -key $KEY -out $CRT -days $DAYS -subj "/CN=localhost"

# Step 3: Export to PFX
openssl pkcs12 -export -out $PFX -inkey $KEY -in $CRT -password pass:$PASSWORD

# Optional: Confirm result
if (Test-Path $PFX) {
    Write-Host "PFX successfully generated at: $PFX"
} else {
    Write-Error "Failed to generate PFX."
}
