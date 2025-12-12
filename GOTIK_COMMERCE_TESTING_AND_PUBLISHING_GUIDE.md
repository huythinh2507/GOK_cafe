# Gotik Commerce - Testing & Publishing Guide

**Package**: Gotik.Commerce v1.0.0
**Status**: Built and Ready
**Location**: `d:\GOK_Cafe_BE\GOK_cafe\Gotik.Commerce\nupkg\`

---

## ⚠️ Important Discovery

During testing, we discovered that **Gotik.Commerce** has dependencies on internal projects:
- GOKCafe.Domain
- GOKCafe.Application
- GOKCafe.Infrastructure

These are currently **project references** and need to be handled before publishing.

---

## 🔧 Two Publishing Options

### Option 1: Publish Dependencies First (Recommended for Public Release)

**Steps**:

1. **Publish GOKCafe.Domain to NuGet**:
```bash
cd GOKCafe.Domain
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Domain.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

2. **Publish GOKCafe.Application to NuGet**:
```bash
cd GOKCafe.Application
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Application.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

3. **Publish GOKCafe.Infrastructure to NuGet**:
```bash
cd GOKCafe.Infrastructure
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Infrastructure.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

4. **Publish Gotik.Commerce to NuGet**:
```bash
cd Gotik.Commerce
dotnet nuget push nupkg/Gotik.Commerce.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

**Pros**:
- ✅ Standard NuGet package structure
- ✅ Users can update dependencies independently
- ✅ Follows best practices

**Cons**:
- ❌ Need to publish 4 packages
- ❌ Exposes internal architecture

---

### Option 2: Create Self-Contained Package (Faster, Private Use)

Bundle everything into one package by copying code instead of using project references.

**Steps**:

1. **Create standalone package**:
```bash
# Create new project
cd d:\GOK_Cafe_BE\GOK_cafe
dotnet new classlib -n Gotik.Commerce.Standalone -f net9.0

# Copy all source files
cp -r GOKCafe.Domain/Entities Gotik.Commerce.Standalone/
cp -r GOKCafe.Domain/Interfaces Gotik.Commerce.Standalone/
cp -r GOKCafe.Application/Services Gotik.Commerce.Standalone/
cp -r GOKCafe.Application/DTOs Gotik.Commerce.Standalone/
cp -r GOKCafe.Infrastructure/Data Gotik.Commerce.Standalone/
cp -r GOKCafe.Infrastructure/Repositories Gotik.Commerce.Standalone/
cp -r Gotik.Commerce/Controllers Gotik.Commerce.Standalone/
cp -r Gotik.Commerce/Composing Gotik.Commerce.Standalone/
cp -r Gotik.Commerce/Extensions Gotik.Commerce.Standalone/
cp -r Gotik.Commerce/App_Plugins Gotik.Commerce.Standalone/

# Update .csproj to remove project references
# Add all NuGet dependencies directly
# Build and pack
```

**Pros**:
- ✅ Single package to manage
- ✅ No external dependencies
- ✅ Faster to publish

**Cons**:
- ❌ Larger package size
- ❌ Code duplication
- ❌ Harder to maintain

---

## 🧪 Local Testing (Without Publishing)

For testing before publishing, use the GOK_cafe solution directly:

### Method 1: Test Within Existing Solution

1. **Update GOKCafe.Web** to use Gotik.Commerce:
```bash
cd GOKCafe.Web
dotnet add reference ../Gotik.Commerce/Gotik.Commerce.csproj
```

2. **Run the Web project**:
```bash
dotnet run
```

3. **Test API endpoints**:
```bash
curl https://localhost:44317/api/v1/products
curl https://localhost:44317/api/v1/categories
curl https://localhost:44317/api/v1/cart
```

### Method 2: Create Local NuGet Feed

1. **Set up local NuGet feed** with all dependencies:
```bash
# Create local feed folder
mkdir D:\LocalNuGetFeed

