window.History = function () {
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

        document.getElementById("lbl-show").innerHTML = language.get("lbl-show");
        document.getElementById("lbl-entries").innerHTML = language.get("lbl-entries");
        document.getElementById("spn-entries-count-label").innerHTML = language.get("spn-entries-count-label");
        document.getElementById("lbl-from").innerHTML = language.get("lbl-from");
        document.getElementById("lbl-to").innerHTML = language.get("lbl-to");
        document.getElementById("btn-search").value = language.get("btn-search");

        let statusPendingLabels = document.getElementsByClassName("st-pending");
        for (let i = 0; i < statusPendingLabels.length; i++) {
            statusPendingLabels[i].innerHTML = language.get("status-pending-label");
        }
        let statusRunningLabels = document.getElementsByClassName("st-running");
        for (let i = 0; i < statusRunningLabels.length; i++) {
            statusRunningLabels[i].innerHTML = language.get("status-running-label");
        }
        let statusDoneLabels = document.getElementsByClassName("st-done");
        for (let i = 0; i < statusDoneLabels.length; i++) {
            statusDoneLabels[i].innerHTML = language.get("status-done-label");
        }
        let statusFailedLabels = document.getElementsByClassName("st-failed");
        for (let i = 0; i < statusFailedLabels.length; i++) {
            statusFailedLabels[i].innerHTML = language.get("status-failed-label");
        }
        let statusWarningLabels = document.getElementsByClassName("st-warning");
        for (let i = 0; i < statusWarningLabels.length; i++) {
            statusWarningLabels[i].innerHTML = language.get("status-warning-label");
        }
        let statusStoppedLabels = document.getElementsByClassName("st-stopped");
        for (let i = 0; i < statusStoppedLabels.length; i++) {
            statusStoppedLabels[i].innerHTML = language.get("status-stopped-label");
        }
        let statusRejectedLabels = document.getElementsByClassName("st-rejected");
        for (let i = 0; i < statusRejectedLabels.length; i++) {
            statusRejectedLabels[i].innerHTML = language.get("status-disapproved-label");
        }
    };

    let language = new window.Language("lang", updateLanguage);
    language.init();

    let uri = window.Common.trimEnd(window.Settings.Uri, "/");
    let divEntries = document.getElementById("entries");
    let divEntriesAction = document.getElementById("entries-action");
    let btnLogout = document.getElementById("btn-logout");
    let btnSearch = document.getElementById("btn-search");
    let txtSearch = document.getElementById("txt-search");
    let slctEntriesCount = document.getElementById("slct-entries-count");
    let btnNextPage = document.getElementById("btn-next-page");
    let btnPreviousPage = document.getElementById("btn-previous-page");
    let lblPages = document.getElementById("lbl-pages");
    let lblEntriesCount = document.getElementById("spn-entries-count");
    let txtFrom = document.getElementById("txt-from");
    let txtTo = document.getElementById("txt-to");
    let lnkRecords = document.getElementById("lnk-records");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
    let lnkNotifications = document.getElementById("lnk-notifications");
    let imgNotifications = document.getElementById("img-notifications");

    let page = 1;
    let numberOfPages = 0;
    let heo = 1;
    let suser = getUser();
    let from = null;
    let to = null;
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
                } else {
                    window.Common.get(uri + "/has-notifications?a=" + encodeURIComponent(user.Username), function (hasNotifications) {
                        if (u.UserProfile === 0 || u.UserProfile === 1) {
                            lnkRecords.style.display = "inline";
                            lnkManager.style.display = "inline";
                            lnkDesigner.style.display = "inline";
                            lnkApproval.style.display = "inline";
                            lnkUsers.style.display = "inline";
                            lnkNotifications.style.display = "inline";
                        }

                        if (u.UserProfile === 0) {
                            lnkProfiles.style.display = "inline";
                        }

                        if (hasNotifications === true) {
                            imgNotifications.src = "images/notification-active.png";
                        } else {
                            imgNotifications.src = "images/notification.png";
                        }

                        divEntries.style.display = "block";
                        divEntriesAction.style.display = "block";

                        btnLogout.onclick = function () {
                            window.deleteUser();
                            window.Common.redirectToLoginPage();
                        };

                        document.getElementById("spn-username").innerHTML = " (" + u.Username + ")";

                        window.Common.get(uri + "/history-entry-status-date-min",
                            function (dateMin) {
                                window.Common.get(uri + "/history-entry-status-date-max",
                                    function (dateMax) {

                                        from = new Date(dateMin);
                                        to = new Date(dateMax);

                                        from = new Date(from.getFullYear(), from.getMonth(), from.getDate(), 0, 0, 0);
                                        to.setDate(to.getDate() + 1);

                                        window.Common.get(uri + "/history-entries-count-by-date?s=" + encodeURIComponent(txtSearch.value) + "&from=" + from.getTime() + "&to=" + to.getTime(),
                                            function (count) {

                                                $(txtFrom).datepicker({
                                                    changeMonth: true,
                                                    changeYear: true,
                                                    dateFormat: "dd-mm-yy",
                                                    onSelect: function () {
                                                        from = $(this).datepicker("getDate");
                                                    }
                                                });

                                                $(txtFrom).datepicker("setDate", from);

                                                $(txtTo).datepicker({
                                                    changeMonth: true,
                                                    changeYear: true,
                                                    dateFormat: "dd-mm-yy",
                                                    onSelect: function () {
                                                        to = $(this).datepicker("getDate");
                                                    }
                                                });

                                                $(txtTo).datepicker("setDate", to);

                                                updatePagerControls(count);

                                                btnNextPage.onclick = function () {
                                                    page++;
                                                    if (page > 1) {
                                                        window.Common.disableButton(btnPreviousPage, false);
                                                    }

                                                    if (page >= numberOfPages) {
                                                        window.Common.disableButton(btnNextPage, true);
                                                    } else {
                                                        window.Common.disableButton(btnNextPage, false);
                                                    }

                                                    lblPages.innerHTML = page + " / " + numberOfPages;
                                                    loadEntries();
                                                };

                                                window.Common.disableButton(btnPreviousPage, true);

                                                btnPreviousPage.onclick = function () {
                                                    page--;
                                                    if (page === 1) {
                                                        window.Common.disableButton(btnPreviousPage, true);
                                                    }

                                                    if (page < numberOfPages) {
                                                        window.Common.disableButton(btnNextPage, false);
                                                    }

                                                    lblPages.innerHTML = page + " / " + numberOfPages;
                                                    loadEntries();
                                                };

                                                btnSearch.onclick = function () {
                                                    page = 1;
                                                    updatePager();
                                                    loadEntries();
                                                };

                                                txtSearch.onkeyup = function (e) {
                                                    e.preventDefault();

                                                    if (e.keyCode === 13) {
                                                        page = 1;
                                                        updatePager();
                                                        loadEntries();
                                                    }
                                                };

                                                slctEntriesCount.onchange = function () {
                                                    page = 1;
                                                    updatePagerControls(count);
                                                    loadEntries();
                                                };

                                                loadEntries();

                                            },
                                            function () { }, auth);

                                    },
                                    function () { }, auth);


                            },
                            function () { }, auth);

                    }, function () { }, auth);
                }
            }, function () {
                window.logout();
            }, auth);
    }

    function updatePager() {

        window.Common.get(uri + "/history-entries-count-by-date?s=" + encodeURIComponent(txtSearch.value) + "&from=" + from.getTime() + "&to=" + to.getTime(),
            function (count) {
                updatePagerControls(count);
            },
            function () { }, auth);
    }

    function updatePagerControls(count) {
        lblEntriesCount.innerHTML = count;

        numberOfPages = count / getEntriesCount();
        let numberOfPagesInt = parseInt(numberOfPages);
        if (numberOfPages > numberOfPagesInt) {
            numberOfPages = numberOfPagesInt + 1;
        } else if (numberOfPagesInt === 0) {
            numberOfPages = 1;
        } else {
            numberOfPages = numberOfPagesInt;
        }

        lblPages.innerHTML = page + " / " + numberOfPages;

        if (page >= numberOfPages) {
            window.Common.disableButton(btnNextPage, true);
        } else {
            window.Common.disableButton(btnNextPage, false);
        }

        if (page === 1) {
            window.Common.disableButton(btnPreviousPage, true);
        }
    }

    function getEntriesCount() {
        if (slctEntriesCount.selectedIndex === -1) {
            return 10;
        }

        return slctEntriesCount.options[slctEntriesCount.selectedIndex].value;
    }

    function loadEntries() {
        let entriesCount = getEntriesCount();

        window.Common.get(uri + "/search-history-entries-by-page-order-by?s=" + encodeURIComponent(txtSearch.value) + "&from=" + from.getTime() + "&to=" + to.getTime() + "&page=" + page + "&entriesCount=" + entriesCount + "&heo=" + heo,
            function (data) {
                let items = [];

                for (let i = 0; i < data.length; i++) {
                    let val = data[i];
                    let lt = window.Common.launchType(val.LaunchType);
                    let entryStatus = window.Common.status(language, val.Status);
                    items.push("<tr>"
                        + "<input type='hidden' class='entryId' value='" + val.Id + "'>"
                        + "<td class='status'>" + entryStatus + "</td>"
                        + "<td class='date'>" + val.StatusDate + "</td>"
                        + "<td class='id' title='" + val.WorkflowId + "'>" + val.WorkflowId + "</td>"
                        + "<td class='name'>" + val.Name + "</td>"
                        + "<td class='lt'>" + lt + "</td>"
                        + "<td class='desc' title='" + val.Description + "'>" + val.Description + "</td>"
                        + "</tr>");
                }

                let table = "<table id='entries-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th id='th-status' class='status'>Status</th>"
                    + "<th id='th-date' class='date'>Date 🔻</th>"
                    + "<th id='th-id' class='id'>Id</th>"
                    + "<th id='th-name' class='name'>Name</th>"
                    + "<th id='th-lt' class='lt'>LaunchType</th>"
                    + "<th id='th-desc' class='desc'>Description</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                document.getElementById("entries").innerHTML = table;

                let entriesTable = document.getElementById("entries-table");

                entriesTable.getElementsByTagName("tbody")[0].style.height = (document.getElementById("entries").offsetHeight - 35) + "px";

                let rows = entriesTable.getElementsByTagName("tbody")[0].getElementsByTagName("tr");
                if (rows.length > 0) {
                    let hrow = entriesTable.getElementsByTagName("thead")[0].getElementsByTagName("tr")[0];
                    hrow.querySelector(".status").style.width = rows[0].querySelector(".status").offsetWidth + "px";
                    hrow.querySelector(".date").style.width = rows[0].querySelector(".date").offsetWidth + "px";
                    hrow.querySelector(".id").style.width = rows[0].querySelector(".id").offsetWidth + "px";
                    hrow.querySelector(".name").style.width = rows[0].querySelector(".name").offsetWidth + "px";
                    hrow.querySelector(".lt").style.width = rows[0].querySelector(".lt").offsetWidth + "px";
                    hrow.querySelector(".desc").style.width = rows[0].querySelector(".desc").offsetWidth + "px";
                }

                let descriptions = entriesTable.querySelectorAll(".desc");
                for (let i = 0; i < descriptions.length; i++) {
                    descriptions[i].style.width = entriesTable.offsetWidth - 600 + "px";
                }

                for (let i = 0; i < rows.length; i++) {
                    rows[i].onclick = function () {
                        //let selected = document.getElementsByClassName("selected");
                        //if (selected.length > 0) {
                        //    selected[0].className = selected[0].className.replace("selected", "");
                        //}
                        //this.className += "selected";

                        let entryId = this.getElementsByClassName("entryId")[0].value;

                        window.Common.get(uri + "/history-entry-logs?id=" + entryId, function (logs) {
                            let grabMe = document.getElementById("grabMe");
                            grabMe.innerHTML = window.Common.escape(logs).replace(/\r\n/g, "<br>");

                            new jBox('Modal', {
                                width: 800,
                                height: 420,
                                title: 'Logs',
                                content: $('#grabMe'),
                            }).open();

                        }, function () {
                            window.Common.toastError("An error occured while retrieving logs.");
                        }, auth);
                    };
                }

                let thDate = document.getElementById("th-date");
                let thId = document.getElementById("th-id");
                let thName = document.getElementById("th-name");
                let thLt = document.getElementById("th-lt");
                let thDesc = document.getElementById("th-desc");
                let thStatus = document.getElementById("th-status");

                if (heo === 0) { // By Date ascending
                    thDate.innerHTML = "Date&nbsp;&nbsp;🔺";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 1) { // By Date descending
                    thDate.innerHTML = "Date&nbsp;&nbsp;🔻";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 2) { // By Id ascending
                    thId.innerHTML = "Id&nbsp;&nbsp;🔺";
                    thDate.innerHTML = "Date";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                } else if (heo === 3) { // By Id descending
                    thId.innerHTML = "Id&nbsp;&nbsp;🔻";
                    thDate.innerHTML = "Date";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 4) { // By Name ascending
                    thName.innerHTML = "Name&nbsp;&nbsp;🔺";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 5) { // By Name descending
                    thName.innerHTML = "Name&nbsp;&nbsp;🔻";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 6) { // By LaunchType ascending
                    thLt.innerHTML = "LaunchType&nbsp;&nbsp;🔺";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 7) { // By LaunchType descending
                    thLt.innerHTML = "LaunchType&nbsp;&nbsp;🔻";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thDesc.innerHTML = "Description";
                    thStatus.innerHTML = "Status";
                } else if (heo === 8) { // By Description ascending
                    thDesc.innerHTML = "Description&nbsp;&nbsp;🔺";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thStatus.innerHTML = "Status";
                } else if (heo === 9) { // By Description descending
                    thDesc.innerHTML = "Description&nbsp;&nbsp;🔻";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thStatus.innerHTML = "Status";
                } else if (heo === 10) { // By Status ascending
                    thStatus.innerHTML = "Status&nbsp;&nbsp;🔺";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                } else if (heo === 11) { // By Status descending
                    thStatus.innerHTML = "Status&nbsp;&nbsp;🔻";
                    thDate.innerHTML = "Date";
                    thId.innerHTML = "Id";
                    thName.innerHTML = "Name";
                    thLt.innerHTML = "LaunchType";
                    thDesc.innerHTML = "Description";
                }

                thDate.onclick = function () {
                    if (heo === 1) {
                        heo = 0;
                        loadEntries();
                    } else {
                        heo = 1;
                        loadEntries();
                    }
                };

                thId.onclick = function () {
                    if (heo === 2) {
                        heo = 3;
                        loadEntries();
                    } else {
                        heo = 2;
                        loadEntries();
                    }
                };

                thName.onclick = function () {
                    if (heo === 4) {
                        heo = 5;
                        loadEntries();
                    } else {
                        heo = 4;
                        loadEntries();
                    }
                };

                thLt.onclick = function () {
                    if (heo === 6) {
                        heo = 7;
                        loadEntries();
                    } else {
                        heo = 6;
                        loadEntries();
                    }
                };

                thDesc.onclick = function () {
                    if (heo === 8) {
                        heo = 9;
                        loadEntries();
                    } else {
                        heo = 8;
                        loadEntries();
                    }
                };

                thStatus.onclick = function () {
                    if (heo === 10) {
                        heo = 11;
                        loadEntries();
                    } else {
                        heo = 10;
                        loadEntries();
                    }
                };

            }, function () {
                //alert("An error occured while retrieving entries. Check that Wexflow Windows Service is running correctly.");
            }, auth);
    }

}