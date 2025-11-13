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
$workflowId=1
Invoke-WebRequest -Uri "http://localhost:8000/api/v1/start?w=$workflowId" -Method Post -Headers $headers -Body $jsonPayload
