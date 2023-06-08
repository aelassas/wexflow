package com.wexflow;


import android.os.Handler;
import android.os.Looper;
import android.util.Log;

class UpdateButtonsTask {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private Boolean force;

    UpdateButtonsTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }

    private Workflow doInBackground(Boolean force) {
        try {
            this.force = force;
            return this.client.getWorkflow(LoginActivity.Username, LoginActivity.Password, this.activity.getWorkflowId());
        } catch (Exception e) {
            this.exception = e;
            return null;
        }
    }


    private void onPostExecute(Workflow workflow) {
        if (this.exception != null) {
            Log.e("Wexflow", this.exception.toString());
        } else {
            if (!workflow.getEnabled()) {
                this.activity.getTxtInfo().setText(R.string.workflow_disabled);
                this.activity.getBtnStart().setEnabled(false);
                this.activity.getBtnSuspend().setEnabled(false);
                this.activity.getBtnResume().setEnabled(false);
                this.activity.getBtnStop().setEnabled(false);
            } else {
                if (!this.force && !this.activity.workflowStatusChanged(workflow)) return;

                this.activity.getBtnStart().setEnabled(!workflow.getRunning());
                this.activity.getBtnStop().setEnabled(workflow.getRunning() && !workflow.getPaused());
                this.activity.getBtnSuspend().setEnabled(workflow.getRunning() && !workflow.getPaused());
                this.activity.getBtnResume().setEnabled(workflow.getPaused());
                this.activity.getBtnApprove().setEnabled(workflow.getApproval() && workflow.getWaitingForApproval());
                this.activity.getBtnDisapprove().setEnabled(workflow.getApproval() && workflow.getWaitingForApproval());

                if (workflow.getApproval() && workflow.getWaitingForApproval() && !workflow.getPaused()) {
                    this.activity.getTxtInfo().setText(R.string.workflow_waiting_for_approval);
                } else {
                    if (workflow.getRunning() && !workflow.getPaused()) {
                        this.activity.getTxtInfo().setText(R.string.workflow_running);
                    } else if (workflow.getPaused()) {
                        this.activity.getTxtInfo().setText(R.string.workflow_suspended);
                    } else {
                        this.activity.getTxtInfo().setText("");
                    }
                }

            }
        }
    }

    void executeAsync() {
        final Handler handler = new Handler(Looper.getMainLooper());
        Thread thread = new Thread(() -> {
            final Workflow workflow = doInBackground(true);
            handler.post(() -> onPostExecute(workflow));
        });
        thread.start();
    }

    void execute(Handler handler) {
        final Workflow workflow = doInBackground(false);
        handler.post(() -> onPostExecute(workflow));
    }
}