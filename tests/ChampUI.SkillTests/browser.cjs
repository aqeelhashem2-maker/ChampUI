const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const path = require('node:path');
const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');

const [origin, ...scenarios] = process.argv.slice(2);
let browser;
before(async () => { browser = await chromium.launch(); });
after(async () => { await browser?.close(); });

for (const scenario of scenarios) {
  test(scenario, async () => {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    try {
      await page.goto(origin);
      await page.locator('button[aria-label="Dark mode"]:enabled').waitFor();
      const link = page.locator(`nav a[href="/skill-${scenario}"]`);
      await Promise.all([page.waitForURL(origin + '/skill-' + scenario), link.click()]);
      await require(path.join(__dirname, scenario, 'check.cjs'))(page);
      assert.deepEqual(errors, []);
    } finally {
      await page.close();
    }
  });
}
