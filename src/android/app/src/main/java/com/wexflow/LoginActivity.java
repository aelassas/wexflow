package com.wexflow;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.inputmethod.EditorInfo;
import android.widget.AutoCompleteTextView;
import android.widget.Button;
import android.widget.EditText;

import androidx.appcompat.app.AppCompatActivity;
import androidx.preference.PreferenceManager;

import org.json.JSONException;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * A login screen that offers login via email/password.
 */
public class LoginActivity extends AppCompatActivity {

    public static String Username = "";
    public static String Password = "";

    private SharedPreferences sharedPref;

    // UI references.
    private AutoCompleteTextView mUsernameView;
    private EditText mPasswordView;

    private final ExecutorService executor = Executors.newSingleThreadExecutor();
    private final Handler handler = new Handler(Looper.getMainLooper());

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        sharedPref = PreferenceManager.getDefaultSharedPreferences(this);

        // Set up the login form.
        mUsernameView = findViewById(R.id.username);

        mPasswordView = findViewById(R.id.password);
        mPasswordView.setOnEditorActionListener((textView, id, keyEvent) -> {
            if (id == EditorInfo.IME_ACTION_DONE || id == EditorInfo.IME_NULL) {
                login();
                return true;
            }
            return false;
        });

        Button mEmailSignInButton = findViewById(R.id.email_sign_in_button);
        mEmailSignInButton.setOnClickListener(view -> login());

        Button mSettingsButton = findViewById(R.id.settings);
        mSettingsButton.setOnClickListener(view -> {
            Intent intent = new Intent(LoginActivity.this, SettingsActivity.class);
            LoginActivity.this.startActivity(intent);
        });

    }

    private void login() {
        String username = mUsernameView.getText().toString();
        String password = mPasswordView.getText().toString();

        if (username.isEmpty()) {
            mUsernameView.setError(getString(R.string.error_field_required));
        }

        if (password.isEmpty()) {
            mPasswordView.setError(getString(R.string.error_field_required));
        }

        if (!username.isEmpty() && !password.isEmpty()) {
            Username = username;
            Password = md5(password);
            UserLoginTask task = new UserLoginTask(username, password);
            executor.execute(() -> {
                Boolean success = task.doInBackground();
                handler.post(() -> task.onPostExecute(success));
            });
        }
    }

    private String md5(final String s) {
        final String MD5 = "MD5";
        try {
            // Create MD5 Hash
            MessageDigest digest = java.security.MessageDigest.getInstance(MD5);
            digest.update(s.getBytes());
            byte[] messageDigest = digest.digest();

            // Create Hex String
            StringBuilder hexString = new StringBuilder();
            for (byte aMessageDigest : messageDigest) {
                StringBuilder h = new StringBuilder(Integer.toHexString(0xFF & aMessageDigest));
                while (h.length() < 2) {
                    h.insert(0, "0");
                }
                hexString.append(h);
            }
            return hexString.toString();

        } catch (NoSuchAlgorithmException e) {
            e.printStackTrace();
        }
        return "";
    }

    /**
     * Represents an asynchronous login task used to authenticate
     * the user.
     */
    public class UserLoginTask {

        private final String mUsername;
        private final String mPassword;
        private Boolean restrictedAccess;
        private Boolean errorOccurred;
        private Boolean userNotFound;

        UserLoginTask(String username, String password) {
            mUsername = username;
            mPassword = password;
            restrictedAccess = false;
            errorOccurred = false;
            userNotFound = false;
        }

        protected Boolean doInBackground() {

            try {
                String uri = sharedPref.getString(SettingsActivity.KEY_PREF_WEXFLOW_URI, getResources().getString(R.string.pref_wexflow_defualt_value));
                WexflowServiceClient client = new WexflowServiceClient(uri);
                String passwordHash = md5(this.mPassword);
                User user = client.getUser(mUsername, passwordHash, mUsername);

                String password = user.getPassword();
                UserProfile up = user.getUserProfile();

                if ((up.equals(UserProfile.SuperAdministrator) || up.equals(UserProfile.Administrator)) && password.equals(passwordHash)) {
                    return true;
                } else {
                    if (up.equals(UserProfile.Restricted)) {
                        restrictedAccess = true;
                    }

                    return false;
                }

            } catch (JSONException e) {
                userNotFound = true;
                return false;
            } catch (Exception e) {
                errorOccurred = true;
                return false;
            }

        }

        protected void onPostExecute(final Boolean success) {
            if (success) {
                Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                LoginActivity.this.startActivity(intent);
            } else {

                if (userNotFound) {
                    mUsernameView.setError(getString(R.string.error_user_not_found));
                    mUsernameView.requestFocus();
                } else {
                    if (errorOccurred) {
                        mPasswordView.setError(getString(R.string.error_exception));
                    } else {
                        if (restrictedAccess)
                            mPasswordView.setError(getString(R.string.error_restricted_access));
                        else {
                            mPasswordView.setError(getString(R.string.error_incorrect_password));
                        }
                    }
                    mPasswordView.requestFocus();
                }
            }
        }


    }
}

