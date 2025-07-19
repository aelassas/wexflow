<?php

$baseUrl = 'http://localhost:8000/api/v1';
$username = 'admin';
$password = 'wexflow2018';
$workflowId = 1;

/**
 * Sends a login request and returns the access token.
 */
function login($username, $password, $stayConnected = false)
{
  global $baseUrl;

  $url = $baseUrl . '/login';
  $payload = json_encode([
    'username' => $username,
    'password' => $password,
    'stayConnected' => $stayConnected
  ]);

  $ch = curl_init($url);
  curl_setopt_array($ch, [
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_POST => true,
    CURLOPT_ENCODING => "",
    CURLOPT_MAXREDIRS => 10,
    CURLOPT_TIMEOUT => 0,
    CURLOPT_FOLLOWLOCATION => true,
    CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
    CURLOPT_HTTPHEADER => ['Content-Type: application/json'],
    CURLOPT_POSTFIELDS => $payload
  ]);

  $response = curl_exec($ch);
  $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
  curl_close($ch);

  if ($httpCode !== 200) {
    throw new Exception("Login failed: HTTP $httpCode - $response");
  }

  $data = json_decode($response, true);
  return $data['access_token'] ?? null;
}

/**
 * Starts the workflow with the given ID using the access token.
 */
function startWorkflow($workflowId, $token)
{
  global $baseUrl;

  $url = $baseUrl . '/start?w=' . urlencode($workflowId);

  $ch = curl_init($url);
  curl_setopt_array($ch, [
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_POST => true,
    CURLOPT_ENCODING => "",
    CURLOPT_MAXREDIRS => 10,
    CURLOPT_TIMEOUT => 0,
    CURLOPT_FOLLOWLOCATION => true,
    CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
    CURLOPT_HTTPHEADER => array(
      "Content-Type: application/json",
      "Content-Length: 0",
      "Authorization: Bearer $token"
    ),
  ]);

  $response = curl_exec($ch);
  $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
  curl_close($ch);

  if ($httpCode !== 200) {
    throw new Exception("Start workflow failed: HTTP $httpCode - $response");
  }

  $data = json_decode($response, true);
  return $data;
}

try {
  $token = login($username, $password);
  $jobId = startWorkflow($workflowId, $token);
  echo "Workflow $workflowId started successfully. Job ID: " . json_encode($jobId) . "\n";
} catch (Exception $e) {
  echo "Error: " . $e->getMessage() . "\n";
}
?>
