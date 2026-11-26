const assert = require('node:assert/strict');
const path = require('node:path');
const { after, before, test } = require('node:test');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');

const [origin, evidence] = process.argv.slice(2);
let browser;

before(async () => { browser = await chromium.launch(); });
after(async () => { await browser?.close(); });

async function run(name, viewport, check) {
    const context = await browser.newContext({ viewport });
    await context.tracing.start({ screenshots: true, snapshots: true });
    const page = await context.newPage();
    page.setDefaultTimeout(10000);
    const errors = [];
    const assets = new Map();
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => {
        if (message.type() === 'error') errors.push(message.text());
    });
    page.on('requestfailed', request => {
        if (request.failure()?.errorText !== 'net::ERR_ABORTED') errors.push(request.url());
    });
    page.on('response', response => {
        const asset = new URL(response.url()).pathname;
        assets.set(asset, response);
        if (response.status() >= 400) errors.push(`${response.status()} ${asset}`);
    });

    try {
        await page.goto(origin);
        await page.getByRole('heading', { name: 'Dashboard', exact: true }).waitFor();
        await page.waitForFunction(() => Array.from(document.querySelectorAll('.ui-topbar button'))
            .some(button => button.textContent.includes('Dark mode') && !button.disabled));
        await check(page, assets);
        assert.deepEqual(errors, []);
    } catch (error) {
        await page.screenshot({ path: path.join(evidence, `${name}-failure.png`), fullPage: true });
        throw error;
    } finally {
        await context.tracing.stop({ path: path.join(evidence, `${name}.zip`) });
        await context.close();
    }
}

const desktop = { width: 1440, height: 1080 };
const nav = page => page.locator('.ui-sidebar__nav');
const heading = (page, name) => page.getByRole('heading', { name, exact: true });
const rows = page => page.locator('.ui-grid tbody tr').filter({ has: page.locator('td[data-label]') });

async function open(page, name) {
    await nav(page).getByRole('link', { name, exact: true }).click();
    await heading(page, name).waitFor();
}

async function expectRows(page, ids) {
    await page.waitForFunction(expected => {
        const rows = Array.from(document.querySelectorAll('.ui-grid tbody tr'))
            .filter(row => row.querySelector('td[data-label]'));
        return JSON.stringify(rows.map(row => row.querySelector('td')?.textContent.trim()))
            === JSON.stringify(expected);
    }, ids);
}

