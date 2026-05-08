#!/bin/bash
set -e

echo "Setting up dev container..."

# Remove expired Yarn repository to avoid apt errors
echo "Removing Yarn repository..."
sudo rm -f /etc/apt/sources.list.d/yarn.list

echo "Dev container setup complete!"
