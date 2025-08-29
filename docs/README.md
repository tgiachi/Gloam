# Gloam Documentation

This directory contains the complete documentation for the Gloam roguelike game engine, built with [DocFX](https://dotnet.github.io/docfx/).

## 📚 Documentation Structure

```
docs/
├── index.md                 # Main documentation homepage
├── getting-started.md       # Quick start guide
├── installation.md          # Installation instructions
├── first-game.md           # Tutorial for first game
├── architecture/           # Architectural documentation
│   ├── overview.md
│   ├── core-components.md
│   ├── game-loop.md
│   └── entity-system.md
├── examples/               # Code examples and samples
│   └── code-samples.md
├── api/                    # Auto-generated API documentation
├── images/                 # Documentation assets
├── styles/                 # Custom CSS styles
├── toc.yml                 # Table of contents
├── docfx.json             # DocFX configuration
└── README.md              # This file
```

## 🚀 Building Documentation

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [DocFX](https://dotnet.github.io/docfx/) tool

### Local Development

1. **Install DocFX**:
   ```bash
   dotnet tool install -g docfx
   ```

2. **Build the solution** (to ensure API docs are up-to-date):
   ```bash
   dotnet build Gloam.slnx
   ```

3. **Generate documentation**:
   ```bash
   cd docs
   docfx metadata  # Generate API docs
   docfx build     # Build site
   ```

4. **Serve locally**:
   ```bash
   docfx serve _site
   ```
   Open http://localhost:8080 in your browser

### Using Task Runner

```bash
# Build documentation
task docs-build

# Serve documentation locally
task docs-serve

# Open in browser
task docs-open
```

## 📋 CI/CD Deployment

### Automatic Deployment

Documentation is automatically deployed to GitHub Pages in two scenarios:

#### 1. **On Every Push to Main Branch**
```bash
git add .
git commit -m "Update documentation"
git push origin main
```

#### 2. **On Version Tags**
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

Both scenarios trigger the `.github/workflows/docs.yml` workflow which:
1. Builds the solution
2. Generates API documentation
3. Builds the documentation site
4. Deploys to GitHub Pages

**Note**: Main branch deployments are for development/testing, while tag deployments are for stable releases.

### Manual Deployment

You can also deploy manually using the GitHub Actions interface:

1. Go to **Actions** tab in GitHub
2. Select **Deploy Documentation (Manual/Test)** workflow
3. Click **Run workflow**
4. Fill in the version and environment details

### Testing Documentation Locally

Before deploying, you can test the documentation locally:

```bash
# Build documentation
cd docs
docfx build

# Serve locally
docfx serve _site

# Open http://localhost:8080 in your browser
```

### Troubleshooting GitHub Pages

If you get a 404 error:

1. **Check GitHub Pages Settings**:
   - Go to repository Settings → Pages
   - Ensure source is set to "GitHub Actions"
   - Wait a few minutes after first deployment

2. **Verify Repository Name**:
   - URL should be: `https://tgiachi.github.io/Gloam/` (note the capital G)
   - Repository name is case-sensitive

3. **Check Workflow Status**:
   - Go to Actions tab
   - Look for "Deploy Documentation" workflow runs
   - Check if they completed successfully

4. **Manual Test Deploy**:
   - Use the "Deploy Documentation (Manual/Test)" workflow
   - This will deploy immediately for testing

## 🎨 Customization

### Styling

Custom styles are located in `styles/main.css`. The documentation uses:
- Dawn theme color palette (pink, orange, yellow, purple, blue, mint)
- Responsive design for mobile and desktop
- Dark/light mode support

### Logo and Branding

- Logo: `images/logo.png`
- Configured in `docfx.json` under `_appLogoPath`
- Consistent branding across all pages

### Navigation

Table of contents is defined in `toc.yml` with hierarchical structure:
- Getting Started
- Architecture
- Examples
- API Reference

## 📝 Contributing to Documentation

### Adding New Content

1. **Conceptual docs**: Add `.md` files in appropriate directories
2. **Code examples**: Update `examples/code-samples.md`
3. **API docs**: Automatically generated from code comments
4. **Update TOC**: Modify `toc.yml` to include new pages

### Writing Guidelines

- Use clear, concise language
- Include code examples where helpful
- Use proper markdown formatting
- Test links and cross-references
- Follow existing structure and style

### Code Comments for API Docs

API documentation is generated from XML comments in code:

```csharp
/// <summary>
/// Brief description of the class/method
/// </summary>
/// <param name="parameterName">Description of parameter</param>
/// <returns>Description of return value</returns>
public class ExampleClass
{
    // Implementation
}
```

## 🔍 Troubleshooting

### Common Issues

**Documentation not building:**
```bash
# Clean and rebuild
cd docs
rm -rf api/ _site/
docfx metadata
docfx build
```

**Missing API documentation:**
- Ensure all projects are included in `docfx.json`
- Check that XML documentation is enabled in `.csproj` files
- Verify code has proper XML comments

**Broken links:**
- Check `toc.yml` for correct file paths
- Verify cross-references use proper syntax: `<xref:Namespace.Class>`
- Test locally with `docfx serve`

**Styling issues:**
- Check `styles/main.css` for custom styles
- Verify logo path in `docfx.json`
- Test responsive design on different screen sizes

### Getting Help

- **DocFX Documentation**: https://dotnet.github.io/docfx/
- **GitHub Issues**: Report documentation issues
- **GitHub Discussions**: Ask questions about documentation

## 📊 Documentation Metrics

- **API Coverage**: 6 projects fully documented
- **Conceptual Docs**: 8 comprehensive guides
- **Code Examples**: 15+ practical examples
- **Languages**: English (primary)
- **Build Time**: ~2-3 minutes
- **Site Size**: ~50-100 pages

## 🎯 Best Practices

1. **Keep it updated**: Update docs with code changes
2. **Test locally**: Always test documentation locally before committing
3. **Use cross-references**: Link related content appropriately
4. **Include examples**: Show practical usage patterns
5. **Maintain structure**: Follow established organization
6. **Version appropriately**: Use semantic versioning for releases

---

Built with ❤️ for the Gloam community