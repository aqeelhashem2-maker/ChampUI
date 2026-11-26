const { chromium } = require(process.env.PLAYWRIGHT_MODULE);
const assert = require('node:assert/strict');
const { before, after, test } = require('node:test');
const [origin, evidence] = process.argv.slice(2);
let browser;

before(async () => { browser = await chromium.launch(); });
after(async () => { await browser?.close(); });

async function run(name, action) {
    const context = await browser.newContext();
    await context.tracing.start({ screenshots: true, snapshots: true });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    try {
        await page.goto(origin + '/dialog-audit');
        await page.waitForFunction(() => !document.querySelector('button[aria-label="Dark mode"]').disabled);
        await action(page);
        assert.deepEqual(errors, []);
    } catch (error) {
        await page.screenshot({ path: evidence + '/' + name + '-failure.png' });
        throw error;
    } finally {
        await context.tracing.stop({ path: evidence + '/' + name + '.zip' });
        await context.close();
    }
}

const modal = page => page.getByRole('dialog', { name: 'Audit modal', exact: true });
const active = async (page, locator) => {
    const element = await locator.elementHandle();
    assert.ok(element);
    await page.waitForFunction(node => document.activeElement === node, element);
};

test('keyboard launch, title and description, both wraps, inert background and Escape', async () => {
    await run('keyboard', async page => {
        for (let n = 0; n < 35 && !await page.locator('#launch').evaluate(el => document.activeElement === el); n++) {
            await page.keyboard.press('Tab');
        }
        await active(page, page.locator('#launch'));
        await page.keyboard.press('Enter');
        const dialog = modal(page);
        await dialog.waitFor();
        const close = dialog.getByRole('button', { name: 'Close', exact: true });
        await active(page, close);
        assert.equal(await dialog.getAttribute('aria-modal'), 'true');
        assert.equal(await dialog.evaluate(node => node.matches(':modal')), true);
        const description = await dialog.getAttribute('aria-describedby');
        assert.equal(await page.locator('#' + description).textContent(), 'Audit dialog description.');
        await page.keyboard.press('Shift+Tab');
        await active(page, page.locator('#final'));
        await page.keyboard.press('Tab');
        await active(page, close);
        await page.keyboard.press('Tab');
        await active(page, page.locator('#nested-open'));
        // The closed accessibility tree excludes background buttons while the modal is active.
        const accessibilityTree = JSON.stringify(await page.accessibility.snapshot({ interestingOnly: false }));
        assert.doesNotMatch(accessibilityTree, /"name":"Background action"/);
        await page.keyboard.press('Escape');
        await dialog.waitFor({ state: 'hidden' });
        await active(page, page.locator('#launch'));
        assert.equal(await page.locator('#background-count').textContent(), '0');
        await page.waitForFunction(() => document.querySelector('#closed-count').textContent.trim() === '1');
    });
});

test('mouse close, backdrop, repeated cycles and parent-controlled close', async () => {
    await run('pointer', async page => {
        const launcher = page.locator('#launch');
        for (let n = 0; n < 3; n++) {
            await launcher.click();
            const dialog = modal(page);
            await dialog.waitFor();
            const close = dialog.getByRole('button', { name: 'Close', exact: true });
            await active(page, close);
            if (n === 0) await close.click();
            if (n === 1) {
                const point = await page.locator('#background').boundingBox();
                await page.mouse.click(point.x + 4, point.y + 4);
            }
            if (n === 2) await page.locator('#final').click();
            await dialog.waitFor({ state: 'hidden' });
            await active(page, launcher);
        }
        assert.equal(await page.locator('#background-count').textContent(), '0');
        await page.waitForFunction(() => document.querySelector('#closed-count').textContent.trim() === '2');
        await page.locator('#background').click();
        await page.waitForFunction(() => document.querySelector('#background-count').textContent.trim() === '1');
    });
});

test('clicks inside the modal and on its backdrop do not invoke Blazor ancestor actions', async () => {
    await run('ancestor-click', async page => {
        for (const dismiss of ['close button', 'backdrop']) {
            await page.locator('#launch').click();
            const dialog = modal(page);
            await dialog.waitFor();
            await dialog.getByText('Audit dialog description.').click();
            if (dismiss === 'close button') {
                await dialog.getByRole('button', { name: 'Close', exact: true }).click();
            } else {
                await page.mouse.click(8, 8);
            }
            await dialog.waitFor({ state: 'hidden' });
            await active(page, page.locator('#launch'));
            assert.equal(await page.locator('#ancestor-clicks').textContent(), '0');
        }
    });
});

