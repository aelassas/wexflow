package com.wexflow;

import java.util.Comparator;

class WorkflowComparator implements Comparator<Workflow> {

    @Override
    public int compare(Workflow o1, Workflow o2) {
        int id1 = o1.getId();
        int id2 = o2.getId();

        if (id1 > id2) {
            return 1;
        } else if (id1 < id2) {
            return -1;
        }

        return 0;
    }
}
