package com.wexflow;

enum LaunchType {
    Startup,
    Trigger,
    Periodic,
    Cron;

    public static LaunchType fromInteger(int x) {
        switch (x) {
            case 0:
                return Startup;
            case 1:
                return Trigger;
            case 2:
                return Periodic;
            case 3:
                return Cron;
            default:
                return null;
        }
    }
}
