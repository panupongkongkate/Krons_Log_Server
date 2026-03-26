import path from 'node:path';
import { expect, test, type Locator } from '@playwright/test';

const apiKey = 'test-api-key-123';

test('Swagger UI authorize and exercise all endpoints', async ({ page, baseURL }) => {
  await page.goto('/swagger/index.html');
  await expect(page.getByRole('heading', { name: /Krons_Log_Server/i }).first()).toBeVisible();

  await page.getByRole('button', { name: 'Authorize' }).click();
  const authDialog = page.locator('.dialog-ux').filter({ hasText: 'X-Api-Key' });
  await expect(authDialog).toBeVisible();
  await authDialog.locator('input').fill(apiKey);
  await authDialog.getByRole('button', { name: 'Apply credentials' }).click();
  await expect(authDialog.getByText('Authorized', { exact: false })).toBeVisible();
  await authDialog.getByRole('button', { name: 'Close' }).click();

  const healthOperation = page.locator('.opblock.opblock-get').filter({ hasText: '/api/logs/health' });
  await healthOperation.locator('.opblock-summary-control').click();
  await healthOperation.getByRole('button', { name: 'Try it out' }).click();
  await healthOperation.getByRole('button', { name: 'Execute' }).click();
  await expect(healthOperation.locator('.responses-inner')).toContainText('"success": true');
  await expect(healthOperation.locator('.responses-inner')).toContainText('"service": "Krons_Log_Server"');

  const uploadOperation = page.locator('.opblock.opblock-post').filter({ hasText: '/api/logs/upload' });
  await uploadOperation.locator('.opblock-summary-control').click();
  await uploadOperation.getByRole('button', { name: 'Try it out' }).click();

  const sampleFile = path.resolve(__dirname, 'fixtures', 'sample-upload.txt');
  await uploadOperation.locator('input[type="file"]').setInputFiles(sampleFile);

  await fillParameter(uploadOperation, 'machineName', 'TEST-PLAYWRIGHT');
  await fillParameter(uploadOperation, 'appVersion', '1.0.0-playwright');
  await fillParameter(uploadOperation, 'fileName', 'sample-upload.txt');
  await fillParameter(uploadOperation, 'createdAtUtc', '2026-03-27T00:00:00Z');

  await uploadOperation.getByRole('button', { name: 'Execute' }).click();
  await expect(uploadOperation.locator('.responses-inner')).toContainText('"success": true');
  await expect(uploadOperation.locator('.responses-inner')).toContainText('"machineName": "TEST-PLAYWRIGHT"');
  await expect(uploadOperation.locator('.responses-inner')).toContainText('"fileName": "sample-upload.txt"');
  await expect(uploadOperation.locator('.responses-inner')).toContainText('Storage');

  await page.goto(`${baseURL}/api/logs/health`);
  await expect(page.locator('body')).toContainText('Krons_Log_Server');
});

async function fillParameter(operation: Locator, parameterName: string, value: string) {
  const row = operation.locator('tr').filter({ hasText: parameterName }).first();
  const textbox = row.getByRole('textbox').first();
  await textbox.fill(value);
}