test('dashboard renders metrics, activity, recent orders and executes mapped assets', async () => {
    await run('dashboard', desktop, async (page, assets) => {
        const html = await page.locator('html').evaluate(element => ({ dir: element.dir, lang: element.lang }));
        assert.deepEqual(html, { dir: 'ltr', lang: 'en' });
        assert.equal(await page.locator('.dashboard-shell').getAttribute('data-theme'), 'light');
        assert.equal(await page.title(), 'Dashboard · ChampUI Dashboard Starter');
        await page.getByRole('link', { name: 'ChampUI Dashboard Starter' }).waitFor();
        const favicon = await page.locator('link[rel=icon]').getAttribute('href');
        assert.match(favicon, /^dashboard-icon(?:\.[a-z0-9]+)?\.svg$/);
        assert.ok((await page.request.get(new URL(favicon, origin).href)).ok());
        assert.deepEqual(await page.locator('.metric-value').allTextContents(), ['$48,250', '286', '476', '+14.1%']);
        assert.equal(await page.locator('.activity-list li').count(), 4);
        assert.equal(await rows(page).count(), 4);
        assert.ok(await page.getByText('Fictional sample data').isVisible());
        const chart = page.getByRole('img', { name: 'Monthly revenue chart' });
        assert.ok(await chart.isVisible());
        assert.match(await chart.locator('path.ui-chart__line').getAttribute('d'), /^M/);
        const urls = await page.evaluate(() => ({
            styles: Array.from(document.querySelectorAll('link[rel="stylesheet"]'), link => new URL(link.href).pathname),
            scripts: Array.from(document.querySelectorAll('script[src]'), script => new URL(script.src).pathname),
            grid: getComputedStyle(document.querySelector('.dashboard-metrics')).display,
            shell: getComputedStyle(document.querySelector('.dashboard-shell')).display,
            accent: getComputedStyle(document.querySelector('.dashboard-shell')).getPropertyValue('--ui-accent').trim(),
            champuiReady: typeof window.ChampUI?.downloadFile,
            appReady: typeof window.dashboard?.setLanguage,
            serverReady: typeof window.Blazor?.reconnect
        }));
        assert.deepEqual(urls.styles.map(url => url.replace(/\.[a-z0-9]+(?=\.css$)/, '')),
            ['/_content/ChampUI/champui.css', '/dashboard.css']);
        assert.deepEqual(urls.scripts.map(url => url.replace(/\.[a-z0-9]{8,}(?=\.js$)/, '')),
            ['/_content/ChampUI/js/champui.js', '/dashboard.js', '/_framework/blazor.web.js']);
        for (const url of [...urls.styles, ...urls.scripts]) {
            const response = assets.get(url);
            assert.ok(response?.ok(), `failed asset: ${url}`);
            assert.ok((await response.body()).length > 100, `empty asset: ${url}`);
        }
        assert.deepEqual([urls.grid, urls.shell, urls.accent, urls.champuiReady, urls.appReady, urls.serverReady],
            ['grid', 'grid', '#4f46e5', 'function', 'function', 'function']);
        assert.equal(await page.getByRole('button', { name: 'Toggle navigation' }).count(), 0);
        await page.screenshot({ path: path.join(evidence, 'dashboard-desktop.png'), fullPage: true });
        await page.getByRole('link', { name: 'View orders' }).click();
        await heading(page, 'Orders').waitFor();
        await open(page, 'Dashboard');
    });
});

test('customers grid searches, shows an empty state, sorts, filters and pages', async () => {
    await run('customers', desktop, async page => {
        await open(page, 'Customers');
        await expectRows(page, ['Maya Chen', 'Omar Nasser', 'Sofia Martin', 'Daniel Brooks', 'Leila Haddad', 'Noah Williams']);
        const search = page.getByRole('searchbox', { name: 'Search customers…' });
        await search.fill('Opal');
        await expectRows(page, ['Noura Ali']);
        assert.ok(await rows(page).getByText('Onboarding').isVisible());
        await search.fill('nonexistent-customer');
        await page.getByText('No customers found').waitFor();
        await search.fill('');
        await expectRows(page, ['Maya Chen', 'Omar Nasser', 'Sofia Martin', 'Daniel Brooks', 'Leila Haddad', 'Noah Williams']);
        await page.getByRole('button', { name: 'Name', exact: true }).click();
        await expectRows(page, ['Adam Reed', 'Amira Saleh', 'Daniel Brooks', 'Elena Rossi', 'Ivy Tran', 'Leila Haddad']);
        await page.getByRole('button', { name: 'Next page' }).click();
        await expectRows(page, ['Maya Chen', 'Noah Williams', 'Noura Ali', 'Omar Nasser', 'Samir Patel', 'Sofia Martin']);
        await page.getByRole('button', { name: 'Previous page' }).click();
        await page.getByRole('button', { name: 'Filter Company' }).click();
        await page.getByPlaceholder('Filter…').fill('Cedar');
        await expectRows(page, ['Amira Saleh']);
        await page.getByRole('button', { name: 'Clear Company' }).click();
        await expectRows(page, ['Adam Reed', 'Amira Saleh', 'Daniel Brooks', 'Elena Rossi', 'Ivy Tran', 'Leila Haddad']);
        await page.screenshot({ path: path.join(evidence, 'customers-desktop.png'), fullPage: true });
    });
});

