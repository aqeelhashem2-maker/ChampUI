const assert = require('node:assert/strict');
const { chromium } = require(process.env.CHAMPUI_PLAYWRIGHT_MODULE);
const { mkdir } = require('node:fs/promises');
const { join } = require('node:path');

async function verify() {
    const [baseUrl, directory] = process.argv.slice(2);
    await mkdir(directory, { recursive: true });
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage({ viewport: { width: 1440, height: 1000 }, deviceScaleFactor: 1 });
        const errors = [];
        page.on('pageerror', error => errors.push(error.message));
        page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });

        for (const [route, name] of [['/', 'foundations'], ['/forms', 'forms'], ['/data', 'data'], ['/charts', 'charts']]) {
            await page.goto(new URL(route, baseUrl).href);
            await page.locator('.ui-shell').waitFor();
            await page.evaluate(() => document.fonts.ready);
            await page.mouse.move(0, 0);
            await page.screenshot({ path: join(directory, name + '-light.png'), animations: 'disabled' });
        }

        await page.goto(new URL('/', baseUrl).href);
        await page.getByRole('button', { name: 'Switch theme' }).click();
        await page.locator('.ui-shell[data-theme="dark"]').waitFor();
        await page.getByRole('button', { name: 'العربية' }).click();
        await page.locator('.ui-shell[dir="rtl"][lang="ar"]').waitFor();
        await page.evaluate(() => document.activeElement?.blur());
        await page.mouse.move(0, 0);
        await page.screenshot({ path: join(directory, 'foundations-rtl-dark.png'), animations: 'disabled' });
        assert.deepEqual(errors, []);
        console.log('Captured five neutral baselines at 1440 × 1000; Chromium', browser.version(), '; console errors: 0');
    } finally {
        await browser.close();
    }
}

verify().catch(error => { console.error(error); process.exitCode = 1; });
