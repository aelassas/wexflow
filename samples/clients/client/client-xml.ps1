$xmlFile = "C:\WexflowTesting\Xml\Products.xml"

# Explicitly read the file as plain text with no PowerShell object wrapping
$WorkflowXMLString = [System.IO.File]::ReadAllText($xmlFile)

# Build JSON payload
$jsonPayload = @{
    WorkflowId = 138
    Variables = @(
        @{
            Name  = "xml"
            Value = $WorkflowXMLString
        }
    )
} | ConvertTo-Json

# Login and get JWT token
$loginPayload = @{
    username      = 'admin'
    password      = 'wexflow2018'
    stayConnected = $false
} | ConvertTo-Json

$res = Invoke-WebRequest -Uri "http://localhost:8000/api/v1/login" -Method Post -Body $loginPayload -ContentType "application/json"
$token = ($res.Content | ConvertFrom-Json).access_token

# Build headers
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# Start workflow with variables
Invoke-WebRequest -Uri "http://localhost:8000/api/v1/start-with-variables" -Method Post -Headers $headers -Body $jsonPayload
