package com.example;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

import okhttp3.*;
import okhttp3.sse.EventSource;
import okhttp3.sse.EventSourceListener;
import okhttp3.sse.EventSources;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.JsonNode;

public class WorkflowSSEClient {

    private static final String BASE_URL = "http://localhost:8000/api/v1";
    private static final String USERNAME = "admin";
    private static final String PASSWORD = "wexflow2018";
    private static final int WORKFLOW_ID = 41;

    private static final HttpClient httpClient = HttpClient.newHttpClient();
    private static final ObjectMapper objectMapper = new ObjectMapper();

    public static void main(String[] args) throws Exception {
        String token = login(USERNAME, PASSWORD);
        String jobId = startWorkflow(token, WORKFLOW_ID);

        System.out.printf("Workflow %d started. Job ID: %s%n", WORKFLOW_ID, jobId);

        String sseUrl = String.format("%s/sse/%d/%s", BASE_URL, WORKFLOW_ID, jobId);
        listenToSse(sseUrl, token);
    }

    private static String login(String user, String pass) throws IOException, InterruptedException {
        String jsonBody = String.format(
            "{\"username\":\"%s\", \"password\":\"%s\", \"stayConnected\": false}", user, pass);

        HttpRequest request = HttpRequest.newBuilder()
            .uri(URI.create(BASE_URL + "/login"))
            .header("Content-Type", "application/json")
            .POST(HttpRequest.BodyPublishers.ofString(jsonBody))
            .timeout(Duration.ofSeconds(10))
            .build();

        HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());

        if (response.statusCode() != 200) {
            throw new RuntimeException("Login failed: HTTP " + response.statusCode());
        }

        JsonNode jsonNode = objectMapper.readTree(response.body());
        return jsonNode.get("access_token").asText();
    }

    private static String startWorkflow(String token, int workflowId) throws IOException, InterruptedException {
        HttpRequest request = HttpRequest.newBuilder()
            .uri(URI.create(BASE_URL + "/start?w=" + workflowId))
            .header("Authorization", "Bearer " + token)
            .POST(HttpRequest.BodyPublishers.noBody())
            .timeout(Duration.ofSeconds(10))
            .build();

        HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());

        if (response.statusCode() != 200) {
            throw new RuntimeException("Start workflow failed: HTTP " + response.statusCode());
        }

        // The start endpoint returns the jobId as JSON string, e.g. "jobid-uuid-string"
        return objectMapper.readTree(response.body()).asText();
    }

    private static void listenToSse(String sseUrl, String token) {
        OkHttpClient client = new OkHttpClient.Builder()
            .retryOnConnectionFailure(true)
            .build();

        Request request = new Request.Builder()
            .url(sseUrl)
            .addHeader("Authorization", "Bearer " + token)
            .build();

        EventSourceListener listener = new EventSourceListener() {
            @Override
            public void onOpen(EventSource eventSource, Response response) {
                System.out.println("SSE connection opened");
            }

            @Override
            public void onEvent(EventSource eventSource, String id, String type, String data) {
                System.out.println("Received event:");
                System.out.println("Type: " + type);
                System.out.println("Data: " + data);

                try {
                    JsonNode json = objectMapper.readTree(data);
                    System.out.println("Parsed JSON:");
                    System.out.println(objectMapper.writerWithDefaultPrettyPrinter().writeValueAsString(json));
                } catch (Exception e) {
                    System.err.println("Failed to parse SSE JSON: " + e.getMessage());
                }

                // Close connection if you want after first event:
                eventSource.cancel();
            }

            @Override
            public void onClosed(EventSource eventSource) {
                System.out.println("SSE connection closed");
            }

            @Override
            public void onFailure(EventSource eventSource, Throwable t, Response response) {
                System.err.println("SSE connection error: " + t.getMessage());
                if (response != null) {
                    System.err.println("Response code: " + response.code());
                }
            }
        };

        EventSource.Factory factory = EventSources.createFactory(client);
        factory.newEventSource(request, listener);

        // Prevent JVM from exiting immediately to keep SSE alive:
        try {
            Thread.sleep(10 * 60 * 1000);  // 10 minutes; adjust as needed
        } catch (InterruptedException ignored) {}
    }
}
