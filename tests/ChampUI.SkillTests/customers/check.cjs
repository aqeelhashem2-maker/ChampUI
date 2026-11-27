const assert = require('node:assert/strict');

module.exports = async function checkCustomers(page) {
    await page.getByRole('button', {name: 'Dark mode', exact: true})
        .and(page.locator(':enabled')).waitFor();

    const search = page.getByRole('searchbox', {name: 'Search accounts'});
    const noor = page.getByRole('row').filter({hasText: 'Noor Sami'});
    const rana = page.getByRole('row').filter({hasText: 'Rana Ali'});
    await search.fill('Noor');
    await rana.waitFor({state: 'hidden'});
    const details = noor.getByRole('button', {name: 'Details'});
    await details.focus();
    await page.keyboard.press('Enter');

    const dialog = page.getByRole('dialog', {name: 'Account details'});
    await dialog.getByText('noor@example.com', {exact: true}).waitFor();
    await page.waitForFunction(() => document.activeElement === document.querySelector('dialog[open] button'));
    await page.keyboard.press('Escape');
    await dialog.waitFor({state: 'hidden'});
    const focused = noor.locator('button:focus');
    await focused.waitFor();
    assert.equal(await focused.textContent().then(text => text.trim()), 'Details');

    await search.fill('absent');
    await page.getByText('No matching accounts', {exact: true}).waitFor();
    await search.fill('');
    await rana.waitFor();
    await noor.waitFor();
};
