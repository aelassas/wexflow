window.WexflowEditor = function () {
    "use strict";

    hljs.initHighlightingOnLoad();

    var id = "wf-designer";
    var uri = window.Common.trimEnd(window.Settings.Uri, "/");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkEditor = document.getElementById("lnk-editor");
    //var lnkApproval = document.getElementById("lnk-approval");
    var lnkUsers = document.getElementById("lnk-users");
    var lnkProfiles = document.getElementById("lnk-profiles");
    var suser = getUser();
    var username = "";
    var password = "";
    var auth = "";
    //var osname = window.Common.os();

    if (suser === null || suser === "") {
        window.Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);

        username = user.Username;
        password = user.Password;
        auth = "Basic " + btoa(username + ":" + password);

        window.Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (!u || user.Password !== u.Password) {
                    window.Common.redirectToLoginPage();
                } else {

                    if (u.UserProfile === 0 || u.UserProfile === 1) {
                        lnkManager.style.display = "inline";
                        lnkDesigner.style.display = "inline";
                        lnkEditor.style.display = "inline";
                        //lnkApproval.style.display = "inline";
                        lnkUsers.style.display = "inline";

                        if (u.UserProfile === 0) {
                            lnkProfiles.style.display = "inline";
                        }

                        var btnLogout = document.getElementById("btn-logout");
                        var divDesigner = document.getElementById("wf-designer");
                        divDesigner.style.display = "block";

                        btnLogout.onclick = function () {
                            window.deleteUser();
                            window.Common.redirectToLoginPage();
                        };

                        btnLogout.innerHTML = "Logout (" + u.Username + ")";
                        loadWorkflows();
                    } else {
                        window.Common.redirectToLoginPage();
                    }

                }
            },
            function () { }, auth);
    }

    var selectedId = -1;
    var editorWorkflowId = -1;
    var currentWorkflowId = -1;
    var workflows = {};
    var workflowInfos = {};
    var workflowTasks = {};
    //var timer = null;
    //var timerInterval = 1000; // ms
    var saveCalled = false;
    var editors = new Map();
    var editorChanged = false;
    var workflowChangedAndSaved = false;
    var editorCanceled = false;
    var newWorkflow = false;
    var isDarkTheme = false;
    var loadXmlCalled = false;
    var loadJsonCalled = false;
    var timeoutInterval = 300; // Timeout interval after db query
    var maxRetries = 0; // retry is disabled
    var retries = 0;
    //var statusRetries = 0;

    var rightPanelHtml = "<div style='margin: 0 0 10px 0;'><button id='wf-xml' type='button' class='wf-action-left btn btn-dark btn-xs'>XML</button> <button id='wf-json' type='button' class='wf-action-left btn btn-dark btn-xs'>JSON</button> <input id='wf-theme' type='checkbox' checked data-toggle='toggle' data-size='mini' data-on='Bright' data-off='Dark' data-width='70' style='display: none;'><small id='wf-xml-shortcut' style='float: right; margin: 7px; display: none;'> CTRL+ALT+H: Keyboard shortcuts</small></div>" +
        "<pre id='wf-xml-container' style='display: none;'></pre>" +
        "<table class='wf-designer-table'>" +
        "<tbody>" +
        "<tr><td class='wf-title'>Id</td><td class='wf-value'><input class='form-control' id='wf-id' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Name</td><td class='wf-value'><input class='form-control' id='wf-name' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Description</td><td class='wf-value'><input class='form-control' id='wf-desc' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>LaunchType</td><td class='wf-value'><select class='form-control' id='wf-launchType'><option value=''></option><option value='startup'>Startup</option><option value='trigger'>Trigger</option><option value='periodic'>Periodic</option>><option value='cron'>Cron</option></select></td></tr>" +
        "<tr><td class='wf-title'>Period</td><td class='wf-value'><input class='form-control' id='wf-period' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Cron expression</td><td class='wf-value'><input class='form-control' id='wf-cron' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Enabled</td><td class='wf-value'><input id='wf-enabled' type='checkbox' checked/></td></tr>" +
        "<tr><td class='wf-title'>Approval</td><td class='wf-value'><input id='wf-approval' type='checkbox' /></td></tr>" +
        "<tr><td class='wf-title'>EnableParallelJobs</td><td class='wf-value'><input id='wf-enable-pj' type='checkbox' checked/></td></tr>" +
        //"<tr><td class='wf-title'>Path</td><td id='wf-path' class='wf-value'></td></tr>" +
        //"<tr><td class='wf-title'>Status</td><td id='wf-status' class='wf-value'></td></tr>" +
        "</tbody>" +
        "</table>" +
        "<div id='wf-local-vars'>" +
        "<h5 class='wf-task-title'>Local variables" +
        "<button id='wf-add-var' type='button' class='btn btn-dark btn-xs'>New variable</button>" +
        "</h5>" +
        "<table class='wf-designer-table wf-local-vars' style='display: none;'>" +
        "<tr><th>Name</th><th>Value</th><th style='width:100%'></th></tr>" +
        "</table>" +
        "</div>" +
        "<div id='wf-tasks'>" +
        "</div>" +
        "<button type='button' id='wf-add-task' class='btn btn-dark btn-xs'>New task</button>";
    //+ "<h3 id='wf-execution-graph-title'><a class='colpsible-panel' data-toggle='collapse' data-parent='#accordion' style='font-weight: bold' href='#wf-execution-graph'>Execution graph</a></h3>" +
    //"<div id='wf-execution-graph' class='panel-collapse collapse'></div>";

    var html = "<div id='wf-container'>"
        + "<div id='wf-search'>"
        + "<div id='wf-search-text-container'>"
        + "<input class='form-control' id='wf-search-text' type='text' name='fname'>"
        + "</div>"
        + "<div style='float: right'>"
        + "<button id='wf-search-action' type='button' class='btn btn-primary btn-xs' style='float: left; margin-top: 5px'>Search</button>"
        + "<input id='wf-delete-workflows' type='image' src='images/delete.png' title='Delete selection' style='float: left; margin-top: 2px;'>"
        + "</div>"
        + "</div>"
        + "<div id='wf-workflows'></div>"
        + "<div id='wf-action'>"
        + "<button type='button' id='wf-add-workflow' class='btn btn-dark btn-xs'>New workflow</button>"
        + "<button id='wf-delete' type='button' class='wf-action-right btn btn-danger btn-xs'>Delete</button>"
        + "<button id='wf-save' type= 'button' class='wf-action-right btn btn-secondary btn-xs'>Save</button>"
        + "<button id='wf-cancel' type= 'button' class='wf-action-right btn btn-secondary btn-xs'>Cancel</button>"
        + "<small id='wf-shortcut' style='float: right; margin: 7px; display: none;'> CTRL+S: Save</small>"
        + "</div>"
        + "<div id='wf-designer-right-panel' style='display: none;'>"
        + rightPanelHtml
        + "</div>"
        + "</div>";

    document.getElementById(id).innerHTML = html;

    var searchButton = document.getElementById("wf-search-action");
    var searchText = document.getElementById("wf-search-text");

    searchButton.onclick = function () {
        loadWorkflows();
    };

    searchText.onkeyup = function (event) {
        event.preventDefault();
        if (event.keyCode === 13) { // Enter
            loadWorkflows();
        }
    };

    var workflowsToDelete = [];
    var deleteWorkflowsButton = document.getElementById("wf-delete-workflows");
    deleteWorkflowsButton.onclick = function () {
        //console.log(workflowsToDelete);
        if (workflowsToDelete.length > 0) {
            var confirmRes = confirm("Are you sure you want to delete selected workflows?");
            if (confirmRes === true) {
                window.Common.post(uri + "/delete-workflows", function (res) {
                    if (res === true) {
                        window.Common.toastSuccess("Workflows deleted with success.");
                        workflowsToDelete = [];
                        loadWorkflows();
                        document.getElementById("wf-designer-right-panel").style.display = "none";
                        document.getElementById("wf-xml-container").style.display = "none";
                        document.getElementById("wf-shortcut").style.display = "none";
                        document.getElementById("wf-cancel").style.display = "none";
                        document.getElementById("wf-save").style.display = "none";
                        document.getElementById("wf-delete").style.display = "none";
                        //clearInterval(timer);
                    } else {
                        window.Common.toastError("An error occured while deleting workflows.");
                    }
                }, function () {
                    window.Common.toastError("An error occured while deleting workflows.");
                }, {
                    //"Username": username,
                    //"Password": password,
                    "WorkflowsToDelete": workflowsToDelete
                }, auth);
            }
        }
    };

    // CTRL+S
    window.onkeydown = function (event) {
        if (selectedId !== -1 && newWorkflow === false) { // CTRL+S
            if ((event.ctrlKey || event.metaKey || event.keyCode === 17 || event.keyCode === 224 || event.keyCode === 91 || event.keyCode === 93) && event.keyCode === 83) {
                var selected = document.getElementsByClassName("selected");
                if (selected.length > 0) {
                    var id = parseInt(selected[0].getElementsByClassName("wf-id")[0].innerHTML);
                    //console.log(id);
                    save(currentWorkflowId, id, true, false);
                    event.preventDefault();
                    return false;
                }
            }
        }
    };

    // local variables
    var addVar = function (workflowId) {
        var wfVarsTable = document.getElementsByClassName("wf-local-vars")[0];

        var row = wfVarsTable.insertRow(-1);
        var cell1 = row.insertCell(0);
        var cell2 = row.insertCell(1);
        var cell3 = row.insertCell(2);

        cell1.innerHTML = "<input class='form-control wf-var-key' type='text'>";
        cell2.innerHTML = "<input class='form-control wf-var-value' type='text'>";
        cell2.className = "wf-value";
        cell3.innerHTML = "<button type='button' class='wf-remove-var btn btn-danger btn-xs'>Delete</button>";

        workflowInfos[workflowId].LocalVariables.push({ "Key": "", "Value": "" });

        // events
        var index = workflowInfos[workflowId].LocalVariables.length - 1;

        var wfVarKey = wfVarsTable.getElementsByClassName("wf-var-key")[index];
        wfVarKey.onkeyup = function () {
            index = getElementIndex(wfVarValue.parentElement.parentElement) - 1;
            workflowInfos[workflowId].LocalVariables[index].Key = this.value;
        };

        var wfVarValue = wfVarsTable.getElementsByClassName("wf-var-value")[index];
        wfVarValue.onkeyup = function () {
            index = getElementIndex(wfVarValue.parentElement.parentElement) - 1;
            workflowInfos[workflowId].LocalVariables[index].Value = this.value;
        };

        var btnVarDelete = wfVarsTable.getElementsByClassName("wf-remove-var")[index];
        btnVarDelete.onclick = function () {
            index = getElementIndex(wfVarValue.parentElement.parentElement) - 1;
            workflowInfos[workflowId].LocalVariables = deleteRow(workflowInfos[workflowId].LocalVariables, index);
            this.parentElement.parentElement.remove();
        };
    };

    document.getElementById("wf-add-workflow").onclick = function () {

        window.Common.get(uri + "/workflow-id",
            function (newWorkflowId) {
                newWorkflow = true;
                loadXmlCalled = false;
                loadJsonCalled = false;
                var res = false;

                var xmlContainer = document.getElementById("wf-xml-container");
                var workflowEditor = getEditor(editorWorkflowId);
                //if (typeof workflowEditor !== "undefined") {
                //    editorChanged = true;
                //    var editor = workflowEditor.editor;
                //    var editXml = getEditXml(editorWorkflowId);

                //    if (editXml === true && editorChanged === true) {
                //        res = confirm("The XML of the workflow " + editorWorkflowId + " has changed. Do you want to save it?");
                //        if (res === true) {
                //            save(editorWorkflowId,
                //                editorWorkflowId,
                //                true,
                //                true,
                //                function () {
                //                    editor.setValue("", -1);
                //                    xmlContainer.style.display = "none";
                //                    editorChanged = false;
                //                    setEditXml(editorWorkflowId, false);
                //                    hideThemeButton();
                //                    document.getElementById("wf-xml").disabled = false;
                //                    document.getElementById("wf-json").disabled = false;
                //                });
                //        } else {
                //            editor.setValue("", -1);
                //            xmlContainer.style.display = "none";
                //            setEditXml(editorWorkflowId, false);
                //            editorChanged = false;
                //            editorCanceled = true;
                //        }
                //    } else {
                //        editor.setValue("", -1);
                //        xmlContainer.style.display = "none";
                //        setEditXml(editorWorkflowId, false);
                //        editorChanged = false;
                //    }
                //} else {
                //    editorChanged = false;
                //    setEditXml(editorWorkflowId, false);
                //}

                editorChanged = false;
                setEditXml(editorWorkflowId, false);
                setEditJson(editorWorkflowId, false);

                // Reset editor
                var workflowEditor = getEditor(editorWorkflowId);
                if (typeof workflowEditor !== "undefined") {
                    xmlContainer.style.display = "none";
                    workflowEditor.editor.setValue("", -1);
                    setEditXml(editorWorkflowId, false);
                }

                if (res === false) {
                    selectedId = -1;
                    saveCalled = false;
                    var wfRightPanel = document.getElementById("wf-designer-right-panel");
                    wfRightPanel.innerHTML = rightPanelHtml;
                    wfRightPanel.style.display = "block";
                    document.getElementById("wf-shortcut").style.display = "none";
                    document.getElementById("wf-cancel").style.display = "block";
                    document.getElementById("wf-save").style.display = "block";
                    document.getElementById("wf-add-task").style.display = "block";
                    document.getElementById("wf-delete").style.display = "none";
                    document.getElementById("wf-xml").style.display = "none";
                    document.getElementById("wf-json").style.display = "none";
                    //document.getElementById("wf-theme").style.display = "none";
                    hideThemeButton();
                    document.getElementById("wf-xml-container").style.display = "none";

                    document.getElementById("wf-cancel").onclick = function () {
                        if (saveCalled === true) {
                            var wfIdStr = document.getElementById("wf-id").value;
                            if (isInt(wfIdStr)) {
                                var workflowId = parseInt(wfIdStr);
                                editorCanceled = true;
                                cancel(workflowId);
                            }
                        }
                    };

                    var selected = document.getElementsByClassName("selected");
                    if (selected.length > 0) {
                        selected[0].className = selected[0].className.replace("selected", "");
                    }

                    var previousId = -1;
                    document.getElementById("wf-id").value = newWorkflowId;

                    workflowInfos[newWorkflowId] = {
                        "Id": newWorkflowId,
                        "Name": document.getElementById("wf-name").value,
                        "Description": document.getElementById("wf-desc").value,
                        "LaunchType": launchTypeReverse(document.getElementById("wf-launchType").value),
                        "Period": document.getElementById("wf-period").value,
                        "CronExpression": document.getElementById("wf-cron").value,
                        "IsEnabled": document.getElementById("wf-enabled").checked,
                        "IsApproval": document.getElementById("wf-approval").checked,
                        "EnableParallelJobs": document.getElementById("wf-enable-pj").checked,
                        "LocalVariables": []
                    };

                    workflowTasks[newWorkflowId] = [];
                    selectedId = newWorkflowId;
                    editorWorkflowId = newWorkflowId;

                    document.getElementById("wf-id").onkeyup = function () {
                        var workflowId = parseInt(this.value);

                        if (previousId === -1) {
                            workflowInfos[workflowId] = {
                                "Id": workflowId,
                                "Name": document.getElementById("wf-name").value,
                                "Description": document.getElementById("wf-desc").value,
                                "LaunchType": launchTypeReverse(document.getElementById("wf-launchType").value),
                                "Period": document.getElementById("wf-period").value,
                                "CronExpression": document.getElementById("wf-cron").value,
                                "IsEnabled": document.getElementById("wf-enabled").checked,
                                "IsApproval": document.getElementById("wf-approval").checked,
                                "EnableParallelJobs": document.getElementById("wf-enable-pj").checked,
                                "LocalVariables": []
                            };

                            workflowTasks[workflowId] = [];
                        } else {
                            workflowInfos[workflowId] = workflowInfos[previousId];
                            workflowInfos[workflowId].Id = workflowId;
                            workflowTasks[workflowId] = workflowTasks[previousId];
                        }
                        previousId = workflowId;
                        selectedId = workflowId;
                        editorWorkflowId = workflowId;
                    };

                    // Input events
                    document.getElementById("wf-name").onkeyup = function () {
                        var that = this;
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].Name = this.value;

                            //if (this.value !== "" && saveCalled === false) {
                            //    window.Common.get(uri + "/workflowsFolder",
                            //        function (workflowsFolder) {

                            //            if (osname === "Linux" || osname === "UNIX" || osname === "Mac/iOS") {
                            //                workflowInfos[workflowId].Path = window.Common.trimEnd(workflowsFolder, "/") + "/" + that.value + ".xml";
                            //                document.getElementById("wf-path").innerHTML = workflowInfos[workflowId].Path;
                            //            } else {
                            //                workflowInfos[workflowId].Path = window.Common.trimEnd(workflowsFolder, "\\") + "\\" + that.value + ".xml";
                            //                document.getElementById("wf-path").innerHTML = workflowInfos[workflowId].Path;
                            //            }
                            //        },
                            //        function () {
                            //            window.Common.toastError("An error occured while retrieving workflowsFolder.");
                            //        });
                            //}
                        }
                    };

                    document.getElementById("wf-launchType").onchange = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].LaunchType = launchTypeReverse(this.value);
                        }
                    };

                    document.getElementById("wf-period").onkeyup = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].Period = this.value;
                        }
                    };

                    document.getElementById("wf-cron").onkeyup = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].CronExpression = this.value;
                        }
                    };

                    document.getElementById("wf-enabled").onchange = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].IsEnabled = this.checked;
                        }
                    };

                    document.getElementById("wf-approval").onchange = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].IsApproval = this.checked;
                        }
                    };

                    document.getElementById("wf-enable-pj").onchange = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].EnableParallelJobs = this.checked;
                        }
                    };

                    document.getElementById("wf-desc").onkeyup = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            workflowInfos[workflowId].Description = this.value;
                        }
                    };

                    document.getElementById("wf-delete").onclick = function () {
                        document.getElementById("wf-delete").disabled = true;
                        deleteWorkflow();
                    };

                    document.getElementById("wf-save").onclick = function () {
                        saveClick(true, newWorkflow);
                    };

                    // Local variables
                    document.getElementById("wf-add-var").onclick = function () {
                        var wfIdStr = document.getElementById("wf-id").value;
                        if (isInt(wfIdStr)) {
                            var workflowId = parseInt(wfIdStr);
                            document.getElementsByClassName("wf-local-vars")[0].style.display = "table";
                            addVar(workflowId);
                        } else {
                            window.Common.toastInfo("Please enter a valid workflow id.");
                        }
                    };

                    window.Common.get(uri + "/task-names",
                        function (taskNames) {
                            document.getElementById("wf-add-task").onclick = function () {

                                var wfIdStr = document.getElementById("wf-id").value;
                                if (isInt(wfIdStr)) {
                                    var workflowId = parseInt(wfIdStr);
                                    addTask(workflowId, taskNames);
                                } else {
                                    window.Common.toastInfo("Please enter a valid workflow id.");
                                }
                            };
                        },
                        function () {
                            window.Common.toastError("An error occured while retrieving task names.");
                        }, auth);
                }
            },
            function () {
                window.Common.toastError("An error occured while getting a new workflow id.");
            }, auth);

    };

    function saveClick(checkId, scrollToWorkflow) {

        var wfSelectedWorkflow = document.getElementsByClassName("selected");
        if (wfSelectedWorkflow.length > 0) {
            selectedId = parseInt(wfSelectedWorkflow[0].getElementsByClassName("wf-id")[0].innerHTML);
        }

        var wfIdStr = document.getElementById("wf-id").value;
        if (isInt(wfIdStr)) {
            var workflowId = parseInt(wfIdStr);

            if (checkId === true) {
                window.Common.get(uri + "/is-workflow-id-valid/" + workflowId,
                    function (res) {
                        if (res === true || saveCalled === true) {
                            if (document.getElementById("wf-name").value === "") {
                                window.Common.toastInfo("Enter a name for this workflow.");
                            } else {
                                var lt = document.getElementById("wf-launchType").value;
                                if (lt === "") {
                                    window.Common.toastInfo("Select a launchType for this workflow.");
                                } else {
                                    if (lt === "periodic" && document.getElementById("wf-period").value === "") {
                                        window.Common.toastInfo("Enter a period for this workflow.");
                                    } else {
                                        if (lt === "cron" && document.getElementById("wf-cron").value === "") {
                                            window.Common.toastInfo("Enter a cron expression for this workflow.");
                                        } else {
                                            var saveFunc = function () {
                                                save(workflowId,
                                                    selectedId === -1 ? workflowId : selectedId,
                                                    true,
                                                    scrollToWorkflow,
                                                    function () {
                                                        newWorkflow = false;
                                                        saveCalled = true;

                                                        document.getElementById("wf-xml").style.display = "inline-block";
                                                        document.getElementById("wf-json").style.display = "inline-block";
                                                        //showThemeButton();
                                                        document.getElementById("wf-shortcut").style.display = "block";
                                                        document.getElementById("wf-cancel").style.display = "block";
                                                        document.getElementById("wf-save").style.display = "block";
                                                        document.getElementById("wf-delete").style.display = "block";
                                                    });
                                            };

                                            // Period validation
                                            if (lt === "periodic" && document.getElementById("wf-period").value !== "") {
                                                var period = document.getElementById("wf-period").value;
                                                window.Common.get(uri + "/is-period-valid/" + period,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            window.Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } // Cron expression validation
                                            else if (lt === "cron" && document.getElementById("wf-cron").value !== "") {
                                                var expression = document.getElementById("wf-cron").value;
                                                var expressionEncoded = encodeURIComponent(expression);

                                                window.Common.get(uri + "/is-cron-expression-valid?e=" + expressionEncoded,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                            }
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } else {
                                                saveFunc();
                                            }

                                        }
                                    }
                                }
                            }
                        } else {
                            window.Common.toastInfo("The workflow id is already in use. Enter another one.");
                        }
                    },
                    function () { }, auth
                );
            } else {

                if (document.getElementById("wf-name").value === "") {
                    window.Common.toastInfo("Enter a name for this workflow.");
                } else {
                    var lt = document.getElementById("wf-launchType").value;
                    if (lt === "") {
                        window.Common.toastInfo("Select a launchType for this workflow.");
                    } else {
                        if (lt === "periodic" && document.getElementById("wf-period").value === "") {
                            window.Common.toastInfo("Enter a period for this workflow.");
                        } else {
                            if (lt === "cron" && document.getElementById("wf-cron").value === "") {
                                window.Common.toastInfo("Enter a cron expression for this workflow.");
                            } else {
                                var saveFunc = function () {
                                    save(workflowId,
                                        selectedId === -1 ? workflowId : selectedId,
                                        true,
                                        scrollToWorkflow,
                                        function () {
                                            newWorkflow = false;
                                            saveCalled = true;

                                            document.getElementById("wf-xml").style.display = "inline-block";
                                            document.getElementById("wf-json").style.display = "inline-block";
                                            //showThemeButton();
                                            document.getElementById("wf-shortcut").style.display = "block";
                                            document.getElementById("wf-cancel").style.display = "block";
                                            document.getElementById("wf-save").style.display = "block";
                                            document.getElementById("wf-delete").style.display = "block";
                                        });
                                };

                                // Period validation
                                if (lt === "periodic" && document.getElementById("wf-period").value !== "") {
                                    var period = document.getElementById("wf-period").value;
                                    window.Common.get(uri + "/is-period-valid/" + period,
                                        function (res) {
                                            if (res === true) {
                                                saveFunc();
                                            } else {
                                                window.Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                            }
                                        },
                                        function () { }, auth
                                    );
                                } // Cron expression validation
                                else if (lt === "cron" && document.getElementById("wf-cron").value !== "") {
                                    var expression = document.getElementById("wf-cron").value;
                                    var expressionEncoded = encodeURIComponent(expression);

                                    window.Common.get(uri + "/is-cron-expression-valid?e=" + expressionEncoded,
                                        function (res) {
                                            if (res === true) {
                                                saveFunc();
                                            } else {
                                                if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                    openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                }
                                            }
                                        },
                                        function () { }, auth
                                    );
                                } else {
                                    saveFunc();
                                }

                            }
                        }
                    }
                }

            }

        } else {
            window.Common.toastInfo("Enter a valid workflow id.");
        }
    }

    function openInNewTab(url) {
        var win = window.open(url, "_blank");
        if (typeof win !== "undefined" && win !== null) {
            win.focus();
        }
    }

    function save(workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback) {
        currentWorkflowId = workflowId;
        document.getElementById("wf-save").disabled = true;
        //clearInterval(timer);
        workflowInfos[workflowId] = workflowInfos[selectedWorkflowId];
        workflowTasks[workflowId] = workflowTasks[selectedWorkflowId];

        var workflowEditor = getEditor(workflowId);
        var editXml = getEditXml(workflowId);
        var editJson = getEditJson(workflowId);

        //console.log("workflowId: " + workflowId + ", editXml: " + editXml + ", workflowEditor: " + workflowEditor);
        if (editXml === true && typeof workflowEditor !== "undefined") { // XML editing
            //console.log("save-xml");
            var editor = workflowEditor.editor;
            var xml = editor.getValue();
            validateAndSaveXml(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
        } else if (editJson === true && typeof workflowEditor !== "undefined") { // XML editing
            //console.log("saveJson");
            var editor = workflowEditor.editor;
            var json = JSON.parse(editor.getValue());
            //console.log(json);
            saveWorkflow(workflowId, selectedWorkflowId, scrollToWorkflow, callback, json);
        } else {
            //console.log("save");
            saveWorkflow(workflowId, selectedWorkflowId, scrollToWorkflow, callback);
        }

    }

    function validateAndSaveXml(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback) {
        window.Common.post(uri + "/is-xml-workflow-valid", function (res) {
            if (res === true) {
                saveXml(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
                retries = 0;
            } else {
                window.Common.toastError("The XML of the workflow " + workflowId + " is not valid.");
                setEditXml(workflowId, true);
                editorChanged = false;
                document.getElementById("wf-save").disabled = false;
            }
        }, function () {
            //window.Common.toastError("An error occured while saving the workflow " + workflowId + " from XML.");
            if (retries < maxRetries) {
                //console.log("validateAndSaveXml.error");
                setTimeout(function () {
                    validateAndSaveXml(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
                }, timeoutInterval);
                retries++;
            } else {
                document.getElementById("wf-save").disabled = false;
                window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                retries = 0;
            }

        }, { "xml": xml }, auth);   // End of isXmlWorkflowValid.
    }

    function saveXml(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback) {
        //console.log("save-xml");
        var parser = new DOMParser();
        var xmlDoc = parser.parseFromString(xml, "text/xml");
        var vcurrentWorkflowId = parseInt(xmlDoc.getElementsByTagName("Workflow")[0].getAttribute("id"));

        if (vcurrentWorkflowId !== selectedWorkflowId) {  // Check currentWorkflowId.
            window.Common.get(uri + "/is-workflow-id-valid/" + vcurrentWorkflowId, function (res) {
                if (res === true) {
                    saveXmlQuery(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
                } else {
                    document.getElementById("wf-save").disabled = false;
                    window.Common.toastInfo("The workflow id " + vcurrentWorkflowId + " is already in use. Enter another one.");
                }
            },
                function () { }, auth
            );
        } else {
            saveXmlQuery(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
        }
    }

    function saveXmlQuery(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback) {
        var json = {
            workflowId: selectedWorkflowId,
            //username: username,
            //password: password,
            xml: xml
        };
        window.Common.post(uri + "/save-xml",
            function (res) {
                if (res === true) {

                    var parser = new DOMParser();
                    var xmlDoc = parser.parseFromString(xml, "text/xml");
                    currentWorkflowId = parseInt(xmlDoc.getElementsByTagName("Workflow")[0].getAttribute("id"));
                    editorWorkflowId = currentWorkflowId;
                    workflowInfos[currentWorkflowId] = workflowInfos[workflowId];
                    workflowTasks[currentWorkflowId] = workflowTasks[workflowId];

                    if (workflowId !== currentWorkflowId) {
                        deleteEditor(workflowId);
                        var currentEditor = getEditor(currentWorkflowId);

                        if (typeof currentEditor === "undefined") {
                            var editor = ace.edit("wf-xml-container");
                            editor.setOptions({
                                maxLines: Infinity
                            });
                            if (isDarkTheme === true) {
                                editor.setTheme("ace/theme/pastel_on_dark");
                            } else {
                                editor.setTheme("ace/theme/github");
                            }
                            editor.setReadOnly(false);
                            editor.setFontSize("100%");
                            editor.setPrintMarginColumn(false);
                            editor.getSession().setMode("ace/mode/xml");

                            editor.commands.addCommand({
                                name: "showKeyboardShortcuts",
                                bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                                exec: function (editor) {
                                    ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                                        module.init(editor);
                                        editor.showKeyboardShortcuts()
                                    })
                                }
                            });

                            var xmlValue = editor.setValue(xml, -1);
                            editor.clearSelection();
                            editor.resize(true);
                            editor.focus();

                            editor.on("change", function () {
                                if (loadXmlCalled) {
                                    xmlEditorOnChange(currentWorkflowId);
                                } else if (loadJsonCalled) {
                                    jsonEditorOnChange(currentWorkflowId);
                                }
                            });

                            editors.set(currentWorkflowId, { editor: editor, editXml: false, value: xmlValue });
                        }

                        window.Common.post(uri + "/delete?w=" + workflowId,
                            function (res) {
                                if (res === true) {
                                    window.Common.toastSuccess("Workflow " + workflowId + " deleted with success.");

                                    loadWorkflows(function () {
                                        // Select the workflow
                                        if (selectWorkflow === true || workflowId !== currentWorkflowId) {

                                            var wfWorkflowsTable = document.getElementById("wf-workflows-table");

                                            for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                                                var row = wfWorkflowsTable.rows[i];
                                                var wfId = row.getElementsByClassName("wf-id")[0];
                                                if (typeof wfId !== "undefined" && wfId !== null) {
                                                    var swId = parseInt(wfId.innerHTML);

                                                    if (swId === currentWorkflowId) {

                                                        var selected = document.getElementsByClassName("selected");
                                                        if (selected.length > 0) {
                                                            selected[0].className = selected[0].className.replace("selected", "");
                                                        }

                                                        row.className += "selected";

                                                        if (scrollToWorkflow === true || currentWorkflowId !== selectedWorkflowId) {
                                                            // Scroll to the workflow
                                                            row.scrollIntoView(true);
                                                        }

                                                    }
                                                }
                                            }

                                            // Update the status
                                            //updateWorkflowStatus(currentWorkflowId);

                                            // Show the xml button
                                            document.getElementById("wf-xml").style.display = "inline";
                                            document.getElementById("wf-json").style.display = "inline";

                                            document.getElementById("wf-xml").onclick = function () {
                                                document.getElementById("wf-xml").disabled = true;
                                                document.getElementById("wf-json").disabled = false;
                                                loadXml(currentWorkflowId);
                                            };

                                            document.getElementById("wf-json").onclick = function () {
                                                document.getElementById("wf-xml").disabled = false;
                                                document.getElementById("wf-json").disabled = true;
                                                loadJson(currentWorkflowId);
                                            };

                                            // Reload right panel
                                            loadRightPanel(currentWorkflowId, false);

                                            document.getElementById("wf-xml-shortcut").style.display = "block";
                                            //document.getElementById("wf-theme").style.display = "block";
                                            showThemeButton();
                                        }

                                        // Reset editor
                                        if (typeof workflowEditor !== "undefined") {
                                            workflowChangedAndSaved = true;
                                        }

                                        document.getElementById("wf-xml").onclick = function () {
                                            document.getElementById("wf-xml").disabled = true;
                                            document.getElementById("wf-json").disabled = false;
                                            loadXml(currentWorkflowId);
                                        };

                                        document.getElementById("wf-json").onclick = function () {
                                            document.getElementById("wf-json").disabled = true;
                                            document.getElementById("wf-xml").disabled = false;
                                            loadJson(currentWorkflowId);
                                        };

                                        setEditXml(workflowId, false);
                                        setEditXml(currentWorkflowId, false);
                                        editorChanged = false;

                                        if (typeof callback !== "undefined") {
                                            callback();
                                        }
                                        document.getElementById("wf-save").disabled = false;
                                        retries = 0;
                                        window.Common.toastSuccess("workflow " + currentWorkflowId + " saved and loaded with success.");
                                    }, currentWorkflowId, true);

                                } else {
                                    window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                                }
                            }, function () {
                                window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                            }, "", auth);

                    } else {
                        loadWorkflows(function () {
                            // Select the workflow
                            if (selectWorkflow === true || workflowId !== currentWorkflowId) {

                                var wfWorkflowsTable = document.getElementById("wf-workflows-table");

                                for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                                    var row = wfWorkflowsTable.rows[i];
                                    var wfId = row.getElementsByClassName("wf-id")[0];
                                    if (typeof wfId !== "undefined" && wfId !== null) {
                                        var swId = parseInt(wfId.innerHTML);

                                        if (swId === currentWorkflowId) {

                                            var selected = document.getElementsByClassName("selected");
                                            if (selected.length > 0) {
                                                selected[0].className = selected[0].className.replace("selected", "");
                                            }

                                            row.className += "selected";

                                            if (scrollToWorkflow === true || currentWorkflowId !== selectedWorkflowId) {
                                                // Scroll to the workflow
                                                row.scrollIntoView(true);
                                            }

                                        }
                                    }
                                }

                                // Update the status
                                //updateWorkflowStatus(currentWorkflowId);

                                // Show the xml button
                                document.getElementById("wf-xml").style.display = "inline";
                                document.getElementById("wf-json").style.display = "inline";

                                document.getElementById("wf-xml").onclick = function () {
                                    document.getElementById("wf-xml").disabled = true;
                                    document.getElementById("wf-json").disabled = false;
                                    loadXml(currentWorkflowId);
                                };

                                document.getElementById("wf-json").onclick = function () {
                                    document.getElementById("wf-xml").disabled = false;
                                    document.getElementById("wf-json").disabled = true;
                                    loadJson(currentWorkflowId);
                                };

                                // Reload right panel
                                loadRightPanel(currentWorkflowId, false);

                                document.getElementById("wf-xml-shortcut").style.display = "block";
                                //document.getElementById("wf-theme").style.display = "block";
                                showThemeButton();
                            }

                            // Reset editor
                            if (typeof workflowEditor !== "undefined") {
                                workflowChangedAndSaved = true;
                            }

                            document.getElementById("wf-xml").onclick = function () {
                                document.getElementById("wf-xml").disabled = true;
                                document.getElementById("wf-json").disabled = false;
                                loadXml(currentWorkflowId);
                            };

                            document.getElementById("wf-json").onclick = function () {
                                document.getElementById("wf-json").disabled = true;
                                document.getElementById("wf-xml").disabled = false;
                                loadJson(currentWorkflowId);
                            };

                            setEditXml(workflowId, false);
                            setEditXml(currentWorkflowId, false);
                            editorChanged = false;

                            if (typeof callback !== "undefined") {
                                callback();
                            }
                            document.getElementById("wf-save").disabled = false;
                            retries = 0;
                            window.Common.toastSuccess("workflow " + currentWorkflowId + " saved and loaded with success.");
                        }, currentWorkflowId, true);
                    }

                    // Reload workflows list
                    //setTimeout(function () {
                    //}, timeoutInterval);
                } else {
                    //window.Common.toastError("An error occured while saving the workflow " + workflowId + " from XML.");
                    if (retries < maxRetries) {
                        //console.log("saveXml.error");
                        setTimeout(function () {
                            saveXmlQuery(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
                        }, timeoutInterval);
                        retries++;
                    } else {
                        document.getElementById("wf-save").disabled = false;
                        window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                        retries = 0;
                    }
                }
            }, function () {
                //window.Common.toastError("An error occured while saving the workflow " + workflowId + " from XML.");
                if (retries < maxRetries) {
                    //console.log("saveXml.http.error");
                    setTimeout(function () {
                        saveXmlQuery(xml, workflowId, selectedWorkflowId, selectWorkflow, scrollToWorkflow, callback);
                    }, timeoutInterval);
                    retries++;
                } else {
                    document.getElementById("wf-save").disabled = false;
                    window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                    retries = 0;
                }
            }, json, auth);  // End of saveXml.
    }

    function saveWorkflow(workflowId, selectedWorkflowId, scrollToWorkflow, callback, jsonWorkflow) {
        var vcurrentWorkflowId = parseInt(document.getElementById("wf-id").value);

        if (typeof jsonWorkflow !== "undefined") {
            vcurrentWorkflowId = jsonWorkflow.WorkflowInfo.Id;
        }

        if (vcurrentWorkflowId !== selectedWorkflowId) {  // Check currentWorkflowId.
            window.Common.get(uri + "/is-workflow-id-valid/" + vcurrentWorkflowId, function (res) {
                if (res === true) {
                    saveWorkflowQuery(workflowId, selectedWorkflowId, scrollToWorkflow, callback, jsonWorkflow);
                } else {
                    document.getElementById("wf-save").disabled = false;
                    window.Common.toastInfo("The workflow id " + vcurrentWorkflowId + " is already in use. Enter another one.");
                }
            },
                function () { }, auth
            );
        } else {
            saveWorkflowQuery(workflowId, selectedWorkflowId, scrollToWorkflow, callback, jsonWorkflow);
        }
    }

    function saveWorkflowQuery(workflowId, selectedWorkflowId, scrollToWorkflow, callback, jsonWorkflow) {
        var workflowEditor = getEditor(currentWorkflowId);
        var json = jsonWorkflow;
        if (typeof jsonWorkflow === "undefined") {
            json = {
                "WorkflowInfo": workflowInfos[workflowId],
                "Tasks": workflowTasks[workflowId]
            };
        }

        window.Common.post(uri + "/save",
            function (res) {
                if (res === true) {
                    if (typeof callback !== "undefined") {
                        callback();
                    }

                    if (typeof jsonWorkflow !== "undefined") {
                        currentWorkflowId = jsonWorkflow.WorkflowInfo.Id;
                    } else {
                        currentWorkflowId = parseInt(document.getElementById("wf-id").value);
                    }

                    if (workflowId !== currentWorkflowId) {
                        //console.log("deleteWorkflow");
                        window.Common.post(uri + "/delete?w=" + workflowId,
                            function (res) {
                                if (res === true) {
                                    window.Common.toastSuccess("Workflow " + workflowId + " deleted with success.");

                                    loadWorkflows(function () {

                                        editorWorkflowId = currentWorkflowId;
                                        workflowInfos[currentWorkflowId] = workflowInfos[workflowId];
                                        workflowTasks[currentWorkflowId] = workflowTasks[workflowId];

                                        if (workflowId !== currentWorkflowId || typeof workflowEditor === "undefined") {
                                            //deleteEditor(workflowId);
                                            var currentEditor = getEditor(currentWorkflowId);

                                            if (typeof currentEditor === "undefined") {
                                                var editor = ace.edit("wf-xml-container");
                                                editor.setOptions({
                                                    maxLines: Infinity
                                                });
                                                if (isDarkTheme === true) {
                                                    editor.setTheme("ace/theme/pastel_on_dark");
                                                } else {
                                                    editor.setTheme("ace/theme/github");
                                                }
                                                editor.setReadOnly(false);
                                                editor.setFontSize("100%");
                                                editor.setPrintMarginColumn(false);
                                                editor.getSession().setMode("ace/mode/xml");

                                                editor.commands.addCommand({
                                                    name: "showKeyboardShortcuts",
                                                    bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                                                    exec: function (editor) {
                                                        ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                                                            module.init(editor);
                                                            editor.showKeyboardShortcuts()
                                                        })
                                                    }
                                                });

                                                var xmlValue = editor.setValue("", -1);
                                                editor.clearSelection();
                                                editor.resize(true);
                                                editor.focus();

                                                editor.on("change", function () {
                                                    if (loadXmlCalled) {
                                                        xmlEditorOnChange(currentWorkflowId);
                                                    } else if (loadJsonCalled) {
                                                        jsonEditorOnChange(currentWorkflowId);
                                                    }
                                                });

                                                editors.set(currentWorkflowId, { editor: editor, editXml: false, value: xmlValue });

                                                workflowEditor = getEditor(currentWorkflowId);
                                            }
                                        }

                                        // Select the workflow
                                        var wfWorkflowsTable = document.getElementById("wf-workflows-table");

                                        for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                                            var row = wfWorkflowsTable.rows[i];
                                            var wfId = row.getElementsByClassName("wf-id")[0];
                                            if (typeof wfId !== "undefined" && wfId !== null) {
                                                var swId = parseInt(wfId.innerHTML);

                                                if (swId === currentWorkflowId) {
                                                    var selected = document.getElementsByClassName("selected");
                                                    if (selected.length > 0) {
                                                        selected[0].className = selected[0].className.replace("selected", "");
                                                    }

                                                    row.className += "selected";

                                                    if (scrollToWorkflow === true || currentWorkflowId !== selectedWorkflowId) {
                                                        // Scroll to the workflow
                                                        row.scrollIntoView(true);
                                                    }
                                                }
                                            }
                                        }

                                        // Update the status
                                        //updateWorkflowStatus(currentWorkflowId);

                                        // Reload XML
                                        if (typeof workflowEditor !== "undefined" && loadXmlCalled === true) {
                                            loadXml(selectedWorkflowId);
                                        } else if (typeof workflowEditor !== "undefined" && loadJsonCalled === true) {
                                            loadJson(selectedWorkflowId);
                                        }

                                        document.getElementById("wf-xml").style.display = "inline";
                                        document.getElementById("wf-json").style.display = "inline";

                                        document.getElementById("wf-xml").onclick = function () {
                                            document.getElementById("wf-xml").disabled = true;
                                            document.getElementById("wf-json").disabled = false;
                                            loadXml(currentWorkflowId);
                                        };

                                        document.getElementById("wf-json").onclick = function () {
                                            document.getElementById("wf-json").disabled = true;
                                            document.getElementById("wf-xml").disabled = false;
                                            loadJson(currentWorkflowId);
                                        };

                                        document.getElementById("wf-shortcut").style.display = "block";
                                        document.getElementById("wf-delete").style.display = "block";

                                        if (typeof jsonWorkflow !== "undefined") {
                                            // Reload right panel
                                            loadRightPanel(currentWorkflowId, false);
                                        }

                                        editorChanged = false;
                                        setEditXml(workflowId, false);
                                        setEditJson(workflowId, false);
                                        document.getElementById("wf-save").disabled = false;
                                        retries = 0;
                                        window.Common.toastSuccess("workflow " + currentWorkflowId + " saved and loaded with success.");
                                    }, workflowId, true);

                                } else {
                                    window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                                }
                            },
                            function () {
                                window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                            }, "", auth);

                    } else {
                        loadWorkflows(function () {

                            editorWorkflowId = currentWorkflowId;
                            workflowInfos[currentWorkflowId] = workflowInfos[workflowId];
                            workflowTasks[currentWorkflowId] = workflowTasks[workflowId];

                            if (workflowId !== currentWorkflowId || typeof workflowEditor === "undefined") {
                                //deleteEditor(workflowId);
                                var currentEditor = getEditor(currentWorkflowId);

                                if (typeof currentEditor === "undefined") {
                                    var editor = ace.edit("wf-xml-container");
                                    editor.setOptions({
                                        maxLines: Infinity
                                    });
                                    if (isDarkTheme === true) {
                                        editor.setTheme("ace/theme/pastel_on_dark");
                                    } else {
                                        editor.setTheme("ace/theme/github");
                                    }
                                    editor.setReadOnly(false);
                                    editor.setFontSize("100%");
                                    editor.setPrintMarginColumn(false);
                                    editor.getSession().setMode("ace/mode/xml");

                                    editor.commands.addCommand({
                                        name: "showKeyboardShortcuts",
                                        bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                                        exec: function (editor) {
                                            ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                                                module.init(editor);
                                                editor.showKeyboardShortcuts()
                                            })
                                        }
                                    });

                                    var xmlValue = editor.setValue("", -1);
                                    editor.clearSelection();
                                    editor.resize(true);
                                    editor.focus();

                                    editor.on("change", function () {
                                        if (loadXmlCalled) {
                                            xmlEditorOnChange(currentWorkflowId);
                                        } else if (loadJsonCalled) {
                                            jsonEditorOnChange(currentWorkflowId);
                                        }
                                    });

                                    editors.set(currentWorkflowId, { editor: editor, editXml: false, value: xmlValue });

                                    workflowEditor = getEditor(currentWorkflowId);
                                }
                            }

                            // Select the workflow
                            var wfWorkflowsTable = document.getElementById("wf-workflows-table");

                            for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                                var row = wfWorkflowsTable.rows[i];
                                var wfId = row.getElementsByClassName("wf-id")[0];
                                if (typeof wfId !== "undefined" && wfId !== null) {
                                    var swId = parseInt(wfId.innerHTML);

                                    if (swId === currentWorkflowId) {
                                        var selected = document.getElementsByClassName("selected");
                                        if (selected.length > 0) {
                                            selected[0].className = selected[0].className.replace("selected", "");
                                        }

                                        row.className += "selected";

                                        if (scrollToWorkflow === true || currentWorkflowId !== selectedWorkflowId) {
                                            // Scroll to the workflow
                                            row.scrollIntoView(true);
                                        }
                                    }
                                }
                            }

                            // Update the status
                            //updateWorkflowStatus(currentWorkflowId);

                            // Reload XML
                            if (typeof workflowEditor !== "undefined" && loadXmlCalled === true) {
                                loadXml(selectedWorkflowId);
                            } else if (typeof workflowEditor !== "undefined" && loadJsonCalled === true) {
                                loadJson(selectedWorkflowId);
                            }

                            document.getElementById("wf-xml").style.display = "inline";
                            document.getElementById("wf-json").style.display = "inline";

                            document.getElementById("wf-xml").onclick = function () {
                                document.getElementById("wf-xml").disabled = true;
                                document.getElementById("wf-json").disabled = false;
                                loadXml(currentWorkflowId);
                            };

                            document.getElementById("wf-json").onclick = function () {
                                document.getElementById("wf-json").disabled = true;
                                document.getElementById("wf-xml").disabled = false;
                                loadJson(currentWorkflowId);
                            };

                            document.getElementById("wf-shortcut").style.display = "block";
                            document.getElementById("wf-delete").style.display = "block";

                            if (typeof jsonWorkflow !== "undefined") {
                                // Reload right panel
                                loadRightPanel(currentWorkflowId, false);
                            }

                            editorChanged = false;
                            setEditXml(workflowId, false);
                            document.getElementById("wf-save").disabled = false;
                            retries = 0;
                            window.Common.toastSuccess("workflow " + currentWorkflowId + " saved and loaded with success.");
                        }, workflowId, true);
                    }

                    //currentWorkflowId = workflowId;

                    // Reload workflows list
                    //setTimeout(function () {

                    //}, timeoutInterval);
                } else {
                    //window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                    if (retries < maxRetries) {
                        //console.log("saveWorkflow.error");
                        setTimeout(function () {
                            saveWorkflowQuery(workflowId, selectedWorkflowId, scrollToWorkflow, callback);
                        }, timeoutInterval);
                        retries++;
                    } else {
                        document.getElementById("wf-save").disabled = false;
                        window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                        retries = 0;
                    }
                }
            }, function () {
                //window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                if (retries < maxRetries) {
                    //console.log("saveWorkflow.http.error");
                    setTimeout(function () {
                        saveWorkflowQuery(workflowId, selectedWorkflowId, scrollToWorkflow, callback);
                    }, timeoutInterval);
                    retries++;
                } else {
                    document.getElementById("wf-save").disabled = false;
                    window.Common.toastError("An error occured while saving the workflow " + workflowId + ".");
                    retries = 0;
                }
            }, json, auth);
    }

    //function updateWorkflowStatus(workflowId) {
    //    window.Common.get(uri + "/workflow?u=" + encodeURIComponent(username) + "&p=" + encodeURIComponent(password) + "&w=" + workflowId,
    //        function (workflow) {
    //            //console.log("updateWorkflowStatus.workflowId: " + workflowId);
    //            if (typeof workflow !== "undefined") {
    //                updateStatusTimer(workflow);
    //                statusRetries = 0;
    //            } else {
    //                if (statusRetries < maxRetries) {
    //                    setTimeout(function () {
    //                        //console.log("updateWorkflowStatus.error");
    //                        updateWorkflowStatus(workflowId);
    //                    }, timeoutInterval);
    //                    statusRetries++;
    //                } else {
    //                    statusRetries = 0;
    //                }
    //            }
    //        });
    //}

    //function updateStatusTimer(workflow) {
    //    clearInterval(timer);

    //    if (workflow.IsEnabled === true) {
    //        timer = setInterval(function () {
    //            updateStatus(workflow.Id, false);
    //        }, timerInterval);

    //        updateStatus(workflow.Id, true);
    //    } else {
    //        updateStatus(workflow.Id, true);
    //    }
    //}

    //function workflowStatusChanged(workflow) {
    //    var changed = workflows[workflow.Id].IsRunning !== workflow.IsRunning || workflows[workflow.Id].IsPaused !== workflow.IsPaused || workflows[workflow.Id].IsWaitingForApproval !== workflow.IsWaitingForApproval;
    //    workflows[workflow.Id].IsRunning = workflow.IsRunning;
    //    workflows[workflow.Id].IsPaused = workflow.IsPaused;
    //    workflows[workflow.Id].IsWaitingForApproval = workflow.IsWaitingForApproval;
    //    return changed;
    //}

    //function updateStatus(workflowId, force) {
    //    getWorkflow(workflowId,
    //        function (workflow) {
    //            if (typeof workflow !== "undefined") {
    //                if (workflow.IsEnabled === false) {
    //                    notify("This workflow is disabled.");
    //                } else {
    //                    if (force === false && workflowStatusChanged(workflow) === false) return;

    //                    if (workflow.IsApproval === true && workflow.IsWaitingForApproval === true && workflow.IsPaused === false) {
    //                        notify("This workflow is waiting for approval...");
    //                    } else {
    //                        if (workflow.IsRunning === true && workflow.IsPaused === false) {
    //                            notify("This workflow is running...");
    //                        }
    //                        else if (workflow.IsPaused === true) {
    //                            notify("This workflow is suspended.");
    //                        } else {
    //                            notify("");
    //                        }
    //                    }
    //                }
    //            }
    //        });
    //}

    //function notify(status) {
    //    document.getElementById("wf-status").innerHTML = status;
    //}

    function deleteEditor(workflowId) {
        editors.delete(workflowId);
    }

    function deleteWorkflow() {
        var r = confirm("Are you sure you want to delete this workflow?");
        if (r === true) {
            var workflowId = parseInt(document.getElementById("wf-id").value);

            window.Common.post(uri + "/delete?w=" + workflowId,
                function (res) {
                    if (res === true) {
                        window.Common.toastSuccess("Workflow " + workflowId + " deleted with success.");
                        //clearInterval(timer);
                        setTimeout(function () {
                            loadWorkflows();
                            document.getElementById("wf-designer-right-panel").style.display = "none";
                            document.getElementById("wf-xml-container").style.display = "none";
                            document.getElementById("wf-shortcut").style.display = "none";
                            document.getElementById("wf-cancel").style.display = "none";
                            document.getElementById("wf-save").style.display = "none";
                            document.getElementById("wf-delete").style.display = "none";
                            deleteEditor(workflowId);
                            loadXmlCalled = false;
                            loadJsonCalled = false;
                            document.getElementById("wf-delete").disabled = false;
                        }, timeoutInterval);
                    } else {
                        window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                        document.getElementById("wf-delete").disabled = false;
                    }
                },
                function () {
                    window.Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                    document.getElementById("wf-delete").disabled = false;
                }, "", auth);
        } else {
            document.getElementById("wf-delete").disabled = false;
        }
    }

    var collapseCreateId = 0;
    function addTask(workflowId, taskNames) {
        var wfTask = document.createElement("div");
        wfTask.className = "wf-task";
        var newTaskHtml =
            "<h5 class='wf-task-title'>" +
            "<a class='wf-task-title-label colpsible-panel' data-toggle='collapse' data-parent='#accordion' style='font-weight: bold' href='#collapseCreate" + collapseCreateId + "'>" +
            //"<label class='wf-task-title-label'>Task</label>" +
            "Task" +
            "</a>" +
            "<button type='button' class='wf-remove-task btn btn-danger btn-xs' style='display: block;'>Delete</button>" +
            "<button type='button' class='wf-show-doc btn btn-dark btn-xs'>Documentation</button>" +
            "<button type='button' class='wf-show-taskxml btn btn-dark btn-xs'>Xml</button>" +
            "<button type='button' class='wf-add-setting btn btn-dark btn-xs'>New setting</button>" +
            "</h5>" +
            "<div id='collapseCreate" + collapseCreateId + "' class='panel-collapse collapse in'>" +
            "<table id='collapseOne' class='wf-designer-table'>" +
            "<tbody>" +
            "<tr><td class='wf-taskxml' colspan='2'><pre><code class='wf-taskxml-container'></code></pre></td></tr>" +
            "<tr><td class='wf-title'>Id</td><td class='wf-value'><input class='form-control wf-task-id' type='text' /></td></tr>" +
            "<tr><td class='wf-title'>Name</td><td class='wf-value'><select class='form-control wf-task-name'>";

        newTaskHtml += "<option value=''></option>";
        for (var i1 = 0; i1 < taskNames.length; i1++) {
            var taskName = taskNames[i1];
            newTaskHtml += "<option value='" + taskName.Name + "'>" + taskName.Name + "</option>";
        }

        newTaskHtml += "</select></td></tr>" +
            "<tr><td class='wf-title'>Description</td><td class='wf-value'><input class='form-control wf-task-desc' type='text' /></td></tr>" +
            "<tr><td class='wf-title'>Enabled</td><td class='wf-value'><input class='wf-task-enabled' type='checkbox' checked /></td></tr>" +
            "</tbody>" +
            "</table>" +
            "<table class='wf-designer-table wf-settings'>" +
            "</table>" +
            "</div>";

        wfTask.innerHTML = newTaskHtml;

        collapseCreateId++;

        var lastTask = document.getElementsByClassName("wf-task")[workflowTasks[workflowId].length - 1];
        if (typeof lastTask !== "undefined" && lastTask !== null) {
            lastTask.parentNode.insertBefore(wfTask, lastTask.nextSibling);
        } else {
            document.getElementById("wf-tasks").appendChild(wfTask);
        }

        // push
        workflowTasks[workflowId].push({
            "Id": 0,
            "Name": "",
            "Description": "",
            "IsEnabled": true,
            "Settings": []
        });

        // events
        var index = getElementIndex(wfTask);

        var wfTaskId = wfTask.getElementsByClassName("wf-task-id")[0];
        wfTaskId.onkeyup = function () {
            workflowTasks[workflowId][index].Id = wfTaskId.value;
            this.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName(
                "wf-task-title-label")[0].innerHTML = "Task " + wfTaskId.value;
            //loadExecutionGraph();
        };

        var wfTaskName = wfTask.getElementsByClassName("wf-task-name")[0];
        wfTaskName.onchange = function () {
            var wfSettingsTable =
                wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName(
                    "wf-settings")[0];

            workflowTasks[workflowId][index].Name = wfTaskName.value;

            if (wfTaskName.value !== "") {
                window.Common.get(uri + "/settings/" + wfTaskName.value,
                    function (settings) {

                        var wfAddSetting =
                            wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement
                                .getElementsByClassName("wf-add-setting")[0];
                        workflowTasks[workflowId][index].Settings = [];
                        wfSettingsTable.innerHTML = "";
                        for (var i = 0; i < settings.length; i++) {
                            var settingName = settings[i];
                            addSetting(workflowId, wfAddSetting, settingName);
                        }
                    },
                    function () {
                        window.Common.toastError("An error occured while retrieving settings.");
                    }, auth);
            } else {
                workflowTasks[workflowId][index].Settings = [];
                wfSettingsTable.innerHTML = "";
            }
        };

        var wfTaskDesc = wfTask.getElementsByClassName("wf-task-desc")[0];
        wfTaskDesc.onkeyup = function () {
            workflowTasks[workflowId][index].Description = wfTaskDesc.value;
            //loadExecutionGraph();
        };

        var wfTaskEnabled = wfTask.getElementsByClassName("wf-task-enabled")[0];
        wfTaskEnabled.onchange = function () {
            workflowTasks[workflowId][index].IsEnabled = wfTaskEnabled.checked;
        };

        var wfAddSetting = wfTask.getElementsByClassName("wf-add-setting")[0];
        wfAddSetting.onclick = function () {
            addSetting(workflowId, this);
        };

        // remove task
        var wfRemoveTask = wfTask.getElementsByClassName("wf-remove-task")[0];
        wfRemoveTask.onclick = function () {
            removeTask(workflowId, this);
            //loadExecutionGraph();
        };

        // xml
        var wfTaskToXml = wfTask.getElementsByClassName("wf-show-taskxml")[0];
        wfTaskToXml.onclick = function () {
            showTaskXml(workflowId, this);
        };

        // doc
        var wfTaskToXmlDescription = wfTask.getElementsByClassName("wf-show-doc")[0];
        wfTaskToXmlDescription.onclick = function () {
            doc(workflowId, this);
        };

    }

    function removeTask(workflowId, btn) {
        var index = getElementIndex(btn.parentElement.parentElement);
        workflowTasks[workflowId] = deleteRow(workflowTasks[workflowId], index);
        btn.parentElement.parentElement.remove();
    }

    function addSetting(workflowId, btn, sn) {
        var taskName = btn.parentElement.parentElement.getElementsByClassName("wf-task-name")[0].value;

        if (taskName === "") {
            window.Common.toastInfo("Please select a task name.");
        } else {
            window.Common.get(uri + "/settings/" + taskName,
                function (settings) {
                    var wfSettingsTable = btn.parentElement.parentElement.getElementsByClassName("wf-settings")[0];

                    var row = wfSettingsTable.insertRow(-1);
                    var cell1 = row.insertCell(0);
                    var cell2 = row.insertCell(1);
                    var cell3 = row.insertCell(2);
                    var cell4 = row.insertCell(3);

                    cell1.className = "wf-title";
                    //cell1.innerHTML = "<input class='wf-setting-name' type='text' />";

                    var cell1Html = "<select class='wf-setting-name'>";

                    cell1Html += "<option value=''></option>";
                    for (var i = 0; i < settings.length; i++) {
                        var settingName = settings[i];
                        cell1Html += "<option value='" + settingName + "'" + (sn === settingName ? "selected" : "") + ">" + settingName + "</option>";
                    }
                    cell1Html += "</select>";
                    cell1.innerHTML = cell1Html;

                    cell2.className = "wf-setting-value-td";
                    cell2.innerHTML = "<input class='form-control wf-setting-value' type='text' /><table class='wf-attributes'></table>";

                    cell3.className = "wf-add-attribute-td";
                    cell3.innerHTML = "<button type='button' class='wf-add-attribute btn btn-dark btn-xs'>New attribute</button>";
                    cell4.colSpan = "2";
                    cell4.innerHTML = "<button type='button' class='wf-remove-setting btn btn-danger btn-xs'>Delete</button>";

                    var taskIndex = getElementIndex(btn.parentElement.parentElement);
                    var task = workflowTasks[workflowId][taskIndex];
                    task.Settings.push({ "Name": (typeof sn !== "undefined" ? sn : (settings.length === 1 ? settings[0] : "")), "Value": "", "Attributes": [] });

                    var index = task.Settings.length - 1;
                    var wfSettingName = wfSettingsTable.getElementsByClassName("wf-setting-name")[index];
                    wfSettingName.onchange = function () {
                        var index2 = getElementIndex(wfSettingName.parentElement.parentElement);
                        task.Settings[index2].Name = wfSettingName.value;

                        var wfAddAttributeTd =
                            wfSettingName.parentElement.parentElement.getElementsByClassName("wf-add-attribute-td")[0];
                        if (wfSettingName.value === "selectFiles" || wfSettingName.value === "selectAttachments") {
                            wfAddAttributeTd.style.display = "block";
                        } else {
                            wfAddAttributeTd.style.display = "none";
                        }
                    };

                    if (sn === "selectFiles" || sn === "selectAttachments") {
                        var wfAddAttributeTd = wfSettingName.parentElement.parentElement.getElementsByClassName("wf-add-attribute-td")[0];
                        if (wfSettingName.value === "selectFiles" || wfSettingName.value === "selectAttachments") {
                            wfAddAttributeTd.style.display = "block";
                        } else {
                            wfAddAttributeTd.style.display = "none";
                        }
                    }

                    var wfSettingValue = wfSettingsTable.getElementsByClassName("wf-setting-value")[index];
                    if (typeof wfSettingValue !== "undefined" && wfSettingValue !== null) {
                        wfSettingValue.onkeyup = function () {
                            var index2 = getElementIndex(wfSettingValue.parentElement.parentElement);
                            task.Settings[index2].Value = wfSettingValue.value;
                        };
                    }

                    // Add an attribute
                    var wfAddAttr = wfSettingsTable.getElementsByClassName("wf-add-attribute")[index];
                    wfAddAttr.onclick = function () {
                        addAttribute(workflowId, this);
                    };

                    // Remove a setting
                    var wfRemoveSetting = wfSettingsTable.getElementsByClassName("wf-remove-setting")[index];
                    wfRemoveSetting.onclick = function () {
                        removeSetting(workflowId, this);
                        //index--;
                    };
                },
                function () {
                    window.Common.toastError("An error occured while retrieving the settings.");
                }, auth);
        }

    }

    function addAttribute(workflowId, btn) {
        var wfAttributesTable = btn.parentElement.parentElement.getElementsByClassName("wf-attributes")[0];

        var row = wfAttributesTable.insertRow(-1);
        var cell1 = row.insertCell(0);
        var cell2 = row.insertCell(1);
        var cell3 = row.insertCell(2);

        cell1.innerHTML = "<input class='form-control wf-attribute-name' type='text' />";
        cell2.innerHTML = "<input class='form-control wf-attribute-value' type='text' />";
        cell3.innerHTML = "<button type='button' class='wf-remove-attribute btn btn-danger btn-xs'>Delete</button>";

        var taskIndex =
            getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var task = workflowTasks[workflowId][taskIndex];

        var settingIndex = getElementIndex(btn.parentElement.parentElement);
        task.Settings[settingIndex].Attributes.push({ "Name": "", "Value": "" });

        // onkeyup events
        var attributeIndex = task.Settings[settingIndex].Attributes.length - 1;
        var wfAttributeName = wfAttributesTable.getElementsByClassName("wf-attribute-name")[attributeIndex];
        wfAttributeName.onkeyup = function () {
            var index2 = getElementIndex(wfAttributeName.parentElement.parentElement);
            task.Settings[settingIndex].Attributes[index2].Name = wfAttributeName.value;
        };

        var wfAttributeValue = wfAttributesTable.getElementsByClassName("wf-attribute-value")[attributeIndex];
        wfAttributeValue.onkeyup = function () {
            var index2 = getElementIndex(wfAttributeValue.parentElement.parentElement);
            task.Settings[settingIndex].Attributes[index2].Value = wfAttributeValue.value;
        };

        // Remove event
        var wfRemoveAttribute = wfAttributesTable.getElementsByClassName("wf-remove-attribute")[attributeIndex];
        wfRemoveAttribute.onclick = function () {
            removeAttribute(workflowId, this);
            //attributeIndex--;
        };

    }

    function removeAttribute(workflowId, btn) {
        var taskIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var task = workflowTasks[workflowId][taskIndex];

        var settingIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var attributeIndex = getElementIndex(btn.parentElement.parentElement);

        task.Settings[settingIndex].Attributes = deleteRow(task.Settings[settingIndex].Attributes, attributeIndex);
        btn.parentElement.parentElement.remove();
    }

    function removeSetting(workflowId, btn) {
        var taskIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var task = workflowTasks[workflowId][taskIndex];
        var index = getElementIndex(btn.parentElement.parentElement);
        task.Settings = deleteRow(task.Settings, index);
        btn.parentElement.parentElement.remove();
    }

    function getElementIndex(node) {
        var index = 0;
        while ((node = node.previousElementSibling)) {
            index++;
        }
        return index;
    }

    function deleteRow(arr, row) {
        arr = arr.slice(0); // make copy
        arr.splice(row, 1);
        return arr;
    }

    function isInt(str) {
        return /^\+?(0|[1-9]\d*)$/.test(str);
    }

    function escapeXml(xml) {
        return xml.replace(/[<>&'"]/g, function (c) {
            switch (c) {
                case '<': return '&lt;';
                case '>': return '&gt;';
                case '&': return '&amp;';
                case '\'': return '&apos;';
                case '"': return '&quot;';
            }
            return c;
        });
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
                return "startup";
            case 1:
                return "trigger";
            case 2:
                return "periodic";
            case 3:
                return "cron";
            default:
                return "";
        }
    }

    function launchTypeReverse(lt) {
        switch (lt) {
            case "startup":
                return 0;
            case "trigger":
                return 1;
            case "periodic":
                return 2;
            case "cron":
                return 3;
            default:
                return -1;
        }
    }

    function setSelectedIndex(s, v) {
        for (var i = 0; i < s.options.length; i++) {
            if (s.options[i].value === v) {
                s.options[i].selected = true;
                return;
            }
        }
    }

    function getWorkflow(wid, func) {
        window.Common.get(uri + "/workflow?w=" + wid,
            function (w) {
                func(w);
            },
            function () { }, auth);
    }

    function getTasks(wid, func) {
        window.Common.get(uri + "/tasks/" + wid,
            function (tasks) {
                func(tasks);
            },
            function () { }, auth);
    }

    function getXml(wid, func) {
        window.Common.get(uri + "/xml/" + wid,
            function (val) {
                func(val);
            },
            function () { }, auth);
    }

    function getJson(wid, func) {
        window.Common.get(uri + "/json/" + wid,
            function (val) {
                func(val);
            },
            function () { }, auth);
    }

    //function loadExecutionGraph(workflow) {

    //    var layout = {
    //        name: 'dagre'
    //    };

    //    var style = [
    //        {
    //            selector: 'node',
    //            style: {
    //                'content': 'data(name)',
    //                'text-opacity': 0.7,
    //                'text-valign': 'center',
    //                'text-halign': 'right',
    //                'background-color': '#373737' // 11479e
    //            }
    //        },
    //        {
    //            selector: 'edge',
    //            style: {
    //                'curve-style': 'bezier',
    //                'width': 3,
    //                'target-arrow-shape': 'triangle',
    //                'line-color': '#ffb347', // 9dbaea
    //                'target-arrow-color': '#ffb347' // 9dbaea
    //            }
    //        }
    //    ];
    //    var wfExecutionGraph = document.getElementById('wf-execution-graph');

    //    if (typeof workflow === "undefined" || workflow === null || workflow.IsExecutionGraphEmpty === true) {

    //        var nodes = [];
    //        var edges = [];

    //        var wfTasks = document.getElementsByClassName("wf-task");
    //        for (var index4 = 0; index4 < wfTasks.length; index4++) {
    //            var wfTask = wfTasks[index4];
    //            var wfDesc = wfTask.getElementsByClassName("wf-task-desc")[0].value;
    //            var taskLabel = wfTask.getElementsByClassName("wf-task-title-label")[0].innerHTML + ': ' + wfDesc;
    //            nodes.push({ data: { id: 'n' + index4, name: taskLabel } });
    //        }

    //        for (var index5 = 0; index5 < nodes.length; index5++) {
    //            var node = nodes[index5];
    //            var source = node.data.id;
    //            if (index5 + 1 < nodes.length) {
    //                var target = nodes[index5 + 1].data.id;
    //                edges.push({ data: { source: source, target: target } });
    //            }
    //        }

    //        cytoscape({
    //            container: wfExecutionGraph,
    //            boxSelectionEnabled: false,
    //            autounselectify: true,
    //            layout: layout,
    //            style: style,
    //            elements: {
    //                nodes: nodes,
    //                edges: edges
    //            }
    //        });
    //    } else if (workflow.IsExecutionGraphEmpty === false) {

    //        window.Common.get(uri + "/graph/" + workflow.Id,
    //            function (wfNodes) {
    //                var nodes = [];
    //                var edges = [];

    //                for (var i = 0; i < wfNodes.length; i++) {
    //                    var wfNode = wfNodes[i];
    //                    nodes.push({ data: { id: wfNode.Id, name: wfNode.Name } });
    //                    if (wfNode.ParentId !== "n-1") {
    //                        edges.push({ data: { source: wfNode.ParentId, target: wfNode.Id } });
    //                    }
    //                }

    //                cytoscape({
    //                    container: wfExecutionGraph,
    //                    boxSelectionEnabled: false,
    //                    autounselectify: true,
    //                    layout: layout,
    //                    style: style,
    //                    elements: {
    //                        nodes: nodes,
    //                        edges: edges
    //                    }
    //                });
    //            },
    //            function () {
    //                window.Common.toastError("An error occured while retrieving the execution graph of this workflow.");
    //            }, auth);
    //    }
    //    // end of execution graph
    //}

    function loadXml() {
        getXml(currentWorkflowId,
            function (xml) {
                //setTimeout(function () {
                document.getElementById("wf-xml-shortcut").style.display = "block";
                //document.getElementById("wf-theme").style.display = "block";
                showThemeButton();

                var workflowId = currentWorkflowId;
                if (typeof xml === "undefined" && retries < maxRetries) {
                    retries++;
                    loadXml(workflowId);
                } else {
                    loadJsonCalled = false;
                    loadXmlCalled = true;
                    document.getElementById("wf-xml-container").style.display = "block";

                    document.getElementById("wf-shortcut").style.display = "block";
                    document.getElementById("wf-cancel").style.display = "block";
                    document.getElementById("wf-save").style.display = "block";
                    document.getElementById("wf-delete").style.display = "block";

                    var currentEditor = editors.get(workflowId);
                    var currentWorkflowEditor = null;
                    if (typeof currentEditor !== "undefined") {
                        currentWorkflowEditor = currentEditor.editor;
                    }

                    var editor = ace.edit("wf-xml-container");
                    editor.setOptions({
                        maxLines: Infinity
                    });
                    if (isDarkTheme === true) {
                        editor.setTheme("ace/theme/pastel_on_dark");
                    } else {
                        editor.setTheme("ace/theme/github");
                    }
                    editor.setReadOnly(false);
                    editor.setFontSize("100%");
                    editor.setPrintMarginColumn(false);
                    editor.getSession().setMode("ace/mode/xml");

                    editor.commands.addCommand({
                        name: "showKeyboardShortcuts",
                        bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                        exec: function (editor) {
                            ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                                module.init(editor);
                                editor.showKeyboardShortcuts()
                            })
                        }
                    });

                    var xmlValue = editor.setValue(xml, -1);
                    editor.clearSelection();
                    editor.resize(true);
                    editor.focus();

                    editor.on("change", function () {
                        if (loadXmlCalled === true) {
                            xmlEditorOnChange(workflowId);
                        } else if (loadJsonCalled === true) {
                            jsonEditorOnChange(workflowId);
                        }
                    });

                    if (currentWorkflowEditor === null) {
                        editors.set(workflowId, { editor: editor, editXml: false, editJson: false, value: xmlValue });
                    } else {
                        currentEditor.editor = editor;
                        currentEditor.editXml = false;
                        currentEditor.editJson = false;
                        currentEditor.value = xmlValue;
                    }
                    retries = 0;
                }
                //}, timeoutInterval);

            });
    }

    function loadJson() {
        getJson(currentWorkflowId,
            function (json) {
                //setTimeout(function () {
                document.getElementById("wf-xml-shortcut").style.display = "block";
                //document.getElementById("wf-theme").style.display = "block";
                showThemeButton();

                var workflowId = currentWorkflowId;
                if (typeof json === "undefined" && retries < maxRetries) {
                    retries++;
                    loadJson(workflowId);
                } else {
                    loadXmlCalled = false;
                    loadJsonCalled = true;
                    document.getElementById("wf-xml-container").style.display = "block";

                    document.getElementById("wf-shortcut").style.display = "block";
                    document.getElementById("wf-cancel").style.display = "block";
                    document.getElementById("wf-save").style.display = "block";
                    document.getElementById("wf-delete").style.display = "block";

                    var currentEditor = editors.get(workflowId);
                    var currentWorkflowEditor = null;
                    if (typeof currentEditor !== "undefined") {
                        currentWorkflowEditor = currentEditor.editor;
                    }

                    var editor = ace.edit("wf-xml-container");
                    editor.setOptions({
                        maxLines: Infinity
                    });
                    if (isDarkTheme === true) {
                        editor.setTheme("ace/theme/pastel_on_dark");
                    } else {
                        editor.setTheme("ace/theme/github");
                    }
                    editor.setReadOnly(false);
                    editor.setFontSize("100%");
                    editor.setPrintMarginColumn(false);
                    editor.getSession().setMode("ace/mode/json");

                    editor.commands.addCommand({
                        name: "showKeyboardShortcuts",
                        bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                        exec: function (editor) {
                            ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                                module.init(editor);
                                editor.showKeyboardShortcuts()
                            })
                        }
                    });

                    var jsonValue = editor.setValue(JSON.stringify(json, null, '\t'), -1);
                    //editor.setReadOnly(true);
                    editor.clearSelection();
                    editor.resize(true);
                    editor.focus();

                    editor.on("change", function () {
                        if (loadXmlCalled === true) {
                            xmlEditorOnChange(workflowId);
                        } else if (loadJsonCalled === true) {
                            jsonEditorOnChange(workflowId);
                        }
                    });

                    if (currentWorkflowEditor === null) {
                        editors.set(workflowId, { editor: editor, editXml: false, editJson: false, value: jsonValue });
                    } else {
                        currentEditor.editor = editor;
                        currentEditor.editXml = false;
                        currentEditor.editJson = false;
                        currentEditor.value = jsonValue;
                    }
                    retries = 0;
                }
                //}, timeoutInterval);

            });
    }

    function xmlEditorOnChange(workflowId) {
        //console.log("xmlEditorOnChange");
        var weditor = getEditor(workflowId);
        if (typeof weditor !== "undefined") {
            var xml = weditor.editor.getValue();

            var editXml = getEditXml(workflowId);
            var xmlVal = getEditorValue(workflowId);

            setEditJson(workflowId, false);

            if (editXml === false && editorChanged === false && workflowId === editorWorkflowId && xmlVal !== xml) {
                setEditXml(workflowId, true);
            }
            //console.log("workflowId: " + workflowId + ", editorWorkflowId: " + editorWorkflowId + ", editXml: " + getEditXml(workflowId));
        }
    }

    function jsonEditorOnChange(workflowId) {
        //console.log("jsonEditorOnChange");
        var weditor = getEditor(workflowId);
        if (typeof weditor !== "undefined") {
            var json = weditor.editor.getValue();

            var editJson = getEditXml(workflowId);
            var jsonVal = getEditorValue(workflowId);

            setEditXml(workflowId, false);

            if (editJson === false && editorChanged === false && workflowId === editorWorkflowId && jsonVal !== json) {
                setEditJson(workflowId, true);
            }
            //console.log("workflowId: " + workflowId + ", editorWorkflowId: " + editorWorkflowId + ", editXml: " + getEditXml(workflowId));
        }
    }

    function getEditor(workflowId) {
        return editors.get(workflowId);
    }

    function getEditXml(workflowId) {
        var editor = editors.get(workflowId);
        if (typeof editor !== "undefined") {
            return editor.editXml;
        }
        return false;
    }

    function setEditXml(workflowId, val) {
        var editor = editors.get(workflowId);
        if (typeof editor !== "undefined") {
            editor.editXml = val;
        }
    }

    function getEditJson(workflowId) {
        var editor = editors.get(workflowId);
        if (typeof editor !== "undefined") {
            return editor.editJson;
        }
        return false;
    }

    function setEditJson(workflowId, val) {
        var editor = editors.get(workflowId);
        if (typeof editor !== "undefined") {
            editor.editJson = val;
        }
    }

    function getEditorValue(workflowId) {
        var editor = editors.get(workflowId);
        if (typeof editor !== "undefined") {
            return editor.value;
        }
        return "";
    }

    function showTaskXml(workflowId, btn) {
        var index = getElementIndex(btn.parentElement.parentElement);
        var task = workflowTasks[workflowId][index];

        window.Common.post(uri + "/task-to-xml",
            function (xml) {
                var xmlContainer = btn.parentElement.parentElement.getElementsByClassName("wf-taskxml")[0];
                xmlContainer.style.display = 'table-cell';
                var codeContainer = xmlContainer.getElementsByClassName("wf-taskxml-container")[0];
                codeContainer.innerHTML = escapeXml(xml);
                hljs.highlightBlock(codeContainer);
            },
            function () {
                window.Common.toastError("An error occured while retrieving the XML of the task " + task.Id + ".");
            }, task, auth);
    }

    function doc(workflowId, btn) {
        var index = getElementIndex(btn.parentElement.parentElement);
        var task = workflowTasks[workflowId][index];
        var taskName = task.Name;

        if (taskName !== '') {
            var url = "https://github.com/aelassas/Wexflow/wiki/" + taskName;
            var win = window.open(url, '_blank');
            win.focus();
        }
    }

    function showThemeButton() {
        if (isDarkTheme === true) {
            $('#wf-theme').bootstrapToggle('off');
        } else {
            $('#wf-theme').bootstrapToggle('on');
        }
        $('#wf-theme').change(function () {
            isDarkTheme = !$(this).prop('checked');
            var weditor = getEditor(editorWorkflowId);
            if (typeof weditor !== 'undefined') {
                var editor = weditor.editor;
                if (isDarkTheme === true) {
                    editor.setTheme("ace/theme/pastel_on_dark");
                } else {
                    editor.setTheme("ace/theme/github");
                }
            }
        });
        var elts = document.getElementsByClassName("toggle");
        if (elts.length > 0) {
            elts[0].style.display = "inline-block";
        }
    }

    function hideThemeButton() {
        var elts = document.getElementsByClassName("toggle");
        if (elts.length > 0) {
            elts[0].style.display = "none";
        }
    }

    function loadRightPanel(workflowId, workflowChanged) {
        currentWorkflowId = workflowId;
        //showThemeButton();
        document.getElementById("wf-xml-shortcut").style.display = "none";
        hideThemeButton();

        var xmlContainer = document.getElementById("wf-xml-container");
        var workflowEditor = getEditor(workflowId);
        //console.log("workflowId: " + workflowId + ", workflowChanged: " + workflowChanged + ", workflowEditor: " + workflowEditor);
        //if (workflowChanged === true && typeof workflowEditor !== "undefined") {
        //    editorChanged = true;
        //    var editXml = getEditXml(workflowId);
        //    if (editXml === true && editorChanged === true && workflowChangedAndSaved === false) {
        //        var res = confirm("The XML of the workflow " + workflowId + " has changed. Do you want to save it?");
        //        if (res === true) {
        //            save(workflowId,
        //                workflowId,
        //                false,
        //                false,
        //                function () {
        //                    workflowChangedAndSaved = true;
        //                    editorChanged = false;
        //                    setEditXml(workflowId, false);
        //                    xmlContainer.style.display = "none";
        //                    editorWorkflowId = selectedId;
        //                    setEditXml(editorWorkflowId, false);

        //                    // Reload workflows list
        //                    //setTimeout(function () {
        //                    loadWorkflows(function () {
        //                        // Select the workflow
        //                        var wfWorkflowsTable = document.getElementById("wf-workflows-table");

        //                        for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
        //                            var row = wfWorkflowsTable.rows[i];
        //                            var wfId = row.getElementsByClassName("wf-id")[0];
        //                            if (typeof wfId !== "undefined" && wfId !== null) {
        //                                var swId = parseInt(wfId.innerHTML);

        //                                if (swId === selectedId) {
        //                                    var selected = document.getElementsByClassName("selected");
        //                                    if (selected.length > 0) {
        //                                        selected[0].className = selected[0].className.replace("selected", "");
        //                                    }

        //                                    row.className += "selected";

        //                                    // Scroll to the workflow
        //                                    //row.scrollIntoView(true);
        //                                }
        //                            }
        //                        }

        //                        // Update the status
        //                        //updateWorkflowStatus(selectedId);

        //                        // Show the xml button
        //                        document.getElementById("wf-xml").style.display = "inline";
        //                        document.getElementById("wf-json").style.display = "inline";

        //                        document.getElementById("wf-xml").onclick = function () {
        //                            document.getElementById("wf-xml").disabled = true;
        //                            document.getElementById("wf-json").disabled = false;
        //                            loadXml(currentWorkflowId);
        //                        };

        //                        document.getElementById("wf-json").onclick = function () {
        //                            document.getElementById("wf-json").disabled = true;
        //                            document.getElementById("wf-xml").disabled = false;
        //                            loadJson(currentWorkflowId);
        //                        };

        //                        // Reload right panel
        //                        loadRightPanel(selectedId, false);


        //                        // Reset editor
        //                        if (typeof workflowEditor !== "undefined") {
        //                            workflowChangedAndSaved = true;
        //                        }

        //                        setEditXml(selectedId, false);
        //                        editorChanged = false;

        //                        document.getElementById("wf-xml").disabled = false;
        //                        document.getElementById("wf-json").disabled = false;
        //                    });
        //                    //}, timeoutInterval);

        //                });
        //        } else {
        //            setEditXml(workflowId, false);
        //            editorChanged = false;
        //            editorCanceled = true;
        //            editorWorkflowId = selectedId;
        //            setEditXml(editorWorkflowId, false);
        //            document.getElementById("wf-xml").disabled = false;
        //            document.getElementById("wf-json").disabled = false;
        //        }
        //    } else {
        //        setEditXml(workflowId, false);
        //        editorChanged = false;
        //        document.getElementById("wf-xml").disabled = false;
        //        document.getElementById("wf-json").disabled = false;
        //    }
        //} else {
        //    editorChanged = false;
        //    setEditXml(workflowId, false);
        //    //console.log("workflowId: " + workflowId + ", workflowEditor: " + workflowEditor);
        //    if (typeof workflowEditor === "undefined") {
        //        document.getElementById("wf-xml").disabled = false;
        //        document.getElementById("wf-json").disabled = false;
        //    }
        //}

        setEditXml(workflowId, false);
        setEditJson(workflowId, false);
        if (workflowChanged === true) {
            document.getElementById("wf-xml").disabled = false;
            document.getElementById("wf-json").disabled = false;
        } else {
            document.getElementById("wf-xml").disabled = loadXmlCalled;
            document.getElementById("wf-json").disabled = loadJsonCalled;
        }


        // Reset editor
        if (typeof workflowEditor !== "undefined") {
            setEditXml(workflowId, false);
        }

        document.getElementById("wf-xml").onclick = function () {
            document.getElementById("wf-xml").disabled = true;
            document.getElementById("wf-json").disabled = false;
            loadXml(currentWorkflowId);
        };

        document.getElementById("wf-json").onclick = function () {
            document.getElementById("wf-xml").disabled = false;
            document.getElementById("wf-json").disabled = true;
            loadJson(currentWorkflowId);
        };

        document.getElementById("wf-shortcut").style.display = "block";
        document.getElementById("wf-cancel").style.display = "block";
        document.getElementById("wf-save").style.display = "block";
        document.getElementById("wf-delete").style.display = "block";

        document.getElementById("wf-xml").style.display = "inline";
        document.getElementById("wf-json").style.display = "inline";

        if (document.getElementById("wf-designer-right-panel").style.display === "none") {
            document.getElementById("wf-designer-right-panel").style.display = "block";
        }

        document.getElementById("wf-delete").onclick = function () {
            document.getElementById("wf-delete").disabled = true;
            deleteWorkflow();
        };

        getWorkflow(workflowId,
            function (workflow) {

                workflowInfos[workflow.Id] = {
                    "Id": workflow.Id,
                    "Name": workflow.Name,
                    "LaunchType": workflow.LaunchType,
                    "Period": workflow.Period,
                    "CronExpression": workflow.CronExpression,
                    "IsEnabled": workflow.IsEnabled,
                    "IsApproval": workflow.IsApproval,
                    "Description": workflow.Description,
                    "Path": workflow.Path,
                    "LocalVariables": workflow.LocalVariables
                };

                var wfId = document.getElementById("wf-id");
                wfId.value = workflow.Id;
                wfId.onkeyup = function () {
                    workflowInfos[workflowId].Id = wfId.value;
                };

                var wfName = document.getElementById("wf-name");
                wfName.value = workflow.Name;
                wfName.onkeyup = function () {
                    workflowInfos[workflowId].Name = wfName.value;
                };

                var lt = launchType(workflow.LaunchType);
                var wfLt = document.getElementById("wf-launchType");
                setSelectedIndex(wfLt, lt);
                wfLt.onchange = function () {
                    workflowInfos[workflowId].LaunchType = launchTypeReverse(wfLt.value);
                };

                var wfPeriod = document.getElementById("wf-period");
                wfPeriod.onkeyup = function () {
                    workflowInfos[workflowId].Period = wfPeriod.value;
                };

                if (workflow.LaunchType === 2 || workflow.Period !== "00.00:00:00") {
                    wfPeriod.value = workflow.Period;
                } else {
                    wfPeriod.value = "";
                }

                var wfCron = document.getElementById("wf-cron");
                wfCron.onkeyup = function () {
                    workflowInfos[workflowId].CronExpression = wfCron.value;
                };
                if (workflow.LaunchType === 3 || workflow.CronExpression !== "") {
                    wfCron.value = workflow.CronExpression;
                } else {
                    wfCron.value = "";
                }

                var wfEnabled = document.getElementById("wf-enabled");
                wfEnabled.checked = workflow.IsEnabled;
                wfEnabled.onchange = function () {
                    workflowInfos[workflowId].IsEnabled = wfEnabled.checked;
                };

                var wfApproval = document.getElementById("wf-approval");
                wfApproval.checked = workflow.IsApproval;
                wfApproval.onchange = function () {
                    workflowInfos[workflowId].IsApproval = wfApproval.checked;
                };

                var wfEnableParallelJobs = document.getElementById("wf-enable-pj");
                wfEnableParallelJobs.checked = workflow.EnableParallelJobs;
                wfEnableParallelJobs.onchange = function () {
                    workflowInfos[workflowId].EnableParallelJobs = wfEnableParallelJobs.checked;
                };

                var wfDesc = document.getElementById("wf-desc");
                wfDesc.value = workflow.Description;
                wfDesc.onkeyup = function () {
                    workflowInfos[workflowId].Description = wfDesc.value;
                };

                //document.getElementById("wf-path").innerHTML = workflow.Path;

                // Status
                //updateWorkflowStatus(workflow.Id);

                // Local variables
                if (workflow.LocalVariables.length > 0) {
                    var varsHtml = "<h5 class='wf-task-title'>Local variables" +
                        "<button id='wf-add-var' type='button' class='btn btn-dark btn-xs'>New variable</button>" +
                        "</h5>" +
                        "<table class='wf-designer-table wf-local-vars'>" +
                        "<tr><th>Name</th><th>Value</th><th style='width:100%'></th></tr>";
                    for (var q = 0; q < workflow.LocalVariables.length; q++) {
                        var variable = workflow.LocalVariables[q];
                        var varKey = variable.Key;
                        var varValue = variable.Value;
                        varsHtml += "<tr>";
                        varsHtml += "<td><input class='form-control wf-var-key' type='text' value='" + varKey + "'></td>";
                        varsHtml += "<td class='wf-value'><input class='form-control wf-var-value' type='text' value='" + varValue + "'></td>";
                        varsHtml += "<td><button type='button' class='wf-remove-var btn btn-danger btn-xs'>Delete</button></td>";
                        varsHtml += "</tr>";
                    }
                    varsHtml += "</table>";
                    document.getElementById("wf-local-vars").innerHTML = varsHtml;

                    // Bind keys modifications
                    var bindVarKey = function (index) {
                        var wfVarKey = document.getElementsByClassName("wf-var-key")[index];
                        wfVarKey.onkeyup = function () {
                            workflowInfos[workflowId].LocalVariables[index].Key = wfVarKey.value;
                        };
                    };

                    var wfVarKeys = document.getElementsByClassName("wf-var-key");
                    for (var r = 0; r < wfVarKeys.length; r++) {
                        bindVarKey(r);
                    }

                    // Bind values modifications
                    var bindVarValue = function (index) {
                        var wfVarValue = document.getElementsByClassName("wf-var-value")[index];
                        wfVarValue.onkeyup = function () {
                            workflowInfos[workflowId].LocalVariables[index].Value = wfVarValue.value;
                        };
                    };

                    var wfVarValues = document.getElementsByClassName("wf-var-value");
                    for (var s = 0; s < wfVarValues.length; s++) {
                        bindVarValue(s);
                    }

                    // Bind delete variables
                    var bindDeleteVar = function (index) {
                        var wfVarDelete = document.getElementsByClassName("wf-remove-var")[index];
                        wfVarDelete.onclick = function () {
                            index = getElementIndex(wfVarDelete.parentElement.parentElement) - 1;
                            workflowInfos[workflowId].LocalVariables = deleteRow(workflowInfos[workflowId].LocalVariables, index);
                            wfVarDelete.parentElement.parentElement.remove();
                        };
                    };

                    var wfVarDeleteBtns = document.getElementsByClassName("wf-remove-var");
                    for (var t = 0; t < wfVarDeleteBtns.length; t++) {
                        bindDeleteVar(t);
                    }

                    // Bind add variables
                    document.getElementById("wf-add-var").onclick = function () {
                        addVar(workflowId);
                    };

                } else {
                    document.getElementById("wf-local-vars").innerHTML = "<h5 class='wf-task-title'>Local variables" +
                        "<button id='wf-add-var' type='button' class='btn btn-dark btn-xs'>New variable</button>" +
                        "</h5>" +
                        "<table class='wf-designer-table wf-local-vars' style='display: none;'>" +
                        "<tr><th>Name</th><th>Value</th><th style='width:100%'></th></tr>" +
                        "</table>";

                    document.getElementById("wf-add-var").onclick = function () {
                        document.getElementsByClassName("wf-local-vars")[0].style.display = "table";
                        addVar(workflowId);
                    };
                }

                // Tasks
                loadWorkflowTasks(workflowId, workflow);
            });

        if (editorCanceled === true) {

            editorCanceled = false;

            // Reset the editor
            if (typeof workflowEditor !== "undefined") {
                //workflowEditor.editor.setValue("", -1);
                xmlContainer.style.display = "none";
                setEditXml(workflowId, false);
                editorChanged = false;
            }

            // Select the workflow
            var wfWorkflowsTable = document.getElementById("wf-workflows-table");
            for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                var row = wfWorkflowsTable.rows[i];
                var wfId = row.getElementsByClassName("wf-id")[0];
                if (typeof wfId !== "undefined" && wfId !== null) {
                    var swId = parseInt(wfId.innerHTML);

                    if (swId === selectedId) {
                        var selected = document.getElementsByClassName("selected");
                        if (selected.length > 0) {
                            selected[0].className = selected[0].className.replace("selected", "");
                        }

                        row.className += "selected";

                        // Scroll to the workflow
                        //row.scrollIntoView(true);
                    }
                }
            }

            // Update the status
            //updateWorkflowStatus(selectedId);

            loadRightPanel(selectedId, true);
        }
    }


    function loadWorkflowTasks(workflowId, workflow) {
        getTasks(workflowId,
            function (tasks) {
                if (typeof tasks == "undefined" && retries < maxRetries) {
                    setTimeout(function () { // try again after delay
                        //console.log("loadWorkflowTasks");
                        loadWorkflowTasks(workflowId, workflow);
                    }, timeoutInterval);
                    retries++;
                } else {
                    loadTasks(tasks, workflowId, workflow);
                    retries = 0;
                }

            });
    }

    function loadTasks(tasks, workflowId, workflow) {

        workflowTasks[workflowId] = tasks;

        var tasksHtml = "";

        window.Common.get(uri + "/task-names",
            function (taskNames) {

                for (var i = 0; i < tasks.length; i++) {

                    var task = tasks[i];

                    tasksHtml += "<div class='wf-task'>" +
                        //"<h5 class='wf-task-title'><label class='wf-task-title-label'>Task " + task.Id + "</label>" +
                        "<h5 class='wf-task-title'>" +
                        "<a class='wf-task-title-label colpsible-panel' data-toggle='collapse' data-parent='#accordion' style='font-weight: bold' href='#collapse" + task.Id + "'>" + "Task " + task.Id + "</a>" +
                        "<button type='button' class='wf-remove-task btn btn-danger btn-xs'>Delete</button>" +
                        "<button type='button' class='wf-show-doc btn btn-dark btn-xs'>Documentation</button>" +
                        "<button type='button' class='wf-show-taskxml btn btn-dark btn-xs'>Xml</button>" +
                        "<button type='button' class='wf-add-setting btn btn-dark btn-xs'>New setting</button>" +
                        "</h5>" +
                        "<div id='collapse" + task.Id + "' class='panel-collapse collapse'>" +
                        "<table class='wf-designer-table'>" +
                        "<tbody>" +
                        "<tr><td class='wf-taskxml' colspan='2'><pre><code class='wf-taskxml-container'></code></pre></td></tr>" +
                        "<tr><td class='wf-title'>Id</td><td class='wf-value'><input class='form-control wf-task-id' type='text' value='" + task.Id + "'" + (workflow.IsExecutionGraphEmpty === false ? "readonly" : "") + "/></td></tr>" +
                        "<tr><td class='wf-title'>Name</td><td class='wf-value'><select class='form-control wf-task-name'>";

                    for (var i1 = 0; i1 < taskNames.length; i1++) {
                        var taskName = taskNames[i1];
                        tasksHtml += "<option value='" + taskName.Name + "' " + (taskName.Name === task.Name ? "selected" : "") + ">" + taskName.Name + "</option>";
                    }

                    tasksHtml += "</select>" +
                        "</td></tr>" +
                        "<tr><td class='wf-title'>Description</td><td class='wf-value'><input class='form-control wf-task-desc' type='text' value='" + task.Description + "' /></td></tr>" +
                        "<tr><td class='wf-title'>Enabled</td><td class='wf-value'><input class='wf-task-enabled' type='checkbox'   " + (task.IsEnabled ? "checked" : "") + " /></td></tr>" +
                        "</tbody>" +
                        "</table>" +
                        //"<div class='wf-add-setting-title'>" +
                        //"</div>" +
                        "<table class='wf-designer-table wf-settings'>" +
                        "<tbody>";

                    // task settings
                    for (var j = 0; j < task.Settings.length; j++) {
                        var setting = task.Settings[j];
                        tasksHtml +=
                            "<tr><td class='wf-setting-name-td wf-title'>" +
                            "<input class='wf-setting-name' type='text' value='" + setting.Name + "' readonly />" +
                            "</td><td class='wf-setting-value-td'>";

                        tasksHtml += "<input class='form-control wf-setting-value' type='text' value='" + setting.Value + "'  />";

                        // settings attributes (for selectFiles and selectAttachments settings only)
                        tasksHtml += "<table class='wf-attributes'>";

                        for (var k = 0; k < setting.Attributes.length; k++) {
                            var attr = setting.Attributes[k];
                            tasksHtml +=
                                "<tr>" +
                                "<td><input class='form-control wf-attribute-name' type='text' value='" + attr.Name + "'  /></td>" +
                                "<td><input class='form-control wf-attribute-value' type='text' value='" + attr.Value + "'  /></td>" +
                                "<td><button type='button' class='wf-remove-attribute btn btn-danger btn-xs'>Delete</button></td>" +
                                "</tr>";
                        }

                        tasksHtml += "</table>";

                        tasksHtml += "</td>" +
                            "<td class='wf-add-attribute-td'><button type='button' class='wf-add-attribute btn btn-dark btn-xs'>New attribute</button></td>" +
                            "<td colspan='2'><button type='button' class='wf-remove-setting btn btn-danger btn-xs'>Delete</button></td>" +
                            "</tr>";
                    }

                    tasksHtml += "</tbody>" +
                        "</table>" +
                        "</div>" +
                        "</div>";
                }

                document.getElementById("wf-tasks").innerHTML = tasksHtml;

                // load settings in select tags
                /*var wfSettingNameTds = document.getElementsByClassName("wf-setting-name-td");
                
                for (var i2 = 0; i2 < wfSettingNameTds.length; i2++) {
                    var wfSettingNameTd = wfSettingNameTds[i2];
                    var tn = wfSettingNameTd.getElementsByClassName("wf-setting-name-hidden")[0].value;
 
                    window.Common.get(uri + "/settings/" + tn, function (settings) {
                        var tdHtml = "<select class='wf-setting-name'>";
                        tdHtml += "<option value=''></option>";
                        for (var i3 = 0; i3 < settings.length; i3++) {
                            var setting = settings[i3];
                            tdHtml += "<option value='" + setting + "'"+ (tn===setting?"selected":"") +">" + setting + "</option>";
                        }
                        wfSettingNameTd.innerHTML = tdHtml;
                    });
                }*/

                if (workflow.IsExecutionGraphEmpty === true) {
                    document.getElementById("wf-add-task").style.display = "block";
                    var wfRemoveTaskBtns = document.getElementsByClassName("wf-remove-task");
                    for (var i4 = 0; i4 < wfRemoveTaskBtns.length; i4++) {
                        var wfRemoveTaskBtn = wfRemoveTaskBtns[i4];
                        wfRemoveTaskBtn.style.display = "block";
                    }
                } else {
                    document.getElementById("wf-add-task").style.display = "none";
                }

                var wfAddAttributesTds = document.getElementsByClassName("wf-add-attribute-td");
                for (var i3 = 0; i3 < wfAddAttributesTds.length; i3++) {
                    var wfAddAttributeTd = wfAddAttributesTds[i3];
                    var settingValue = wfAddAttributeTd.parentElement.getElementsByClassName("wf-setting-name")[0].value;
                    if (settingValue === "selectFiles" || settingValue === "selectAttachments") {
                        wfAddAttributeTd.style.display = "block";
                    }
                }

                // Show Task function XML
                var wfShowTaskXmLs = document.getElementsByClassName("wf-show-taskxml");
                for (var i5 = 0; i5 < wfShowTaskXmLs.length; i5++) {
                    var wfShowTaskXml = wfShowTaskXmLs[i5];
                    wfShowTaskXml.onclick = function () {
                        showTaskXml(workflowId, this);
                    };
                }

                // Show Task function XML description
                var wfShowTaskXmLDescriptions = document.getElementsByClassName("wf-show-doc");
                for (var i6 = 0; i6 < wfShowTaskXmLDescriptions.length; i6++) {
                    var wfShowTaskXmlDescription = wfShowTaskXmLDescriptions[i6];
                    wfShowTaskXmlDescription.onclick = function () {
                        //showTaskXml(workflowId, this, "/taskToXmlDescription");
                        doc(workflowId, this);
                    };
                }


                // Add/Remove a setting
                var wfAddSettings = document.getElementsByClassName("wf-add-setting");
                for (var l = 0; l < wfAddSettings.length; l++) {
                    var wfAddSetting = wfAddSettings[l];
                    wfAddSetting.onclick = function () {
                        addSetting(workflowId, this);
                    };
                }

                // Remove a setting
                var wfRemoveSettings = document.getElementsByClassName("wf-remove-setting");
                for (var m = 0; m < wfRemoveSettings.length; m++) {
                    var wfRemoveSetting = wfRemoveSettings[m];
                    wfRemoveSetting.onclick = function () {
                        removeSetting(workflowId, this);
                    };
                }

                // Add/Remove attribute
                var wfAddAttributes = document.getElementsByClassName("wf-add-attribute");
                for (var o = 0; o < wfAddAttributes.length; o++) {
                    var wfAddAttribute = wfAddAttributes[o];
                    wfAddAttribute.onclick = function () {
                        addAttribute(workflowId, this);
                    };
                }

                // Remove attribute
                var wfRemoveAttributes = document.getElementsByClassName("wf-remove-attribute");
                for (var n = 0; n < wfRemoveAttributes.length; n++) {
                    var wfRemoveAttribute = wfRemoveAttributes[n];
                    wfRemoveAttribute.onclick = function () {
                        removeAttribute(workflowId, this);
                    };
                }

                // Remove task for linear wf
                var wfRemoveTasks = document.getElementsByClassName("wf-remove-task");
                for (var p = 0; p < wfRemoveTasks.length; p++) {
                    var wfRemoveTask = wfRemoveTasks[p];
                    wfRemoveTask.onclick = function () {
                        removeTask(workflowId, this);

                    };
                }

                // Add task for linear wf
                var wfAddTask = document.getElementById("wf-add-task");
                wfAddTask.onclick = function () {
                    addTask(workflowId, taskNames);
                };

                var bindWfTaskId = function (m) {
                    var wfTaskId = document.getElementsByClassName("wf-task-id")[m];
                    wfTaskId.onkeyup = function () {
                        workflowTasks[workflowId][m].Id = wfTaskId.value;
                        this.parentElement.parentElement.parentElement.parentElement.parentElement
                            .getElementsByClassName("wf-task-title-label")[0].innerHTML =
                            "Task " + wfTaskId.value;
                        //loadExecutionGraph(workflow);
                    };
                };

                var bindWfTaskName = function (m) {
                    var wfTaskName = document.getElementsByClassName("wf-task-name")[m];
                    wfTaskName.onchange = function () {

                        // Reset settings
                        var wfSettingsTable =
                            wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName("wf-settings")[0];

                        workflowTasks[workflowId][m].Name = wfTaskName.value;

                        if (wfTaskName.value !== "") {
                            window.Common.get(uri + "/settings/" + wfTaskName.value,
                                function (settings) {

                                    var wfAddSetting =
                                        wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement
                                            .getElementsByClassName("wf-add-setting")[0];
                                    workflowTasks[workflowId][m].Settings = [];
                                    wfSettingsTable.innerHTML = "";
                                    for (var i = 0; i < settings.length; i++) {
                                        var settingName = settings[i];
                                        addSetting(workflowId, wfAddSetting, settingName);
                                    }
                                },
                                function () {
                                    window.Common.toastError("An error occured while retrieving settings.");
                                }, auth);
                        } else {
                            workflowTasks[workflowId][m].Settings = [];
                            wfSettingsTable.innerHTML = "";
                        }

                    };
                };

                var bindWfTaskDesc = function (m) {
                    var wfTaskDesc = document.getElementsByClassName("wf-task-desc")[m];
                    wfTaskDesc.onkeyup = function () {
                        workflowTasks[workflowId][m].Description = wfTaskDesc.value;
                        //loadExecutionGraph();
                    };
                };

                var bindWfTaskEnabled = function (m) {
                    var wfTaskEnabled =
                        document.getElementsByClassName("wf-task-enabled")[m];
                    wfTaskEnabled.onchange = function () {
                        workflowTasks[workflowId][m].IsEnabled = wfTaskEnabled.checked;
                    };
                };

                var bindwfSettingName = function (m, n) {
                    var wfSettingName =
                        document.getElementsByClassName("wf-settings")[m].getElementsByClassName(
                            "wf-setting-name")[n];
                    wfSettingName.onkeyup = function () {
                        workflowTasks[workflowId][m].Settings[n].Name = wfSettingName.value;
                        var wfAddAttributeTd =
                            wfSettingName.parentElement.parentElement.getElementsByClassName(
                                "wf-add-attribute-td")[0];
                        if (wfSettingName.value === "selectFiles" ||
                            wfSettingName.value === "selectAttachments") {
                            wfAddAttributeTd.style.display = "block";
                        } else {
                            wfAddAttributeTd.style.display = "none";
                        }
                    };
                };

                var bindwfSettingValue = function (m, n) {
                    var wfSettingValue = document.getElementsByClassName("wf-settings")[m]
                        .getElementsByClassName("wf-setting-value")[n];
                    if (typeof wfSettingValue !== "undefined" && wfSettingValue !== null) {
                        wfSettingValue.onkeyup = function () {
                            workflowTasks[workflowId][m].Settings[n].Value =
                                wfSettingValue.value;
                        };
                    }
                };

                var bindwfAttributeName = function (m, n, o) {
                    var wfAttributeName = document.getElementsByClassName("wf-settings")[m]
                        .getElementsByClassName("wf-setting-value-td")[n]
                        .getElementsByClassName("wf-attribute-name")[o];
                    if (typeof wfAttributeName !== "undefined" && wfAttributeName !== null) {
                        wfAttributeName.onkeyup = function () {
                            workflowTasks[workflowId][m].Settings[n].Attributes[o].Name =
                                wfAttributeName.value;
                        };
                    }
                };

                var bindwfAttributeValue = function (m, n, o) {
                    var wfAttributeValue = document.getElementsByClassName("wf-settings")[m]
                        .getElementsByClassName("wf-setting-value-td")[n]
                        .getElementsByClassName("wf-attribute-value")[o];
                    if (typeof wfAttributeValue !== "undefined" && wfAttributeValue !== null) {
                        wfAttributeValue.onkeyup = function () {
                            workflowTasks[workflowId][m].Settings[n].Attributes[o].Value =
                                wfAttributeValue.value;
                        };
                    }
                };

                for (var index1 = 0; index1 < tasks.length; index1++) {
                    var wftask = tasks[index1];
                    bindWfTaskId(index1);
                    bindWfTaskName(index1);
                    bindWfTaskDesc(index1);
                    bindWfTaskEnabled(index1);

                    for (var index2 = 0; index2 < wftask.Settings.length; index2++) {
                        var wfsetting = wftask.Settings[index2];
                        bindwfSettingName(index1, index2);
                        bindwfSettingValue(index1, index2);

                        for (var index3 = 0;
                            index3 < wfsetting.Attributes.length;
                            index3++) {
                            bindwfAttributeName(index1, index2, index3);
                            bindwfAttributeValue(index1, index2, index3);
                        }
                    }
                }

                // Execution graph
                //loadExecutionGraph(workflow);

            },

            function () {
                window.Common.toastError("An error occured while retrieving task names.");
            }, auth);
    }

    function cancel(workflowId) {
        setEditXml(workflowId, false);
        editorChanged = false;
        loadRightPanel(workflowId, true);
    }

    function loadWorkflows(callback, workflowId, recursive) {
        window.Common.get(uri + "/search?s=" + encodeURIComponent(searchText.value),
            function (data) {
                if (typeof workflowId !== "undefined" && typeof recursive !== "undefined" && recursive === true) {
                    var workflowFound = false;
                    for (var i = 0; i < data.length; i++) {
                        var val = data[i];
                        if (val.Id === workflowId) {
                            workflowFound = true;
                            break;
                        }
                    }

                    if (workflowFound === false && retries < maxRetries) {
                        setTimeout(function () {
                            //console.log("recursive: loadWorkflows");
                            retries++;
                            loadWorkflows(callback, workflowId, recursive);
                        }, timeoutInterval);
                    } else {
                        loadWorkflowsResponse(data, callback);
                        retries = 0;
                    }
                } else {
                    loadWorkflowsResponse(data, callback);
                }
            },
            function () {
                window.Common.toastError("An error occured while retrieving workflows. Check that wexflow server is running correctly.");
            }, auth);

    }

    function loadWorkflowsResponse(data, callback) {
        data.sort(compareById);

        var items = [];
        for (var i = 0; i < data.length; i++) {
            var val = data[i];
            workflows[val.Id] = val;

            items.push("<tr>" +
                "<td><input class='wf-delete' type='checkbox'></td>" +
                "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>" +
                "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>" +
                "</tr>");

        }

        var table = "<table id='wf-workflows-table' class='table'>" +
            "<thead class='thead-dark'>" +
            "<tr>" +
            "<th><input id='wf-delete-all' type='checkbox'></th>" +
            "<th class='wf-id'>Id</th>" +
            "<th class='wf-n'>Name</th>" +
            "</tr>" +
            "</thead>" +
            "<tbody>" +
            items.join("") +
            "</tbody>" +
            "</table>";

        document.getElementById("wf-workflows").innerHTML = table;
        document.getElementById("wf-action").style.display = "block";

        var workflowsTable = document.getElementById("wf-workflows-table");

        // selection changed event
        var rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
        for (var j = 0; j < rows.length; j++) {
            var row = rows[j];
            var idTd = row.getElementsByClassName("wf-id")[0];
            var nameTd = row.getElementsByClassName("wf-n")[0];

            var rowOnClick = function (idTd, nameTd) {
                var currentRow = null;
                if (idTd !== null) {
                    currentRow = idTd.parentElement;
                } else if (nameTd !== null) {
                    currentRow = nameTd.parentElement;
                }

                let workflowId = parseInt(idTd);
                setEditJson(workflowId, false);
                setEditXml(workflowId, false);

                loadXmlCalled = false;
                loadJsonCalled = false;
                newWorkflow = false;
                editorCanceled = false;
                selectedId = parseInt(currentRow.getElementsByClassName("wf-id")[0].innerHTML);
                var selected = document.getElementsByClassName("selected");

                workflowChangedAndSaved = false;
                //var editXml = getEditXml(editorWorkflowId);
                //if (editXml === true) {
                //    loadRightPanel(editorWorkflowId, true);

                //    document.getElementById("wf-cancel").onclick = function () {
                //        editorCanceled = true;
                //        cancel(editorWorkflowId);
                //    };
                //} else {
                var xmlContainer = document.getElementById("wf-xml-container");
                xmlContainer.style.display = "none";

                if (selected.length > 0) {
                    selected[0].className = selected[0].className.replace("selected", "");
                }
                currentRow.className += "selected";

                loadRightPanel(selectedId, true);

                document.getElementById("wf-cancel").onclick = function () {
                    editorCanceled = true;
                    cancel(selectedId);
                };

                editorWorkflowId = selectedId;
                //}

                document.getElementById("wf-save").onclick = function () {
                    saveClick(false, false);
                    editorChanged = false;
                };
            };

            idTd.onclick = function () {
                rowOnClick(this, null);
            };

            nameTd.onclick = function () {
                rowOnClick(null, this);
            };

            //row.onclick = function () {
            //    loadXmlCalled = false;
            //    newWorkflow = false;
            //    editorCanceled = false;
            //    selectedId = parseInt(this.getElementsByClassName("wf-id")[0].innerHTML);
            //    var selected = document.getElementsByClassName("selected");

            //    workflowChangedAndSaved = false;
            //    var editXml = getEditXml(editorWorkflowId);
            //    if (editXml === true) {
            //        loadRightPanel(editorWorkflowId, true);

            //        document.getElementById("wf-cancel").onclick = function () {
            //            editorCanceled = true;
            //            cancel(editorWorkflowId);
            //        };
            //    } else {
            //        var xmlContainer = document.getElementById("wf-xml-container");
            //        xmlContainer.style.display = "none";

            //        if (selected.length > 0) {
            //            selected[0].className = selected[0].className.replace("selected", "");
            //        }
            //        this.className += "selected";

            //        loadRightPanel(selectedId, true);

            //        document.getElementById("wf-cancel").onclick = function () {
            //            editorCanceled = true;
            //            cancel(selectedId);
            //        };

            //        editorWorkflowId = selectedId;
            //    }

            //    document.getElementById("wf-save").onclick = function () {
            //        saveClick(false, false);
            //        editorChanged = false;
            //    };

            //};

            var deleteWorkflowCheckBox = row.getElementsByClassName("wf-delete")[0];
            deleteWorkflowCheckBox.onchange = function () {
                var currentRow = this.parentElement.parentElement;
                var workflowId = parseInt(currentRow.getElementsByClassName("wf-id")[0].innerHTML);
                var dbId = workflows[workflowId].DbId;
                if (this.checked === true) {
                    workflowsToDelete.push(dbId);
                } else {
                    var dbIdIndex = workflowsToDelete.indexOf(dbId);
                    if (dbIdIndex > -1) {
                        workflowsToDelete.splice(dbIdIndex, 1);
                    }
                }
            };
        }

        var checkBoxDeleteAll = document.getElementById("wf-delete-all");
        checkBoxDeleteAll.onclick = function () {
            for (var j = 0; j < rows.length; j++) {
                var row = rows[j];
                var workflowId = parseInt(row.getElementsByClassName("wf-id")[0].innerHTML);
                var dbId = workflows[workflowId].DbId;
                var deleteWorkflowCheckBox = row.getElementsByClassName("wf-delete")[0];
                if (deleteWorkflowCheckBox.checked === false) {
                    deleteWorkflowCheckBox.checked = true;
                    workflowsToDelete.push(dbId);
                } else {
                    deleteWorkflowCheckBox.checked = false;
                    var dbIdIndex = workflowsToDelete.indexOf(dbId);
                    if (dbIdIndex > -1) {
                        workflowsToDelete.splice(dbIdIndex, 1);
                    }
                }
            }
        };

        if (typeof callback !== "undefined") {
            callback();
        }
        // End of get workflows
    }

}