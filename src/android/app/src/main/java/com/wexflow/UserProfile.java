package com.wexflow;

enum UserProfile {
    SuperAdministrator,
    Administrator,
    Restricted;

    public static UserProfile fromInteger(int x) {
        switch (x) {
            case 0:
                return SuperAdministrator;
            case 1:
                return Administrator;
            case 2:
                return Restricted;
            default:
                return null;
        }
    }
}
