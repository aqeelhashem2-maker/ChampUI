const assert = require('node:assert/strict');
const { test } = require('node:test');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');

const [origin] = process.argv.slice(2);
const disabled = process.env.CHAMPUI_P2_CASE === 'missing';

test(disabled ? 'Missing logo falls back and disabled guide denies direct and interactive navigation' :
    'External logo path falls back without requesting external assets', async () => {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage();
        const external = [];
        const broken = [];
        page.on('request', request => {
            const url = new URL(request.url());
            if (url.protocol.startsWith('http') && url.origin !== origin) external.push(url.href);
        });
        page.on('response', response => {
            if (response.status() >= 400) broken.push(response.url());
        });
        await page.goto(disabled ? new URL('/focus-audit', origin).href : origin);
        await page.waitForFunction(() => !document.querySelector('button[aria-label="Dark mode"]').disabled);
        const brand = page.getByRole('link', { name: 'ChampUI Dashboard Starter' });
        assert.equal(await brand.locator('img').count(), 0);
        assert.ok(await brand.locator('.dashboard-brand-mark svg').isVisible());
        const icon = await page.locator('link[rel=icon]').getAttribute('href');
        assert.match(icon, /dashboard-icon(?:\.[a-z0-9]+)?\.svg$/);
        assert.ok((await page.request.get(new URL(icon, origin).href)).ok());
        assert.deepEqual(external, []);
        assert.deepEqual(broken, []);

        const sidebar = page.locator('.ui-sidebar__nav');
        if (disabled) {
            assert.equal(await sidebar.getByRole('link', { name: 'Getting Started' }).count(), 0);
            for (const path of ['/getting-started', '/GETTING-STARTED/', '/getting-started?from=bookmark']) {
                const result = await page.request.get(new URL(path, origin).href);
                assert.equal(result.status(), 404, path);
                assert.doesNotMatch(await result.text(), /Project structure|ApiDashboardData/);
            }
            await page.getByRole('link', { name: 'Open developer guide from fixture' }).click();
            await page.getByRole('heading', { name: 'Page not found' }).waitFor();
            assert.equal(await page.getByText('Project structure', { exact: true }).count(), 0);
        } else {
            assert.ok(await sidebar.getByRole('link', { name: 'Getting Started' }).isVisible());
        }
    } finally {
        await browser.close();
    }
});
