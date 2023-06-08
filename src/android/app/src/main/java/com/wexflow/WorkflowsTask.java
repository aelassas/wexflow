package com.wexflow;

import static com.wexflow.Constants.COL_ID;
import static com.wexflow.Constants.COL_LAUNCHTYPE;
import static com.wexflow.Constants.COL_NAME;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.ListView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

class WorkflowsTask {

    private final MainActivity activity;
    private final WexflowServiceClient client;
    private Exception exception;
    private Timer timer;

    WorkflowsTask(MainActivity activity) {
        this.activity = activity;
        this.client = new WexflowServiceClient(activity.getUri());
    }

    private List<Workflow> doInBackground() {
        try {
            return this.client.getWorkflows();
        } catch (Exception e) {
            this.exception = e;
            return null;
        }
    }

    private void onPostExecute(List<Workflow> workflows) {
        if (this.exception != null) {
            Log.e("Wexflow", exception.toString());
            Toast.makeText(this.activity.getBaseContext(), R.string.workflows_error, Toast.LENGTH_LONG).show();
        } else {
            ArrayList<HashMap<String, String>> list = new ArrayList<>();

            for (Workflow workflow : workflows) {
                HashMap<String, String> temp = new HashMap<>();
                temp.put(COL_ID, String.valueOf(workflow.getId()));
                temp.put(COL_NAME, workflow.getName());
                temp.put(COL_LAUNCHTYPE, String.valueOf(workflow.getLaunchType()));
                list.add(temp);
                this.activity.getWorkflows().put(workflow.getId(), workflow);
            }

            final ListViewAdapter adapter = new ListViewAdapter(this.activity, list);
            ListView lvWorkflows = this.activity.getLvWorkflows();
            lvWorkflows.setAdapter(adapter);

            lvWorkflows.setOnItemClickListener((parent, view, position, id) -> {
                adapter.setSelection(position);
                activity.setWorkflowId((int) id);

                if (timer != null) {
                    timer.cancel();
                    timer.purge();
                }

                Workflow workflow = activity.getWorkflows().get((int) id);
                if (workflow.getEnabled()) {
                    timer = new Timer();
                    final Handler handler = new Handler(Looper.getMainLooper());
                    timer.schedule(new TimerTask() {
                        @Override
                        public void run() {
                            UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                            updateButtonsTask.execute(handler);
                        }
                    }, 0, 500);
                    UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                    updateButtonsTask.executeAsync();
                } else {
                    UpdateButtonsTask updateButtonsTask = new UpdateButtonsTask(activity);
                    updateButtonsTask.executeAsync();
                }

                if (workflow.getRunning()) {
                    WexflowServiceClient.JOBS.put(workflow.getId(), workflow.getInstanceId());
                }

            });
        }
    }

    void execute() {
        final Handler handler = new Handler(Looper.getMainLooper());
        Thread thread = new Thread(() -> {
            final List<Workflow> workflows = doInBackground();
            handler.post(() -> onPostExecute(workflows));
        });
        thread.start();
    }
}
