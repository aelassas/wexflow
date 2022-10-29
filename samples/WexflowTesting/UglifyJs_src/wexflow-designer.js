function WexflowDesigner(id, uri) {
    "use strict";

    hljs.initHighlightingOnLoad();

    uri = trimEnd(uri, "/");
    var selectedId = -1;
    var workflows = {};
    var workflowInfos = {};
    var workflowTasks = {};
    var timer = null;
    var timerInterval = 500; // ms
    var saveCalled = false;

    var rightPanelHtml = "<h3><button id='wf-xml' type='button' class='wf-action-left btn btn-dark btn-sm'>Xml</button></h3>" +
        "<pre><code id='wf-xml-container' class='xml'></code></pre>" +
        "<table class='wf-designer-table'>" +
        "<tbody>" +
        "<tr><td class='wf-title'>Id</td><td class='wf-value'><input id='wf-id' type='text'  /></td></tr>" +
        "<tr><td class='wf-title'>Name</td><td class='wf-value'><input id='wf-name' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>LaunchType</td><td class='wf-value'><select id='wf-launchType'><option value=''></option><option value='startup'>Startup</option><option value='trigger'>Trigger</option><option value='periodic'>Periodic</option>><option value='cron'>Cron</option></select></td></tr>" +
        "<tr><td class='wf-title'>Period</td><td class='wf-value'><input id='wf-period' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Cron expression</td><td class='wf-value'><input id='wf-cron' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Enabled</td><td class='wf-value'><input id='wf-enabled' type='checkbox' checked/></td></tr>" +
        "<tr><td class='wf-title'>Description</td><td class='wf-value'><input id='wf-desc' type='text' /></td></tr>" +
        "<tr><td class='wf-title'>Path</td><td id='wf-path' class='wf-value'></td></tr>" +
        "<tr><td class='wf-title'>Status</td><td id='wf-status' class='wf-value'></td></tr>" +
        "</tbody>" +
        "</table>" +
        "<div id='wf-tasks'>" +
        "</div>" +
        "<button type='button' id='wf-add-task' class='btn btn-dark btn-sm'>New task</button>" +
        "<h3 id='wf-execution-graph-title'>Execution graph</h3>" +
        "<div id='wf-execution-graph'></div>";

    var html = "<div id='wf-container'>" +
        "<div id='wf-workflows'></div>" +
        "<div id='wf-action'>" +
        "<button type='button' id='wf-add-workflow' class='btn btn-dark btn-sm'>New workflow</button>" +
        "<button id='wf-delete' type='button' class='wf-action-right btn btn-danger btn-sm'>Delete</button>" +
        "<button id='wf-save' type= 'button' class='wf-action-right btn btn-secondary btn-sm'>Save</button>" +
        "<button id='wf-cancel' type= 'button' class='wf-action-right btn btn-secondary btn-sm'>Cancel</button>" +
        "</div>"+
        "<div id='wf-designer-right-panel' style='display: none;'>" +
        rightPanelHtml +
        "</div>" +
        "</div>";

    document.getElementById(id).innerHTML = html;

    document.getElementById("wf-add-workflow").onclick = function () {
        saveCalled = false;
        var wfRightPanel = document.getElementById("wf-designer-right-panel");
        wfRightPanel.innerHTML = rightPanelHtml;
        wfRightPanel.style.display = "block";
        document.getElementById("wf-cancel").style.display = "block";
        document.getElementById("wf-save").style.display = "block";
        document.getElementById("wf-add-task").style.display = "block";
        document.getElementById("wf-delete").style.display = "none";
        document.getElementById("wf-xml").style.display = "none";

        document.getElementById("wf-cancel").onclick = function () {
            if (saveCalled === true) {
                var wfIdStr = document.getElementById("wf-id").value;
                if (isInt(wfIdStr)) {
                    var workflowId = parseInt(wfIdStr);
                    cancel(workflowId);
                }
            }
        };

        var selected = document.getElementsByClassName("selected");
        if (selected.length > 0) {
            selected[0].className = selected[0].className.replace("selected", "");
        }

        var previousId = -1;
        document.getElementById("wf-id").onkeyup = function () {
            var workflowId = parseInt(this.value);

            if (previousId === -1) {
                workflowInfos[workflowId] = {
                    "Id": workflowId,
                    "Name": document.getElementById("wf-name").value,
                    "LaunchType": launchTypeReverse(document.getElementById("wf-launchType").value),
                    "Period": document.getElementById("wf-period").value,
                    "CronExpression": document.getElementById("wf-cron").value,
                    "IsEnabled": document.getElementById("wf-enabled").checked,
                    "Description": document.getElementById("wf-desc").value,
                    "Path": "",
                    "IsNew": true
                };

                workflowTasks[workflowId] = [];
            } else {
                workflowInfos[workflowId] = workflowInfos[previousId];
                workflowInfos[workflowId].Id = workflowId;
                workflowTasks[workflowId] = workflowTasks[previousId];
            }
            previousId = workflowId;
        };

        // Input events
        document.getElementById("wf-name").onkeyup = function () {
            var that = this;
            var wfIdStr = document.getElementById("wf-id").value;
            if (isInt(wfIdStr)) {
                var workflowId = parseInt(wfIdStr);
                workflowInfos[workflowId].Name = this.value;

                if (this.value !== "" && saveCalled === false) {
                    get(uri + "/workflowsFolder",
                        function (workflowsFolder) {
                            workflowInfos[workflowId].Path = trimEnd(workflowsFolder, "\\") + "\\" + that.value + ".xml";
                            document.getElementById("wf-path").innerHTML = workflowInfos[workflowId].Path;
                        },
                        function () {
                            alert("An error occured while retrieving workflowsFolder.");
                        });
                }
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

        document.getElementById("wf-desc").onkeyup = function () {
            var wfIdStr = document.getElementById("wf-id").value;
            if (isInt(wfIdStr)) {
                var workflowId = parseInt(wfIdStr);
                workflowInfos[workflowId].Description = this.value;
            }
        };

        document.getElementById("wf-delete").onclick = deleteWorkflow;

        document.getElementById("wf-save").onclick = function () {
            saveClick(true);
        };

        get(uri + "/taskNames",
            function (taskNames) {
                document.getElementById("wf-add-task").onclick = function () {

                    var wfIdStr = document.getElementById("wf-id").value;
                    if (isInt(wfIdStr)) {
                        var workflowId = parseInt(wfIdStr);
                        addTask(workflowId, taskNames);
                    } else {
                        alert("Please enter a valid workflow id.");
                    }
                };
            },
            function () {
                alert("An error occured while retrieving task names.");
            });

    };

    function saveClick(checkId) {

        var selectedId = -1;
        var wfSelectedWorkflow = document.getElementsByClassName("selected");
        if (wfSelectedWorkflow.length > 0) {
            selectedId = parseInt(wfSelectedWorkflow[0].getElementsByClassName("wf-id")[0].innerHTML);
        }

        var wfIdStr = document.getElementById("wf-id").value;
        if (isInt(wfIdStr)) {
            var workflowId = parseInt(wfIdStr);
            
            if (checkId === true) {
                get(uri + "/isWorkflowIdValid/" + workflowId,
                    function(res) {
                        if (res === true || saveCalled === true) {
                            if (document.getElementById("wf-name").value === "") {
                                alert("Enter a name for this workflow.");
                            } else {
                                var lt = document.getElementById("wf-launchType").value;
                                if (lt === "") {
                                    alert("Select a launchType for this workflow.");
                                } else {
                                    if (lt === "periodic" && document.getElementById("wf-period").value === "") {
                                        alert("Enter a period for this workflow.");
                                    } else {
                                        if (lt === "cron" && document.getElementById("wf-cron").value === "") {
                                            alert("Enter a cron expression for this workflow.");
                                        } else {
                                            var saveFunc = function () {
                                                save(workflowId,
                                                    selectedId === -1 ? workflowId : selectedId,
                                                    function () {
                                                        saveCalled = true;
                                                        workflowInfos[workflowId].IsNew = false;
                                                        document.getElementById("wf-delete").style.display = "inline-block";
                                                    });
                                            };

                                            // Period validation
                                            if (lt === "periodic" && document.getElementById("wf-period").value !== "") {
                                                var period = document.getElementById("wf-period").value;
                                                get(uri + "/isPeriodValid/" + period,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            alert("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                        }
                                                    }
                                                );
                                            } // Cron expression validation
                                            else if (lt === "cron" && document.getElementById("wf-cron").value !== "") {
                                                var expression = document.getElementById("wf-cron").value;
                                                var expressionEncoded = encodeURIComponent(expression);

                                                get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                            }
                                                        }
                                                    }
                                                );
                                            } else {
                                                saveFunc();
                                            }

                                        }
                                    }
                                }
                            }
                        } else {
                            alert("The workflow id is already in use. Enter another one.");
                        }
                    });
            } else {
                save(workflowId,
                    selectedId === -1 ? workflowId : selectedId,
                    function () {
                        saveCalled = true;
                        workflowInfos[workflowId].IsNew = false;
                        document.getElementById("wf-delete").style.display = "inline-block";
                    });
            }

        } else {
            alert("Enter a valid workflow id.");
        }
    }

    function openInNewTab(url) {
        var win = window.open(url, "_blank");
        if (typeof win !== "undefined" && win !== null) {
            win.focus();
        }
    }

    function save(workflowId, selectedId, callback) {
        clearInterval(timer);
        workflowInfos[workflowId] = workflowInfos[selectedId];
        workflowTasks[workflowId] = workflowTasks[selectedId];

        var json = { "Id": selectedId, "WorkflowInfo": workflowInfos[workflowId], "Tasks": workflowTasks[workflowId] };
        post(uri + "/save", function (res) {
            if (res === true) {
                if (typeof callback !== "undefined") {
                     callback();
                }

                // Reload workflows list
                setTimeout(function () {
                    loadWorkflows(function () {
                        // Select the workflow
                        var wfWorkflowsTable = document.getElementById("wf-workflows-table");

                        for (var i = 0; i < wfWorkflowsTable.rows.length; i++) {
                            var row = wfWorkflowsTable.rows[i];
                            var wfId = row.getElementsByClassName("wf-id")[0];
                            if (typeof wfId !== "undefined" && wfId !== null) {
                                var swId = parseInt(wfId.innerHTML);

                                if (swId === workflowId) {
                                    var selected = document.getElementsByClassName("selected");
                                    if (selected.length > 0) {
                                        selected[0].className = selected[0].className.replace("selected", "");
                                    }

                                    row.className += "selected";

                                    // Scroll to the workflow
                                    row.scrollIntoView(true);
                                }
                            }
                        }

                        // Update the status
                        get(uri + "/workflow/" + workflowId,
                            function(workflow) {
                                updateStatusTimer(workflow);
                            });

                        // Show the xml button
                        document.getElementById("wf-xml-container").innerHTML = '';

                        document.getElementById("wf-xml").style.display = "inline";

                        document.getElementById("wf-xml").onclick = function () {
                            loadXml(workflowId);
                        };

                    });
                }, 700); // is 500ms sufficient?

                alert("workflow " + workflowInfos[workflowId].Id + " saved and loaded with success.");
            } else {
                alert("An error occured while saving the workflow " + workflowId + ".");
            }
        }, function () {
            alert("An error occured while saving the workflow " + workflowId + ".");
        }, json);
    }

    function updateStatusTimer(workflow) {
        clearInterval(timer);

        if (workflow.IsEnabled === true) {
            timer = setInterval(function () {
                updateStatus(workflow.Id, false);
            }, timerInterval);

            updateStatus(workflow.Id, true);
        } else {
            updateStatus(workflow.Id, true);
        }
    }

    function workflowStatusChanged(workflow) {
        var changed = workflows[workflow.Id].IsRunning !== workflow.IsRunning ||
            workflows[workflow.Id].IsPaused !== workflow.IsPaused;
        workflows[workflow.Id].IsRunning = workflow.IsRunning;
        workflows[workflow.Id].IsPaused = workflow.IsPaused;
        return changed;
    }

    function updateStatus(workflowId, force) {
        getWorkflow(workflowId,
            function (workflow) {
                if (workflow.IsEnabled === false) {
                    notify("This workflow is disabled.");
                } else {
                    if (force === false && workflowStatusChanged(workflow) === false) return;

                    if (workflow.IsRunning === true && workflow.IsPaused === false) {
                        notify("This workflow is running...");
                    } else if (workflow.IsPaused === true) {
                        notify("This workflow is suspended.");
                    } else {
                        notify("This workflow is not running.");
                    }
                }
            });
    }

    function notify(status) {
        document.getElementById("wf-status").innerHTML = status;
    }

    function deleteWorkflow() {
        var r = confirm("Are you sure you want to delete this workflow?");
        if (r === true) {
            var workflowId = parseInt(document.getElementById("wf-id").value);

            post(uri + "/delete/" + workflowId,
                function (res) {
                    if (res === true) {
                        clearInterval(timer);
                        setTimeout(function () {
                            loadWorkflows();
                            document.getElementById("wf-designer-right-panel").style.display = "none";
                            document.getElementById("wf-cancel").style.display = "none";
                            document.getElementById("wf-save").style.display = "none";
                            document.getElementById("wf-delete").style.display = "none";
                        }, 500);
                    } else {
                        alert("An error occured while deleting this workflow.");
                    }
                },
                function () {
                    alert("An error occured while deleting this workflow.");
                });
        }
    }

    function addTask(workflowId, taskNames) {
        var wfTask = document.createElement("div");
        wfTask.className = "wf-task";
        var newTaskHtml =
            "<h5 class='wf-task-title'>" +
            "<label class='wf-task-title-label'>Task</label>" +
            "<button type='button' class='wf-remove-task btn btn-danger btn-sm' style='display: block;'>Delete</button>" +
            "<button type='button' class='wf-show-doc btn btn-dark btn-sm'>Documentation</button>" +
            "<button type='button' class='wf-show-taskxml btn btn-dark btn-sm'>Xml</button>" +
            "<button type='button' class='wf-add-setting btn btn-dark btn-sm'>New setting</button>" +
            "</h5>" +
            "<table class='wf-designer-table'>" +
            "<tbody>" +
            "<tr><td class='wf-taskxml' colspan='2'><pre><code class='wf-taskxml-container'></code></pre></td></tr>" +
            "<tr><td class='wf-title'>Id</td><td class='wf-value'><input class='wf-task-id' type='text' /></td></tr>" +
            "<tr><td class='wf-title'>Name</td><td class='wf-value'><select class='wf-task-name'>";

        newTaskHtml += "<option value=''></option>";
        for (var i1 = 0; i1 < taskNames.length; i1++) {
            var taskName = taskNames[i1];
            newTaskHtml += "<option value='" + taskName + "'>" + taskName + "</option>";
        }

        newTaskHtml += "</select></td></tr>" +
            "<tr><td class='wf-title'>Description</td><td class='wf-value'><input class='wf-task-desc' type='text' /></td></tr>" +
            "<tr><td class='wf-title'>Enabled</td><td class='wf-value'><input class='wf-task-enabled' type='checkbox' checked /></td></tr>" +
            "</tbody>" +
            "</table>" +
            "<table class='wf-designer-table wf-settings'>" +
            "</table>";

        wfTask.innerHTML = newTaskHtml;

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
            this.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName("wf-task-title-label")[0].innerHTML = "Task " + wfTaskId.value;
            loadExecutionGraph();
        }

        var wfTaskName = wfTask.getElementsByClassName("wf-task-name")[0];
        wfTaskName.onchange = function () {
            var wfSettingsTable = wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName("wf-settings")[0];

            workflowTasks[workflowId][index].Name = wfTaskName.value;

            if (wfTaskName.value !== "") {
                get(uri + "/settings/" + wfTaskName.value,
                    function(settings) {
                        
                        var wfAddSetting = wfTaskName.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName("wf-add-setting")[0];
                        workflowTasks[workflowId][index].Settings = [];
                        wfSettingsTable.innerHTML = "";
                        for (var i = 0; i < settings.length; i++) {
                            var settingName = settings[i];
                            addSetting(workflowId, wfAddSetting, settingName);
                        }
                    },
                    function() {
                        alert("An error occured while retrieving settings.");
                    });
            } else {
                workflowTasks[workflowId][index].Settings = [];
                wfSettingsTable.innerHTML = "";
            }
        }

        var wfTaskDesc = wfTask.getElementsByClassName("wf-task-desc")[0];
        wfTaskDesc.onkeyup = function () {
            workflowTasks[workflowId][index].Description = wfTaskDesc.value;
            loadExecutionGraph();
        }

        var wfTaskEnabled = wfTask.getElementsByClassName("wf-task-enabled")[0];
        wfTaskEnabled.onchange = function () {
            workflowTasks[workflowId][index].IsEnabled = wfTaskEnabled.checked;
        }

        var wfAddSetting = wfTask.getElementsByClassName("wf-add-setting")[0];
        wfAddSetting.onclick = function() {
            addSetting(workflowId, this);
        };

        // remove task
        var wfRemoveTask = wfTask.getElementsByClassName("wf-remove-task")[0];
        wfRemoveTask.onclick = function() {
            removeTask(workflowId, this);
            loadExecutionGraph();
        };

        // xml
        var wfTaskToXml = wfTask.getElementsByClassName("wf-show-taskxml")[0];
        wfTaskToXml.onclick = function() {
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
            alert("Please select a task name.");
        } else {
            get(uri + "/settings/" + taskName, function(settings) {
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
                    cell1Html += "<option value='" + settingName + "'" + (sn === settingName ? "selected" : "")+">" + settingName + "</option>";
                }
                cell1Html += "</select>";
                cell1.innerHTML = cell1Html;

                cell2.className = "wf-setting-value-td";
                cell2.innerHTML = "<input class='wf-setting-value' type='text' /><table class='wf-attributes'></table>";

                cell3.className = "wf-add-attribute-td";
                cell3.innerHTML = "<button type='button' class='wf-add-attribute btn btn-dark btn-sm'>New attribute</button>";
                cell4.colSpan = "2";
                cell4.innerHTML = "<button type='button' class='wf-remove-setting btn btn-danger btn-sm'>Delete</button>";

                var taskIndex = getElementIndex(btn.parentElement.parentElement);
                var task = workflowTasks[workflowId][taskIndex];
                task.Settings.push({ "Name": (typeof sn !== "undefined" ? sn : (settings.length ===1 ? settings[0]:"")), "Value": "", "Attributes": [] });
                
                var index = task.Settings.length - 1;
                var wfSettingName = wfSettingsTable.getElementsByClassName("wf-setting-name")[index];
                wfSettingName.onchange = function () {
                    var index2 = getElementIndex(wfSettingName.parentElement.parentElement);
                    task.Settings[index2].Name = wfSettingName.value;
                    
                    var wfAddAttributeTd = wfSettingName.parentElement.parentElement.getElementsByClassName("wf-add-attribute-td")[0];
                    if (wfSettingName.value === "selectFiles" || wfSettingName.value === "selectAttachments") {
                        wfAddAttributeTd.style.display = "block";
                    } else {
                        wfAddAttributeTd.style.display = "none";
                    }
                }

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
                    }
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
            }, function() {
                alert("An error occured while retrieving the settings.");
            });
        }

    }

    function addAttribute(workflowId, btn) {
        var wfAttributesTable = btn.parentElement.parentElement.getElementsByClassName("wf-attributes")[0];

        var row = wfAttributesTable.insertRow(-1);
        var cell1 = row.insertCell(0);
        var cell2 = row.insertCell(1);
        var cell3 = row.insertCell(2);

        cell1.innerHTML = "<input class='wf-attribute-name' type='text' />";
        cell2.innerHTML = "<input class='wf-attribute-value' type='text' />";
        cell3.innerHTML = "<button type='button' class='wf-remove-attribute btn btn-danger btn-sm'>Delete</button>";

        var taskIndex =
            getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement);
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
        var taskIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var task = workflowTasks[workflowId][taskIndex];

        var settingIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement);
        var attributeIndex = getElementIndex(btn.parentElement.parentElement);

        task.Settings[settingIndex].Attributes = deleteRow(task.Settings[settingIndex].Attributes, attributeIndex);
        btn.parentElement.parentElement.remove();
    }

    function removeSetting(workflowId, btn) {
        var taskIndex = getElementIndex(btn.parentElement.parentElement.parentElement.parentElement.parentElement);
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

    function trimEnd(string, charToRemove) {
        while (string.charAt(string.length - 1) === charToRemove) {
            string = string.substring(0, string.length - 1);
        }

        return string;
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

    function get(url, callback, errorCallback) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200 && callback) {
                var data = JSON.parse(this.responseText);
                callback(data);
            }
        };
        xmlhttp.onerror = function () {
            if (errorCallback) errorCallback();
        };
        xmlhttp.open("GET", url, true);
        xmlhttp.send();

    }

    function post(url, callback, errorCallback, json) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200 && callback) {
                var data = JSON.parse(this.responseText);
                callback(data);
            }
        };
        xmlhttp.onerror = function () {
            if (errorCallback) errorCallback();
        };
        xmlhttp.open("POST", url, true);
        //xmlhttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        xmlhttp.send(JSON.stringify(json));
    }

    function getWorkflow(wid, func) {
        get(uri + "/workflow/" + wid,
            function (w) {
                func(w);
            });
    }

    function getTasks(wid, func) {
        get(uri + "/tasks/" + wid,
            function (tasks) {
                func(tasks);
            });
    }

    function getXml(wid, func) {
        get(uri + "/xml/" + wid,
            function (tasks) {
                func(tasks);
            });
    }

    function loadExecutionGraph(workflow) {

        var layout = {
            name: 'dagre'
        };

        var style = [
            {
                selector: 'node',
                style: {
                    'content': 'data(name)',
                    'text-opacity': 0.7,
                    'text-valign': 'center',
                    'text-halign': 'right',
                    'background-color': '#373737' // 11479e
                }
            },
            {
                selector: 'edge',
                style: {
                    'curve-style': 'bezier',
                    'width': 4,
                    'target-arrow-shape': 'triangle',
                    'line-color': '#ffb347', // 9dbaea
                    'target-arrow-color': '#ffb347' // 9dbaea
                }
            }
        ];
        var wfExecutionGraph = document.getElementById('wf-execution-graph');
        
        if (typeof workflow === "undefined" || workflow === null || workflow.IsExecutionGraphEmpty === true) {

            var nodes = [];
            var edges = [];

            var wfTasks = document.getElementsByClassName("wf-task");
            for (var index4 = 0; index4 < wfTasks.length; index4++) {
                var wfTask = wfTasks[index4];
                var wfDesc = wfTask.getElementsByClassName("wf-task-desc")[0].value;
                var taskLabel = wfTask.getElementsByClassName("wf-task-title-label")[0].innerHTML + ': ' + wfDesc;
                nodes.push({ data: { id: 'n' + index4, name: taskLabel } });
            }

            for (var index5 = 0; index5 < nodes.length; index5++) {
                var node = nodes[index5];
                var source = node.data.id;
                if (index5 + 1 < nodes.length) {
                    var target = nodes[index5 + 1].data.id;
                    edges.push({ data: { source: source, target: target } });
                }
            }

            cytoscape({
                container: wfExecutionGraph,
                boxSelectionEnabled: false,
                autounselectify: true,
                layout: layout,
                style: style,
                elements: {
                    nodes: nodes,
                    edges: edges
                }
            });
        } else if (workflow.IsExecutionGraphEmpty === false) {

            get(uri + "/graph/" + workflow.Id,
                function(wfNodes) {
                    var nodes = [];
                    var edges = [];

                    for (var i = 0; i < wfNodes.length; i++) {
                        var wfNode = wfNodes[i];
                        nodes.push({ data: { id: wfNode.Id, name: wfNode.Name } });
                        if (wfNode.ParentId !== "n-1") {
                            edges.push({ data: { source: wfNode.ParentId, target: wfNode.Id } }); 
                        }
                    }

                    cytoscape({
                        container: wfExecutionGraph,
                        boxSelectionEnabled: false,
                        autounselectify: true,
                        layout: layout,
                        style: style,
                        elements: {
                            nodes: nodes,
                            edges: edges
                        }
                    });
                },
                function() {
                    alert("An error occured while retrieving the execution graph of this workflow.");
                });
        }
        // end of execution graph
    }

    function loadXml(workflowId) {
        getXml(workflowId,
            function (xml) {
                var xmlContainer = document.getElementById("wf-xml-container");
                xmlContainer.innerHTML = escapeXml(xml);
                hljs.highlightBlock(xmlContainer);
            });
    }

    function showTaskXml(workflowId, btn) {
        var index = getElementIndex(btn.parentElement.parentElement);
        var task = workflowTasks[workflowId][index];

        post(uri + "/taskToXml",
            function (xml) {
                var xmlContainer = btn.parentElement.parentElement.getElementsByClassName("wf-taskxml")[0];
                xmlContainer.style.display = 'table-cell';
                var codeContainer = xmlContainer.getElementsByClassName("wf-taskxml-container")[0];
                codeContainer.innerHTML = escapeXml(xml);
                hljs.highlightBlock(codeContainer);
            },
            function () {
                alert("An error occured while retrieving the Xml of the task " + task.Id + ".");
            }, task);
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

    function loadRightPanel(workflowId) {

        document.getElementById("wf-cancel").style.display = "block";
        document.getElementById("wf-save").style.display = "block";
        document.getElementById("wf-delete").style.display = "inline-block";
        document.getElementById("wf-xml").style.display = "inline";

        if (document.getElementById('wf-designer-right-panel').style.display === 'none') {
            document.getElementById('wf-designer-right-panel').style.display = 'block';
        }

        document.getElementById("wf-xml-container").innerHTML = '';

        document.getElementById("wf-xml").onclick = function () {
            loadXml(workflowId);
        };

        document.getElementById("wf-delete").onclick = deleteWorkflow;

        getWorkflow(workflowId,
            function (workflow) {

                workflowInfos[workflow.Id] = {
                    "Id": workflow.Id,
                    "Name": workflow.Name,
                    "LaunchType": workflow.LaunchType,
                    "Period": workflow.Period,
                    "CronExpression": workflow.CronExpression,
                    "IsEnabled": workflow.IsEnabled,
                    "Description": workflow.Description,
                    "Path": workflow.Path,
                    "IsNew": false
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
                wfPeriod.value = workflow.Period;
                wfPeriod.onkeyup = function () {
                    workflowInfos[workflowId].Period = wfPeriod.value;
                };

                var wfCron = document.getElementById("wf-cron");
                wfCron.value = workflow.CronExpression;
                wfCron.onkeyup = function () {
                    workflowInfos[workflowId].CronExpression = wfCron.value;
                };

                var wfEnabled = document.getElementById("wf-enabled");
                wfEnabled.checked = workflow.IsEnabled;
                wfEnabled.onchange = function () {
                    workflowInfos[workflowId].IsEnabled = wfEnabled.checked;
                };

                var wfDesc = document.getElementById("wf-desc");
                wfDesc.value = workflow.Description;
                wfDesc.onkeyup = function () {
                    workflowInfos[workflowId].Description = wfDesc.value;
                };

                document.getElementById("wf-path").innerHTML = workflow.Path;

                // Status
                updateStatusTimer(workflow);

                // Tasks
                getTasks(workflowId,
                    function (tasks) {

                        workflowTasks[workflowId] = tasks;

                        var tasksHtml = "";

                        get(uri + "/taskNames",
                            function (taskNames) {

                                for (var i = 0; i < tasks.length; i++) {
                                    var task = tasks[i];

                                    tasksHtml += "<div class='wf-task'>" +
                                        "<h5 class='wf-task-title'><label class='wf-task-title-label'>Task " + task.Id + "</label>" +
                                        "<button type='button' class='wf-remove-task btn btn-danger btn-sm'>Delete</button>" +
                                        "<button type='button' class='wf-show-doc btn btn-dark btn-sm'>Documentation</button>" +
                                        "<button type='button' class='wf-show-taskxml btn btn-dark btn-sm'>Xml</button>" +
                                        "<button type='button' class='wf-add-setting btn btn-dark btn-sm'>New setting</button>" +
                                        "</h5>" +
                                        "<table class='wf-designer-table'>" +
                                        "<tbody>" +
                                        "<tr><td class='wf-taskxml' colspan='2'><pre><code class='wf-taskxml-container'></code></pre></td></tr>" +
                                        "<tr><td class='wf-title'>Id</td><td class='wf-value'><input class='wf-task-id' type='text' value='" + task.Id + "'" + (workflow.IsExecutionGraphEmpty === false ? "readonly" : "") + "/></td></tr>" +
                                        "<tr><td class='wf-title'>Name</td><td class='wf-value'><select class='wf-task-name'>";

                                    for (var i1 = 0; i1 < taskNames.length; i1++) {
                                        var taskName = taskNames[i1];
                                        tasksHtml += "<option value='" + taskName + "' " + (taskName === task.Name ? "selected" : "") + ">" + taskName + "</option>";
                                    }

                                    tasksHtml += "</select>" +
                                        "</td></tr>" +
                                        "<tr><td class='wf-title'>Description</td><td class='wf-value'><input class='wf-task-desc' type='text' value='" + task.Description + "' /></td></tr>" +
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

                                        tasksHtml += "<input class='wf-setting-value' type='text' value='" + setting.Value + "'  />";

                                        // settings attributes (for selectFiles and selectAttachments settings only)
                                        tasksHtml += "<table class='wf-attributes'>";

                                        for (var k = 0; k < setting.Attributes.length; k++) {
                                            var attr = setting.Attributes[k];
                                            tasksHtml +=
                                                "<tr>" +
                                                "<td><input class='wf-attribute-name' type='text' value='" + attr.Name + "'  /></td>" +
                                                "<td><input class='wf-attribute-value' type='text' value='" + attr.Value + "'  /></td>" +
                                                "<td><button type='button' class='wf-remove-attribute btn btn-danger btn-sm'>Delete</button></td>" +
                                                "</tr>";
                                        }

                                        tasksHtml += "</table>";

                                        tasksHtml += "</td>" +
                                            "<td class='wf-add-attribute-td'><button type='button' class='wf-add-attribute btn btn-dark btn-sm'>New attribute</button></td>" +
                                            "<td colspan='2'><button type='button' class='wf-remove-setting btn btn-danger btn-sm'>Delete</button></td>" +
                                            "</tr>";
                                    }

                                    tasksHtml += "</tbody>" +
                                        "</table>" +
                                        "</div > ";
                                }

                                document.getElementById("wf-tasks").innerHTML = tasksHtml;

                                // load settings in select tags
                                /*var wfSettingNameTds = document.getElementsByClassName("wf-setting-name-td");
                                
                                for (var i2 = 0; i2 < wfSettingNameTds.length; i2++) {
                                    var wfSettingNameTd = wfSettingNameTds[i2];
                                    var tn = wfSettingNameTd.getElementsByClassName("wf-setting-name-hidden")[0].value;

                                    get(uri + "/settings/" + tn, function (settings) {
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

                                var wfAddAttributsTds = document.getElementsByClassName("wf-add-attribute-td");
                                for (var i3 = 0; i3 < wfAddAttributsTds.length; i3++) {
                                    var wfAddAttributeTd = wfAddAttributsTds[i3];
                                    var settingValue =
                                        wfAddAttributeTd.parentElement.getElementsByClassName("wf-setting-name")[0].value;
                                    if (settingValue === "selectFiles" || settingValue === "selectetAttachments") {
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
                                        loadExecutionGraph();
                                    };
                                }

                                // Add task for linear wf
                                var wfAddTask = document.getElementById("wf-add-task");
                                wfAddTask.onclick = function () {
                                    addTask(workflowId, taskNames);
                                }

                                var bindWfTaskId = function (m) {
                                    var wfTaskId = document.getElementsByClassName("wf-task-id")[m];
                                    wfTaskId.onkeyup = function () {
                                        workflowTasks[workflowId][m].Id = wfTaskId.value;
                                        this.parentElement.parentElement.parentElement.parentElement.parentElement.getElementsByClassName("wf-task-title-label")[0].innerHTML = "Task " + wfTaskId.value;
                                        loadExecutionGraph(workflow);
                                    }
                                }

                                var bindWfTaskName = function (m) {
                                    var wfTaskName = document.getElementsByClassName("wf-task-name")[m];
                                    wfTaskName.onchange = function () {
                                        workflowTasks[workflowId][m].Name = wfTaskName.value;
                                    }
                                }

                                var bindWfTaskDesc = function (m) {
                                    var wfTaskDesc = document.getElementsByClassName("wf-task-desc")[m];
                                    wfTaskDesc.onkeyup = function () {
                                        workflowTasks[workflowId][m].Description = wfTaskDesc.value;
                                        loadExecutionGraph();
                                    }
                                }

                                var bindWfTaskEnabled = function (m) {
                                    var wfTaskEnabled =
                                        document.getElementsByClassName("wf-task-enabled")[m];
                                    wfTaskEnabled.onchange = function () {
                                        workflowTasks[workflowId][m].IsEnabled = wfTaskEnabled.checked;
                                    }
                                }

                                var bindwfSettingName = function (m, n) {
                                    var wfSettingName =
                                        document.getElementsByClassName("wf-settings")[m].getElementsByClassName("wf-setting-name")[n];
                                    wfSettingName.onkeyup = function () {
                                        workflowTasks[workflowId][m].Settings[n].Name = wfSettingName.value;
                                        var wfAddAttributeTd = wfSettingName.parentElement.parentElement.getElementsByClassName("wf-add-attribute-td")[0];
                                        if (wfSettingName.value === "selectFiles" || wfSettingName.value === "selectAttachments") {
                                            wfAddAttributeTd.style.display = "block";
                                        } else {
                                            wfAddAttributeTd.style.display = "none";
                                        }
                                    }
                                }

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
                                loadExecutionGraph(workflow);

                            },
                            function () {
                                alert("An error occured while retrieving task names.");
                            });
                    });
            });
    }

    function cancel(workflowId) {
        loadRightPanel(workflowId);
    }

    function loadWorkflows(callback) {
        get(uri + "/workflows",
            function (data) {
                data.sort(compareById);

                var items = [];
                for (var i = 0; i < data.length; i++) {
                    var val = data[i];
                    workflows[val.Id] = val;

                    items.push("<tr>" +
                        "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>" +
                        "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>" +
                        "</tr>");

                }

                var table = "<table id='wf-workflows-table' class='table table-hover'>" +
                    "<thead>" +
                    "<tr>" +
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
                    rows[j].onclick = function () {
                        selectedId = parseInt(this.getElementsByClassName("wf-id")[0].innerHTML);

                        var selected = document.getElementsByClassName("selected");
                        if (selected.length > 0) {
                            selected[0].className = selected[0].className.replace("selected", "");
                        }

                        this.className += "selected";

                        loadRightPanel(selectedId);

                        document.getElementById("wf-cancel").onclick = function () {
                            cancel(selectedId);
                        };

                        document.getElementById("wf-save").onclick = function () {
                            saveClick(false);
                        };
                    };
                }

                if (typeof callback !== "undefined") {
                    callback();
                }
                // End of get workflows
            },
            function () {
                alert(
                    "An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
            });

    }

    loadWorkflows();
    // End of wexflow Designer
}