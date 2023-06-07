package com.wexflow;

import android.util.Base64;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
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

    private static String toBase64(String str) {
        byte[] data = str.getBytes(StandardCharsets.UTF_8);
        return Base64.encodeToString(data, Base64.DEFAULT);
    }

    private static String post(String urlString, String username, String password) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        String auth = "Basic " + toBase64(username + ":" + password).replace("\n","");
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


    private static String getString(String urlString, String username, String password) throws IOException {
        HttpURLConnection urlConnection;

        URL url = new URL(urlString);

        urlConnection = (HttpURLConnection) url.openConnection();
        String auth = "Basic " + toBase64(username + ":" + password).replace("\n","");
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

    private static JSONArray getJSONArray(String url, String username, String password) throws IOException, JSONException {
        String json = getString(url, username, password);
        return new JSONArray(json);
    }

    private static JSONObject getJSONObject(String url, String username, String password) throws IOException, JSONException {
        String json = getString(url, username, password);
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
        JSONArray jsonArray = getJSONArray(uri, LoginActivity.Username, LoginActivity.Password);
        List<Workflow> workflows = new ArrayList<>();
        for (int i = 0; i < jsonArray.length(); i++) {
            JSONObject jsonObject = jsonArray.getJSONObject(i);
            workflows.add(Workflow.fromJSONObject(jsonObject));
        }
        Collections.sort(workflows, new WorkflowComparator());
        return workflows;
    }

    Workflow getWorkflow(String username, String password, int id) throws IOException, JSONException {
        String uri = this.uri + "/workflow?w=" + id;
        JSONObject jsonObject = getJSONObject(uri, username, password);
        return Workflow.fromJSONObject(jsonObject);
    }

    User getUser(String qusername, String qpassword, String username)throws IOException, JSONException {
        String uri = this.uri + "/user?username=" + username;
        JSONObject jsonObject = getJSONObject(uri, qusername, qpassword);
        return User.fromJSONObject(jsonObject);
    }

    void start(int id) throws IOException {
        String uri = this.uri + "/start?w=" + id;
        String instanceId = post(uri, LoginActivity.Username, LoginActivity.Password);
        JOBS.put(id, instanceId.replace("\"", ""));
    }

    Boolean suspend(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/suspend?w=" + id + "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Username, LoginActivity.Password));
        }
        return false;
    }

    Boolean resume(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/resume?w=" + id+ "&i=" + instanceId;
            String response = post(uri, LoginActivity.Username, LoginActivity.Password);
            return response.isEmpty();
        }
        return false;
    }

    Boolean stop(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/stop?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Username, LoginActivity.Password));
        }
        return false;
    }

    Boolean approve(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/approve?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Username, LoginActivity.Password));
        }
        return false;
    }

    Boolean disapprove(int id) throws IOException {
        String instanceId = JOBS.get(id);
        if(instanceId != null) {
            String uri = this.uri + "/reject?w=" + id+ "&i=" + instanceId;
            return Boolean.valueOf(post(uri, LoginActivity.Username, LoginActivity.Password));
        }
        return false;
    }
}
