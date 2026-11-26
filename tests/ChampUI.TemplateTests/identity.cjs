const assert = require('node:assert/strict');
const { join } = require('node:path');
const { test } = require('node:test');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');

test('published project applies customized identity before and after Blazor hydration', async () => {
    const [url, evidence] = process.argv.slice(2);
    const browser = await chromium.launch();
    try {
        for (const javaScriptEnabled of [false, true]) {
            const page = await browser.newPage({ javaScriptEnabled });
            const errors = [];
            page.on('pageerror', error => errors.push(error.message));
            page.on('response', response => {
                if (response.status() >= 400) errors.push(response.url());
            });

            await page.goto(url);
            const heading = page.getByRole('heading', { name: 'Atlas Operations' });
            await heading.waitFor();
            const toggle = page.getByRole('button', { name: 'الوضع الفاتح' });
            if (javaScriptEnabled) {
                await page.waitForFunction(() => !document.querySelector('button[aria-label="الوضع الفاتح"]').disabled);
            } else {
                assert.equal(await toggle.isDisabled(), true);
            }
            assert.equal(await page.locator('html').getAttribute('lang'), 'ar');
            assert.equal(await page.locator('html').getAttribute('dir'), 'rtl');
            assert.equal(await page.locator('.dashboard-shell').getAttribute('data-theme'), 'dark');
            assert.equal(await page.locator('.dashboard-shell').getAttribute('dir'), 'rtl');
            assert.equal(await page.locator('meta[name=description]').getAttribute('content'), 'Atlas workspace overview');
            assert.equal(await page.title(), 'لوحة المعلومات · Atlas Portal');
            const brand = page.getByRole('link', { name: 'Atlas Operations' });
            assert.match(await brand.innerText(), /Atlas/);
            assert.equal(await page.locator('.workspace-name').innerText(), 'Atlas Operations');
            const logo = brand.locator('img');
            assert.equal(await logo.evaluate(image => image.complete && image.naturalWidth === 38), true);
            const favicon = await page.locator('link[rel=icon]').getAttribute('href');
            assert.equal(new URL(favicon, url).href, await logo.evaluate(image => image.src));
            assert.ok((await page.request.get(new URL(favicon, url).href)).ok());
            if (javaScriptEnabled) {
                await page.screenshot({ path: join(evidence, 'custom-identity.png') });
                await page.setViewportSize({ width: 390, height: 844 });
                await page.getByRole('button', { name: 'فتح أو إغلاق قائمة التنقل' }).click();
                await brand.waitFor();
                await page.waitForFunction(() =>
                    Math.abs(document.querySelector('.ui-sidebar').getBoundingClientRect().right - innerWidth) < 1);
                assert.ok(await logo.isVisible());
                await page.screenshot({ path: join(evidence, 'custom-identity-mobile.png') });
                await page.keyboard.press('Escape');
                await brand.waitFor({ state: 'hidden' });
                await toggle.click();
                await page.waitForFunction(() => document.querySelector('.dashboard-shell').dataset.theme === 'light');
                await page.reload();
                assert.equal(await page.locator('.dashboard-shell').getAttribute('data-theme'), 'dark');
            }
            assert.deepEqual(errors, []);
            await page.close();
        }
    } finally {
        await browser.close();
    }
});