test('orders grid filters, sorts, opens details and downloads complete CSV and XLSX', async () => {
    await run('orders', desktop, async page => {
        await open(page, 'Orders');
        await expectRows(page, ['ORD-1048','ORD-1047','ORD-1046','ORD-1045','ORD-1044','ORD-1043']);
        await page.getByRole('button', { name: 'Total', exact: true }).click();
        await expectRows(page, ['ORD-1042','ORD-1045','ORD-1041','ORD-1039','ORD-1043','ORD-1047']);
        await page.getByRole('button', { name: 'Filter Status' }).click();
        await page.getByPlaceholder('Filter…').fill('Processing');
        await expectRows(page, ['ORD-1042','ORD-1047']);
        await page.getByRole('link', { name: 'ORD-1047' }).click();
        await heading(page, 'Order details').waitFor();
        assert.ok(await page.getByText('Equipment order').isVisible());
        assert.ok(await page.getByText('$875').isVisible());
        await page.getByRole('link', { name: 'Back to orders' }).click();
        await heading(page, 'Orders').waitFor();
        for (const [label, extension] of [['CSV', 'csv'], ['Excel', 'xlsx']]) {
            await page.getByRole('button', { name: 'Export', exact: true }).click();
            const downloadEvent = page.waitForEvent('download');
            await page.getByText(`${label} — all rows`, { exact: true }).click();
            const download = await downloadEvent;
            assert.equal(await download.failure(), null);
            assert.equal(path.extname(download.suggestedFilename()), `.${extension}`);
            await download.saveAs(path.join(evidence, `orders.${extension}`));
        }
    });
});

test('analytics renders three distinct charts and comparison figures', async () => {
    await run('analytics', desktop, async page => {
        await open(page, 'Analytics');
        assert.deepEqual(await page.locator('.metric-value').allTextContents(), ['$229,750', '$203,400', '+13.0%']);
        assert.equal(await page.getByRole('img', { name: 'Revenue comparison chart' }).locator('rect.ui-chart__bar').count(), 12);
        assert.equal(await page.getByRole('img', { name: 'Monthly order volume chart' }).locator('path.ui-chart__line').count(), 1);
        assert.ok(await page.getByRole('img', { name: 'Order breakdown chart' }).isVisible());
        await page.screenshot({ path: path.join(evidence, 'analytics-desktop.png'), fullPage: true });
    });
});

test('settings validates and saves in-session preferences; Arabic localizes all pages', async () => {
    await run('settings-rtl', desktop, async page => {
        await open(page, 'Settings');
        const name = page.getByRole('textbox', { name: 'Workspace name' });
        await name.fill('   ');
        await page.getByRole('button', { name: 'Save preferences' }).click();
        await page.getByText('Enter a workspace name of 1–60 characters.').waitFor();
        assert.equal(await name.getAttribute('aria-invalid'), 'true');
        await name.fill('Atlas Operations');
        await page.getByLabel('Theme', { exact: true }).selectOption({ label: 'Dark' });
        await page.getByLabel('Language', { exact: true }).selectOption({ label: 'العربية' });
        await page.getByRole('button', { name: 'Save preferences' }).click();
        await heading(page, 'الإعدادات').waitFor();
        await page.getByText('حُفظت التفضيلات لهذه الجلسة.').waitFor();
        assert.equal(await page.getByRole('textbox', { name: 'اسم مساحة العمل' }).inputValue(), 'Atlas Operations');
        await page.waitForFunction(() => document.documentElement.dir === 'rtl' && document.documentElement.lang === 'ar');
        assert.equal(await page.locator('.dashboard-shell').getAttribute('data-theme'), 'dark');
        assert.equal(await page.locator('.workspace-name').innerText(), 'Atlas Operations');
        const [sidebar, main, start, end] = await Promise.all(
            ['.ui-sidebar', '.ui-shell__main', '.ui-topbar__start', '.ui-topbar__end'].map(selector =>
                page.locator(selector).boundingBox()));
        assert.ok(sidebar.x > main.x && start.x > end.x);
        await page.screenshot({ path: path.join(evidence, 'settings-arabic.png'), fullPage: true });
        for (const label of ['العملاء', 'الطلبات', 'التحليلات', 'لوحة المعلومات']) await open(page, label);
        await page.getByRole('img', { name: 'رسم الإيرادات الشهرية' }).waitFor();
        await page.screenshot({ path: path.join(evidence, 'dashboard-dark-arabic.png'), fullPage: true });
        await page.getByRole('button', { name: 'English' }).click();
        await heading(page, 'Dashboard').waitFor();
        await page.getByRole('button', { name: 'Light mode' }).click();
        await page.waitForFunction(() => document.querySelector('.dashboard-shell').dataset.theme === 'light');
        await open(page, 'Settings');
        assert.equal(await page.getByRole('textbox', { name: 'Workspace name' }).inputValue(), 'Atlas Operations');
        await page.reload();
        assert.equal(await page.getByRole('textbox', { name: 'Workspace name' }).inputValue(), 'Workspace');
    });
});

