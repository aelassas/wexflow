package com.wexflow;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v7.app.AppCompatActivity;
import android.util.SparseArray;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ListView;
import android.widget.TextView;

public class MainActivity extends AppCompatActivity {

    private static final int RESULT_SETTINGS = 1;

    private String uri;
    private ImageButton btnStart;
    private ImageButton btnSuspend;
    private ImageButton btnResume;
    private ImageButton btnStop;
    private TextView txtInfo;
    private ListView lvWorkflows;
    private Button btnRefresh;
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
        this.btnStart.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Start);
            }
        });

        this.btnSuspend = findViewById(R.id.btnSuspend);
        this.btnSuspend.setEnabled(false);
        this.btnSuspend.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Suspend);
            }
        });

        this.btnResume = findViewById(R.id.btnResume);
        this.btnResume.setEnabled(false);
        this.btnResume.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Resume);
            }
        });

        this.btnStop = findViewById(R.id.btnStop);
        this.btnStop.setEnabled(false);
        this.btnStop.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Stop);
            }
        });

        this.txtInfo = findViewById(R.id.txtInfo);

        this.lvWorkflows = findViewById(R.id.lvWorkflows);

        this.btnRefresh = findViewById(R.id.btnRefresh);
        this.btnRefresh.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                loadWorkflows();
            }
        });

        this.btnApprove = findViewById(R.id.btnApprove);
        this.btnApprove.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Approve);
            }
        });

        this.btnDisapprove = findViewById(R.id.btnDisapprove);
        this.btnDisapprove.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                ActionTask actionTask = new ActionTask(activity);
                actionTask.execute(ActionType.Disapprove);
            }
        });

        loadWorkflows();
    }

    private void loadWorkflows(){
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
        switch (item.getItemId()) {
            case R.id.menu_settings:
                Intent i = new Intent(this, SettingsActivity.class);
                startActivityForResult(i, RESULT_SETTINGS);
                break;
            default:
                break;
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
