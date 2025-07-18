package com.wexflow;

import android.util.Base64;
import android.util.Log;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Dictionary;
import java.util.Hashtable;
import java.util.List;

class WexflowServiceClient {

    private static final int READ_TIMEOUT = 10000;
    private static final int CONNECTION_TIMEOUT = 15000;

    private final String uri;

    public static Dictionary<Integer, String> JOBS = new Hashtable<>();

    WexflowServiceClient(String uri) {
        this.uri = uri.replaceAll("/+$", "");
        preferIPv4Stack();
        disableAddressCache();
        disableKeepAlive();
    }

    // Simple method to extract access_token from JSON response
    private String parseAccessToken(String json) {
        // This is a naive parse, for production use a JSON library like Jackson or Gson
        String tokenKey = "\"access_token\":\"";
        int start = json.indexOf(tokenKey);
        if (start == -1) return null;
        start += tokenKey.length();
        int end = json.indexOf("\"", start);
        if (end == -1) return null;
        return json.substring(start, end);
    }

    public String login(String username, String password) throws Exception {
        HttpURLConnection conn = null;

        try {
            URL url = new URL(this.uri + "/login");
            conn = (HttpURLConnection) url.openConnection();
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setDoOutput(true);
            conn.setConnectTimeout(5000);
            conn.setReadTimeout(5000);

            String jsonInputString = String.format(
                    "{\"username\":\"%s\",\"password\":\"%s\",\"stayConnected\":true}",
                    username, password);

            try (OutputStream os = conn.getOutputStream()) {
                byte[] input = jsonInputString.getBytes(StandardCharsets.UTF_8);
                os.write(input);
            }

            int code = conn.getResponseCode();
            InputStream stream = (code == 200) ? conn.getInputStream() : conn.getErrorStream();

            BufferedReader br = new BufferedReader(
                    new InputStreamReader(stream, StandardCharsets.UTF_8));
            StringBuilder response = new StringBuilder();
            String line;

            while ((line = br.readLine()) != null) {
                response.append(line.trim());
            }

            String json = response.toString();
            Log.d("LoginResponse", json);

            if (code != 200) {
                throw new RuntimeException("Login failed: HTTP " + code + " - " + json);
            }

            // parse JSON
            JSONObject obj = new JSONObject(json);
            return obj.optString("access_token", null);

        } finally {
            if (conn != null) {
                conn.disconnect();
            }
        }
    }

    private static String post(String urlString, String token) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        String auth = "Bearer " + token;
        urlConnection.setRequestProperty("Authorization", auth);
        urlConnection.setRequestMethod("POST");
        urlConnection.setRequestProperty("Connection", "close");
        urlConnection.setUseCaches(false);
        urlConnection.setReadTimeout(READ_TIMEOUT);
        urlConnection.setConnectTimeout(CONNECTION_TIMEOUT);
        urlConnection.setDoOutput(true);
        urlConnection.setDoInput(true);
        urlConnection.connect();

        BufferedReader br = new BufferedReader(new InputStreamReader(urlConnection.getInputStream(), StandardCharsets.UTF_8));
        StringBuilder sb = new StringBuilder();
        String responseLine;
        while ((responseLine = br.readLine()) != null) {
            sb.append(responseLine.trim());
        }

        return sb.toString();
    }


    private static String getString(String urlString, String token) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        String auth = "Bearer " + token;
        urlConnection.setRequestProperty("Authorization", auth);
        urlConnection.setRequestMethod("GET");
        urlConnection.setUseCaches(false);
        urlConnection.setReadTimeout(READ_TIMEOUT);
        urlConnection.setConnectTimeout(CONNECTION_TIMEOUT);
        urlConnection.connect();

        BufferedInputStream in = new BufferedInputStream(urlConnection.getInputStream());
        byte[] contents = new byte[1024];

        int bytesRead;
        StringBuilder sb = new StringBuilder();
        while((bytesRead = in.read(contents)) != -1) {
            sb.append(new String(contents, 0, bytesRead));
        }

        return sb.toString();
    }

    private static JSONArray getJSONArray(String url, String token) throws IOException, JSONException {
        String json = getString(url, token);
        return new JSONArray(json);
    }

    private static JSONObject getJSONObject(String url, String token) throws IOException, JSONException {
        String json = getString(url, token);
        return new JSONObject(json);
    }

    private static void preferIPv4Stack() {
        System.setProperty("java.net.preferIPv4Stack", "true");
    }

    private static void disableKeepAlive() {
        System.setProperty("http.keepAlive.", "false");
    }

    private static void disableAddressCache() {
        System.setProperty("networkaddress.cache.ttl", "0");
        System.setProperty("networkaddress.cache.negative.ttl", "0");
    }

    List<Workflow> getWorkflows() throws IOException, JSONException {
        String uri = this.uri + "/search?s=";
        JSONArray jsonArray = getJSONArray(uri, LoginActivity.Token);
        List<Workflow> workflows = new ArrayList<>();
        for (int i = 0; i < jsonArray.length(); i++) {
            JSONObject jsonObject = jsonArray.getJSONObject(i);
            workflows.add(Workflow.fromJSONObject(jsonObject));
        }
        Collections.sort(workflows, new WorkflowComparator());
        return workflows;
    }

    Workflow getWorkflow(String token, int id) throws IOException, JSONException {
        String uri = this.uri + "/workflow?w=" + id;
        JSONObject jsonObject = getJSONObject(uri, token);
        return Workflow.fromJSONObject(jsonObject);
    }

    User getUser(String token, String username)throws IOException, JSONException {
        String uri = this.uri + "/user?username=" + username;
        JSONObject jsonObject = getJSONObject(uri, token);
        return User.fromJSONObject(jsonObject);
    }

    void start(int id) throws IOException {
        String uri = this.uri + "/start?w=" + id;
        String instanceId = post(uri, LoginActivity.Token);
        JOBS.put(id, instanceId.replace("\"", ""));
    }

    Boolean suspend(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/suspend?w=" + id + "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Token));
        }
        return false;
    }

    Boolean resume(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/resume?w=" + id+ "&i=" + instanceId;
            String response = post(uri, LoginActivity.Token);
            return response.isEmpty();
        }
        return false;
    }

    Boolean stop(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/stop?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Token));
        }
        return false;
    }

    Boolean approve(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/approve?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Token));
        }
        return false;
    }

    Boolean disapprove(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/reject?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Token));
        }
        return false;
    }
}