test('tablet and mobile layout keep navigation usable in both directions', async () => {
    await run('mobile', { width: 900, height: 1024 }, async page => {
        const sidebar = page.locator('.ui-sidebar');
        assert.ok((await sidebar.boundingBox()).width <= 80);
        assert.equal(await page.getByRole('button', { name: 'Toggle navigation' }).count(), 0);
        await nav(page).getByRole('link', { name: 'Analytics' }).click();
        await heading(page, 'Analytics').waitFor();
        await page.setViewportSize({ width: 390, height: 844 });
        await sidebar.waitFor({ state: 'hidden' });
        await page.getByRole('button', { name: 'Toggle navigation' }).click();
        await sidebar.waitFor();
        await page.waitForFunction(() => Math.abs(document.querySelector('.ui-sidebar').getBoundingClientRect().left) < 1);
        await nav(page).getByRole('link', { name: 'Customers' }).click();
        await heading(page, 'Customers').waitFor();
        await sidebar.waitFor({ state: 'hidden' });
        assert.ok(await page.getByRole('searchbox').isVisible());
        await page.getByRole('button', { name: 'العربية' }).click();
        await heading(page, 'العملاء').waitFor();
        await page.getByRole('button', { name: 'فتح أو إغلاق قائمة التنقل' }).click();
        await page.waitForFunction(() => Math.abs(document.querySelector('.ui-sidebar').getBoundingClientRect().right - innerWidth) < 1);
        await nav(page).getByRole('link', { name: 'لوحة المعلومات' }).click();
        await heading(page, 'لوحة المعلومات').waitFor();
        await sidebar.waitFor({ state: 'hidden' });
        assert.ok(await page.getByRole('img', { name: 'رسم الإيرادات الشهرية' }).isVisible());
        assert.equal(await page.locator('.ui-chart__scroll').evaluate(element => element.scrollWidth <= element.clientWidth), true);
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth === innerWidth), true);
        await page.screenshot({ path: path.join(evidence, 'dashboard-mobile-rtl.png'), fullPage: true });
    });
});

async function tabTo(page, target) {
    for (let index = 0; index < 45; index++) {
        if (await target.evaluate(element => element === document.activeElement && element.matches(':focus-visible')))
            return;
        await page.keyboard.press('Tab');
    }
    assert.fail(`Tab did not reach ${target}`);
}

function contrast(first, second) {
    function luminance(color) {
        const channels = color.match(/[\d.]+/g).slice(0, 3).map(Number)
            .map(channel => channel / 255)
            .map(channel => channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4);
        return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
    }

    const values = [luminance(first), luminance(second)].sort((a, b) => a - b);
    return (values[1] + 0.05) / (values[0] + 0.05);
}

