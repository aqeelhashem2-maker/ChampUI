const assert = require('node:assert/strict');
const { test } = require('node:test');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');

const [origin] = process.argv.slice(2);
const selectAll = process.platform === 'darwin' ? 'Meta+a' : 'Control+a';

async function run(route, latency, verify) {
    const browser = await chromium.launch();
    try {
        const page = await browser.newPage();
        const errors = [];
        let forwarded = 0;
        page.on('pageerror', error => errors.push(error.message));
        if (latency) {
            await page.routeWebSocket(/\/_blazor\?/, client => {
                const server = client.connectToServer();
                server.onMessage(message => setTimeout(() => {
                    forwarded++;
                    client.send(message);
                }, latency));
            });
        }
        await page.goto(origin + route);
        await page.waitForFunction(() => !document.querySelector('button[aria-label="Dark mode"]').disabled);
        await verify(page);
        if (latency) assert.ok(forwarded > 0, 'Expected delayed Blazor messages');
        assert.deepEqual(errors, []);
        await page.close();
    } finally {
        await browser.close();
    }
}

for (const latency of [0, 100]) {
    test(`Orders search preserves characters, edits and exact row IDs with ${latency} ms added latency`, async () => {
        await run('/orders', latency, async page => {
            const search = page.getByRole('searchbox');
            const rows = page.locator('tbody tr[data-row] td:first-child');
            const expectQuery = async (value, ids) => {
                // The diagnostics fixture reads the query from the server grid after queued inputs.
                const query = page.locator('#effective-query');
                const sequence = Number(await query.getAttribute('data-seq'));
                await page.locator('#capture-query').click();
                await page.waitForFunction(previous =>
                    Number(document.querySelector('#effective-query').dataset.seq) > previous, sequence);
                assert.equal(await search.inputValue(), value);
                assert.equal(await query.textContent(), value);
                assert.deepEqual((await rows.allTextContents()).map(text => text.trim()), ids);
                await search.click();
                await search.press('End');
            };
            await search.click();
            for (const delay of [0, 15, 65, 120]) {
                await search.press(selectAll);
                await search.press('Backspace');
                await search.pressSequentially('ORD-1048', { delay });
                await expectQuery('ORD-1048', ['ORD-1048']);
                await search.press('Backspace');
                await search.pressSequentially('7');
                await expectQuery('ORD-1047', ['ORD-1047']);
            }
        });
    });
}

test('standalone binding preserves caret edits, composition, Enter and external resets', async () => {
    await run('/text-field-audit', 100, async page => {
        const input = page.getByRole('textbox', { name: 'Audit input' });
        assert.equal(await input.inputValue(), 'Initial value');
        await input.click();
        await input.press(selectAll);
        await input.pressSequentially('abcd');
        await input.press('ArrowLeft');
        await input.press('ArrowLeft');
        await input.pressSequentially('XY');
        await page.waitForFunction(() => document.querySelector('#model').textContent === 'abXYcd');
        assert.equal(await input.inputValue(), 'abXYcd');
        assert.equal(await input.evaluate(node => node.selectionStart), 4);

        // Use Chromium IME composition to replace the selection and commit Unicode text.
        await input.press(selectAll);
        const cdp = await page.context().newCDPSession(page);
        await cdp.send('Input.imeSetComposition', { text: '東京', selectionStart: 2, selectionEnd: 2 });
        await cdp.send('Input.insertText', { text: '東京' });
        await input.press('Enter');
        await page.waitForFunction(() => document.querySelector('#entered').textContent === '東京');
        assert.equal(await input.inputValue(), '東京');

        await page.getByRole('button', { name: 'Replace externally' }).click();
        await page.waitForFunction(() => document.querySelector('#model').textContent === 'Server replacement');
        assert.equal(await input.inputValue(), 'Server replacement');
        await page.getByRole('button', { name: 'Reset' }).click();
        await page.waitForFunction(() => document.querySelector('#model').textContent === '');
        assert.equal(await input.inputValue(), '');
    });
});
