window.onload = function () {
    "use strict";

    let translateEditRecord = function () {
        let jBoxContent = document.getElementsByClassName("jBox-content")[0];
        if (jBoxContent) {
            jBoxContent.querySelector(".edit-record-td-id").innerHTML = language.get("edit-record-td-id");
            jBoxContent.querySelector(".edit-record-td-name").innerHTML = language.get("edit-record-td-name");
            jBoxContent.querySelector(".edit-record-td-description").innerHTML = language.get("edit-record-td-description");
            jBoxContent.querySelector(".edit-record-td-approved").innerHTML = language.get("edit-record-td-approved");
            jBoxContent.querySelector(".edit-record-td-start-date").innerHTML = language.get("edit-record-td-start-date");
            jBoxContent.querySelector(".edit-record-td-end-date").innerHTML = language.get("edit-record-td-end-date");
            jBoxContent.querySelector(".edit-record-td-comments").innerHTML = language.get("edit-record-td-comments");
            jBoxContent.querySelector(".edit-record-td-manager-comments").innerHTML = language.get("edit-record-td-manager-comments");
            jBoxContent.querySelector(".edit-record-td-created-by").innerHTML = language.get("edit-record-td-created-by");
            jBoxContent.querySelector(".edit-record-td-created-on").innerHTML = language.get("edit-record-td-created-on");
            jBoxContent.querySelector(".edit-record-td-modified-by").innerHTML = language.get("edit-record-td-modified-by");
            jBoxContent.querySelector(".edit-record-td-modified-on").innerHTML = language.get("edit-record-td-modified-on");
            jBoxContent.querySelector(".edit-record-td-assigned-to").innerHTML = language.get("edit-record-td-assigned-to");
            jBoxContent.querySelector(".edit-record-td-assigned-on").innerHTML = language.get("edit-record-td-assigned-on");

            jBoxContent.querySelector(".edit-record-td-approvers").innerHTML = language.get("edit-record-td-approvers");
            jBoxContent.querySelector(".th-approved-by").innerHTML = language.get("th-approved-by");
            jBoxContent.querySelector(".th-approved").innerHTML = language.get("th-approved");
            jBoxContent.querySelector(".th-approved-on").innerHTML = language.get("th-approved-on");

            jBoxContent.querySelector(".edit-record-td-versions").innerHTML = language.get("edit-record-td-versions");
            jBoxContent.querySelector(".btn-upload-version").value = language.get("btn-upload-version");
        }
        let jBoxFooter = document.getElementsByClassName("jBox-footer")[0];
        if (jBoxFooter) {
            jBoxFooter.querySelector(".record-save").innerHTML = language.get("record-save");
            jBoxFooter.querySelector(".record-cancel").innerHTML = language.get("record-cancel");
            jBoxFooter.querySelector(".record-delete").innerHTML = language.get("record-delete");
        }
    };

    let translateRecordsTable = function () {
        if (document.getElementById("th-name")) {
            document.getElementById("th-name").innerHTML = language.get("record-name");
        }
        if (document.getElementById("th-approved")) {
            document.getElementById("th-approved").innerHTML = language.get("record-approved");
        }
        if (document.getElementById("th-start-date")) {
            document.getElementById("th-start-date").innerHTML = language.get("record-start-date");
        }
        if (document.getElementById("th-end-date")) {
            document.getElementById("th-end-date").innerHTML = language.get("record-end-date");
        }
        if (document.getElementById("th-assigned-to")) {
            document.getElementById("th-assigned-to").innerHTML = language.get("record-assigned-to");
        }
        if (document.getElementById("th-assigned-on")) {
            document.getElementById("th-assigned-on").innerHTML = language.get("record-assigned-on");
        }
    };

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

        document.getElementById("search-records").placeholder = language.get("search-records");
        document.getElementById("btn-delete").innerHTML = language.get("btn-delete-record");
        document.getElementById("btn-new-record").innerHTML = language.get("btn-new-record");

        translateRecordsTable();
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
    let searchText = document.getElementById("search-records");
    let username = "";
    let password = "";
    let userProfile = -1;
    let auth = "";
    let modal = null;

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

                            userProfile = u.UserProfile;
                            if (u.UserProfile === 0) {
                                lnkProfiles.style.display = "inline";
                            }

                            if (u.UserProfile === 1) {
                                document.getElementById("btn-delete").style.display = "none";
                            }

                            if (hasNotifications === true) {
                                imgNotifications.src = "images/notification-active.png";
                            } else {
                                imgNotifications.src = "images/notification.png";
                            }

                            let btnLogout = document.getElementById("btn-logout");
                            document.getElementById("navigation").style.display = "block";
                            document.getElementById("content").style.display = "block";

                            btnLogout.onclick = function () {
                                window.deleteUser();
                                window.Common.redirectToLoginPage();
                            };
                            document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                            searchText.onkeyup = function (event) {
                                event.preventDefault();

                                if (event.keyCode === 13) { // Enter
                                    loadRecords();
                                }

                                return false;
                            };

                            loadRecords();

                        }, function () { }, auth);
                    } else {
                        window.Common.redirectToLoginPage();
                    }

                }
            }, function () {
                window.logout();
            }, auth);
    }

    function loadRecords() {
        let loadRecordsTable = function (records) {
            let items = [];
            for (let i = 0; i < records.length; i++) {
                let record = records[i];
                items.push("<tr>"
                    + "<td class='check' " + (userProfile == 1 ? "style='display: none;'" : "") + "><input type='checkbox'></td>"
                    + "<td class='id'>" + record.Id + "</td>"
                    + "<td class='name'>" + record.Name + "</td>"
                    + "<td class='approved'>" + "<input class='record-approved' type='checkbox' " + (record.Approved === true ? "checked" : "") + " disabled>" + "</td>"
                    + "<td class='start-date'>" + (record.StartDate === "" ? "-" : record.StartDate) + "</td>"
                    + "<td class='end-date'>" + (record.EndDate === "" ? "-" : record.EndDate) + "</td>"
                    + "<td class='assigned-to'>" + (record.AssignedTo === "" ? "-" : record.AssignedTo) + "</td>"
                    + "<td class='assigned-on'>" + (record.AssignedOn === "" ? "-" : record.AssignedOn) + "</td>"
                    + "</tr>");

            }

            let table = "<table id='records-table' class='table'>"
                + "<thead class='thead-dark'>"
                + "<tr>"
                + "<th class='check' " + (userProfile == 1 ? "style='display: none;'" : "") + "><input id='check-all' type='checkbox'></th>"
                + "<th class='id'></th>"
                + "<th id='th-name' class='name'>" + "Name" + "</th>"
                + "<th id='th-approved' class='approved'>" + "Approved" + "</th>"
                + "<th id='th-start-date' class='start-date'>" + "Start date" + "</th>"
                + "<th id='th-end-date' class='end-date'>" + "End date" + "</th>"
                + "<th id='th-assigned-to' class='assigned-to'>" + "Assigned to" + "</th>"
                + "<th id='th-assigned-on' class='assigned-on'>" + "Assigned on" + "</th>"
                + "</tr>"
                + "</thead>"
                + "<tbody>"
                + items.join("")
                + "</tbody>"
                + "</table>";

            let divNotifications = document.getElementById("content");
            divNotifications.innerHTML = table;

            translateRecordsTable();

            let getRecord = function (recordId) {
                for (let i = 0; i < records.length; i++) {
                    let record = records[i];
                    if (record.Id === recordId) {
                        return record;
                    }
                }
                return null;
            };

            let recordsTable = document.getElementById("records-table");
            let rows = recordsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
            let recordIds = [];

            for (let i = 0; i < rows.length; i++) {
                let row = rows[i];
                let checkBox = row.getElementsByClassName("check")[0].firstChild;
                checkBox.onchange = function () {
                    let currentRow = this.parentElement.parentElement;
                    let recordId = currentRow.getElementsByClassName("id")[0].innerHTML;
                    if (this.checked === true) {
                        recordIds.push(recordId);
                    } else {
                        recordIds = window.Common.removeItemOnce(recordIds, recordId);
                    }
                };

                let recordApproved = row.querySelector(".record-approved").checked;
                if (recordApproved === true) {
                    row.querySelector(".name").innerHTML += '&nbsp;&nbsp;<span class="label label-approved">Approved</span>';
                }


                row.onclick = function (e) {
                    if (e.target.type && e.target.type === "checkbox") {
                        return;
                    }
                    let recordId = this.getElementsByClassName("id")[0].innerHTML;
                    let record = getRecord(recordId);
                    let editedRecord = JSON.parse(JSON.stringify(record));
                    editedRecord.ModifiedBy = username;
                    let restrictEdit = userProfile === 1 && record.CreatedBy !== username;

                    if (modal) {
                        modal.destroy();
                    }

                    modal = new jBox('Modal', {
                        width: 800,
                        height: 420,
                        title: language.get("record-information"),
                        content: document.getElementById("edit-record").innerHTML,
                        footer: document.getElementById("edit-record-footer").innerHTML,
                        overlay: true,
                        //isolateScroll: false,
                        delayOpen: 0,
                        onOpen: function () {
                            translateEditRecord();
                            let jBoxContent = document.getElementsByClassName("jBox-content")[0];
                            jBoxContent.querySelector(".record-id").value = record.Id;
                            jBoxContent.querySelector(".record-name").value = record.Name;
                            jBoxContent.querySelector(".record-description").innerHTML = record.Description;
                            jBoxContent.querySelector(".record-approved").checked = record.Approved;
                            jBoxContent.querySelector(".record-start-date").value = record.StartDate;
                            jBoxContent.querySelector(".record-end-date").value = record.EndDate;
                            jBoxContent.querySelector(".record-comments").innerHTML = record.Comments;
                            jBoxContent.querySelector(".record-manager-comments").innerHTML = record.ManagerComments;
                            jBoxContent.querySelector(".record-created-by").value = record.CreatedBy;
                            jBoxContent.querySelector(".record-created-on").value = record.CreatedOn;
                            jBoxContent.querySelector(".record-modified-by").value = record.ModifiedBy;
                            jBoxContent.querySelector(".record-modified-on").value = record.ModifiedOn;
                            jBoxContent.querySelector(".record-assigned-to").value = record.AssignedTo;
                            jBoxContent.querySelector(".record-assigned-on").value = record.AssignedOn;

                            if (restrictEdit === true) {
                                jBoxContent.querySelector(".record-name").disabled = true;
                                jBoxContent.querySelector(".record-description").disabled = true;
                                jBoxContent.querySelector(".record-start-date").disabled = true;
                                jBoxContent.querySelector(".record-end-date").disabled = true;
                                jBoxContent.querySelector(".record-manager-comments").disabled = true;
                            }

                            let approvers = [];
                            for (let i = 0; i < record.Approvers.length; i++) {
                                let approver = record.Approvers[i];
                                approvers.push("<tr>"
                                    + "<td>" + approver.ApprovedBy + "</td>"
                                    + "<td>" + "<input type='checkbox' style='width: auto;' disabled" + (approver.Approved === true ? " checked" : "") + ">" + "</td>"
                                    + "<td>" + approver.ApprovedOn + "</td>"
                                    + "</tr>");
                            }
                            let approversTable = jBoxContent.querySelector(".record-approvers");
                            approversTable.getElementsByTagName("tbody")[0].innerHTML = approvers.join("");

                            if (record.Approvers.length === 0) {
                                approversTable.getElementsByTagName("thead")[0].style.display = "none";
                            } else {
                                approversTable.getElementsByTagName("thead")[0].style.display = "table-header-group";
                            }

                            let versions = [];
                            for (let i = 0; i < record.Versions.length; i++) {
                                let version = record.Versions[i];
                                versions.push("<tr>"
                                    + "<td class='version-id'>" + version.Id + "</td>"
                                    + "<td class='version-file-name'><a class='lnk-version-file-name' href='#'>" + version.FileName + "</a>" + (i === record.Versions.length - 1 ? "&nbsp;&nbsp;<span style='color: #28a745; border: 1px solid #34d058; border-radius: 2px; padding: 3px 4px;'>" + language.get("latest-version") + "</span>" : "") + "</td>"
                                    + "<td class='version-created-on'>" + version.CreatedOn + "</td>"
                                    + "<td class='version-file-size'>" + version.FileSize + "</td>"
                                    + "<td class='version-delete'><input type='button' class='btn-delete-version btn btn-danger btn-xs' value='" + language.get("delete-version") + "'></td>"
                                    + "</tr>");
                            }
                            let versionsTable = jBoxContent.querySelector(".record-versions");
                            versionsTable.innerHTML = versions.join("");

                            // Download
                            let versionFiles = versionsTable.querySelectorAll(".lnk-version-file-name");
                            for (let i = 0; i < versionFiles.length; i++) {
                                let versionFile = versionFiles[i];
                                versionFile.onclick = function () {
                                    let versionId = this.parentElement.parentElement.querySelector(".version-id").innerHTML;
                                    let version = null;
                                    for (let j = 0; j < record.Versions.length; j++) {
                                        if (record.Versions[j].Id === versionId) {
                                            version = record.Versions[j];
                                            break;
                                        }
                                    }
                                    let url = "http://" + encodeURIComponent(username) + ":" + encodeURIComponent(password) + "@" + window.Settings.Hostname + ":" + window.Settings.Port + "/api/v1/download-file?p=" + encodeURIComponent(version.FilePath);
                                    window.open(url, "_self");
                                };
                            }

                            // Delete version
                            let deleteVersionBtns = versionsTable.querySelectorAll(".btn-delete-version");
                            for (let i = 0; i < deleteVersionBtns.length; i++) {
                                let deleteVersionBtn = deleteVersionBtns[i];
                                deleteVersionBtn.onclick = function () {
                                    let versionId = this.parentElement.parentElement.querySelector(".version-id").innerHTML;
                                    let versionIndex = -1;
                                    for (let j = 0; j < editedRecord.Versions.length; j++) {
                                        if (editedRecord.Versions[j].Id === versionId) {
                                            versionIndex = j;
                                            break;
                                        }
                                    }
                                    if (versionIndex > -1) {
                                        editedRecord.Versions.splice(versionIndex, 1);
                                        // Update versions table
                                        let rows = versionsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                                        for (let j = 0; j < rows.length; j++) {
                                            let row = rows[j];
                                            let rowVersionId = row.querySelector(".version-id").innerHTML;
                                            if (rowVersionId === versionId) {
                                                row.remove();
                                            }
                                        }
                                    }
                                };
                            }

                            // Upload version
                            let filedialog = document.getElementById("file-dialog");
                            let uploadFileVersion = function (fd) {
                                jBoxContent.querySelector(".spn-upload-version").innerHTML = language.get("uploading");
                                window.Common.post(uri + "/upload-version?r=" + recordId, function (res) {
                                    if (res.Result === true) {
                                        editedRecord.Versions.push({
                                            RecordId: recordId,
                                            FilePath: res.FilePath,
                                            FileName: res.FileName,
                                            CreatedOn: ""
                                        });

                                        // Add row in .record-versions
                                        let row = versionsTable.insertRow(-1);
                                        let cell1 = row.insertCell(0);
                                        let cell2 = row.insertCell(1);
                                        let cell3 = row.insertCell(2);
                                        let cell4 = row.insertCell(3);
                                        let cell5 = row.insertCell(4);

                                        cell1.classList.add("version-id");
                                        cell1.innerHTML = "";
                                        cell2.classList.add("version-file-name");
                                        cell2.innerHTML = "<a class='lnk-version-file-name' href='#'>" + res.FileName + "</a>";
                                        cell3.classList.add("version-created-on");
                                        cell3.innerHTML = "-";
                                        cell4.classList.add("version-file-size");
                                        cell4.innerHTML = res.FileSize;
                                        cell5.classList.add("version-delete");
                                        cell5.innerHTML = "<input type='button' class='btn-delete-version btn btn-danger btn-xs' value='" + language.get("delete-version") + "'>";

                                        goToBottom(jBoxContent);

                                        // Download version
                                        cell2.querySelector(".lnk-version-file-name").onclick = function () {
                                            let url = "http://" + encodeURIComponent(username) + ":" + encodeURIComponent(password) + "@" + window.Settings.Hostname + ":" + window.Settings.Port + "/wexflow/download-file?p=" + encodeURIComponent(res.FilePath);
                                            window.open(url, "_self");
                                        };

                                        cell5.querySelector(".btn-delete-version").onclick = function () {
                                            // Delete file
                                            window.Common.post(uri + "/delete-temp-version-file?p=" + encodeURIComponent(res.FilePath), function (deleteRes) {
                                                if (deleteRes === true) {
                                                    let versionIndex = -1;
                                                    for (let j = 0; j < editedRecord.Versions.length; j++) {
                                                        if (editedRecord.Versions[j].FilePath === res.FilePath) {
                                                            versionIndex = j;
                                                            break;
                                                        }
                                                    }
                                                    if (versionIndex > -1) {
                                                        editedRecord.Versions.splice(versionIndex, 1);
                                                        // Update versions table
                                                        row.remove();
                                                        window.Common.toastSuccess(language.get("toast-version-file-deleted"));
                                                    }

                                                } else {
                                                    window.Common.toastError(language.get("toast-version-file-delete-error"));
                                                }

                                            }, function () { }, "", auth);
                                        };

                                        jBoxContent.querySelector(".spn-upload-version").innerHTML = "";
                                    }
                                    filedialog.value = "";
                                }, function () { }, fd, auth, true);
                            };

                            jBoxContent.querySelector(".btn-upload-version").onclick = function () {
                                filedialog.click();

                                filedialog.onchange = function (e) {
                                    let file = e.target.files[0];
                                    let fd = new FormData();
                                    fd.append("file", file);

                                    uploadFileVersion(fd);
                                };
                            };

                            // Drag & Drop file
                            jBoxContent.addEventListener("dragover", function (event) {
                                event.stopPropagation();
                                event.preventDefault();
                                // Style the drag-and-drop as a "copy file" operation.
                                event.dataTransfer.dropEffect = "copy";
                            });

                            jBoxContent.addEventListener("drop", function (event) {
                                event.stopPropagation();
                                event.preventDefault();
                                const fileList = event.dataTransfer.files;

                                let file = fileList[0];
                                let fd = new FormData();
                                fd.append("file", file);

                                uploadFileVersion(fd);
                            });

                            let jBoxFooter = document.getElementsByClassName("jBox-footer")[0];
                            if (userProfile === 1 && record.CreatedBy !== username) {
                                jBoxFooter.querySelector(".record-delete").style.display = "none";
                            }

                            jBoxFooter.querySelector(".record-save").onclick = function () {
                                editedRecord.Name = jBoxContent.querySelector(".record-name").value;
                                editedRecord.Description = jBoxContent.querySelector(".record-description").value;
                                editedRecord.StartDate = jBoxContent.querySelector(".record-start-date").value;
                                editedRecord.EndDate = jBoxContent.querySelector(".record-end-date").value;
                                editedRecord.Comments = jBoxContent.querySelector(".record-comments").value;
                                editedRecord.ManagerComments = jBoxContent.querySelector(".record-manager-comments").value;
                                window.Common.post(uri + "/save-record", function (res) {
                                    if (res === true) {
                                        if (username !== record.CreatedBy) {
                                            // Notify approvers
                                            let message = "The record " + record.Name + " was updated by the user " + username + ".";
                                            window.Common.post(uri + "/notify-approvers?r=" + encodeURIComponent(record.Id) + "&m=" + encodeURIComponent(message), function (notifyRes) {
                                                if (notifyRes === true) {
                                                    window.Common.toastInfo(language.get("toast-approvers-notified"));
                                                } else {
                                                    window.Common.toastError(language.get("toast-approvers-notify-error"));
                                                }
                                            }, function () { }, "", auth);
                                        }
                                        // Notify record.AssignedTo
                                        if (record.AssignedTo !== "" && username !== record.AssignedTo) {
                                            let message = "The record " + record.Name + " was updated by the user " + username + ".";
                                            window.Common.post(uri + "/notify?a=" + encodeURIComponent(record.AssignedTo) + "&m=" + encodeURIComponent(message), function (notifyRes) {
                                                if (notifyRes === true) {
                                                    window.Common.toastInfo(language.get("toast-assigned-to-notified"));
                                                } else {
                                                    window.Common.toastError(language.get("toast-assigned-to-notify-error"));
                                                }
                                            }, function () { }, "", auth);
                                        }
                                        modal.close();
                                        modal.destroy();
                                        loadRecords();
                                        window.Common.toastSuccess(language.get("toast-record-saved"));
                                    } else {
                                        window.Common.toastError(language.get("toast-record-save-error"));
                                    }
                                }, function () { }, editedRecord, auth);
                            };

                            jBoxFooter.querySelector(".record-cancel").onclick = function () {
                                window.Common.post(uri + "/delete-temp-version-files", function (res) {
                                    if (res === true) {
                                        window.Common.toastSuccess(language.get("toast-modifications-canceled"));
                                    } else {
                                        window.Common.toastError(language.get("toast-modifications-cancel-error"));
                                    }
                                    modal.close();
                                    modal.destroy();
                                }, function () { }, editedRecord, auth);
                            };

                            jBoxFooter.querySelector(".record-delete").onclick = function () {
                                let cres = confirm(language.get("confirm-delete-record"));
                                if (cres === true) {
                                    window.Common.post(uri + "/delete-records", function (res) {
                                        if (res === true) {
                                            for (let i = 0; i < rows.length; i++) {
                                                let row = rows[i];
                                                let id = row.getElementsByClassName("id")[0].innerHTML;
                                                if (recordId === id) {
                                                    recordIds = window.Common.removeItemOnce(recordIds, recordId);
                                                    row.remove();
                                                    modal.destroy();
                                                }
                                            }

                                        }
                                    }, function () { }, [recordId], auth);
                                }
                            };
                        },
                        onClose: function () {
                            window.Common.post(uri + "/delete-temp-version-files", function (res) {
                                if (res === false) {
                                    window.Common.toastError(language.get("toast-modifications-cancel-error"));
                                }
                            }, function () { }, editedRecord, auth);
                        }
                    });
                    modal.open();
                };
            }

            document.getElementById("check-all").onchange = function () {
                for (let i = 0; i < rows.length; i++) {
                    let row = rows[i];
                    let checkBox = row.getElementsByClassName("check")[0].firstChild;
                    let recordId = row.getElementsByClassName("id")[0].innerHTML;

                    if (checkBox.checked === true) {
                        checkBox.checked = false;
                        recordIds = window.Common.removeItemOnce(recordIds, recordId);
                    } else {
                        checkBox.checked = true;
                        recordIds.push(recordId);
                    }
                }
            };

            document.getElementById("btn-delete").onclick = function () {
                if (recordIds.length === 0) {
                    window.Common.toastInfo(language.get("toast-select-records"));
                } else {
                    let cres = confirm((recordIds.length == 1 ? language.get("confirm-delete-record") : language.get("confirm-delete-records")));
                    if (cres === true) {
                        window.Common.post(uri + "/delete-records", function (res) {
                            if (res === true) {
                                for (let i = recordIds.length - 1; i >= 0; i--) {
                                    let recordId = recordIds[i];
                                    for (let i = 0; i < rows.length; i++) {
                                        let row = rows[i];
                                        let id = row.getElementsByClassName("id")[0].innerHTML;
                                        if (recordId === id) {
                                            recordIds = window.Common.removeItemOnce(recordIds, recordId);
                                            row.remove();
                                        }
                                    }
                                }
                            }
                        }, function () { }, recordIds, auth);
                    }
                }
            };

            document.getElementById("btn-new-record").onclick = function () {
                if (modal) {
                    modal.destroy();
                }

                let newRecord = {};
                newRecord.Versions = [];

                modal = new jBox('Modal', {
                    width: 800,
                    height: 420,
                    title: language.get("record-information"),
                    content: document.getElementById("edit-record").innerHTML,
                    footer: document.getElementById("edit-record-footer").innerHTML,
                    overlay: true,
                    isolateScroll: false,
                    delayOpen: 0,
                    onOpen: function () {
                        translateEditRecord();
                        let jBoxContent = document.getElementsByClassName("jBox-content")[0];
                        jBoxContent.querySelector(".edit-record-tr-id").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-approved").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-created-by").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-created-on").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-modified-by").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-modified-on").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-assigned-to").style.display = "none";
                        jBoxContent.querySelector(".edit-record-tr-assigned-on").style.display = "none";
                        jBoxContent.querySelector(".edit-record-td-start-date").innerHTML += language.get("optional");
                        jBoxContent.querySelector(".edit-record-td-end-date").innerHTML += " (Optional)";
                        jBoxContent.querySelector(".edit-record-tr-approvers").style.display = "none";

                        setTimeout(function () {
                            let recodNameTxt = jBoxContent.querySelector(".record-name");
                            recodNameTxt.focus();
                            recodNameTxt.select();
                        }, 0);

                        // Upload version
                        let filedialog = document.getElementById("file-dialog");
                        let uploadFileVersion = function (fd) {
                            window.Common.post(uri + "/upload-version?r=-1", function (res) {
                                if (res.Result === true) {
                                    newRecord.Versions.push({
                                        RecordId: "-1",
                                        FilePath: res.FilePath,
                                        FileName: res.FileName,
                                        CreatedOn: ""
                                    });

                                    // Add row in .record-versions
                                    let versionsTable = jBoxContent.querySelector(".record-versions");
                                    let row = versionsTable.insertRow(-1);
                                    let cell1 = row.insertCell(0);
                                    let cell2 = row.insertCell(1);
                                    let cell3 = row.insertCell(2);
                                    let cell4 = row.insertCell(3);
                                    let cell5 = row.insertCell(4);

                                    cell1.classList.add("version-id");
                                    cell1.innerHTML = "";
                                    cell2.classList.add("version-file-name");
                                    cell2.innerHTML = "<a class='lnk-version-file-name' href='#'>" + res.FileName + "</a>";
                                    cell3.classList.add("version-created-on");
                                    cell3.innerHTML = "-";
                                    cell4.classList.add("version-file-size");
                                    cell4.innerHTML = res.FileSize;
                                    cell5.classList.add("version-delete");
                                    cell5.innerHTML = "<input type='button' class='btn-delete-version btn btn-danger btn-xs' value='" + language.get("delete-version") + "'>";

                                    goToBottom(jBoxContent);

                                    cell2.querySelector(".lnk-version-file-name").onclick = function () {
                                        let url = "http://" + encodeURIComponent(username) + ":" + encodeURIComponent(password) + "@" + window.Settings.Hostname + ":" + window.Settings.Port + "/api/v1/download-file?p=" + encodeURIComponent(res.FilePath);
                                        window.open(url, "_self");
                                    };

                                    cell5.querySelector(".btn-delete-version").onclick = function () {
                                        // Delete file
                                        window.Common.post(uri + "/delete-temp-version-file?p=" + encodeURIComponent(res.FilePath), function (deleteRes) {
                                            if (deleteRes === true) {
                                                let versionIndex = -1;
                                                for (let j = 0; j < newRecord.Versions.length; j++) {
                                                    if (newRecord.Versions[j].FilePath === res.FilePath) {
                                                        versionIndex = j;
                                                        break;
                                                    }
                                                }
                                                if (versionIndex > -1) {
                                                    newRecord.Versions.splice(versionIndex, 1);
                                                    // Update versions table
                                                    row.remove();
                                                    window.Common.toastSuccess(language.get("toast-version-file-deleted"));
                                                }

                                            } else {
                                                window.Common.toastError(language.get("toast-version-file-delete-error"));
                                            }

                                        }, function () { }, "", auth);
                                    };

                                    jBoxContent.querySelector(".spn-upload-version").innerHTML = "";
                                }
                                filedialog.value = "";
                            }, function () { }, fd, auth, true);
                        };

                        jBoxContent.querySelector(".btn-upload-version").onclick = function () {
                            filedialog.click();

                            filedialog.onchange = function (e) {
                                jBoxContent.querySelector(".spn-upload-version").innerHTML = language.get("uploading");

                                let file = e.target.files[0];
                                let fd = new FormData();
                                fd.append("file", file);

                                uploadFileVersion(fd);
                            };
                        };

                        // Drag & Drop file
                        jBoxContent.addEventListener("dragover", function (event) {
                            event.stopPropagation();
                            event.preventDefault();
                            // Style the drag-and-drop as a "copy file" operation.
                            event.dataTransfer.dropEffect = "copy";
                        });

                        jBoxContent.addEventListener("drop", function (event) {
                            event.stopPropagation();
                            event.preventDefault();
                            const fileList = event.dataTransfer.files;

                            let file = fileList[0];
                            let fd = new FormData();
                            fd.append("file", file);

                            uploadFileVersion(fd);
                        });

                        let jBoxFooter = document.getElementsByClassName("jBox-footer")[0];
                        jBoxFooter.querySelector(".record-delete").style.display = "none";

                        jBoxFooter.querySelector(".record-save").onclick = function () {

                            if (jBoxContent.querySelector(".record-name").value === "") {
                                window.Common.toastInfo(language.get("toast-record-name"));
                                return;
                            }

                            newRecord.Id = "-1";
                            newRecord.Name = jBoxContent.querySelector(".record-name").value;
                            newRecord.Description = jBoxContent.querySelector(".record-description").value;
                            newRecord.StartDate = jBoxContent.querySelector(".record-start-date").value;
                            newRecord.EndDate = jBoxContent.querySelector(".record-end-date").value;
                            newRecord.Comments = jBoxContent.querySelector(".record-comments").value;
                            newRecord.Approved = false;
                            newRecord.ManagerComments = jBoxContent.querySelector(".record-manager-comments").value;
                            newRecord.ModifiedBy = "";
                            newRecord.ModifiedOn = "";
                            newRecord.CreatedBy = username;
                            newRecord.CreatedOn = "";
                            newRecord.AssignedTo = "";
                            newRecord.AssignedOn = "";
                            window.Common.post(uri + "/save-record", function (res) {
                                if (res === true) {
                                    modal.close();
                                    modal.destroy();
                                    loadRecords();
                                    window.Common.toastSuccess(language.get("toast-record-saved"));
                                } else {
                                    window.Common.toastError(language.get("toast-record-save-error"));
                                }
                            }, function () { }, newRecord, auth);
                        };

                        jBoxFooter.querySelector(".record-cancel").onclick = function () {
                            window.Common.post(uri + "/delete-temp-version-files", function (res) {
                                if (res === true) {
                                    window.Common.toastSuccess(language.get("toast-modifications-canceled"));
                                } else {
                                    window.Common.toastError(language.get("toast-modifications-cancel-error"));
                                }
                                modal.close();
                                modal.destroy();
                            }, function () { }, newRecord, auth);
                        };

                        jBoxFooter.querySelector(".record-delete").style.display = "none";
                    },
                    onClose: function () {
                        window.Common.post(uri + "/delete-temp-version-files", function (res) {
                            if (res === false) {
                                window.Common.toastError(language.get("toast-modifications-cancel-error"));
                            }
                        }, function () { }, newRecord, auth);
                    }
                });
                modal.open();

            };

        };

        // Load records
        if (userProfile === 0) {
            window.Common.get(uri + "/search-records?s=" + encodeURIComponent(searchText.value), function (records) {
                loadRecordsTable(records);
            }, function () { }, auth);
        } else if (userProfile === 1) {
            window.Common.get(uri + "/search-records-created-by-or-assigned-to?s=" + encodeURIComponent(searchText.value) + "&c=" + encodeURIComponent(username) + "&a=" + encodeURIComponent(username), function (records) {
                loadRecordsTable(records);
            }, function () { }, auth);
        }

        function goToBottom(element) {
            element.scrollTop = element.scrollHeight - element.clientHeight;
        }
    }
};