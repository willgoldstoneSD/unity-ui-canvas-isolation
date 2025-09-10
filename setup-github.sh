#!/bin/bash

# Setup script for creating the GitHub repository and publishing the package

echo "🚀 Setting up UI Canvas Isolation Package for GitHub"

# Check if we're in a git repository
if [ ! -d ".git" ]; then
    echo "📦 Initializing git repository..."
    git init
    git add .
    git commit -m "Initial commit: UI Canvas Isolation package"
fi

echo "📋 Next steps to publish your package:"
echo ""
echo "1. Create a new repository on GitHub:"
echo "   - Repository name: unity-ui-canvas-isolation"
echo "   - Description: Unity Editor tool for UI canvas isolation workflow"
echo "   - Make it public"
echo "   - Don't initialize with README (we already have one)"
echo ""
echo "2. Add the remote and push:"
echo "   git remote add origin https://github.com/willgoldstone/unity-ui-canvas-isolation.git"
echo "   git branch -M main"
echo "   git push -u origin main"
echo ""
echo "3. Create a release tag:"
echo "   git tag v1.0.0"
echo "   git push origin v1.0.0"
echo ""
echo "4. Enable GitHub Pages:"
echo "   - Go to repository Settings → Pages"
echo "   - Source: Deploy from a branch"
echo "   - Branch: gh-pages"
echo "   - Folder: / (root)"
echo ""
echo "5. Test the package installation:"
echo "   - Create a new Unity project"
echo "   - Add to Packages/manifest.json:"
echo "     \"com.willgoldstone.ui-canvas-isolation\": \"https://github.com/willgoldstone/unity-ui-canvas-isolation.git\""
echo ""
echo "✅ Package setup complete!"
echo "📖 See README.md for detailed usage instructions"

