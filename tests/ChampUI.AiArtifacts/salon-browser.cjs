// Runs only against the extracted, unmodified original SalonDashboard.zip.
const { test } = require('node:test');
const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const base = process.env.SALON_TEST_URL;
if (!base) throw new Error('Set SALON_TEST_URL to the isolated salon URL');

test('original salon navigation, forms, services and display', async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
  try {
    await page.goto(base);
    await page.waitForFunction(() => { const b = document.querySelector('button[aria-label="Dark mode"]'); return b && !b.disabled; });
    assert.equal(await page.locator('.ui-shell').getAttribute('data-theme'), 'light');
    const todayBefore = await page.locator('section[aria-label="Today\'s revenue"] .ui-stat__value').innerText();
    for (const name of ['Customers', 'Employees']) {
      await page.getByRole('link', { name, exact: true }).click();
      await page.getByRole('button', { name: 'Add record' }).click();
      const d = page.getByRole('dialog');
      await d.getByRole('button', { name: 'Save' }).click();
      await d.getByText('Enter valid details.').waitFor();
      await d.getByLabel('Name', { exact: true }).fill(`Example ${name}`);
      await d.getByLabel(name === 'Customers' ? 'Phone' : 'Specialty').fill('Example details');
      await d.getByRole('button', { name: 'Save' }).click();
      await page.getByRole('row').filter({ hasText: `Example ${name}` }).waitFor();
    }
    await page.getByRole('link', { name: 'Services', exact: true }).click();
    await page.getByText('Hair styling').waitFor();
    await page.getByRole('button', { name: 'Add service' }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByLabel('Service name').fill('Example service');
    await dialog.getByLabel('Price (KWD)').fill('15');
    await dialog.getByRole('button', { name: 'Save' }).click();
    await page.getByRole('row').filter({ hasText: 'Example service' }).waitFor();
    await page.getByRole('link', { name: 'Appointments', exact: true }).click();
    await page.getByRole('button', { name: 'New appointment' }).click();
    await page.getByRole('dialog').waitFor();
    await page.keyboard.press('Escape');
    await page.getByRole('dialog').waitFor({ state: 'hidden' });
    assert.equal(await page.getByRole('button', { name: 'New appointment' }).evaluate(e => e === document.activeElement), true);
    await page.getByRole('button', { name: 'New appointment' }).click();
    const bookingDialog = page.getByRole('dialog');
    await bookingDialog.locator('input[type="datetime-local"]').fill('');
    await bookingDialog.getByRole('button', { name: 'Book', exact: true }).click();
    await bookingDialog.getByText('Invalid or overlapping appointment. Choose another time.').waitFor();
    const future = new Date(Date.now() + 10 * 86400000 + 3 * 3600000).toISOString().slice(0, 10);
    await bookingDialog.locator('input[type="datetime-local"]').fill(`${future}T09:30`);
    await bookingDialog.getByRole('button', { name: 'Book', exact: true }).click();
    await bookingDialog.waitFor({ state: 'hidden' });
    const [year, month, day] = future.split('-');
    const row = page.getByRole('row').filter({ hasText: `${day}/${month}/${year} 09:30` });
    await row.waitFor();
    await row.getByText('Scheduled').waitFor();
    await row.getByRole('button', { name: 'Complete' }).click();
    await row.getByText('Completed').waitFor();
    assert.equal(await row.getByRole('button', { name: 'Complete' }).count(), 0);
    await page.getByRole('button', { name: 'New appointment' }).click();
    await bookingDialog.locator('input[type="datetime-local"]').fill(`${future}T09:30`);
    await bookingDialog.getByRole('button', { name: 'Book', exact: true }).click();
    await bookingDialog.getByText('Invalid or overlapping appointment. Choose another time.').waitFor();
    await bookingDialog.locator('input[type="datetime-local"]').fill(`${future}T12:00`);
    await bookingDialog.getByRole('button', { name: 'Book', exact: true }).click();
    await bookingDialog.waitFor({ state: 'hidden' });
    const noon = page.getByRole('row').filter({ hasText: `${day}/${month}/${year} 12:00` });
    await noon.waitFor();
    await noon.getByRole('button', { name: 'Cancel', exact: true }).click();
    await noon.getByText('Cancelled').waitFor();
    await page.getByRole('link', { name: 'Dashboard', exact: true }).click();
    assert.equal(await page.locator('section[aria-label="Today\'s revenue"] .ui-stat__value').innerText(), todayBefore,
      'future completed and cancelled appointments do not increase today revenue');
    await page.getByRole('link', { name: 'Appointments', exact: true }).click();
    await page.getByRole('button', { name: 'New appointment' }).click();
    const today = new Date(Date.now() + 3 * 3600000).toISOString().slice(0, 10);
    await bookingDialog.locator('input[type="datetime-local"]').fill(`${today}T23:00`);
    await bookingDialog.getByRole('button', { name: 'Book', exact: true }).click();
    await bookingDialog.waitFor({ state: 'hidden' });
    await page.getByRole('link', { name: 'Dashboard', exact: true }).click();
    assert.equal(await page.locator('section[aria-label="Today\'s revenue"] .ui-stat__value').innerText(), todayBefore,
      'scheduled same-day appointment does not increase revenue');
    await page.getByRole('link', { name: 'Appointments', exact: true }).click();
    const [ty, tm, td] = today.split('-');
    const todayRow = page.getByRole('row').filter({ hasText: `${td}/${tm}/${ty} 23:00` });
    await todayRow.getByRole('button', { name: 'Complete' }).click();
    await todayRow.getByText('Completed').waitFor();
    await page.getByRole('link', { name: 'Dashboard', exact: true }).click();
    const todayAfter = await page.locator('section[aria-label="Today\'s revenue"] .ui-stat__value').innerText();
    assert.equal(Number.parseFloat(todayAfter), Number.parseFloat(todayBefore) + 18,
      'completed same-day service adds its price exactly once');
    await page.getByRole('link', { name: 'Revenue', exact: true }).click();
    await page.getByText('Monthly revenue').waitFor();
    await page.getByRole('link', { name: 'Settings', exact: true }).click();
    await page.getByRole('button', { name: 'Dark mode' }).click();
    await page.getByRole('button', { name: 'Light mode' }).waitFor();
    assert.equal(await page.locator('.ui-shell').getAttribute('data-theme'), 'dark');
    await page.getByRole('button', { name: 'العربية' }).click();
    await page.waitForFunction(() => document.documentElement.dir === 'rtl');
    await page.setViewportSize({ width: 390, height: 844 });
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    await page.getByRole('button', { name: 'English' }).click();
    await page.waitForFunction(() => document.documentElement.dir === 'ltr');
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
  } finally { await browser.close(); }
});
