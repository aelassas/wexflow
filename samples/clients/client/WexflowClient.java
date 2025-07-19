import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;

public class WexflowClient {

    private static final String BASE_URL = "http://localhost:8000/api/v1";
    private static final String USERNAME = "admin";
    private static final String PASSWORD = "wexflow2018";
    private static final int WORKFLOW_ID = 41;

    public static void main(String[] args) {
        try {
            String token = login(USERNAME, PASSWORD);
            String jobId = startWorkflow(token, WORKFLOW_ID);
            System.out.println("Workflow " + WORKFLOW_ID + " started successfully. Job ID: " + jobId);
        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }
    }

    private static String login(String username, String password) throws Exception {
        URL url = new URL(BASE_URL + "/login");
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();

        conn.setRequestMethod("POST");
        conn.setRequestProperty("Content-Type", "application/json");
        conn.setDoOutput(true);

        String jsonInputString = String.format(
            "{\"username\":\"%s\",\"password\":\"%s\",\"stayConnected\":false}",
            username, password);

        try (OutputStream os = conn.getOutputStream()) {
            byte[] input = jsonInputString.getBytes("utf-8");
            os.write(input);
        }

        int code = conn.getResponseCode();
        if (code != 200) {
            throw new RuntimeException("Login failed: HTTP " + code);
        }

        BufferedReader br = new BufferedReader(
            new InputStreamReader(conn.getInputStream(), "utf-8"));

        StringBuilder response = new StringBuilder();
        String responseLine;
        while ((responseLine = br.readLine()) != null) {
            response.append(responseLine.trim());
        }

        // Response JSON format: { "access_token": "..." }
        String json = response.toString();
        String token = parseAccessToken(json);
        if (token == null) {
            throw new RuntimeException("No access_token found in response");
        }
        return token;
    }

    private static String startWorkflow(String token, int workflowId) throws Exception {
        URL url = new URL(BASE_URL + "/start?w=" + workflowId);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();

        conn.setRequestMethod("POST");
        conn.setRequestProperty("Authorization", "Bearer " + token);
        conn.setDoOutput(true);

        int code = conn.getResponseCode();
        if (code != 200) {
            throw new RuntimeException("Start workflow failed: HTTP " + code);
        }

        BufferedReader br = new BufferedReader(
            new InputStreamReader(conn.getInputStream(), "utf-8"));

        StringBuilder response = new StringBuilder();
        String responseLine;
        while ((responseLine = br.readLine()) != null) {
            response.append(responseLine.trim());
        }

        return response.toString();
    }

    // Simple method to extract access_token from JSON response
    private static String parseAccessToken(String json) {
        // This is a naive parse, for production use a JSON library like Jackson or Gson
        String tokenKey = "\"access_token\":\"";
        int start = json.indexOf(tokenKey);
        if (start == -1) return null;
        start += tokenKey.length();
        int end = json.indexOf("\"", start);
        if (end == -1) return null;
        return json.substring(start, end);
    }
}
