window.onload = function () {
    "use strict";

    let translateTable = function () {
        if (document.getElementById("th-assigned-by")) {
            document.getElementById("th-assigned-by").innerHTML = language.get("th-assigned-by");
        }
        if (document.getElementById("th-assigned-on")) {
            document.getElementById("th-assigned-on").innerHTML = language.get("th-assigned-on");
        }
        if (document.getElementById("th-message")) {
            document.getElementById("th-message").innerHTML = language.get("th-message");
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

        document.getElementById("search-notifications").placeholder = language.get("search-notifications");
        document.getElementById("btn-delete").innerHTML = language.get("btn-delete-notification");
        document.getElementById("btn-mark-as-unread").innerHTML = language.get("btn-mark-as-unread");
        document.getElementById("btn-mark-as-read").innerHTML = language.get("btn-mark-as-read");

        translateTable();
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
    let searchText = this.document.getElementById("search-notifications");
    let user = null;
    let username = "";
    let auth = "";

    let suser = getUser();

    if (suser === null || suser === "") {
        window.Common.redirectToLoginPage();
    } else {
        user = JSON.parse(suser);

        username = user.Username;
        let password = user.Password;
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
                                    loadNotifications();
                                }

                                return false;
                            };

                            loadNotifications();

                        }, function () { }, auth);
                    } else {
                        window.Common.redirectToLoginPage();
                    }

                }
            }, function () {
                window.logout();
            }, auth);
    }

    function loadNotifications() {
        window.Common.get(uri + "/search-notifications?a=" + encodeURIComponent(user.Username) + "&s=" + encodeURIComponent(searchText.value), function (notifications) {

            let items = [];
            for (let i = 0; i < notifications.length; i++) {
                let notification = notifications[i];
                items.push("<tr>"
                    + "<td class='check'><input type='checkbox'></td>"
                    + "<td class='id'>" + notification.Id + "</td>"
                    + "<td class='assigned-by " + (notification.IsRead === false ? "bold" : "") + "'>" + notification.AssignedBy + "</td>"
                    + "<td class='assigned-on " + (notification.IsRead === false ? "bold" : "") + "'>" + notification.AssignedOn + "</td>"
                    + "<td class='message " + (notification.IsRead === false ? "bold" : "") + "'>" + notification.Message + "</td>"
                    + "</tr>");

            }

            let table = "<table id='notifications-table' class='table'>"
                + "<thead class='thead-dark'>"
                + "<tr>"
                + "<th class='check'><input id='check-all' type='checkbox'></th>"
                + "<th class='id'></th>"
                + "<th id='th-assigned-by' class='assigned-by'>" + "Assigned by" + "</th>"
                + "<th id='th-assigned-on' class='assigned-on'>" + "Assigned on" + "</th>"
                + "<th id='th-message' class='message'>" + "Message" + "</th>"
                + "</tr>"
                + "</thead>"
                + "<tbody>"
                + items.join("")
                + "</tbody>"
                + "</table>";

            let divNotifications = document.getElementById("content");
            divNotifications.innerHTML = table;

            translateTable();

            let notificationsTable = document.getElementById("notifications-table");
            let rows = notificationsTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
            let notificationIds = [];
            for (let i = 0; i < rows.length; i++) {
                let row = rows[i];
                let checkBox = row.getElementsByClassName("check")[0].firstChild;
                checkBox.onchange = function () {
                    let currentRow = this.parentElement.parentElement;
                    let notificationId = currentRow.getElementsByClassName("id")[0].innerHTML;
                    if (this.checked === true) {
                        notificationIds.push(notificationId);
                    } else {
                        notificationIds = window.Common.removeItemOnce(notificationIds, notificationId);
                    }
                };
            }

            document.getElementById("btn-mark-as-read").onclick = function () {
                window.Common.post(uri + "/mark-notifications-as-read", function (res) {
                    if (res === true) {
                        window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
                            for (let i = 0; i < notificationIds.length; i++) {
                                let notificationId = notificationIds[i];
                                for (let j = 0; j < rows.length; j++) {
                                    let row = rows[j];
                                    let id = row.getElementsByClassName("id")[0].innerHTML;
                                    if (notificationId === id) {
                                        row.getElementsByClassName("assigned-by")[0].classList.remove("bold");
                                        row.getElementsByClassName("assigned-on")[0].classList.remove("bold");
                                        row.getElementsByClassName("message")[0].classList.remove("bold");

                                        // Notify assignedBy
                                        //for (let k = 0; k < notifications.length; k++) {
                                        //    let notification = notifications[k];
                                        //    if (notificationId === notification.Id) {

                                        //        let message = "The user " + username + " has read his notification: " + notification.Message;
                                        //        window.Common.post(uri + "/notify?a=" + encodeURIComponent(notification.AssignedBy) + "&m=" + encodeURIComponent(message), function (notifyRes) {
                                        //            if (notifyRes === true) {
                                        //                window.Common.toastInfo("The assignor was notified that you read the notifictaion assigned on " + notification.AssignedOn + ".");
                                        //            } else {
                                        //                window.Common.toastError("An error occured while notifying the assignor.");
                                        //            }
                                        //        }, function () { }, "", auth);

                                        //        break;
                                        //    }
                                        //}
                                    }
                                }

                                if (hasNotifications === true) {
                                    imgNotifications.src = "images/notification-active.png";
                                } else {
                                    imgNotifications.src = "images/notification.png";
                                }
                            }
                        }, function () { }, auth);
                    }
                }, function () { }, notificationIds, auth);
            };

            document.getElementById("btn-mark-as-unread").onclick = function () {
                window.Common.post(uri + "/mark-notifications-as-unread", function (res) {
                    if (res === true) {
                        window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
                            for (let i = 0; i < notificationIds.length; i++) {
                                let notificationId = notificationIds[i];
                                for (let j = 0; j < rows.length; j++) {
                                    let row = rows[j];
                                    let id = row.getElementsByClassName("id")[0].innerHTML;
                                    if (notificationId === id) {
                                        if (row.getElementsByClassName("assigned-by")[0].classList.contains("bold") === false) {
                                            row.getElementsByClassName("assigned-by")[0].classList.add("bold");
                                        }
                                        if (row.getElementsByClassName("assigned-on")[0].classList.contains("bold") === false) {
                                            row.getElementsByClassName("assigned-on")[0].classList.add("bold");
                                        }
                                        if (row.getElementsByClassName("message")[0].classList.contains("bold") === false) {
                                            row.getElementsByClassName("message")[0].classList.add("bold");
                                        }
                                    }
                                }

                                if (hasNotifications === true) {
                                    imgNotifications.src = "images/notification-active.png";
                                } else {
                                    imgNotifications.src = "images/notification.png";
                                }
                            }
                        }, function () { }, auth);
                    }
                }, function () { }, notificationIds, auth);
            };

            document.getElementById("btn-delete").onclick = function () {
                if (notificationIds.length === 0) {
                    window.Common.toastInfo(language.get("toast-select-notifications"));
                } else {
                    let cres = confirm(notificationIds.length == 1 ? language.get("confirm-delete-notification") : language.get("confirm-delete-notifications"));
                    if (cres === true) {
                        window.Common.post(uri + "/delete-notifications", function (res) {
                            if (res === true) {
                                for (let i = notificationIds.length - 1; i >= 0; i--) {
                                    let notificationId = notificationIds[i];
                                    for (let j = 0; j < rows.length; j++) {
                                        let row = rows[j];
                                        let id = row.getElementsByClassName("id")[0].innerHTML;
                                        if (notificationId === id) {
                                            notificationIds = window.Common.removeItemOnce(notificationIds, notificationId);
                                            row.remove();

                                            // Notify assignedBy
                                            //for (let k = 0; k < notifications.length; k++) {
                                            //    let notification = notifications[k];
                                            //    if (notificationId === notification.Id) {

                                            //        let message = "The user " + username + " has read his notification: " + notification.Message;
                                            //        window.Common.post(uri + "/notify?a=" + encodeURIComponent(notification.AssignedBy) + "&m=" + encodeURIComponent(message), function (notifyRes) {
                                            //            if (notifyRes === true) {
                                            //                window.Common.toastInfo("The assignor was notified that you read the notifictaion assigned on " + notification.AssignedOn + ".");
                                            //            } else {
                                            //                window.Common.toastError("An error occured while notifying the assignor.");
                                            //            }
                                            //        }, function () { }, "", auth);

                                            //        break;
                                            //    }
                                            //}

                                        }
                                    }
                                }
                            }
                        }, function () { }, notificationIds, auth);
                    }
                }
            };

            document.getElementById("check-all").onchange = function () {
                for (let i = 0; i < rows.length; i++) {
                    let row = rows[i];
                    let checkBox = row.getElementsByClassName("check")[0].firstChild;
                    let notificationId = row.getElementsByClassName("id")[0].innerHTML;

                    if (checkBox.checked === true) {
                        checkBox.checked = false;
                        notificationIds = window.Common.removeItemOnce(notificationIds, notificationId);
                    } else {
                        checkBox.checked = true;
                        notificationIds.push(notificationId);
                    }
                }
            };

        }, function () { }, auth);
    }
};