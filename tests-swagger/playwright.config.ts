import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 120_000,
  expect: {
    timeout: 15_000,
  },
  use: {
    baseURL: 'http://127.0.0.1:5015',
    headless: true,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  webServer: {
    command: 'dotnet run --no-launch-profile --urls http://127.0.0.1:5015',
    cwd: '..',
    url: 'http://127.0.0.1:5015/swagger/index.html',
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
