package com.wexflow;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.Toast;

class ActionTask {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private ActionType actionType;
    private Boolean succeeded;

    ActionTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }

    private void doInBackground(ActionType actionType) {
        try {
            this.actionType = actionType;
            switch (this.actionType) {
                case Start:
                    this.client.start(this.activity.getWorkflowId());
                    succeeded = true;
                    break;
                case Suspend:
                    succeeded = this.client.suspend(this.activity.getWorkflowId());
                    break;
                case Resume:
                    succeeded = this.client.resume(this.activity.getWorkflowId());
                    break;
                case Stop:
                    succeeded = this.client.stop(this.activity.getWorkflowId());
                    break;
                case Approve:
                    succeeded = this.client.approve(this.activity.getWorkflowId());
                    break;
                case Disapprove:
                    succeeded = this.client.disapprove(this.activity.getWorkflowId());
                    break;
                default:
                    break;
            }
        } catch (Exception e) {
            this.exception = e;
        }
    }

    private void onPostExecute() {
        if (this.exception != null) {
            Log.e("Wexflow", this.exception.toString());
            Toast.makeText(this.activity.getBaseContext(), "An error occurred: " + this.exception.toString(), Toast.LENGTH_LONG).show();
        } else {
            if (this.actionType == ActionType.Suspend || this.actionType == ActionType.Stop) {
                UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(this.activity);
                updateButtonsTask.executeAsync();
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.append("Workflow ").append(this.activity.getWorkflowId()).append(" ");

            switch (this.actionType) {
                case Start:
                    stringBuilder.append("started.");
                    break;
                case Suspend:
                    stringBuilder.append("was suspended.");
                    break;
                case Resume:
                    stringBuilder.append("was resumed.");
                    break;
                case Stop:
                    stringBuilder.append("was stopped.");
                    break;
                case Approve:
                    stringBuilder.append("was approved.");
                    break;
                case Disapprove:
                    stringBuilder.append("was rejected.");
                    break;
                default:
                    break;
            }

            if (succeeded) {
                Toast.makeText(this.activity.getBaseContext(), stringBuilder.toString(), Toast.LENGTH_SHORT).show();
            } else {
                Toast.makeText(this.activity.getBaseContext(), "Not supported.", Toast.LENGTH_SHORT).show();
            }

        }
    }

    void execute(final ActionType at) {
        final Handler handler = new Handler(Looper.getMainLooper());

        Thread thread = new Thread(() -> {
            doInBackground(at);
            handler.post(this::onPostExecute);
        });
        thread.start();
    }
}