async function inspectFocus(target, boundary) {
    const result = await target.evaluate(async (element, boundary) => {
        const visible = boundary === 'composite' ? element.parentElement :
            boundary === 'checkbox' ? element.nextElementSibling : element;
        const style = getComputedStyle(visible);
        // UiCheckbox transitions the outline on its visible proxy.
        void style.outlineWidth;
        await Promise.all(visible.getAnimations()
            .filter(animation => animation.transitionProperty?.startsWith('outline'))
            .map(animation => animation.finished.catch(() => {})));
        const inner = getComputedStyle(element);
        let background = 'rgb(255, 255, 255)';
        for (let outer = visible.parentElement; outer; outer = outer.parentElement) {
            const color = getComputedStyle(outer).backgroundColor;
            if (color !== 'transparent' && color !== 'rgba(0, 0, 0, 0)') {
                background = color;
                break;
            }
        }
        const bounds = visible.getBoundingClientRect();
        return {
            active: document.activeElement === element,
            focusVisible: element.matches(':focus-visible'),
            boundary: visible.className,
            outline: style.outlineStyle,
            thickness: parseFloat(style.outlineWidth),
            color: style.outlineColor,
            background,
            shadow: style.boxShadow,
            innerOutline: inner.outlineStyle,
            width: bounds.width,
            height: bounds.height
        };
    }, boundary);
    assert.equal(result.active, true, JSON.stringify(result));
    assert.equal(result.outline, 'solid', JSON.stringify(result));
    assert.equal(result.thickness, 2);
    assert.ok(result.width > 0 && result.height > 0);
    assert.ok(contrast(result.color, result.background) >= 3, JSON.stringify(result));
    if (boundary === 'composite') {
        assert.match(result.boundary, /\bui-input\b/);
        assert.equal(result.shadow, 'none');
        assert.equal(result.innerOutline, 'none');
    } else {
        assert.equal(result.focusVisible, true);
    }
    return result;
}

test('Orders mouse and Tab focus use one contrast-tested outer boundary', async () => {
    await run('orders-focus', { width: 1440, height: 900 }, async page => {
        await open(page, 'Orders');
        const search = page.getByRole('searchbox');
        await search.click();
        const mouse = await inspectFocus(search, 'composite');
        await page.screenshot({ path: path.join(evidence, 'orders-search-after.png'), clip: { x: 242, y: 74, width: 760, height: 220 } });
        await page.keyboard.press('Tab');
        const columns = page.getByRole('button', { name: 'Columns' });
        await inspectFocus(columns, 'standalone');
        await page.keyboard.press('Space');
        const chooser = page.getByRole('listbox', { name: 'Columns' });
        await chooser.waitFor();
        const column = chooser.getByRole('checkbox', { name: 'Customer' });
        await tabTo(page, column);
        await inspectFocus(column, 'standalone');
        await page.keyboard.press('Space');
        await page.getByRole('columnheader', { name: 'Customer', exact: true }).waitFor({ state: 'hidden' });
        await page.keyboard.press('Space');
        await page.getByRole('columnheader', { name: 'Customer', exact: true }).waitFor();
        await columns.click();
        await page.keyboard.press('Shift+Tab');
        const keyboard = await inspectFocus(search, 'composite');
        assert.equal(keyboard.focusVisible, true);
        // Send committed text in one input event so a Server render cannot interrupt synthetic key bursts.
        await page.keyboard.insertText('ORD-1048');
        await expectRows(page, ['ORD-1048']);
        require('node:fs').writeFileSync(path.join(evidence, 'orders-focus-computed.json'), JSON.stringify({ mouse, keyboard }, null, 2));
    });
});

