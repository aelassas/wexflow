window.Language = function (domId, updateLanguage) {
    "use strict";

    let self = this;

    this.get = function (keyword) {
        return window.languageModule.languages[self.getLanguage()][keyword] || window.languageModule.languages["en"][keyword];
    };

    this.setLanguage = function (code) {
        setValue("wf-language", code);
    };

    this.getLanguage = function () {
        let code = getValue("wf-language");
        if (!code) {
            return "en";
        }
        return code;
    };

    /* Main */
    this.init = function () {
        let getSpaces = function () {
            let languageName = self.get("language");
            let spacesToAdd = 8 - languageName.length;
            let spaces = "";
            if (spacesToAdd > 0) {
                for (let j = 0; j < spacesToAdd; j++) {
                    spaces += "&nbsp;&nbsp;";
                }
            }
            return spaces;
        };
        let html = '<button class="btn btn-default btn-xs dropdown-toggle" type="button" data-toggle="dropdown">'
            + '<span id="lang-label">' + self.get("language") + getSpaces() + '</span> <span class="caret"></span>'
            + '</button>'
            + '<ul class="dropdown-menu" role="menu">';

        for (let i = 0; i < window.languageModule.codes.length; i++) { // 8
            html += '<li><div class="lang"><img src="' + window.languageModule.codes[i].Icon + '" alt="">&nbsp;' + window.languageModule.codes[i].Name + '<input type="hidden" class="lang-code" value="' + window.languageModule.codes[i].Code + '" /></div></li>';
        }
        html += ' </ul>';

        let langDomElt = document.getElementById(domId);
        langDomElt.classList.add("btn-group");
        langDomElt.innerHTML = html;

        let langs = document.getElementsByClassName("lang");
        for (let i = 0; i < langs.length; i++) {
            langs[i].onclick = function () {
                let code = this.querySelector(".lang-code").value;
                self.setLanguage(code);
                updateLanguage(self);
                document.getElementById("lang-label").innerHTML = self.get("language") + getSpaces();
            }
        }
        updateLanguage(self);
    }

    /* Internal functions */
    function setValue(key, value) {
        if (isIE() || isFirefox()) {
            setCookie(key, value, 365);
        } else {
            window.localStorage.setItem(key, value);
        }
    }

    function getValue(key) {
        if (isIE() || isFirefox()) {
            return getCookie(key);
        } else {
            return window.localStorage.getItem(key);
        }
    }

    function setCookie(cname, cvalue, exdays) {
        let d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        let expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
    }

    function getCookie(cname) {
        let name = cname + "=";
        let ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) === 0) {
                return c.substring(name.length, c.length);
            }
        }
        return "";
    }

    function isIE() {
        let ua = navigator.userAgent;
        /* MSIE used to detect old browsers and Trident used to newer ones*/
        let is_ie = ua.indexOf("MSIE ") > -1 || ua.indexOf("Trident/") > -1;

        return is_ie;
    }

    function isFirefox() {
        let ua = navigator.userAgent;
        let is_firefox = ua.toLowerCase().indexOf("firefox") > -1;

        return is_firefox;
    }
};