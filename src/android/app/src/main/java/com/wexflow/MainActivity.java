package com.wexflow;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.SparseArray;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ListView;
import android.widget.TextView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.preference.PreferenceManager;

public class MainActivity extends AppCompatActivity {

    //private static final int RESULT_SETTINGS = 1;

    private final ActivityResultLauncher<Intent> startActivityIntent = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                // Add same code that you want to add in onActivityResult method
            });

    private String uri;
    private ImageButton btnStart;
    private ImageButton btnSuspend;
    private ImageButton btnResume;
    private ImageButton btnStop;
    private TextView txtInfo;
    private ListView lvWorkflows;
    private Button btnApprove;
    private Button btnDisapprove;
    private int workflowId;
    private SparseArray<Workflow> workflows;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        this.workflows = new SparseArray<>();

        SharedPreferences sharedPref = PreferenceManager.getDefaultSharedPreferences(this);
        this.uri = sharedPref.getString(SettingsActivity.KEY_PREF_WEXFLOW_URI, this.getResources().getString(R.string.pref_wexflow_defualt_value));

        final MainActivity activity = this;
        this.btnStart = findViewById(R.id.btnStart);
        this.btnStart.setEnabled(false);
        this.btnStart.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Start);
        });

        this.btnSuspend = findViewById(R.id.btnSuspend);
        this.btnSuspend.setEnabled(false);
        this.btnSuspend.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Suspend);
        });

        this.btnResume = findViewById(R.id.btnResume);
        this.btnResume.setEnabled(false);
        this.btnResume.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Resume);
        });

        this.btnStop = findViewById(R.id.btnStop);
        this.btnStop.setEnabled(false);
        this.btnStop.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Stop);
        });

        this.txtInfo = findViewById(R.id.txtInfo);

        this.lvWorkflows = findViewById(R.id.lvWorkflows);

        Button btnRefresh = findViewById(R.id.btnRefresh);
        btnRefresh.setOnClickListener(view -> loadWorkflows());

        this.btnApprove = findViewById(R.id.btnApprove);
        this.btnApprove.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Approve);
        });

        this.btnDisapprove = findViewById(R.id.btnDisapprove);
        this.btnDisapprove.setOnClickListener(v -> {
            ActionTask actionTask = new ActionTask(activity);
            actionTask.execute(ActionType.Disapprove);
        });

        loadWorkflows();
    }

    private void loadWorkflows() {
        this.txtInfo.setText("");
        WorkflowsTask workflowsTask = new WorkflowsTask(this);
        workflowsTask.execute();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.settings, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (item.getItemId() == R.id.menu_settings) {
            Intent i = new Intent(this, SettingsActivity.class);
            //startActivityForResult(i, RESULT_SETTINGS);
            startActivityIntent.launch(i);
        }

        return true;
    }

    public Boolean workflowStatusChanged(Workflow workflow) {
        Boolean changed = this.workflows.get(workflow.getId()).getRunning() != workflow.getRunning() || this.workflows.get(workflow.getId()).getPaused() != workflow.getPaused() || this.workflows.get(workflow.getId()).getWaitingForApproval() != workflow.getWaitingForApproval();
        this.workflows.get(workflow.getId()).setRunning(workflow.getRunning());
        this.workflows.get(workflow.getId()).setPaused(workflow.getPaused());
        this.workflows.get(workflow.getId()).setWaitingForApproval(workflow.getWaitingForApproval());
        return changed;
    }

    public ImageButton getBtnStart() {
        return btnStart;
    }

    public ImageButton getBtnSuspend() {
        return btnSuspend;
    }

    public ImageButton getBtnResume() {
        return btnResume;
    }

    public ImageButton getBtnStop() {
        return btnStop;
    }

    public Button getBtnApprove() {
        return btnApprove;
    }

    public Button getBtnDisapprove() {
        return btnDisapprove;
    }

    public TextView getTxtInfo() {
        return txtInfo;
    }

    public ListView getLvWorkflows() {
        return lvWorkflows;
    }

    public int getWorkflowId() {
        return workflowId;
    }

    public void setWorkflowId(int workflowId) {
        this.workflowId = workflowId;
    }

    public String getUri() {
        return uri;
    }

    public SparseArray<Workflow> getWorkflows() {
        return workflows;
    }

}
