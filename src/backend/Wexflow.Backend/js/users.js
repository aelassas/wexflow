window.Users = function () {
    "use strict";

    let updateLanguage = function (language) {
        document.getElementById("lnk-records").innerHTML = language.get("lnk-records");
        document.getElementById("lnk-approval").innerHTML = language.get("lnk-approval");

        document.getElementById("lnk-dashboard").innerHTML = language.get("lnk-dashboard");
        document.getElementById("lnk-manager").innerHTML = language.get("lnk-manager");
        document.getElementById("lnk-designer").innerHTML = language.get("lnk-designer");
        document.getElementById("lnk-history").innerHTML = language.get("lnk-history");
        document.getElementById("lnk-users").innerHTML = language.get("lnk-users");
        document.getElementById("lnk-profiles").innerHTML = language.get("lnk-profiles");
        document.getElementById("spn-logout").innerHTML = language.get("spn-logout");

        document.getElementById("users-search-action").value = language.get("btn-search");
        document.getElementById("new-user-action").value = language.get("new-user-action");
        document.getElementById("save-action").value = language.get("save-action");
        document.getElementById("delete-action").value = language.get("delete-action");
        document.getElementById("tr-createdOn-label").innerHTML = language.get("tr-createdOn-label");
        document.getElementById("tr-modifiedOn-label").innerHTML = language.get("tr-modifiedOn-label");
        document.getElementById("username-text-label").innerHTML = language.get("username");
        document.getElementById("userprofile-slct-label").innerHTML = language.get("userprofile-slct-label");
        document.getElementById("email-text-label").innerHTML = language.get("email-text-label");
        document.getElementById("change-password").innerHTML = language.get("change-password");
        document.getElementById("old-password-text-label").innerHTML = language.get("old-password-text-label");
        document.getElementById("lbl-new-password").innerHTML = language.get("lbl-new-password");
        document.getElementById("confirm-password-text-label").innerHTML = language.get("confirm-password-text-label");
    };

    let language = new window.Language("lang", updateLanguage);
    language.init();

    let uri = window.Common.trimEnd(window.Settings.Uri, "/");

    let lnkRecords = document.getElementById("lnk-records");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
    let lnkNotifications = document.getElementById("lnk-notifications");
    let imgNotifications = document.getElementById("img-notifications");

    let divUsers = document.getElementById("users");
    let divUsersTable = document.getElementById("users-table");
    let divUserActions = document.getElementById("user-actions");
    let divUserProfile = document.getElementById("user-profile");
    let slctProfile = document.getElementById("slct-profile");
    let oldPasswordText = document.getElementById("old-password-text");
    let newPasswordText = document.getElementById("new-password-text");
    let confirmPasswordText = document.getElementById("confirm-password-text");
    let deleteAction = document.getElementById("delete-action");
    let saveAction = document.getElementById("save-action");
    let newUserAction = document.getElementById("new-user-action");
    let btnLogout = document.getElementById("btn-logout");
    let oldPasswordTr = document.getElementById("old-password-tr");
    let newPasswordTr = document.getElementById("new-password-tr");
    let confirmPasswordTr = document.getElementById("confirm-password-tr");
    let lblNewPassword = document.getElementById("lbl-new-password");
    let txtUsername = document.getElementById("username-text");
    let changePass = document.getElementById("change-password");
    let emailText = document.getElementById("email-text");
    let txtId = document.getElementById("txt-id");
    let txtCreatedOn = document.getElementById("txt-createdOn");
    let txtModifiedOn = document.getElementById("txt-modifiedOn");
    let trId = document.getElementById("tr-id");
    let trCreatedOn = document.getElementById("tr-createdOn");
    let trModifiedOn = document.getElementById("tr-modifiedOn");
    let txtSearch = document.getElementById("users-search-text");
    let btnSearch = document.getElementById("users-search-action");
    let selectedUsername;
    let selectedUsernameTd;
    let selectedUserId;
    let selectedUserProfile;
    let selectedUserProfileTd;
    let logedinUser;
    let logedinUserProfile;
    let newUser = false;
    let changePassword = false;
    let selectedTr;
    let uo = 0;
    let thUsername;
    let qusername = "";
    let qpassword = "";
    let auth = "";

    let suser = getUser();

    if (suser === null || suser === "") {
        window.Common.redirectToLoginPage();
    } else {
        let user = JSON.parse(suser);

        qusername = user.Username;
        qpassword = user.Password;
        auth = "Basic " + btoa(qusername + ":" + qpassword);

        window.Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (!u || user.Password !== u.Password) {
                    window.Common.redirectToLoginPage();
                } else if (u.UserProfile === 0 || u.UserProfile === 1) {

                    window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {

                        logedinUser = u.Username;
                        logedinUserProfile = u.UserProfile;

                        divUsers.style.display = "block";
                        lnkRecords.style.display = "inline";
                        lnkManager.style.display = "inline";
                        lnkDesigner.style.display = "inline";
                        lnkApproval.style.display = "inline";
                        lnkUsers.style.display = "inline";
                        lnkNotifications.style.display = "inline";

                        if (u.UserProfile === 0) {
                            lnkProfiles.style.display = "inline";
                        }

                        if (hasNotifications === true) {
                            imgNotifications.src = "images/notification-active.png";
                        } else {
                            imgNotifications.src = "images/notification.png";
                        }

                        document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                        btnLogout.onclick = function () {
                            window.deleteUser();
                            window.Common.redirectToLoginPage();
                        };

                        if (u.UserProfile === 1) {
                            newUserAction.style.display = "none";
                        }

                        loadUsers();

                    }, function () { }, auth);
                } else {
                    window.Common.redirectToLoginPage();
                }

            }, function () {
                window.logout();
            }, auth);
    }

    btnSearch.onclick = function () {
        loadUsers();
    };

    txtSearch.onkeyup = function (e) {
        e.preventDefault();
        if (e.keyCode === 13) {
            loadUsers();
        }
    };

    function loadUsers(usernameToSelect, scroll) {
        window.Common.get(uri + "/search-users?keyword=" + encodeURIComponent(txtSearch.value) + "&uo=" + uo,
            function (data) {

                let items = [];
                for (let i = 0; i < data.length; i++) {
                    let val = data[i];
                    let tr;

                    if (usernameToSelect === val.Username) {
                        tr = "<tr class='selected'>";
                    } else {
                        tr = "<tr>";
                    }

                    items.push(
                        tr
                        + "<td class='userid'>" + val.Id + "</td>"
                        + "<td class='username'>" + val.Username + "</td>"
                        + "<td class='userprofile'>" + userProfileToText(val.UserProfile) + "</td>"
                        + "</tr>"
                    );
                }

                let table = "<table id='wf-users-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th id='th-id' class='userid'>Id</th>"
                    + "<th id='th-username' class='username'>Username&nbsp;&nbsp;🔺</th>"
                    + "<th class='userprofile'>Profile</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                divUsersTable.innerHTML = table;

                let usersTable = document.getElementById("wf-users-table");

                usersTable.getElementsByTagName("tbody")[0].style.height = (divUsersTable.offsetHeight - 35) + "px";

                let rows = usersTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                if (rows.length > 0) {
                    let hrow = usersTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                    hrow.querySelector(".username").style.width = rows[0].querySelector(".username").offsetWidth + "px";
                    hrow.querySelector(".userprofile").style.width = rows[0].querySelector(".userprofile").offsetWidth + "px";
                }

                let profiles = usersTable.querySelectorAll(".userprofile");
                for (let i = 0; i < profiles.length; i++) {
                    profiles[i].style.width = usersTable.offsetWidth - 60 + "px";
                }

                thUsername = document.getElementById("th-username");
                thUsername.onclick = function () {
                    if (uo === 0) {
                        uo = 1;
                    } else {
                        uo = 0;
                    }
                    loadUsers(selectedUsername, true);
                };

                if (uo === 0) {
                    thUsername.innerHTML = "Username&nbsp;&nbsp;🔺";
                } else {
                    thUsername.innerHTML = "Username&nbsp;&nbsp;🔻";
                }

                // set selectedUsernameTd
                let selected = document.getElementsByClassName("selected");
                if (selected.length > 0) {
                    selectedTr = selected[0];
                    selectedUsernameTd = selectedTr.getElementsByClassName("username")[0];
                    selectedUserProfileTd = selectedTr.getElementsByClassName("userprofile")[0];
                }

                for (let j = 0; j < rows.length; j++) {

                    let row = rows[j];
                    if (scroll === true) {
                        let userIdTd = row.getElementsByClassName("userid")[0];

                        if (typeof userIdTd !== "undefined" && userIdTd !== null) {
                            let userId = userIdTd.innerHTML;
                            if (userId === selectedUserId) {
                                row.scrollIntoView(true);
                            }

                        }
                    }

                    row.onclick = function () {
                        let selected = document.getElementsByClassName("selected");
                        if (selected.length > 0) {
                            selectedTr = selected[0];
                            selectedTr.className = selectedTr.className.replace("selected", "");
                        }

                        this.className += "selected";
                        selectedTr = this;

                        selectedUsernameTd = this.getElementsByClassName("username")[0];
                        selectedUserProfileTd = this.getElementsByClassName("userprofile")[0];
                        selectedUsername = selectedUsernameTd.innerHTML;
                        selectedUserId = this.getElementsByClassName("userid")[0].innerHTML;
                        selectedUserProfile = userProfileToInt(this.getElementsByClassName("userprofile")[0].innerHTML);

                        loadRightPanel();

                        oldPasswordTr.style.display = "none";
                        newPasswordTr.style.display = "none";
                        confirmPasswordTr.style.display = "none";
                        changePass.style.display = "block";

                        //if (selectedUsername !== logedinUser && selectedUserProfile === 0) {
                        if ((selectedUsername !== logedinUser && selectedUserProfile === 0) || (logedinUserProfile === 1 && selectedUsername !== logedinUser && selectedUserProfile === 1)) {
                            //newPasswordTr.style.display = "table-row";
                            //lblNewPassword.innerHTML = "Password";
                            changePass.style.display = "none";
                        } else {
                            lblNewPassword.innerHTML = language.get("lbl-new-password");
                        }

                        newUser = false;
                        changePassword = false;
                    };
                }

            },
            function () { }, auth);
    }

    function userProfileToText(userProfile) {
        switch (userProfile) {
            case 0:
                return "SuperAdministrator";
            case 1:
                return "Administrator";
            case 2:
                return "Restricted";
            default:
                return "Unknown";
        }
    }

    function userProfileToInt(userProfile) {
        switch (userProfile) {
            case "SuperAdministrator":
                return 0;
            case "Administrator":
                return 1;
            case "Restricted":
                return 2;
            default:
                return -1;
        }
    }

    function loadRightPanel() {
        divUserActions.style.display = "block";
        divUserProfile.style.display = "block";

        if (selectedUsername === logedinUser) {
            deleteAction.style.display = "none";
            slctProfile.value = 0;
            slctProfile.disabled = true;
        } else {
            oldPasswordTr.style.display = "none";
        }

        window.Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
            function (u) {
                txtId.value = u.Id;
                //txtCreatedOn.value = window.Common.formatDate(new Date(u.CreatedOn));
                txtCreatedOn.value = u.CreatedOn;

                //if (u.ModifiedOn === -62135596800000) {
                if (u.ModifiedOn.indexOf("0001") > -1) {
                    txtModifiedOn.value = "-";
                } else {
                    //txtModifiedOn.value = window.Common.formatDate(new Date(u.ModifiedOn));
                    txtModifiedOn.value = u.ModifiedOn;
                }

                txtUsername.value = u.Username;
                slctProfile.value = u.UserProfile;
                emailText.value = u.Email;
                oldPasswordText.value = "";
                newPasswordText.value = "";
                confirmPasswordText.value = "";
                trId.style.display = "table-row";
                trCreatedOn.style.display = "table-row";
                trModifiedOn.style.display = "table-row";

                if (u.UserProfile === 2) {
                    slctProfile.disabled = false;
                    deleteAction.style.display = "block";
                    oldPasswordTr.style.display = "none";
                }

                if (u.Username === logedinUser || (u.Username !== logedinUser && u.UserProfile === 0)) {
                    oldPasswordTr.style.display = "";
                    slctProfile.disabled = true;
                    deleteAction.style.display = "none";
                }

                if (u.Username !== logedinUser && u.UserProfile === 0) {
                    txtUsername.disabled = true;
                    emailText.disabled = true;
                    saveAction.style.display = "none";
                } if (u.Username === logedinUser && u.UserProfile === 0) {
                    saveAction.style.display = "block";
                    emailText.disabled = false;
                } else if (logedinUserProfile === 0 && u.Username !== logedinUser && (u.UserProfile === 1 || u.UserProfile === 2)) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    slctProfile.disabled = false;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "block";
                } else if (u.Username === logedinUser && u.UserProfile === 1) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "none";

                } else if (u.UserProfile === 2) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    slctProfile.disabled = true;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "block";
                } else {
                    txtUsername.disabled = true;
                    emailText.disabled = true;
                    slctProfile.disabled = true;
                    saveAction.style.display = "none";
                    deleteAction.style.display = "none";
                }

            },
            function () { }, auth);
    }

    newUserAction.onclick = function () {
        divUserActions.style.display = "block";
        divUserProfile.style.display = "block";
        deleteAction.style.display = "none";

        txtId.value = "";
        txtCreatedOn.value = "";
        txtModifiedOn.value = "";
        trId.style.display = "none";
        trCreatedOn.style.display = "none";
        trModifiedOn.style.display = "none";

        txtUsername.disabled = false;
        oldPasswordTr.style.display = "none";
        lblNewPassword.innerHTML = language.get("password");
        txtUsername.value = "";
        slctProfile.disabled = false;
        slctProfile.value = -1;
        newPasswordText.value = "";
        oldPasswordText.value = "";
        confirmPasswordText.value = "";
        emailText.disabled = false;
        emailText.value = "";

        newPasswordTr.style.display = "table-row";
        confirmPasswordTr.style.display = "table-row";
        changePass.style.display = "none";

        saveAction.style.display = "block";

        if (typeof selectedTr !== "undefined") {
            selectedTr.className = selectedTr.className.replace("selected", "");
        }

        newUser = true;
    };

    changePass.onclick = function () {
        if (selectedUserProfile === 0) {
            oldPasswordTr.style.display = "table-row";
        }
        newPasswordTr.style.display = "table-row";
        confirmPasswordTr.style.display = "table-row";

        changePass.style.display = "none";
        changePassword = true;
    };

    deleteAction.onclick = function () {
        let r = confirm(language.get("confirm-user-delete"));

        if (r === true) {
            if (selectedUsername !== logedinUser) {

                window.Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
                    function (u) {
                        window.Common.post(uri + "/delete-user?username=" + encodeURIComponent(selectedUsername) + "&password=" + encodeURIComponent(u.Password),
                            function (val) {
                                if (val === true) {
                                    window.Common.toastSuccess(language.get("toast-user-deleted"));
                                    loadUsers();
                                    divUserActions.style.display = "none";
                                    divUserProfile.style.display = "none";
                                } else {
                                    window.Common.toastError(language.get("toast-user-delete-error"));
                                }
                            },
                            function () { }, "", auth);
                    },
                    function () { }, auth);
            }
        }

    };

    //confirmPasswordText.onkeyup = function(e) {
    //    e.preventDefault();

    //    if (e.keyCode === 13) {
    //        save();
    //    }

    //};

    saveAction.onclick = function () {
        save();
    };


    function save() {
        if (newUser === true) {

            let username = txtUsername.value;
            let up = parseInt(getSelectedProfile());
            let password = newPasswordText.value;
            let confirmedPassword = confirmPasswordText.value;

            if (username === "") {
                window.Common.toastInfo(language.get("toast-username"));
            } else {
                window.Common.get(uri + "/user?username=" + encodeURIComponent(username),
                    function (u) {
                        if (typeof u === "undefined") {
                            if (up === -1) {
                                window.Common.toastInfo(language.get("toast-userprofile"));
                            } else {
                                if (password === "" || confirmedPassword === "") {
                                    window.Common.toastInfo(language.get("toast-password"));
                                } else {
                                    if (password !== confirmedPassword) {
                                        window.Common.toastInfo(language.get("toast-password-error"));
                                    } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                                        window.Common.toastInfo(language.get("toast-email-error"));
                                    } else {
                                        let hashedPass = window.MD5(password);
                                        window.Common.post(
                                            uri + "/insert-user?username=" + encodeURIComponent(username) + "&password=" + hashedPass + "&up=" + up + "&email=" + encodeURIComponent(emailText.value),
                                            function (val) {
                                                if (val === true) {
                                                    window.Common.toastSuccess(language.get("toast-user-created"));

                                                    window.Common.get(uri + "/user?username=" + encodeURIComponent(username),
                                                        function (user) {

                                                            selectedUserId = user.Id;
                                                            selectedUsername = user.Username;
                                                            newUser = false;

                                                            loadUsers(username, true);

                                                            if (user.UserProfile === 1 || user.UserProfile == 2) {
                                                                deleteAction.style.display = "block";

                                                            } else if (user.UserProfile === 0) {
                                                                slctProfile.disabled = true;
                                                                saveAction.style.display = "none";
                                                                txtUsername.disabled = true;
                                                                emailText.disabled = true;
                                                                changePass.style.display = "none";
                                                                newPasswordTr.style.display = "none";
                                                                confirmPasswordTr.style.display = "none";
                                                            }

                                                            trId.style.display = "table-row";
                                                            trCreatedOn.style.display = "table-row";
                                                            trModifiedOn.style.display = "table-row";
                                                            txtId.value = user.Id;
                                                            //txtCreatedOn.value = window.Common.formatDate(new Date(user.CreatedOn));
                                                            txtCreatedOn.value = user.CreatedOn;
                                                            txtModifiedOn.value = "-";

                                                        },
                                                        function () { }, auth);

                                                } else {
                                                    window.Common.toastError(language.get("toast-user-create-error"));
                                                }
                                            },
                                            function () { }, "", auth);
                                    }
                                }

                            }
                        } else {
                            window.Common.toastInfo(language.get("toast-username-exists"));
                        }

                    },
                    function () { }, auth);
            }

        } else {
            let up2 = parseInt(getSelectedProfile());
            if (changePassword === false) {
                if (txtUsername.value === "") {
                    window.Common.toastInfo(language.get("toast-username"));
                } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                    window.Common.toastInfo(language.get("toast-email-error"));
                } else if (up2 === -1) {
                    window.Common.toastInfo(language.get("toast-userprofile"));
                } else {
                    window.Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                        function (u) {
                            if (typeof u !== "undefined" && u !== null && u.Username !== selectedUsername) {
                                window.Common.toastInfo(language.get("toast-username-exists"));
                            } else {

                                if (selectedUsername !== logedinUser && selectedUserProfile === 0) {

                                    if (newPasswordText.value === "") {
                                        window.Common.toastInfo(language.get("toast-password"));
                                    } else {
                                        let pass = window.MD5(newPasswordText.value);

                                        if (pass !== u.Password) {
                                            window.Common.toastInfo(language.get("toast-password-incorrect"));
                                        } else {
                                            updateUsernameAndPassword();
                                        }
                                    }

                                } else {
                                    updateUsernameAndPassword();
                                }

                            }

                        },
                        function () { }, auth);
                }


            } else {
                window.Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
                    function (u) {
                        let oldPassword = window.MD5(oldPasswordText.value);
                        if (u.UserProfile === 0 && u.Password !== oldPassword) {
                            window.Common.toastInfo(language.get("toast-old-password-incorrect"));
                        } else {

                            if (newPasswordText.value !== confirmPasswordText.value) {
                                window.Common.toastInfo(language.get("toast-new-password-error"));
                            } else if (newPasswordText.value === "" || confirmPasswordText.value === "") {
                                window.Common.toastInfo(language.get("toast-new-password"));
                            } else {
                                let newPassword = window.MD5(newPasswordText.value);
                                let up = getSelectedProfile();

                                if (txtUsername.value === "") {
                                    window.Common.toastInfo(language.get("toast-username"));
                                } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                                    window.Common.toastInfo(language.get("toast-email-error"));
                                } else if (up === -1) {
                                    window.Common.toastInfo(language.get("toast-userprofile"));
                                } else {
                                    window.Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                                        function (u) {
                                            if (typeof u !== "undefined" &&
                                                u !== null &&
                                                u.Username !== selectedUsername) {
                                                window.Common.toastInfo(language.get("toast-username-exists"));
                                            } else {

                                                window.Common.post(uri + "/update-user?userId=" + selectedUserId + "&username=" + encodeURIComponent(txtUsername.value) + "&password=" + encodeURIComponent(newPassword) + "&up=" + up + "&email=" + encodeURIComponent(emailText.value),
                                                    function (val) {
                                                        if (val === true) {
                                                            auth = "Basic " + btoa(qusername + ":" + (selectedUsername === logedinUser ? newPassword : qpassword));
                                                            window.Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                                                                function (user) {
                                                                    if (selectedUsername === logedinUser) {
                                                                        qpassword = user.Password;
                                                                        auth = "Basic " + btoa(qusername + ":" + qpassword);
                                                                        window.deleteUser();
                                                                        window.authorize(txtUsername.value, user.Password, user.UserProfile);

                                                                        btnLogout.innerHTML = "Logout (" + txtUsername.value + ")";
                                                                    }

                                                                    selectedUsernameTd.innerHTML = txtUsername.value;

                                                                    if (logedinUser === selectedUsername) {
                                                                        logedinUser = txtUsername.value;
                                                                    }

                                                                    selectedUsername = txtUsername.value;

                                                                    trId.style.display = "table-row";
                                                                    trCreatedOn.style.display = "table-row";
                                                                    trModifiedOn.style.display = "table-row";
                                                                    txtId.value = user.Id;
                                                                    //txtCreatedOn.value = window.Common.formatDate(new Date(user.CreatedOn));
                                                                    txtCreatedOn.value = user.CreatedOn;
                                                                    //txtModifiedOn.value = window.Common.formatDate(new Date(user.ModifiedOn));
                                                                    txtModifiedOn.value = user.ModifiedOn;

                                                                    if (logedinUser !== selectedUsername && user.UserProfile === 0) {
                                                                        slctProfile.disabled = true;
                                                                        saveAction.style.display = "none";
                                                                        txtUsername.disabled = true;
                                                                        emailText.disabled = true;
                                                                        changePass.style.display = "none";
                                                                    }

                                                                    window.Common.toastSuccess(language.get("toast-user-updated"));
                                                                },
                                                                function () { }, auth);
                                                        } else {
                                                            window.Common.toastError(language.get("toast-user-update-error"));
                                                        }
                                                    },
                                                    function () { }, "", auth);
                                            }
                                        },
                                        function () { }, auth
                                    );
                                }

                            }
                        }

                    },
                    function () { }, auth);
            }

        }
    }

    function updateUsernameAndPassword() {
        let up = parseInt(getSelectedProfile());

        window.Common.post(uri + "/update-username-email-user-profile?userId=" + selectedUserId + "&username=" + encodeURIComponent(txtUsername.value) + "&email=" + encodeURIComponent(emailText.value) + "&up=" + up,
            function (val) {
                if (val === true) {
                    window.Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                        function (user) {
                            if (logedinUser === selectedUsername) {
                                qpassword = user.Password;
                                //btnLogout.innerHTML = "Logout (" + txtUsername.value + ")";
                                document.getElementById("spn-username").innerHTML = " (" + txtUsername.Username + ")";

                                window.deleteUser();
                                window.authorize(txtUsername.value, user.Password, user.UserProfile);

                            }
                            window.Common.toastSuccess(language.get("toast-user-updated"));
                            selectedUsernameTd.innerHTML = txtUsername.value;
                            selectedUserProfileTd.innerHTML = userProfileToText(user.UserProfile);

                            if (logedinUser === selectedUsername) {
                                logedinUser = txtUsername.value;
                            }

                            selectedUsername = txtUsername.value;

                            trId.style.display = "table-row";
                            trCreatedOn.style.display = "table-row";
                            trModifiedOn.style.display = "table-row";
                            txtId.value = user.Id;
                            //txtCreatedOn.value = window.Common.formatDate(new Date(user.CreatedOn));
                            txtCreatedOn.value = user.CreatedOn;
                            //txtModifiedOn.value = window.Common.formatDate(new Date(user.ModifiedOn));
                            txtModifiedOn.value = user.ModifiedOn;

                            if (logedinUser !== selectedUsername && user.UserProfile === 0) {
                                slctProfile.disabled = true;
                                saveAction.style.display = "none";
                                txtUsername.disabled = true;
                                emailText.disabled = true;
                                changePass.style.display = "none";
                                deleteAction.style.display = "none";
                                newPasswordTr.style.display = "none";
                                confirmPasswordTr.style.display = "none";
                            }

                        },
                        function () { }, auth);
                } else {
                    window.Common.toastError(language.get("toast-user-update-error"));
                }
            },
            function () { }, "", auth);
    }

    function getSelectedProfile() {
        return slctProfile.options[slctProfile.selectedIndex].value;
    }

    function validateEmail(email) {
        let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

}