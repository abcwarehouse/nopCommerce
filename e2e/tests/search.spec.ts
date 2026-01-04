import { test, expect } from '@playwright/test';

test.describe('Search functionality', () => {
  test('should be able to search for products', async ({ page }) => {
    await page.goto('/');
    
    // Example search test - adjust selectors to match your site
    const searchInput = page.locator('input[type="search"], input[name="q"]').first();
    
    if (await searchInput.isVisible()) {
      await searchInput.fill('test product');
      await searchInput.press('Enter');
      
      // Wait for search results
      await page.waitForLoadState('networkidle');
      
      // Verify we're on a search results page
      expect(page.url()).toContain('search');
    }
  });
});