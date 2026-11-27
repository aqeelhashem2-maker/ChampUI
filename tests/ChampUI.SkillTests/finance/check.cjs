const assert = require('node:assert/strict');

module.exports = async function checkFinance(page) {
    const report = page.getByRole('region', { name: 'Quarter revenue' });
    await report.waitFor();
    assert.match(await report.innerText(), /3,250\.75 SAR/);
    const amounts = page.getByRole('list', { name: 'Monthly amounts' });
    assert.deepEqual(await amounts.locator('bdi').allTextContents(), ['1,200.25 SAR', '850.40 SAR', '1,200.10 SAR']);
    const chart = page.getByRole('img', { name: 'Monthly revenue in SAR' });
    assert.equal(await chart.locator('rect.ui-chart__bar').count(), 3);

    const surface = () => report.evaluate(el => getComputedStyle(el).backgroundColor);
    const light = await surface();
    await page.getByRole('button', { name: 'Dark mode' }).click();
    await page.locator('.dashboard-shell[data-theme="dark"]').waitFor();
    assert.notEqual(await surface(), light);

    await page.setViewportSize({ width: 390, height: 844 });
    await page.getByRole('button', { name: 'العربية' }).click();
    await page.locator('html[lang="ar"][dir="rtl"] .dashboard-shell[lang="ar"][dir="rtl"]').waitFor();
    await page.getByRole('heading', { name: 'تقرير الإيرادات' }).waitFor();
    await page.getByRole('img', { name: 'الإيرادات الشهرية بالريال السعودي' }).waitFor();
    assert.equal(await page.getByRole('list', { name: 'المبالغ الشهرية' }).getByRole('listitem').count(), 3);
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1), true);
    await page.getByRole('button', { name: 'English' }).click();
    await page.locator('html[lang="en"][dir="ltr"] .dashboard-shell[dir="ltr"]').waitFor();
    await page.getByRole('button', { name: 'Light mode' }).click();
    await page.locator('.dashboard-shell[data-theme="light"]').waitFor();
};
