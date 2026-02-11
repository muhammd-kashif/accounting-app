(function () {
    var storageKey = "accounting-theme";

    function applyTheme(mode) {
        var isDark = mode === "dark";
        document.body.classList.toggle("theme-dark", isDark);
        document.body.classList.toggle("theme-light", !isDark);
    }

    function getTheme() {
        return document.body.classList.contains("theme-dark") ? "dark" : "light";
    }

    function initTheme() {
        var saved = localStorage.getItem(storageKey);
        var prefersDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
        applyTheme(saved || (prefersDark ? "dark" : "light"));
    }

    window.getTheme = function () {
        return getTheme();
    };

    window.setTheme = function (mode) {
        localStorage.setItem(storageKey, mode);
        applyTheme(mode);
        return getTheme();
    };

    window.toggleTheme = function () {
        var next = getTheme() === "dark" ? "light" : "dark";
        localStorage.setItem(storageKey, next);
        applyTheme(next);
        return next;
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initTheme);
    } else {
        initTheme();
    }
})();
