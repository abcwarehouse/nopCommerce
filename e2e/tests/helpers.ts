import { Page } from '@playwright/test';

/**
 * Closes common popups and modals that may appear on the site
 */
export async function closePopups(page: Page) {
  const closeSelector = '.ltkpopup-close';
  const maxAttempts = 3;
  
  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    try {
      console.log(`Attempt ${attempt} to close popup...`);
      
      // Wait for the popup close button to appear
      const closeButton = page.locator(closeSelector).first();
      
      // Wait for it to be visible
      await closeButton.waitFor({ state: 'visible', timeout: 15000 });
      
      // Give it extra time to be fully interactive
      await page.waitForTimeout(1000);
      
      // Try clicking with force to bypass any overlays
      await closeButton.click({ force: true, timeout: 5000 });
      
      // Wait a bit and verify it's gone
      await page.waitForTimeout(1000);
      
      const isStillVisible = await closeButton.isVisible().catch(() => false);
      if (!isStillVisible) {
        console.log('Successfully closed popup');
        return;
      }
      
      console.log(`Popup still visible after attempt ${attempt}`);
    } catch (e) {
      console.log(`Attempt ${attempt} failed: ${e.message}`);
      
      if (attempt === maxAttempts) {
        console.log('No popup detected or could not close after multiple attempts');
      }
    }
  }
}
