const assert = require('node:assert/strict');

module.exports = async function checkSalonDay(page) {
    await page.locator('button[aria-label="Dark mode"]:enabled').waitFor();
    const day = page.getByLabel('Appointment day');
    const list = page.getByRole('list', { name: 'Appointments', exact: true });
    await list.getByText('10:30 · Haircut').waitFor();
    assert.equal(await day.inputValue(), '2026-11-26');

    await day.fill('2026-11-27');
    await list.getByText('14:15 · Hair styling').waitFor();
    assert.equal(await list.getByRole('listitem').count(), 1);

    await day.fill('2026-11-28');
    await page.getByRole('status').filter({ hasText: 'No appointments' }).waitFor();
    assert.equal(await list.count(), 0);

    await day.fill('2026-11-26');
    await list.getByText('10:30 · Haircut').waitFor();
    assert.equal(await page.getByText('No appointments', { exact: true }).count(), 0);

    await page.getByRole('button', { name: 'العربية' }).click();
    await page.locator('html[lang="ar"][dir="rtl"]').waitFor();
    const arabicList = page.getByRole('list', { name: 'المواعيد', exact: true });
    await arabicList.getByText('10:30 · قص الشعر').waitFor();
    assert.equal(await page.getByLabel('يوم الموعد').inputValue(), '2026-11-26');
};
