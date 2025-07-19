<?php

function login($username, $password, $stayConnected = false) {
    $url = 'http://localhost:8000/api/v1/login';
    $data = json_encode([
        'username' => $username,
        'password' => $password,
        'stayConnected' => $stayConnected
    ]);

    $ch = curl_init($url);
    curl_setopt_array($ch, [
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_POST => true,
        CURLOPT_HTTPHEADER => ['Content-Type: application/json'],
        CURLOPT_POSTFIELDS => $data
    ]);

    $response = curl_exec($ch);
    $statusCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    if ($statusCode !== 200) {
        throw new Exception("Login failed: HTTP $statusCode");
    }

    $json = json_decode($response, true);
    return $json['access_token'];
}

function startWorkflow($token, $workflowId) {
    $url = "http://localhost:8000/api/v1/start?w=$workflowId";

    $ch = curl_init($url);
    curl_setopt_array($ch, [
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_POST => true,
        CURLOPT_HTTPHEADER => [
            "Authorization: Bearer $token"
        ]
    ]);

    $response = curl_exec($ch);
    $statusCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    if ($statusCode !== 200) {
        throw new Exception("Start workflow failed: HTTP $statusCode");
    }

    return json_decode($response, true);
}

function listenToSse($url, $token) {
    $headers = [
        "Authorization: Bearer $token",
        "Accept: text/event-stream"
    ];

    $ch = curl_init($url);
    curl_setopt_array($ch, [
        CURLOPT_HTTPHEADER => $headers,
        CURLOPT_WRITEFUNCTION => function($ch, $data) {
            if (strpos($data, "data: ") === 0) {
                $json = trim(substr($data, 6));
                $decoded = json_decode($json, true);
                if ($decoded !== null) {
                    echo "Received SSE JSON:\n";
                    print_r($decoded);
                } else {
                    echo "Invalid SSE JSON\n";
                }
            }
            return strlen($data);
        },
        CURLOPT_TIMEOUT => 0,
        CURLOPT_RETURNTRANSFER => false,
    ]);

    echo "Opening SSE connection...\n";
    curl_exec($ch);

    if (curl_errno($ch)) {
        echo "SSE error: " . curl_error($ch) . "\n";
    }

    curl_close($ch);
}

// ---- Main execution ----
try {
    $username = 'admin';
    $password = 'wexflow2018';
    $workflowId = 41;
    $baseUrl = 'http://localhost:8000/api/v1';

    $token = login($username, $password);
    $jobId = startWorkflow($token, $workflowId);
    echo "Workflow $workflowId started. Job ID: $jobId\n";

    $sseUrl = "$baseUrl/sse/$workflowId/$jobId";
    listenToSse($sseUrl, $token);

} catch (Exception $e) {
    echo 'Error: ' . $e->getMessage() . "\n";
}
