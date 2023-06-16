window.WexflowManager = function () {
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

        if (document.getElementById("wf-start")) {
            document.getElementById("wf-start").innerHTML = language.get("wf-start");
        }
        if (document.getElementById("wf-pause")) {
            document.getElementById("wf-pause").innerHTML = language.get("wf-pause");
        }
        if (document.getElementById("wf-resume")) {
            document.getElementById("wf-resume").innerHTML = language.get("wf-resume");
        }
        if (document.getElementById("wf-stop")) {
            document.getElementById("wf-stop").innerHTML = language.get("wf-stop");
        }
        if (document.getElementById("wf-approve")) {
            document.getElementById("wf-approve").innerHTML = language.get("wf-approve");
        }
        if (document.getElementById("wf-reject")) {
            document.getElementById("wf-reject").innerHTML = language.get("wf-reject");
        }
        if (document.getElementById("wf-search-action")) {
            document.getElementById("wf-search-action").innerHTML = language.get("btn-search");
        }

        if (document.getElementById("th-job-id")) {
            document.getElementById("th-job-id").innerHTML = language.get("th-job-id");
        }
        if (document.getElementById("th-job-startedOn")) {
            document.getElementById("th-job-startedOn").innerHTML = language.get("th-job-startedOn");
        }
        if (document.getElementById("th-job-n")) {
            document.getElementById("th-job-n").innerHTML = language.get("th-wf-n");
        }
        if (document.getElementById("th-job-d")) {
            document.getElementById("th-job-d").innerHTML = language.get("th-wf-d");
        }

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

    let id = "wf-manager";
    let uri = window.Common.trimEnd(window.Settings.Uri, "/");
    let lnkRecords = document.getElementById("lnk-records");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
    let lnkNotifications = document.getElementById("lnk-notifications");
    let imgNotifications = document.getElementById("img-notifications");
    let selectedId = -1;
    let workflows = {};
    let jobs = [];
    let workflowJobs = {};
    let workflowTimer = null;
    let jobTimer = null;
    let timerInterval = 1000; // ms
    let username = "";
    let password = "";
    let auth = "";

    let html = "<div id='wf-container'>"
        + "<div id='wf-cmd'>"
        + "<button id='wf-start' type='button' class='btn btn-primary btn-xs'>" + language.get("wf-start") + "</button>"
        + "<button id='wf-pause' type='button' class='btn btn-secondary btn-xs'>" + language.get("wf-pause") + "</button>"
        + "<button id='wf-resume' type='button' class='btn btn-secondary btn-xs'>" + language.get("wf-resume") + "</button>"
        + "<button id='wf-stop' type='button' class='btn btn-danger btn-xs'>" + language.get("wf-stop") + "</button>"
        + "<button id='wf-approve' type='button' class='btn btn-primary btn-xs'>" + language.get("wf-approve") + "</button>"
        + "<button id='wf-reject' type='button' class='btn btn-warning btn-xs'>" + language.get("wf-reject") + "</button>"
        + "</div>"
        + "<div id='wf-notifier'>"
        + "<input id='wf-notifier-text' type='text' name='fname' readonly>"
        + "</div>"
        + "<div id='wf-search'>"
        + "<div id='wf-search-text-container'>"
        + "<input id='wf-search-text' type='text' name='fname' autocomplete='off' >"
        + "</div>"
        + "<button id='wf-search-action' type='button' class='btn btn-primary btn-xs'>" + language.get("btn-search") + "</button>"
        + "</div>"
        + "<div id='wf-jobs'>"
        + "<table id='wf-jobs-table' class='table'>"
        + "<thead class='thead-dark'>"
        + "<tr>"
        + "<th id='th-job-id' class='wf-jobId'>" + language.get("th-job-id") + "</th>"
        + "<th id='th-job-startedOn' class='wf-startedOn'>" + language.get("th-job-startedOn") + "</th>"
        + "<th id='th-job-n' class='wf-n'>" + language.get("th-wf-n") + "</th>"
        + "<th id='th-job-d' class='wf-d'>" + language.get("th-wf-d") + "</th>"
        + "</tr>"
        + "</thead>"
        + "<tbody>"
        + "</tbody>"
        + "</table>"
        + "</div>"
        + "<div id='wf-workflows'>"
        + "</div>"
        + "</div>";

    document.getElementById(id).innerHTML = html;

    let startButton = document.getElementById("wf-start");
    let suspendButton = document.getElementById("wf-pause");
    let resumeButton = document.getElementById("wf-resume");
    let stopButton = document.getElementById("wf-stop");
    let approveButton = document.getElementById("wf-approve");
    let rejectButton = document.getElementById("wf-reject");
    let searchButton = document.getElementById("wf-search-action");
    let searchText = document.getElementById("wf-search-text");
    let suser = getUser();

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
                } else {
                    if (u.UserProfile === 0 || u.UserProfile === 1) {
                        window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
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

                            let btnLogout = document.getElementById("btn-logout");
                            let divWorkflows = document.getElementById("wf-manager");
                            divWorkflows.style.display = "block";

                            btnLogout.onclick = function () {
                                window.deleteUser();
                                window.Common.redirectToLoginPage();
                            };
                            document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                            window.Common.disableButton(startButton, true);
                            window.Common.disableButton(suspendButton, true);
                            window.Common.disableButton(resumeButton, true);
                            window.Common.disableButton(stopButton, true);
                            window.Common.disableButton(approveButton, true);
                            window.Common.disableButton(rejectButton, true);

                            searchButton.onclick = function () {
                                loadWorkflows();
                                notify("");
                                window.Common.disableButton(startButton, true);
                                window.Common.disableButton(suspendButton, true);
                                window.Common.disableButton(resumeButton, true);
                                window.Common.disableButton(stopButton, true);
                                window.Common.disableButton(approveButton, true);
                                window.Common.disableButton(rejectButton, true);
                            };

                            searchText.onkeyup = function (event) {
                                event.preventDefault();

                                if (event.keyCode === 13) { // Enter
                                    loadWorkflows();
                                    notify("");
                                    window.Common.disableButton(startButton, true);
                                    window.Common.disableButton(suspendButton, true);
                                    window.Common.disableButton(resumeButton, true);
                                    window.Common.disableButton(stopButton, true);
                                    window.Common.disableButton(approveButton, true);
                                    window.Common.disableButton(rejectButton, true);
                                }
                            };

                            loadWorkflows();

                        }, function () { }, auth);
                    } else {
                        window.Common.redirectToLoginPage();
                    }

                }
            }, function () {
                window.logout();
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

    function loadWorkflows() {
        window.Common.get(uri + "/search?s=" + encodeURIComponent(searchText.value),
            function (data) {
                data.sort(compareById);
                let items = [];

                for (let i = 0; i < data.length; i++) {
                    let val = data[i];
                    workflows[val.Id] = val;
                    let lt = launchType(val.LaunchType);
                    items.push("<tr>"
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

                let divWorkflows = document.getElementById("wf-workflows");
                divWorkflows.innerHTML = table;

                let workflowsTable = document.getElementById("wf-workflows-table");

                workflowsTable.getElementsByTagName("tbody")[0].style.height = (divWorkflows.offsetHeight - 45) + "px";

                let rows = workflowsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                if (rows.length > 0) {
                    let hrow = workflowsTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                    hrow.querySelector(".wf-id").style.width = rows[0].querySelector(".wf-id").offsetWidth + "px";
                    hrow.querySelector(".wf-n").style.width = rows[0].querySelector(".wf-n").offsetWidth + "px";
                    hrow.querySelector(".wf-e").style.width = rows[0].querySelector(".wf-e").offsetWidth + "px";
                    hrow.querySelector(".wf-a").style.width = rows[0].querySelector(".wf-a").offsetWidth + "px";
                    hrow.querySelector(".wf-d").style.width = rows[0].querySelector(".wf-d").offsetWidth + "px";
                }

                let descriptions = workflowsTable.querySelectorAll(".wf-d");
                for (let i = 0; i < descriptions.length; i++) {
                    descriptions[i].style.width = workflowsTable.offsetWidth - 495 + "px";
                }

                function getWorkflow(wid, func) {
                    window.Common.get(uri + "/workflow?w=" + wid, function (d) {
                        func(d);
                    },
                        function () { }, auth);
                }

                function updateButtons(wid, force) {
                    getWorkflow(wid, function (workflow) {
                        if (workflow.IsEnabled === false) {
                            notify("This workflow is disabled.");
                            window.Common.disableButton(startButton, true);
                            window.Common.disableButton(suspendButton, true);
                            window.Common.disableButton(resumeButton, true);
                            window.Common.disableButton(stopButton, true);
                            window.Common.disableButton(approveButton, true);
                            window.Common.disableButton(rejectButton, true);
                            clearInterval(workflowTimer);
                        }
                        else {
                            if (force === false && workflowStatusChanged(workflow) === false) return;

                            window.Common.disableButton(startButton, false);

                            notify("");
                        }
                    });
                }

                function workflowStatusChanged(workflow) {
                    let changed = workflows[workflow.Id].IsEnabled !== workflow.IsEnabled;
                    workflows[workflow.Id].IsEnabled = workflow.IsEnabled;
                    return changed;
                }

                function updateJobButtons(wid, jobId, force) {
                    window.Common.get(uri + "/job?w=" + wid + "&i=" + jobId, function (job) {
                        if (job) {
                            if (force === false && jobStatusChanged(job) === false) return;

                            window.Common.disableButton(stopButton, !(job.IsRunning && !job.IsPaused));
                            window.Common.disableButton(suspendButton, !(job.IsRunning && !job.IsPaused));
                            window.Common.disableButton(resumeButton, !job.IsPaused);
                            window.Common.disableButton(approveButton, !(job.IsWaitingForApproval && job.IsApproval));
                            window.Common.disableButton(rejectButton, !(job.IsWaitingForApproval && job.IsApproval));

                            if (job.IsApproval === true && job.IsWaitingForApproval === true && job.IsPaused === false) {
                                notify("This job is waiting for approval...");
                            } else {
                                if (job.IsRunning === true && job.IsPaused === false) {
                                    notify("This job is running...");
                                }
                                else if (job.IsPaused === true) {
                                    notify("This job is suspended.");
                                } else {
                                    notify("");
                                }
                            }
                        } else {
                            clearInterval(jobTimer);
                            window.Common.disableButton(stopButton, true);
                            window.Common.disableButton(suspendButton, true);
                            window.Common.disableButton(resumeButton, true);
                            notify("");
                        }
                    }, function () { }, auth);
                }

                function jobStatusChanged(job) {
                    if (!job || !workflowJobs[job.InstanceId]) {
                        return true;
                    }
                    let changed = workflowJobs[job.InstanceId].IsRunning !== job.IsRunning || workflowJobs[job.InstanceId].IsPaused !== job.IsPaused || workflowJobs[job.InstanceId].IsWaitingForApproval !== job.IsWaitingForApproval;
                    workflowJobs[job.InstanceId].IsRunning = job.IsRunning;
                    workflowJobs[job.InstanceId].IsPaused = job.IsPaused;
                    workflowJobs[job.InstanceId].IsWaitingForApproval = job.IsWaitingForApproval;
                    return changed;
                }

                for (let i = 0; i < rows.length; i++) {
                    rows[i].onclick = function () {
                        selectedId = parseInt(this.getElementsByClassName("wf-id")[0].innerHTML);

                        let selected = workflowsTable.querySelectorAll(".selected");
                        if (selected.length > 0) {
                            selected[0].classList.remove("selected");
                        }

                        this.className += "selected";

                        let jobsTable = document.getElementById("wf-jobs-table");
                        jobsTable.getElementsByTagName("tbody")[0].style.height = (document.getElementById("wf-jobs").offsetHeight - 45) + "px";

                        clearInterval(workflowTimer);

                        if (workflows[selectedId].IsEnabled === true) {
                            workflowTimer = setInterval(function () {
                                updateButtons(selectedId, false);

                                // Jobs
                                window.Common.get(uri + "/jobs?w=" + selectedId, function (data) {
                                    if (data) {
                                        workflowJobs = {};
                                        let currentJobs = [];
                                        for (let i = 0; i < data.length; i++) {
                                            let job = data[i];
                                            currentJobs.push(job.InstanceId);
                                            workflowJobs[job.InstanceId] = job;
                                        }

                                        for (let i = 0; i < currentJobs.length; i++) {
                                            let jobId = currentJobs[i];

                                            if (jobs.includes(jobId) === false) {
                                                // Add
                                                let row = jobsTable.getElementsByTagName('tbody')[0].insertRow();

                                                for (let i = 0; i < data.length; i++) {
                                                    let job = data[i];
                                                    if (job.InstanceId === jobId) {
                                                        let cell1 = row.insertCell(0);
                                                        let cell2 = row.insertCell(1);
                                                        let cell3 = row.insertCell(2);
                                                        let cell4 = row.insertCell(3);
                                                        cell1.className = "wf-jobId";
                                                        cell1.innerHTML = job.InstanceId;
                                                        cell2.className = "wf-startedOn";
                                                        cell2.innerHTML = job.StartedOn;
                                                        cell3.className = "wf-n";
                                                        cell3.innerHTML = job.Name;
                                                        cell4.className = "wf-d";
                                                        cell4.innerHTML = job.Description;
                                                        break;
                                                    }
                                                }

                                                //let rows = (jobsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                                                //if (rows.length > 0) {
                                                //    let hrow = jobsTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                                                //    hrow.querySelector(".wf-jobId").style.width = rows[0].querySelector(".wf-jobId").offsetWidth + "px";
                                                //    hrow.querySelector(".wf-startedOn").style.width = rows[0].querySelector(".wf-startedOn").offsetWidth + "px";
                                                //    hrow.querySelector(".wf-n").style.width = rows[0].querySelector(".wf-n").offsetWidth + "px";
                                                //    hrow.querySelector(".wf-d").style.width = rows[0].querySelector(".wf-d").offsetWidth + "px";
                                                //}

                                                let descriptions = jobsTable.querySelectorAll(".wf-d");
                                                for (let i = 0; i < descriptions.length; i++) {
                                                    descriptions[i].style.width = workflowsTable.offsetWidth - 515 + "px";
                                                }

                                                jobs.push(jobId);
                                            } else {
                                                for (let j = 0; j < jobs.length; j++) {
                                                    if (currentJobs.includes(jobs[j]) === false) {
                                                        // Remove
                                                        let rows = (jobsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                                                        for (let k = 0; k < rows.length; k++) {
                                                            let row = rows[k];
                                                            let jobId = row.querySelector(".wf-jobId").innerHTML;
                                                            if (jobId === jobs[j]) {
                                                                jobsTable.deleteRow(k + 1);

                                                                break;
                                                            }
                                                        }

                                                        //if (rows.length > 0) {
                                                        //    let hrow = jobsTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                                                        //    hrow.querySelector(".wf-jobId").style.width = rows[0].querySelector(".wf-jobId").offsetWidth + "px";
                                                        //    hrow.querySelector(".wf-startedOn").style.width = rows[0].querySelector(".wf-startedOn").offsetWidth + "px";
                                                        //    hrow.querySelector(".wf-n").style.width = rows[0].querySelector(".wf-n").offsetWidth + "px";
                                                        //    hrow.querySelector(".wf-d").style.width = rows[0].querySelector(".wf-d").offsetWidth + "px";
                                                        //}

                                                        let descriptions = jobsTable.querySelectorAll(".wf-d");
                                                        for (let i = 0; i < descriptions.length; i++) {
                                                            descriptions[i].style.width = workflowsTable.offsetWidth - 515 + "px";
                                                        }

                                                        remove(jobs, jobs[j]);
                                                    }
                                                }
                                            }
                                        }

                                        if (currentJobs.length === 0) {
                                            (jobsTable.getElementsByTagName("tbody")[0]).innerHTML = '';
                                            jobs = [];
                                        }

                                        // On row click
                                        let rows = (jobsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                                        for (let k = 0; k < rows.length; k++) {
                                            let row = rows[k];
                                            row.onclick = function () {
                                                let jobId = row.querySelector(".wf-jobId").innerHTML;

                                                let selected = jobsTable.getElementsByTagName("tbody")[0].querySelectorAll(".selected");
                                                if (selected.length > 0) {
                                                    selected[0].classList.remove("selected");
                                                }

                                                this.className += "selected";

                                                if (jobTimer) {
                                                    clearInterval(jobTimer);
                                                }

                                                jobTimer = setInterval(function () {
                                                    updateJobButtons(selectedId, jobId, false);

                                                }, timerInterval);

                                                updateJobButtons(selectedId, jobId, true);

                                            };
                                        }
                                    }
                                }, function () {
                                }, auth);

                            }, timerInterval);

                            updateButtons(selectedId, true);
                        } else {
                            updateButtons(selectedId, true);
                        }

                    };
                }

                startButton.onclick = function () {
                    let startUri = uri + "/start?w=" + selectedId;
                    window.Common.post(startUri, function () {
                    }, function () { }, "", auth);
                };

                suspendButton.onclick = function () {
                    let selectedJob = document.getElementById("wf-jobs-table").querySelector(".selected");
                    let jobId = selectedJob.querySelector(".wf-jobId").innerHTML;

                    let suspendUri = uri + "/suspend?w=" + selectedId + "&i=" + jobId;
                    window.Common.post(suspendUri, function (res) {
                        if (res === true) {
                            updateJobButtons(selectedId, jobId, true);
                        } else {
                            window.Common.toastInfo(language.get("op-not-supported"));
                        }
                    }, function () { }, "", auth);
                };

                resumeButton.onclick = function () {
                    let selectedJob = document.getElementById("wf-jobs-table").querySelector(".selected");
                    let jobId = selectedJob.querySelector(".wf-jobId").innerHTML;

                    let resumeUri = uri + "/resume?w=" + selectedId + "&i=" + jobId;
                    window.Common.post(resumeUri, function () {
                        updateJobButtons(selectedId, jobId, true);
                    }, function () { }, "", auth);
                };

                stopButton.onclick = function () {
                    let selectedJob = document.getElementById("wf-jobs-table").querySelector(".selected");
                    let jobId = selectedJob.querySelector(".wf-jobId").innerHTML;

                    let stopUri = uri + "/stop?w=" + selectedId + "&i=" + jobId;
                    window.Common.post(stopUri,
                        function (res) {
                            if (res === true) {
                                updateJobButtons(selectedId, jobId, true);
                            } else {
                                window.Common.toastInfo(language.get("op-not-supported"));
                            }
                        },
                        function () { }, "", auth);
                };

                approveButton.onclick = function () {
                    window.Common.disableButton(approveButton, true);
                    window.Common.disableButton(stopButton, true);
                    let selectedJob = document.getElementById("wf-jobs-table").querySelector(".selected");
                    let jobId = selectedJob.querySelector(".wf-jobId").innerHTML;
                    let approveUri = uri + "/approve?w=" + selectedId + "&i=" + jobId;
                    window.Common.post(approveUri,
                        function (res) {
                            if (res === true) {
                                updateJobButtons(selectedId, jobId, true);
                                window.Common.toastSuccess(language.get("job-part-1") + jobId + language.get("job-approved"));
                            } else {
                                window.Common.disableButton(approveButton, false);
                                window.Common.disableButton(stopButton, false);
                                window.Common.toastError(language.get("job-approved-error-part-1") + jobId + language.get("job-approved-error-part-2") + selectedId + ".");
                            }
                        },
                        function () { }, "", auth);
                };

                rejectButton.onclick = function () {
                    window.Common.disableButton(rejectButton, true);
                    window.Common.disableButton(approveButton, true);
                    window.Common.disableButton(stopButton, true);
                    let selectedJob = document.getElementById("wf-jobs-table").querySelector(".selected");
                    let jobId = selectedJob.querySelector(".wf-jobId").innerHTML;
                    let rejectUri = uri + "/reject?w=" + selectedId + "&i=" + jobId;
                    window.Common.post(rejectUri,
                        function (res) {
                            if (res === true) {
                                updateJobButtons(selectedId, jobId, true);
                                window.Common.toastSuccess(language.get("job-part-1") + jobId + language.get("job-rejected"));
                            } else {
                                window.Common.disableButton(disapproveButton, true);
                                window.Common.disableButton(approveButton, false);
                                window.Common.disableButton(stopButton, false);
                                window.Common.toastError(language.get("job-rejected-error-part-1") + jobId + language.get("job-approved-error-part-2") + selectedId + ".");
                            }
                        },
                        function () { }, "", auth);
                };

                // End of get workflows
            },
            function () {
                window.Common.toastError(language.get("workflows-server-error"));
            }, auth);
    }

    function notify(msg) {
        document.getElementById("wf-notifier-text").value = msg;
    }

    function remove(array, e) {
        const index = array.indexOf(e);
        if (index > -1) {
            array.splice(index, 1);
        }
    }
}