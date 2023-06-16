window.Profiles = function () {
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
        document.getElementById("users-save-action").value = language.get("save-action");

        if (document.getElementById("th-wf-id")) {
            document.getElementById("th-wf-id").innerHTML = language.get("th-wf-id");
        }
        if (document.getElementById("th-wf-n")) {
            document.getElementById("th-wf-n").innerHTML = language.get("th-wf-n");
        }
        if (document.getElementById("th-wf-lt")) {
            document.getElementById("th-wf-lt").innerHTML = language.get("th-wf-lt");
        }
        if (document.getElementById("th-wf-e")) {
            document.getElementById("th-wf-e").innerHTML = language.get("th-wf-e");
        }
        if (document.getElementById("th-wf-a")) {
            document.getElementById("th-wf-a").innerHTML = language.get("th-wf-a");
        }
        if (document.getElementById("th-wf-d")) {
            document.getElementById("th-wf-d").innerHTML = language.get("th-wf-d");
        }
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

    let divProfiles = document.getElementById("profiles");
    let btnLogout = document.getElementById("btn-logout");
    let txtSearch = document.getElementById("users-search-text");
    let divUsersTable = document.getElementById("users-table");
    let btnSearch = document.getElementById("users-search-action");
    let divWorkflows = document.getElementById("workflows");
    let btnSave = document.getElementById("users-save-action");
    let suser = getUser();
    let uo = 0;
    let selectedUserId = "";
    let selectedUsername = "";
    let workflows = [];
    let userWorkflows = [];    // [{"UserId": 1, "WorkflowId": 6}, ...]
    let username = "";
    let password = "";
    let auth = "";

    if (suser === null || suser === "") {
        window.Common.redirectToLoginPage();
    } else {
        let user = JSON.parse(suser);

        username = user.Username;
        password = user.Password;
        auth = "Basic " + btoa(username + ":" + password);

        window.Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (!u || user.Password !== u.Password) {
                    window.Common.redirectToLoginPage();
                } else if (u.UserProfile === 0) {
                    window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
                        divProfiles.style.display = "block";
                        lnkRecords.style.display = "inline";
                        lnkManager.style.display = "inline";
                        lnkDesigner.style.display = "inline";
                        lnkApproval.style.display = "inline";
                        lnkUsers.style.display = "inline";
                        lnkProfiles.style.display = "inline";
                        lnkNotifications.style.display = "inline";

                        document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                        if (hasNotifications === true) {
                            imgNotifications.src = "images/notification-active.png";
                        } else {
                            imgNotifications.src = "images/notification.png";
                        }

                        btnLogout.onclick = function () {
                            window.deleteUser();
                            window.Common.redirectToLoginPage();
                        };

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
        loadUsers(selectedUsername, true);
    };

    txtSearch.onkeyup = function (e) {
        if (e.keyCode === 13) {
            loadUsers(selectedUsername, true);
        }
    }

    function loadUsers(usernameToSelect, scroll) {
        window.Common.get(uri + "/search-admins?keyword=" + encodeURIComponent(txtSearch.value) + "&uo=" + uo,
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
                        //+ "<td class='userprofile'>" + userProfileToText(val.UserProfile) + "</td>"
                        + "</tr>"
                    );
                }

                let table = "<table id='wf-users-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th id='th-id'>Id</th>"
                    + "<th id='th-username' class='username'>Username&nbsp;&nbsp;🔺</th>"
                    //+ "<th>Profile</th>"
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
                }

                let usernames = usersTable.querySelectorAll(".username");
                for (let i = 0; i < usernames.length; i++) {
                    usernames[i].style.width = usersTable.offsetWidth + "px";
                }

                let thUsername = document.getElementById("th-username");
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
                            let selectedTr = selected[0];
                            selectedTr.className = selectedTr.className.replace("selected", "");
                        }

                        this.className += "selected";

                        selectedUserId = this.getElementsByClassName("userid")[0].innerHTML;
                        let selectedUsernameTd = this.getElementsByClassName("username")[0];
                        selectedUsername = selectedUsernameTd.innerHTML;

                        userWorkflows = [];
                        loadRightPanel(selectedUserId);
                    };
                }

            },
            function () { }, auth);
    }

    function loadRightPanel() {
        window.Common.get(uri + "/search?s=",
            function (data) {
                btnSave.style.display = "block";

                data.sort(compareById);
                let items = [];

                for (let i = 0; i < data.length; i++) {
                    let val = data[i];
                    workflows[val.Id] = val;
                    let lt = launchType(val.LaunchType);
                    items.push("<tr>"
                        + "<td class='wf-check'><input type='checkbox'></td>"
                        + "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>"
                        + "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>"
                        + "<td class='wf-lt'>" + lt + "</td>"
                        + "<td class='wf-e'><input type='checkbox' readonly disabled " + (val.IsEnabled ? "checked" : "") + "></input></td>"
                        + "<td class='wf-a'><input type='checkbox' readonly disabled " + (val.IsApproval ? "checked" : "") + "></input></td>"
                        + "<td class='wf-d' title='" + val.Description + "'>" + val.Description + "</td>"
                        + "</tr>");

                }

                let table = "<table id='wf-workflows-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th><input id='check-all' type='checkbox'></th>"
                    + "<th id='th-wf-id' class='wf-id'>" + language.get("th-wf-id") + "</th>"
                    + "<th id='th-wf-n' class='wf-n'>" + language.get("th-wf-n") + "</th>"
                    + "<th id='th-wf-lt' class='wf-lt'>" + language.get("th-wf-lt") + "</th>"
                    + "<th id='th-wf-e' class='wf-e'>" + language.get("th-wf-e") + "</th>"
                    + "<th id='th-wf-a' class='wf-a'>" + language.get("th-wf-a") + "</th>"
                    + "<th id='th-wf-d' class='wf-d'>" + language.get("th-wf-d") + "</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                divWorkflows.innerHTML = table;

                let workflowsTable = document.getElementById("wf-workflows-table");

                workflowsTable.getElementsByTagName("tbody")[0].style.height = (divWorkflows.offsetHeight - 45) + "px";

                let descriptions = workflowsTable.querySelectorAll(".wf-d");
                for (let i = 0; i < descriptions.length; i++) {
                    descriptions[i].style.width = workflowsTable.offsetWidth - 515 + "px";
                }

                let enabledFields = workflowsTable.querySelectorAll(".wf-e");
                for (let i = 0; i < enabledFields.length; i++) {
                    enabledFields[i].style.width = 75 + "px";
                }

                let approvalFields = workflowsTable.querySelectorAll(".wf-a");
                for (let i = 0; i < approvalFields.length; i++) {
                    approvalFields[i].style.width = 75 + "px";
                }

                let rows = workflowsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                if (rows.length > 0) {
                    let hrow = workflowsTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                    hrow.querySelector(".wf-id").style.width = rows[0].querySelector(".wf-id").offsetWidth + "px";
                    hrow.querySelector(".wf-n").style.width = rows[0].querySelector(".wf-n").offsetWidth + "px";
                    hrow.querySelector(".wf-e").style.width = rows[0].querySelector(".wf-e").offsetWidth + "px";
                    hrow.querySelector(".wf-a").style.width = rows[0].querySelector(".wf-a").offsetWidth + "px";
                    hrow.querySelector(".wf-d").style.width = rows[0].querySelector(".wf-d").offsetWidth + "px";
                }

                for (let i = 0; i < rows.length; i++) {
                    let row = rows[i];
                    let checkBox = row.getElementsByClassName("wf-check")[0].firstChild;

                    checkBox.onchange = function () {
                        let currentRow = this.parentElement.parentElement;
                        let workflowId = parseInt(currentRow.getElementsByClassName("wf-id")[0].innerHTML);
                        let workflowDbId = workflows[workflowId].DbId;

                        if (this.checked === true) {
                            userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                        } else {
                            //let index = -1;
                            for (let j = userWorkflows.length - 1; j > -1; j--) {
                                if (userWorkflows[j].WorkflowId === workflowDbId) {
                                    //index = j;
                                    //break;
                                    userWorkflows.splice(j, 1);
                                }
                            }

                            //if (index > -1) {
                            //    userWorkflows.splice(index, 1);
                            //}
                        }
                        //console.log(userWorkflows);
                    };
                }

                // Check the boxes from the relations in db
                window.Common.get(uri + "/user-workflows?u=" + selectedUserId,
                    function (res) {

                        let workflowsTable = document.getElementById("wf-workflows-table");
                        let rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                        for (let i = 0; i < rows.length; i++) {
                            let row = rows[i];
                            let checkBox = row.getElementsByClassName("wf-check")[0].firstChild;
                            let workflowId = parseInt(row.getElementsByClassName("wf-id")[0].innerHTML);
                            let workflowDbId = workflows[workflowId].DbId;

                            for (let j = 0; j < res.length; j++) {
                                if (workflowDbId === res[j].DbId) {
                                    checkBox.checked = true;
                                    //console.log(workflowDbId);
                                    userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                                    break;
                                }
                            }
                        }
                    }, function () {
                        window.Common.toastError("An error occured while retrieving user workflows.");
                    }, auth);

                document.getElementById("check-all").onchange = function () {
                    let workflowsTable = document.getElementById("wf-workflows-table");
                    let rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                    for (let i = 0; i < rows.length; i++) {
                        let row = rows[i];
                        let checkBox = row.getElementsByClassName("wf-check")[0].firstChild;
                        let workflowId = parseInt(row.getElementsByClassName("wf-id")[0].innerHTML);
                        let workflowDbId = workflows[workflowId].DbId;

                        if (checkBox.checked === true) {
                            checkBox.checked = false;

                            //let index = -1;
                            for (let j = userWorkflows.length - 1; j > -1; j--) {
                                if (userWorkflows[j].WorkflowId === workflowDbId) {
                                    //index = j;
                                    //break;
                                    userWorkflows.splice(j, 1);
                                }
                            }

                            //if (index > -1) {
                            //    userWorkflows.splice(index, 1);
                            //}
                        } else {
                            checkBox.checked = true;
                            userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                        }

                    }

                    //console.log(userWorkflows);
                };

                // End of get workflows
            }, function () {
                window.Common.toastError("An error occured while retrieving workflows. Check that Wexflow server is running correctly.");
            }, auth);

    }

    function compareById(wf1, wf2) {
        if (wf1.Id < wf2.Id) {
            return -1;
        } else if (wf1.Id > wf2.Id) {
            return 1;
        }
        return 0;
    }

    function launchType(lt) {
        switch (lt) {
            case 0:
                return "Startup";
            case 1:
                return "Trigger";
            case 2:
                return "Periodic";
            case 3:
                return "Cron";
            default:
                return "";
        }
    }

    btnSave.onclick = function () {
        window.Common.post(uri + "/save-user-workflows", function (res) {
            if (res === true) {
                window.Common.toastSuccess("Workflow relations saved with success.");
            } else {
                window.Common.toastError("An error occured while saving workflow relations.");
            }
        }, function () {
            window.Common.toastError("An error occured while saving workflow relations.");
        }, { "UserId": selectedUserId, "UserWorkflows": userWorkflows }, auth);
    };

}