test('dark RTL nested modal restores the inner opener, top-layer disposal restores background', async () => {
    await run('nested-disposal', async page => {
        await page.getByRole('button', { name: 'Dark mode' }).click();
        await page.getByRole('button', { name: 'العربية' }).click();
        await page.waitForFunction(() => document.documentElement.dir === 'rtl' &&
            document.querySelector('.dashboard-shell').dataset.theme === 'dark');
        await page.locator('#launch').click();
        const dialog = modal(page);
        await dialog.waitFor();
        assert.equal(await dialog.evaluate(node => getComputedStyle(node).direction), 'rtl');
        assert.equal(await dialog.evaluate(node => getComputedStyle(node, '::backdrop').backgroundColor), 'rgba(3, 5, 9, 0.66)');
        await page.screenshot({ path: evidence + '/dialog-dark-rtl.png' });
        const nestedOpener = page.locator('#nested-open');
        await nestedOpener.click();
        const nested = page.getByRole('dialog', { name: 'Nested modal' });
        await nested.waitFor();
        const close = nested.getByRole('button', { name: 'Close', exact: true });
        await active(page, close);
        await page.keyboard.press('Tab');
        await active(page, close);
        await page.keyboard.press('Escape');
        await nested.waitFor({ state: 'hidden' });
        await active(page, nestedOpener);
        await page.locator('#dispose').click();
        await dialog.waitFor({ state: 'detached' });
        await active(page, page.locator('#launch'));
        await page.locator('#background').click();
        await page.waitForFunction(() => document.querySelector('#background-count').textContent.trim() === '1');
        await page.locator('#launch').click();
        await modal(page).waitFor();
        await page.keyboard.press('Escape');
        await modal(page).waitFor({ state: 'hidden' });
        await page.waitForFunction(() => document.querySelector('#closed-count').textContent.trim() === '1');
    });
});

test('no tabbable controls keeps focus on the dialog and Escape restores the launcher', async () => {
    await run('empty', async page => {
        const launcher = page.locator('#no-controls');
        await launcher.click();
        const dialog = page.getByRole('dialog', { name: 'Empty modal' });
        await dialog.waitFor();
        await active(page, dialog);
        await page.keyboard.press('Tab');
        await active(page, dialog);
        await page.keyboard.press('Shift+Tab');
        await active(page, dialog);
        await page.keyboard.press('Escape');
        await dialog.waitFor({ state: 'hidden' });
        await active(page, launcher);
    });
});

test('Tab uses newly inserted controls and recovers when the focused control is removed', async () => {
    await run('dynamic', async page => {
        await page.locator('#open-dynamic').click();
        const dialog = page.getByRole('dialog', { name: 'Dynamic modal', exact: true });
        await dialog.waitFor();
        const close = dialog.getByRole('button', { name: 'Close', exact: true });
        const insert = page.locator('#insert');
        const remove = page.locator('#remove');
        await active(page, close);
        await page.keyboard.press('Tab');
        await active(page, insert);
        await page.keyboard.press('Enter');
        await remove.waitFor();
        await active(page, insert); // An ordinary rerender must preserve valid focus.
        await page.keyboard.press('Tab');
        await active(page, remove);
        await page.keyboard.press('Enter');
        await remove.waitFor({ state: 'detached' });
        await active(page, close);
        await page.keyboard.press('Shift+Tab');
        await active(page, insert);
        await page.keyboard.press('Escape');
        await dialog.waitFor({ state: 'hidden' });
        await active(page, page.locator('#open-dynamic'));
    });
});

test('closing restores the workflow heading when the launcher is disabled or removed', async () => {
    await run('unavailable-opener', async page => {
        for (const action of ['disable-launcher', 'remove-launcher']) {
            await page.locator('#open-fallback').click();
            const dialog = page.getByRole('dialog', { name: 'Fallback modal' });
            await dialog.waitFor();
            await page.locator('#' + action).click();
            if (action === 'disable-launcher')
                await page.waitForFunction(() => document.querySelector('#open-fallback').disabled);
            else
                await page.locator('#open-fallback').waitFor({ state: 'detached' });
            await page.keyboard.press('Escape');
            await dialog.waitFor({ state: 'hidden' });
            await active(page, page.getByRole('heading', { name: 'Dialog audit' }));
            await page.locator('#reset-fallback').click();
            await page.waitForFunction(() => document.querySelector('#open-fallback')?.disabled === false);
        }
    });
});

test('rapid callback-driven reopen and repeated Escape notify exactly once per close', async () => {
    await run('rapid-reopen', async page => {
        await page.locator('#launch').click();
        const dialog = modal(page);
        await dialog.waitFor();
        await page.locator('#reopen').click();
        await page.keyboard.down('Escape');
        await page.waitForFunction(() => document.querySelector('#closed-count').textContent.trim() === '1');
        await dialog.waitFor();
        await active(page, dialog.getByRole('button', { name: 'Close', exact: true }));
        await page.keyboard.down('Escape'); // Key repeat stays in the reopened modal.
        await page.keyboard.up('Escape');
        assert.equal(await dialog.isVisible(), true);
        assert.equal(await page.locator('#callback-sequence').textContent(), 'hidden,closed');
        await page.keyboard.press('Escape');
        await dialog.waitFor({ state: 'hidden' });
        await active(page, page.locator('#launch'));
        await page.waitForFunction(() => document.querySelector('#callback-sequence').textContent.trim() === 'hidden,closed,hidden,closed');
    });
});
