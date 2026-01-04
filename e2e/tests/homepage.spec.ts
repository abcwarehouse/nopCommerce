import { test, expect } from '@playwright/test';

test.describe('Homepage', () => {
  test('should load the homepage successfully', async ({ page }) => {
    // Wait for page to be loaded
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    
    // Check that the page title is set
    await expect(page).toHaveTitle(/./);
    
    // Take a screenshot for visual verification
    await page.screenshot({ path: 'test-results/homepage.png', fullPage: true });
  });

  test('should have a working navigation', async ({ page }) => {
    await page.goto('/');
    
    // Example: Check for common navigation elements
    // Adjust selectors based on your actual site structure
    const navigation = page.locator('.categories-in-side-panel');
    await expect(navigation).toBeVisible();
  });
});
