const assert = require('node:assert/strict');

module.exports = async function checkEmployeeForm(page) {
    await page.locator('button[aria-label="Dark mode"]:enabled').waitFor();
    const name = page.getByLabel('Employee name', { exact: true });
    const save = page.getByRole('button', { name: 'Save employee' });
    const saved = page.getByRole('list', { name: 'Saved in this session' });

    await name.fill('   ');
    await save.click();
    await page.getByText('Enter an employee name.', { exact: true }).waitFor();
    assert.equal(await name.getAttribute('aria-invalid'), 'true');
    assert.equal(await saved.getByRole('listitem').count(), 0);

    await page.getByLabel('Department', { exact: true }).selectOption({ label: 'Sales' });
    await name.fill('Sara Ali');
    await save.click();
    await saved.getByText('Sara Ali · Sales').waitFor();
    assert.equal(await saved.getByRole('listitem').count(), 1);
    await page.getByRole('status').filter({ hasText: 'Employee saved' }).waitFor();
    assert.equal(await name.getAttribute('aria-invalid'), null);
};