test('Customers search, sidebar and pagination preserve backwards and forwards keyboard focus', async () => {
    await run('customers-focus', desktop, async page => {
        const customers = nav(page).getByRole('link', { name: 'Customers' });
        await tabTo(page, customers);
        await inspectFocus(customers, 'standalone');
        await page.keyboard.press('Enter');
        await heading(page, 'Customers').waitFor();
        const search = page.getByRole('searchbox');
        await tabTo(page, search);
        await inspectFocus(search, 'composite');
        await search.click();
        await inspectFocus(search, 'composite');
        const next = page.getByRole('button', { name: 'Next page' });
        await tabTo(page, next);
        await inspectFocus(next, 'standalone');
        await page.keyboard.press('Enter');
        await expectRows(page, ['Amira Saleh', 'Elena Rossi', 'Samir Patel', 'Ivy Tran', 'Adam Reed', 'Noura Ali']);
        await page.keyboard.press('Shift+Tab');
        assert.notEqual(await next.evaluate(element => element === document.activeElement), true);
    });
});

test('Settings form has one focus boundary in both themes and in RTL', async () => {
    await run('settings-focus', desktop, async page => {
        await open(page, 'Settings');
        const workspace = page.getByLabel('Workspace name');
        await tabTo(page, workspace);
        await inspectFocus(workspace, 'composite');
        await page.keyboard.press('Tab');
        await inspectFocus(page.getByLabel('Theme', { exact: true }), 'composite');
        await page.keyboard.press('Tab');
        await inspectFocus(page.getByLabel('Language', { exact: true }), 'composite');
        await page.keyboard.press('Shift+Tab');
        await inspectFocus(page.getByLabel('Theme', { exact: true }), 'composite');
        await page.getByRole('button', { name: 'Dark mode' }).click();
        await page.getByRole('button', { name: 'العربية' }).click();
        await heading(page, 'الإعدادات').waitFor();
        await page.waitForFunction(() => document.documentElement.dir === 'rtl' &&
            document.querySelector('.dashboard-shell').dataset.theme === 'dark');
        const name = page.getByLabel('اسم مساحة العمل');
        await tabTo(page, name);
        await inspectFocus(name, 'composite');
        await page.keyboard.press('Tab');
        await inspectFocus(page.getByLabel('السمة', { exact: true }), 'composite');
        await page.screenshot({ path: path.join(evidence, 'settings-focus-dark-rtl.png') });
    });
});

test('Component fixture covers textarea, date, numeric, radio, checkbox, switch, dropdown and dialog', async () => {
    await run('component-focus', desktop, async page => {
        await page.goto(new URL('/focus-audit', origin).href);
        await heading(page, 'Focus audit').waitFor();
        await page.waitForFunction(() => !document.querySelector('button[aria-label="Dark mode"]').disabled);
        for (const rtl of [false, true]) {
            if (rtl) {
                await page.getByRole('button', { name: 'Dark mode' }).click();
                await page.getByRole('button', { name: 'العربية' }).click();
                await page.waitForFunction(() => document.documentElement.dir === 'rtl' &&
                    document.querySelector('.dashboard-shell').dataset.theme === 'dark');
            }
            for (const label of ['Text', 'Notes', 'Category', 'Quantity', 'Date']) {
                const field = page.getByLabel(label, { exact: true });
                await tabTo(page, field);
                await inspectFocus(field, 'composite');
                await field.click();
                await inspectFocus(field, 'composite');
            }
            const standalone = page.getByLabel('Standalone field');
            await tabTo(page, standalone);
            await inspectFocus(standalone, 'standalone');
            const checkbox = page.getByRole('checkbox', { name: 'Confirmed' });
            await tabTo(page, checkbox);
            await inspectFocus(checkbox, 'checkbox');
            const wasChecked = await checkbox.isChecked();
            await page.keyboard.press('Space');
            await page.waitForFunction(expected => document.querySelector('.ui-check input').checked === expected, !wasChecked);
            const radio = page.getByRole('radio', { checked: true });
            await tabTo(page, radio);
            await inspectFocus(radio, 'standalone');
            const toggle = page.getByRole('switch', { name: 'Notifications' });
            await tabTo(page, toggle);
            await inspectFocus(toggle, 'standalone');
            const switchState = await toggle.getAttribute('aria-checked');
            await page.keyboard.press('Space');
            await page.waitForFunction(old => document.querySelector('.ui-switch').getAttribute('aria-checked') !== old, switchState);
            const menu = page.getByRole('button', { name: 'Actions' });
            await tabTo(page, menu);
            await inspectFocus(menu, 'standalone');
            await page.keyboard.press('Enter');
            const item = page.getByRole('menuitem', { name: 'Select action' });
            await item.waitFor();
            await tabTo(page, item);
            await inspectFocus(item, 'standalone');
            await page.keyboard.press('Space');
            await item.waitFor({ state: 'hidden' });
            await page.getByText('Action selected').waitFor();
            const opener = page.getByRole('button', { name: 'Open dialog' });
            await tabTo(page, opener);
            await inspectFocus(opener, 'standalone');
            await page.keyboard.press('Enter');
            const dialog = page.getByRole('dialog', { name: 'Example dialog' });
            await dialog.waitFor();
            const close = dialog.getByRole('button', { name: 'Close' });
            await tabTo(page, close);
            await inspectFocus(close, 'standalone');
            await page.keyboard.press('Tab');
            await inspectFocus(dialog.getByLabel('Dialog name'), 'composite');
            await page.keyboard.press('Escape');
            await dialog.waitFor({ state: 'hidden' });
        }
    });
});

