package main

import (
	"bufio"
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
)

const (
	baseURL    = "http://localhost:8000/api/v1"
	username   = "admin"
	password   = "wexflow2018"
	workflowId = 41
)

type LoginResponse struct {
	AccessToken string `json:"access_token"`
}

func login(user, pass string, stayConnected bool) (string, error) {
	payload := map[string]interface{}{
		"username":      user,
		"password":      pass,
		"stayConnected": stayConnected,
	}
	body, _ := json.Marshal(payload)

	req, err := http.NewRequest("POST", baseURL+"/login", bytes.NewBuffer(body))
	if err != nil {
		return "", err
	}
	req.Header.Set("Content-Type", "application/json")

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("login failed: %s", resp.Status)
	}

	var result LoginResponse
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		return "", err
	}

	return result.AccessToken, nil
}

func startWorkflow(token string, workflowId int) (string, error) {
	url := fmt.Sprintf("%s/start?w=%d", baseURL, workflowId)
	req, err := http.NewRequest("POST", url, nil)
	if err != nil {
		return "", err
	}
	req.Header.Set("Authorization", "Bearer "+token)

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", fmt.Errorf("start workflow failed: %s", resp.Status)
	}

	var jobId string
	if err := json.NewDecoder(resp.Body).Decode(&jobId); err != nil {
		return "", err
	}

	return jobId, nil
}

func listenToSSE(url, token string) error {
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return err
	}
	req.Header.Set("Accept", "text/event-stream")
	req.Header.Set("Authorization", "Bearer "+token)

	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	fmt.Println("SSE connection opened")

	reader := bufio.NewReader(resp.Body)
	for {
		line, err := reader.ReadString('\n')
		if err == io.EOF {
			break
		}
		if err != nil {
			return fmt.Errorf("error reading SSE: %v", err)
		}

		if len(line) > 6 && line[:6] == "data: " {
			data := line[6:]
			var parsed map[string]interface{}
			if err := json.Unmarshal([]byte(data), &parsed); err != nil {
				fmt.Println("Failed to parse SSE JSON:", err)
			} else {
				fmt.Println("Received SSE JSON:")
				out, _ := json.MarshalIndent(parsed, "", "  ")
				fmt.Println(string(out))
			}
			break // Close after first SSE event
		}
	}

	fmt.Println("SSE connection closed")
	return nil
}

func main() {
	token, err := login(username, password, false)
	if err != nil {
		fmt.Println("Login error:", err)
		return
	}

	jobId, err := startWorkflow(token, workflowId)
	if err != nil {
		fmt.Println("Start workflow error:", err)
		return
	}

	fmt.Printf("Workflow %d started. Job ID: %s\n", workflowId, jobId)

	sseUrl := fmt.Sprintf("%s/sse/%d/%s", baseURL, workflowId, jobId)
	if err := listenToSSE(sseUrl, token); err != nil {
		fmt.Println("SSE error:", err)
	}
}
