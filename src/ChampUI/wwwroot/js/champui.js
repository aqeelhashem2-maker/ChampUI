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
 * Legacy helper for callers that manage their own focus trap.
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

// The native top layer makes the background inert, including when dialogs are nested.
// These handlers add cyclic Tab navigation and bridge dismissals to the Blazor state.
(() => {
    const sessions = new WeakMap();
    const stack = [];

    function isUsable(element) {
        return element?.isConnected && !element.matches(':disabled') && !element.closest('[inert]') &&
            element.getClientRects().length > 0 && getComputedStyle(element).visibility === 'visible';
    }

    function focus(element) {
        if (!isUsable(element)) return false;
        element.focus({ preventScroll: true });
        return document.activeElement === element;
    }

    function restoreFocus(dialog, opener, region) {
        const active = document.activeElement;
        if (isUsable(active) && active !== document.body && active !== document.documentElement &&
            !dialog.contains(active)) return;

        const top = stack.findLast(candidate => candidate.isConnected && candidate.open);
        if (top) {
            if (!(top.contains(opener) && focus(opener))) (tabStops(top)[0] ?? top).focus({ preventScroll: true });
            return;
        }
        if (document.querySelector('dialog:modal') || focus(opener)) return;

        // If the opener was deleted or disabled, focus the surviving workflow heading.
        const scope = region?.isConnected ? region : document.querySelector('main, [role="main"]');
        const fallback = scope?.querySelector('h1, h2') ?? scope ?? document.querySelector('h1');
        if (!isUsable(fallback)) return;
        const original = fallback.getAttribute('tabindex');
        if (original === null) fallback.setAttribute('tabindex', '-1');
        focus(fallback);
        if (original === null) fallback.removeAttribute('tabindex');
    }

    function tabStops(dialog) {
        const selector = 'a[href],area[href],button,input,select,textarea,summary,iframe,[tabindex],[contenteditable=true]';
        const elements = Array.from(dialog.querySelectorAll(selector)).filter(element =>
            element.closest('dialog') === dialog && isUsable(element) &&
            (element.tabIndex >= 0 || (element.isContentEditable && !element.hasAttribute('tabindex'))));
        return elements.filter(element => {
            if (!element.matches('input[type=radio]') || !element.name) return true;
            const group = elements.filter(radio =>
                radio.matches('input[type=radio]') && radio.name === element.name && radio.form === element.form);
            return element === (group.find(radio => radio.checked) ?? group[0]);
        }).sort((left, right) =>
            (left.tabIndex || Number.MAX_VALUE) - (right.tabIndex || Number.MAX_VALUE));
    }

    function overBackdrop(dialog, event) {
        if (event.target !== dialog) return false;
        const { left, right, top, bottom } = dialog.getBoundingClientRect();
        return event.clientX < left || event.clientX >= right || event.clientY < top || event.clientY >= bottom;
    }

    window.ChampUI.openDialog = function (dialog) {
        if (!dialog?.isConnected || dialog.open) return;
        sessions.get(dialog)?.();
        const opener = document.activeElement;
        const region = opener?.closest('main, [role="main"]') ?? opener?.parentElement;
        dialog.showModal();
        stack.push(dialog);

        const events = new AbortController();
        const options = { signal: events.signal };
        const removal = new MutationObserver(() => {
            if (!dialog.isConnected) {
                finish();
            } else if (stack.at(-1) === dialog && dialog.open &&
                       (!dialog.contains(document.activeElement) || !isUsable(document.activeElement)) &&
                       ![...document.querySelectorAll('dialog:modal')].some(other =>
                           other !== dialog && other.contains(document.activeElement))) {
                (tabStops(dialog)[0] ?? dialog).focus({ preventScroll: true });
            }
        });
        function finish() {
            events.abort();
            removal.disconnect();
            sessions.delete(dialog);
            stack.splice(stack.indexOf(dialog), 1);
            if (dialog.isConnected && dialog.open) dialog.close();
            restoreFocus(dialog, opener, region);
        }
        sessions.set(dialog, finish);
        removal.observe(document.documentElement, {
            childList: true,
            subtree: true,
            attributeFilter: ['disabled', 'hidden', 'inert', 'tabindex', 'style', 'class']
        });

        const requestClose = () => dialog.querySelector(':scope > .ui-dialog__header [data-ui-dialog-close]')?.click();
        // Escape reaches Blazor via keydown. Keep the native dialog open until Blazor accepts the close.
        dialog.addEventListener('cancel', event => event.preventDefault(), options);
        // A native form with method="dialog" can close independently of a Blazor event.
        dialog.addEventListener('close', () => {
            if (dialog.open) return;
            finish();
            requestClose();
        }, options);

        let backdropDown = false;
        dialog.addEventListener('pointerdown', event => {
            backdropDown = overBackdrop(dialog, event);
        }, options);
        dialog.addEventListener('click', event => {
            if (backdropDown && overBackdrop(dialog, event)) requestClose();
            backdropDown = false;
        }, options);

        dialog.addEventListener('keydown', event => {
            if (event.key !== 'Tab' || event.target.closest('dialog') !== dialog) return;
            const stops = tabStops(dialog);
            const first = stops[0];
            const last = stops.at(-1);
            const active = document.activeElement;
            if (!first || !stops.includes(active) || active === (event.shiftKey ? first : last)) {
                event.preventDefault();
                (event.shiftKey ? last ?? dialog : first ?? dialog).focus();
            }
        }, options);
    };

    window.ChampUI.closeDialog = function (dialog) {
        sessions.get(dialog)?.();
    };
})();