test('Getting Started opens directly and from the sidebar with keyboard, dark mode and mobile RTL', async () => {
    await run('getting-started', desktop, async page => {
        const link = nav(page).getByRole('link', { name: 'Getting Started' });
        await tabTo(page, link);
        await inspectFocus(link, 'standalone');
        await page.keyboard.press('Enter');
        await heading(page, 'Getting Started').waitFor();
        assert.equal(new URL(page.url()).pathname, '/getting-started');
        assert.ok(await page.getByText('Developer guide', { exact: true }).isVisible());
        for (const section of ['Project structure', 'Customize your application', 'Add a page',
            'Replace sample data', 'Connect a REST API', 'Theme and RTL', 'Production checklist']) {
            assert.ok(await heading(page, section).isVisible(), section);
        }
        await page.evaluate(() => window.scrollTo(0, 0));
        await page.screenshot({ path: path.join(evidence, 'getting-started.png'), fullPage: true });
        const details = page.locator('details');
        await tabTo(page, details.locator('summary'));
        await page.keyboard.press('Enter');
        assert.ok(await details.getByText(/response.EnsureSuccessStatusCode/).isVisible());
        const directUrl = new URL('/getting-started', origin).href;
        const direct = await page.request.get(directUrl);
        assert.equal(direct.status(), 200);
        assert.match(await direct.text(), /Project structure/);
        await page.goto(directUrl);
        await heading(page, 'Getting Started').waitFor();
        await page.getByRole('button', { name: 'Dark mode' }).click();
        await page.getByRole('button', { name: 'العربية' }).click();
        await heading(page, 'البدء').waitFor();
        await page.waitForFunction(() => document.documentElement.dir === 'rtl' &&
            document.querySelector('.dashboard-shell').dataset.theme === 'dark');
        await page.setViewportSize({ width: 390, height: 844 });
        await page.getByRole('button', { name: 'فتح أو إغلاق قائمة التنقل' }).click();
        await nav(page).getByRole('link', { name: 'البدء', exact: true }).waitFor();
        await page.keyboard.press('Escape');
        await nav(page).getByRole('link', { name: 'البدء', exact: true }).waitFor({ state: 'hidden' });
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth === innerWidth), true);
        await page.screenshot({ path: path.join(evidence, 'getting-started-mobile-rtl.png'), fullPage: true });
    });
});
