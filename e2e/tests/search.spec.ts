import { test, expect } from '@playwright/test';
import { closePopups } from './helpers';

test.describe('Search functionality', () => {
  test('should be able to search for products', async ({ page }) => {
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    
    // Close any popups that may appear
    await closePopups(page);
    
    // Example search test - adjust selectors to match your site
    const searchInput = page.locator('input[type="search"], input[name="q"]').first();
    
    if (await searchInput.isVisible()) {
      await searchInput.fill('test product');
      await searchInput.press('Enter');
      
      // Wait for search results - use domcontentloaded instead of networkidle for better reliability
      await page.waitForLoadState('domcontentloaded');
      
      // Verify we're on a search results page
      expect(page.url()).toContain('search');
    }
  });
});