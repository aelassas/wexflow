package com.wexflow;

import android.app.Activity;

import androidx.core.content.ContextCompat;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Objects;

import static com.wexflow.Constants.COL_ID;
import static com.wexflow.Constants.COL_LAUNCHTYPE;
import static com.wexflow.Constants.COL_NAME;

class ListViewAdapter extends BaseAdapter {
    private static final int NOT_SELECTED = -1;
    private final ArrayList<HashMap<String, String>> list;
    private final Activity activity;
    private int selectedPos = NOT_SELECTED;

    private TextView txtId;
    private TextView txtName;
    private TextView txtLaunchType;

    ListViewAdapter(Activity activity, ArrayList<HashMap<String, String>> list) {
        super();
        this.activity = activity;
        this.list = list;
    }

    // if called with the same position multiple lines it works as toggle
    void setSelection(int position) {
        if (selectedPos == position) {
            selectedPos = NOT_SELECTED;
        } else {
            selectedPos = position;
        }
        notifyDataSetChanged();
    }

    @Override
    public int getCount() {
        return list.size();
    }

    @Override
    public Object getItem(int position) {
        return list.get(position);
    }

    @Override
    @SuppressWarnings("unchecked")
    public long getItemId(int position) {
        return Long.parseLong(Objects.requireNonNull(((HashMap<String, String>) getItem(position)).get(COL_ID)));
    }

    @Override
    public int getViewTypeCount() {
        return getCount();
    }

    @Override
    public int getItemViewType(int position) {
        return position;
    }


    @Override
    public View getView(int position, View convertView, ViewGroup parent) {

        LayoutInflater inflater = activity.getLayoutInflater();

        if (convertView == null) {
            convertView = inflater.inflate(R.layout.row, parent, false);
            txtId = convertView.findViewById(R.id.txtId);
            txtName = convertView.findViewById(R.id.txtName);
            txtLaunchType = convertView.findViewById(R.id.txtLaunchType);
        }

        if (position == selectedPos) {
            txtId.setTextColor(ContextCompat.getColor(txtId.getContext(), R.color.list_row_selected_text));
            txtName.setTextColor(ContextCompat.getColor(txtName.getContext(), R.color.list_row_selected_text));
            txtLaunchType.setTextColor(ContextCompat.getColor(txtLaunchType.getContext(), R.color.list_row_selected_text));
            convertView.setBackgroundColor(ContextCompat.getColor(convertView.getContext(), R.color.list_row_selected_bg));
        } else {
            txtId.setTextColor(ContextCompat.getColor(txtId.getContext(), R.color.list_row_text));
            txtName.setTextColor(ContextCompat.getColor(txtName.getContext(), R.color.list_row_text));
            txtLaunchType.setTextColor(ContextCompat.getColor(txtLaunchType.getContext(), R.color.list_row_text));
            convertView.setBackgroundColor(ContextCompat.getColor(convertView.getContext(), R.color.list_row_default_bg));
        }

        HashMap<String, String> map = list.get(position);
        txtId.setText(map.get(COL_ID));
        txtName.setText(map.get(COL_NAME));
        txtLaunchType.setText(map.get(COL_LAUNCHTYPE));

        return convertView;
    }

}
