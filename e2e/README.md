# E2E Tests

This directory contains end-to-end tests for nopCommerce using Playwright and TypeScript.

## Setup

Install dependencies:

```bash
cd e2e
npm install
npx playwright install
```

## Running Tests

```bash
# Run all tests
npm test

# Run tests in headed mode (see the browser)
npm run test:headed

# Run tests in debug mode
npm run test:debug

# Open Playwright UI mode
npm run test:ui

# Run tests in specific browser
npm run test:chromium
npm run test:firefox
npm run test:webkit

# View test report
npm run report
```

## Configuration

- **playwright.config.ts** - Playwright configuration including browser settings, base URL, and test options
- **tsconfig.json** - TypeScript configuration for the test suite
- **tests/** - Test files (*.spec.ts)

## Writing Tests

Create test files in the `tests/` directory with the `.spec.ts` extension:

```typescript
import { test, expect } from '@playwright/test';

test('my test', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Expected Title/);
});
```

## Code Generation

Use Playwright's code generator to record interactions:

```bash
npm run codegen
```

## Documentation

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Playwright TypeScript Guide](https://playwright.dev/docs/test-typescript)
