#!/bin/bash
set -e

echo "Setting up dev container..."

# Remove expired Yarn repository to avoid apt errors
echo "Removing Yarn repository..."
sudo rm -f /etc/apt/sources.list.d/yarn.list

# Install Node.js dependencies for e2e tests
echo "Installing npm packages..."
cd e2e && npm install

# Install Playwright browsers and system dependencies
echo "Installing Playwright..."
npx playwright install --with-deps

echo "Dev container setup complete!"
