const assert = require('node:assert/strict');
const { chromium } = require(process.env.CHAMPUI_PLAYWRIGHT_MODULE);
const { join } = require('node:path');
const { writeFile } = require('node:fs/promises');

async function until(check) {
    for (let attempt = 0; attempt < 100; attempt++) {
        if (await check()) return;
        await new Promise(resolve => setTimeout(resolve, 100));
    }
    throw new Error('Expected browser state did not arrive');
}

async function verify() {
    const [url, evidence] = process.argv.slice(2);
    const browser = await chromium.launch();
    const errors = [];
    const page = await browser.newPage({ acceptDownloads: true, viewport: { width: 1440, height: 1000 } });
    page.setDefaultTimeout(12000);
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    const assets = new Map();
    page.on('response', response => {
        if (response.url().includes('/_content/ChampUI/')) assets.set(new URL(response.url()).pathname, response.status());
    });
    await page.context().tracing.start({ screenshots: true, snapshots: true });
    const row = (grid, id) => grid.locator(`tr[data-row-key="${id}"]`);
    const download = async (grid, option, filename) => {
        await grid.getByRole('button', { name: 'Export', exact: true }).click();
        const pending = page.waitForEvent('download');
        await grid.getByRole('menuitem', { name: option, exact: true }).click();
        const file = await pending;
        assert.equal(await file.failure(), null);
        await file.saveAs(join(evidence, filename));
    };
    try {
        assert.equal((await page.goto(url)).status(), 200);
        const theme = page.getByRole('button', { name: 'Toggle theme' });
        await until(() => theme.isEnabled());
        const shell = page.locator('.ui-shell');
        for (const path of ['champui.css', 'js/champui.js']) {
            const match = [...assets].find(([name]) => name.includes(path.split('/').pop().split('.')[0]) &&
                name.startsWith(path.includes('/') ? '/_content/ChampUI/js/' : '/_content/ChampUI/') &&
                name.endsWith(path.includes('/') ? '.js' : '.css'));
            assert.ok(match, `missing browser asset request for ${path}`);
            assert.equal(match[1], 200);
            const canonical = await page.request.get(new URL('/_content/ChampUI/' + path, url).href);
            assert.equal(canonical.status(), 200);
            await writeFile(join(evidence, path.split('/').pop()), await canonical.body());
        }
        assert.equal(await page.evaluate(() => typeof window.ChampUI?.downloadFile), 'function');
        assert.equal(await page.locator('script[type="module"][src*="ChampUI"]').count(), 1);
        assert.ok(await shell.evaluate(el => getComputedStyle(el).getPropertyValue('--ui-accent').trim()));
        console.log('PASS package CSS, JavaScript module, component rendering and InteractiveServer');

        const local = page.locator('#local-grid');
        const remote = page.locator('#virtual-grid');
        await row(local, 1).waitFor();
        await local.getByRole('button', { name: 'Next page' }).click();
        await row(local, 6).waitFor();
        assert.equal(await row(local, 1).count(), 0);
        await download(local, 'CSV — current page', 'consumer-local.csv');
        await download(local, 'Excel — current page', 'consumer-local.xlsx');
        await local.getByRole('searchbox').fill('Record 010');
        await until(async () => (await local.locator('tr[data-row]').count()) === 1);
        await row(local, 10).waitFor();
        await row(local, 10).locator('label.ui-check').click();
        await until(() => row(local, 10).getByRole('checkbox').isChecked());
        console.log('PASS local pagination, search, selection, CSV and XLSX downloads');

        await row(remote, 1).waitFor();
        await remote.getByTestId('virtualize-wrapper').evaluate(el => el.scrollTop = 1600);
        await row(remote, 37).waitFor();
        await remote.getByTestId('virtualize-wrapper').evaluate(el => el.scrollTop = 0);
        await row(remote, 1).waitFor();
        await remote.getByRole('searchbox').fill('Record 02');
        await row(remote, 20).waitFor();
        await remote.getByRole('button', { name: 'ID', exact: true }).click();
        await remote.getByRole('button', { name: 'ID', exact: true }).click();
        await until(async () => await remote.locator('tr[data-row]').first().getAttribute('data-row-key') === '29');
        await row(remote, 29).locator('label.ui-check').click();
        await until(() => row(remote, 29).getByRole('checkbox').isChecked());
        await remote.getByRole('button', { name: 'Export', exact: true }).click();
        assert.equal(await remote.getByRole('menuitem', { name: 'CSV — current page', exact: true }).isDisabled(), true);
        await remote.getByRole('button', { name: 'Export', exact: true }).click();
        await download(remote, 'CSV — all rows', 'consumer-virtual.csv');
        await download(remote, 'Excel — all rows', 'consumer-virtual.xlsx');
        console.log('PASS virtual scrolling, search, sort, selection and export scopes');

        const background = () => shell.evaluate(el => getComputedStyle(el).getPropertyValue('--ui-bg').trim());
        const initialTheme = await shell.getAttribute('data-theme');
        const initialBackground = await background();
        await theme.click();
        await until(async () => await shell.getAttribute('data-theme') !== initialTheme);
        assert.notEqual(await background(), initialBackground);
        await theme.click();
        await until(async () => await shell.getAttribute('data-theme') === initialTheme);
        await page.getByRole('button', { name: 'Toggle Arabic' }).click();
        await until(async () => await shell.getAttribute('dir') === 'rtl');
        assert.equal(await shell.getAttribute('lang'), 'ar');
        assert.equal(await shell.evaluate(el => getComputedStyle(el).direction), 'rtl');
        await page.getByRole('heading', { name: 'اختبار مكتبة الواجهة' }).waitFor();
        await page.screenshot({ path: join(evidence, 'consumer-arabic.png'), fullPage: true });
        assert.deepEqual(errors, []);
        console.log('PASS Arabic RTL, light and dark theme toggles, no browser console or page errors');
    } finally {
        await page.context().tracing.stop({ path: join(evidence, 'consumer-trace.zip') });
        await browser.close();
    }
}

verify().catch(error => { console.error(error); process.exitCode = 1; });
