// ChampUI framework JavaScript utilities
// Loaded automatically as a Razor Class Library static web asset.
window.ChampUI = window.ChampUI || {};

/**
 * Triggers a file download from a base64-encoded byte array.
 * Called by UiDataGrid when exporting CSV or XLSX.
 */
window.ChampUI.downloadFile = function (filename, mimeType, base64) {
    const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 2000);
};

/**
 * Traps focus within a dialog element (used by UiDialog).
 */
window.ChampUI.trapFocus = function (element) {
    if (!element) return;
    const focusable = element.querySelectorAll(
        'a[href],button:not([disabled]),textarea,input,select,[tabindex]:not([tabindex="-1"])'
    );
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const handler = function (e) {
        if (e.key !== 'Tab') return;
        if (e.shiftKey) {
            if (document.activeElement === first) { last.focus(); e.preventDefault(); }
        } else {
            if (document.activeElement === last) { first.focus(); e.preventDefault(); }
        }
    };
    element.addEventListener('keydown', handler);
    if (first) first.focus();
    return handler; // caller can remove it
};

/**
 * Closes any open dropdown panels when clicking outside them.
 */
window.ChampUI.registerOutsideClick = function (element, dotnetRef, methodName) {
    const handler = function (e) {
        if (!element.contains(e.target)) {
            dotnetRef.invokeMethodAsync(methodName);
        }
    };
    document.addEventListener('click', handler, { capture: true });
    return handler;
};
