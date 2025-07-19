package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
)

const (
	baseURL    = "http://localhost:8000/api/v1"
	username   = "admin"
	password   = "wexflow2018"
	workflowId = 41
)

type loginRequest struct {
	Username      string `json:"username"`
	Password      string `json:"password"`
	StayConnected bool   `json:"stayConnected"`
}

type loginResponse struct {
	AccessToken string `json:"access_token"`
}

func login(user, pass string, stayConnected bool) (string, error) {
	payload := loginRequest{
		Username:      user,
		Password:      pass,
		StayConnected: stayConnected,
	}

	data, err := json.Marshal(payload)
	if err != nil {
		return "", fmt.Errorf("failed to marshal login request: %w", err)
	}

	resp, err := http.Post(baseURL+"/login", "application/json", bytes.NewBuffer(data))
	if err != nil {
		return "", fmt.Errorf("failed to send login request: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("login failed: HTTP %d %s", resp.StatusCode, resp.Status)
	}

	var result loginResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		return "", fmt.Errorf("failed to decode login response: %w", err)
	}

	return result.AccessToken, nil
}

func startWorkflow(token string, id int) (string, error) {
	url := fmt.Sprintf("%s/start?w=%d", baseURL, id)
	req, err := http.NewRequest("POST", url, nil)
	if err != nil {
		return "", fmt.Errorf("failed to create start request: %w", err)
	}

	req.Header.Set("Authorization", "Bearer "+token)
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return "", fmt.Errorf("failed to send start request: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("start failed: HTTP %d %s", resp.StatusCode, resp.Status)
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", fmt.Errorf("failed to read start response: %w", err)
	}

	return string(body), nil
}

func main() {
	token, err := login(username, password, false)
	if err != nil {
		log.Fatalf("Login failed: %v", err)
	}

	jobID, err := startWorkflow(token, workflowId)
	if err != nil {
		log.Fatalf("Failed to start workflow %d: %v", workflowId, err)
	}

	fmt.Printf("Workflow %d started successfully. Job ID: %s\n", workflowId, jobID)
}