# Copy all packages
cp GOKCafe.Domain/bin/Release/*.nupkg D:\LocalNuGetFeed/
cp GOKCafe.Application/bin/Release/*.nupkg D:\LocalNuGetFeed/
cp GOKCafe.Infrastructure/bin/Release/*.nupkg D:\LocalNuGetFeed/
cp Gotik.Commerce/nupkg/*.nupkg D:\LocalNuGetFeed/

# Pack dependencies first
cd GOKCafe.Domain && dotnet pack -c Release
cd ../GOKCafe.Application && dotnet pack -c Release
cd ../GOKCafe.Infrastructure && dotnet pack -c Release
```

2. **Create test project**:
```bash
dotnet new umbraco -n TestGotikLocal
cd TestGotikLocal
dotnet nuget add source "D:\LocalNuGetFeed" -n LocalFeed
dotnet add package Gotik.Commerce --version 1.0.0
dotnet run
```

---

## 📋 Pre-Publishing Checklist

### For All Packages

- [ ] **Version Numbers**: Consistent across all packages
- [ ] **README.md**: Up to date and accurate
- [ ] **CHANGELOG.md**: Version history documented
- [ ] **License**: MIT license confirmed
- [ ] **Repository URL**: Correct GitHub URL
- [ ] **Package Tags**: Appropriate tags for discoverability
- [ ] **Dependencies**: All dependencies specified correctly

### For GOKCafe.Domain

- [ ] Update .csproj with NuGet metadata:
```xml
<PropertyGroup>
  <PackageId>GOKCafe.Domain</PackageId>
  <Version>1.0.0</Version>
  <Authors>GOK Cafe Team</Authors>
  <Description>Domain entities and interfaces for GOK Cafe e-commerce system</Description>
  <PackageTags>ecommerce;domain;entities</PackageTags>
  <PackageProjectUrl>https://github.com/huythinh2507/GOK_cafe</PackageProjectUrl>
  <RepositoryUrl>https://github.com/huythinh2507/GOK_cafe</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
</PropertyGroup>
```

### For GOKCafe.Application

- [ ] Update .csproj with NuGet metadata
- [ ] Include AutoMapper profiles
- [ ] Include FluentValidation rules

### For GOKCafe.Infrastructure

- [ ] Update .csproj with NuGet metadata
- [ ] Ensure migrations are included
- [ ] EF Core configurations included

### For Gotik.Commerce

- [x] ✅ Already configured and ready

---

## 🚀 Publishing to NuGet.org

### Prerequisites

1. **Create NuGet.org Account**:
   - Go to https://www.nuget.org
   - Sign up or log in

2. **Generate API Key**:
   - Go to Account → API Keys
   - Create new API key
   - **Scopes**: Push new packages and package versions
   - **Glob Pattern**: `Gotik.*` or `GOKCafe.*`
   - **Expiration**: 365 days (or custom)
   - Copy the key (you'll only see it once!)

### Publishing Commands

```bash
# Set your API key as environment variable (recommended)
export NUGET_API_KEY="your-api-key-here"

# Or use directly in commands
dotnet nuget push package.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

### Publishing Order (Option 1)

**1. Publish GOKCafe.Domain**:
```bash
cd d:\GOK_Cafe_BE\GOK_cafe\GOKCafe.Domain
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Domain.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**2. Publish GOKCafe.Infrastructure** (depends on Domain):
```bash
cd ../GOKCafe.Infrastructure
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Infrastructure.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**3. Publish GOKCafe.Application** (depends on Domain & Infrastructure):
```bash
cd ../GOKCafe.Application
dotnet pack -c Release -o ./nupkg
dotnet nuget push nupkg/GOKCafe.Application.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

**4. Publish Gotik.Commerce** (depends on all above):
```bash
cd ../Gotik.Commerce
dotnet nuget push nupkg/Gotik.Commerce.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

### Verification

After publishing, verify on NuGet.org:
1. Go to https://www.nuget.org/packages/Gotik.Commerce
2. Check package appears (may take 5-10 minutes)
3. Verify dependencies are listed
4. Check README displays correctly

---

## 📱 Publishing to Umbraco Marketplace

### Prerequisites

1. **Create Marketplace Account**:
   - Go to https://marketplace.umbraco.com
   - Sign up with your Umbraco ID

2. **Prepare Marketing Materials**:
   - Package screenshots (API documentation, Swagger UI)
   - Feature description
   - Logo/icon (optional but recommended)

### Submission Steps

1. **Go to Submit Package**:
   - https://marketplace.umbraco.com/submit

2. **Fill in Package Details**:
```
Name: Gotik Commerce
Tagline: Complete E-Commerce API for Umbraco CMS
Category: Commerce
Compatibility: Umbraco 16.3.4+
License: MIT
Price: Free (or set your price)
```

3. **Package Description** (use README.md):
```markdown
# Gotik Commerce

Transform your Umbraco CMS into a powerful e-commerce platform with Gotik Commerce.

## Features
- Complete RESTful API for e-commerce
- Product catalog with dynamic filtering
- Shopping cart (session-based & authenticated)
- Order management
- Odoo ERP integration
- JWT authentication
- Auto-registration via Umbraco Composer

## Installation
```bash
dotnet add package Gotik.Commerce
```

No additional configuration required - services register automatically!
```

4. **Upload Package**:
   - Upload `Gotik.Commerce.1.0.0.nupkg`
   - Or link to NuGet.org package

5. **Add Screenshots**:
   - API endpoint documentation
   - Swagger UI
   - Sample API responses
   - Umbraco backoffice dashboard

6. **Submit for Review**:
   - Review takes 1-3 business days
   - You'll receive email notification

---

## 🧪 Post-Publishing Testing

After publishing to NuGet.org:

### Test Installation

```bash
# Create fresh Umbraco site
dotnet new umbraco -n FreshTest
cd FreshTest

# Wait 10 minutes for NuGet to index

# Install package
dotnet add package Gotik.Commerce

# Configure appsettings.json
# Add connection string for GotikCommerceDb

# Run
dotnet run

# Test APIs
curl https://localhost:5001/api/v1/products
```

### Verify

- [ ] Package installs successfully
- [ ] No dependency errors
- [ ] Composer registers services automatically
- [ ] API endpoints accessible
- [ ] Database migrations work
- [ ] JWT authentication works
- [ ] Swagger UI displays endpoints

---

## 🐛 Troubleshooting

### Issue: "Unable to find package GOKCafe.Domain"

**Solution**: Publish dependency packages first (see Option 1 above)

### Issue: "Package downgrade detected"

**Solution**: Ensure all packages use compatible Umbraco version (16.3.4)

### Issue: "Services not registered"

**Solution**: Verify Composer is loaded:
```csharp
// Check in Umbraco logs
// Should see: "Gotik Commerce: Registering services..."
```

### Issue: "Database not created"

**Solution**: Run migrations manually:
```bash
dotnet ef database update --project Gotik.Commerce
```

---

## 📊 Publishing Metrics to Track

After publishing, monitor:

- **Download count** on NuGet.org
- **GitHub stars** and issues
- **Umbraco Marketplace** rating and reviews
- **Community feedback** and questions

---

## 🔄 Version Update Process

For future updates:

1. **Update version** in .csproj:
```xml
<Version>1.1.0</Version>
```

2. **Update CHANGELOG.md**

3. **Build and pack**:
```bash
dotnet pack -c Release
```

4. **Push update**:
```bash
dotnet nuget push nupkg/Gotik.Commerce.1.1.0.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## 📝 Recommended Approach

### For Immediate Testing (Today)

**Use Method 1**: Test within existing GOKCafe solution
- Add reference to Gotik.Commerce from GOKCafe.Web
- Test all API endpoints
- Verify Composer registration
- Test with real data

### For Public Release (This Week)

**Use Option 1**: Publish all dependencies
1. Prepare GOKCafe.Domain, Application, Infrastructure packages
2. Publish dependencies first
3. Publish Gotik.Commerce
4. Submit to Umbraco Marketplace
5. Announce on social media / blog

---

## ✅ Success Criteria

Before marking as "published successfully":

- [ ] Package available on NuGet.org
- [ ] Can install in fresh Umbraco site
- [ ] All API endpoints work
- [ ] Documentation is accurate
- [ ] No critical bugs reported
- [ ] Listed on Umbraco Marketplace (optional)

---

## 📞 Support Plan

After publishing, set up:

1. **GitHub Issues**: For bug reports and feature requests
2. **Documentation Site**: README + Wiki
3. **Email**: Support contact
4. **Community**: Umbraco forums presence

---

**Next Action**: Choose your approach and start testing or publishing!

**Recommended**: Test in existing solution first, then publish dependencies, then publish Gotik.Commerce.
