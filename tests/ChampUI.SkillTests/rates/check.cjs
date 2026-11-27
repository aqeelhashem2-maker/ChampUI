const assert = require('node:assert/strict');

module.exports = async function checkRates(page) {
    const list = page.getByRole('list', { name: 'Service rates' });
    await list.waitFor();
    await page.waitForFunction(() => document.querySelector('.ui-topbar button[aria-label="Dark mode"]')?.disabled === false);
    assert.deepEqual(await list.getByRole('listitem').allTextContents(), ['Consultation · 80.00 SAR', 'Follow-up · 45.00 SAR']);
    const location = page.getByRole('combobox', { name: 'Location' });
    await location.selectOption({ label: 'Annex' });
    const alert = page.getByRole('alert');
    await alert.waitFor();
    assert.match(await alert.innerText(), /Rates unavailable/);
    assert.equal(await list.count(), 0);
    await location.selectOption({ label: 'New branch' });
    await page.getByText('No rates published', { exact: true }).waitFor();
    assert.equal(await alert.count(), 0);
    await location.selectOption({ label: 'Central' });
    await list.waitFor();
    assert.equal(await list.getByRole('listitem').count(), 2);
};
