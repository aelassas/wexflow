package com.wexflow;

import org.json.JSONException;
import org.json.JSONObject;

public class User {
    private final String password;
    private final UserProfile userProfile;

    public User(String username, String password, UserProfile userProfile){
        this.password = password;
        this.userProfile = userProfile;
    }

    public String getPassword(){
        return this.password;
    }

    public UserProfile getUserProfile(){
        return this.userProfile;
    }

    static User fromJSONObject(JSONObject jsonObject) throws JSONException {
        return new User(
                  jsonObject.getString("Username")
                , jsonObject.getString("Password")
                , UserProfile.fromInteger(jsonObject.getInt("UserProfile"))
        );
    }

}
