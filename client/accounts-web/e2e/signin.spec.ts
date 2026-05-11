import { test, expect } from '@playwright/test';

test('register firm, sign in, land on dashboard', async ({ page }) => {
  const slug = `e2e-${Math.random().toString(36).slice(2, 10)}`;
  const email = `owner-${Math.random().toString(36).slice(2, 10)}@example.com`;
  const password = 'long-enough-password-1';

  await page.goto('/register');
  await page.getByLabel('Firm name').fill('E2E Co');
  await page.getByLabel('Firm slug (kebab-case)').fill(slug);
  await page.getByLabel('Owner email').fill(email);
  await page.getByLabel('Password (min 12 chars)').fill(password);
  await page.getByRole('button', { name: 'Register' }).click();

  await page.waitForURL('**/sign-in');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();

  await page.waitForURL('**/dashboard');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  await expect(page.getByText(/Firm ID/)).toBeVisible();
});
