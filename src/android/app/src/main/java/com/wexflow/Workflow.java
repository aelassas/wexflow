package com.wexflow;

import org.json.JSONException;
import org.json.JSONObject;

class Workflow {
    private final LaunchType launchType;
    private final Boolean isEnabled;
    private final Boolean isApproval;
    private Boolean isWaitingForApproval;
    private final int id;
    private final String instanceId;
    private final String name;
    //private String description;
    private Boolean isRunning;
    private Boolean isPaused;

    private Workflow(int id, String instanceId, String name, LaunchType launchType, Boolean isEnabled, Boolean isApproval, Boolean isWaitingForApproval, Boolean isRunning, Boolean isPaused) {
        this.id = id;
        this.instanceId = instanceId;
        this.name = name;
        this.launchType = launchType;
        this.isEnabled = isEnabled;
        this.isApproval = isApproval;
        this.isWaitingForApproval = isWaitingForApproval;
        //this.description = description;
        this.isRunning = isRunning;
        this.isPaused = isPaused;
    }

    static Workflow fromJSONObject(JSONObject jsonObject) throws JSONException {
        return new Workflow(jsonObject.getInt("Id")
                , jsonObject.getString("InstanceId")
                , jsonObject.getString("Name")
                , LaunchType.fromInteger(jsonObject.getInt("LaunchType"))
                , jsonObject.getBoolean("IsEnabled")
                , jsonObject.getBoolean("IsApproval")
                , jsonObject.getBoolean("IsWaitingForApproval")
                //, jsonObject.getString("Description")
                , jsonObject.getBoolean("IsRunning")
                , jsonObject.getBoolean("IsPaused"));
    }

    int getId() {
        return id;
    }

    String getInstanceId() {
        return instanceId;
    }

    String getName() {
        return name;
    }

    LaunchType getLaunchType() {
        return launchType;
    }

    Boolean getEnabled() {
        return isEnabled;
    }

    Boolean getApproval() {
        return isApproval;
    }

    Boolean getWaitingForApproval() {
        return isWaitingForApproval;
    }

    void setWaitingForApproval(Boolean waitingForApproval) { isWaitingForApproval = waitingForApproval; }

    Boolean getRunning() {
        return isRunning;
    }

    void setRunning(Boolean running) {
        isRunning = running;
    }

    Boolean getPaused() {
        return isPaused;
    }

    void setPaused(Boolean paused) {
        isPaused = paused;
    }
}
