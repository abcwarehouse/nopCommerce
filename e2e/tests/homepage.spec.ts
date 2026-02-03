import { test, expect } from '@playwright/test';
import { closePopups } from './helpers';

test.describe('Homepage', () => {
  test('should load the homepage successfully', async ({ page }) => {
    // Wait for page to be loaded
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    
    // Close any popups that may appear
    await closePopups(page);
    
    // Check that the page title is set
    await expect(page).toHaveTitle(/./);
    
    // Wait for page to settle before taking screenshot
    await page.waitForLoadState('load');
    
    // Take a screenshot for visual verification with increased timeout
    await page.screenshot({ 
      path: 'test-results/homepage.png', 
      fullPage: true,
      timeout: 30000 // 30 seconds timeout for full-page screenshot
    });
  });

  test('should have a working navigation', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    
    // Close any popups that may appear
    await closePopups(page);
    
    // Example: Check for common navigation elements
    // Adjust selectors based on your actual site structure
    const navigation = page.locator('.categories-in-side-panel');
    await expect(navigation).toBeVisible();
  });
});
