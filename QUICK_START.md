# Quick Start - Sharing Your NuGet Package

## 📦 Package Created Successfully!

**File**: `GOKCafe.Commerce.Package/nupkg/GOKCafe.Commerce.1.0.0.nupkg`

---

## 🚀 How to Share with Your Friend (3 Options)

### Option 1: Local Network (Fastest for Testing)
```bash
# Your friend adds your folder as a NuGet source:
dotnet nuget add source "D:\GOK_Cafe_BE\GOK_cafe\GOKCafe.Commerce.Package\nupkg" -n "GOKLocal"

# Then installs:
dotnet add package GOKCafe.Commerce --version 1.0.0 --source GOKLocal
```

### Option 2: Send the File
1. Send: `GOKCafe.Commerce.1.0.0.nupkg` (49KB)
2. Friend saves to a folder
3. Friend runs:
```bash
dotnet add package GOKCafe.Commerce --version 1.0.0 --source "C:\path\to\folder"
```

### Option 3: GitHub Packages (Best for Production)
```bash
# You push once:
dotnet nuget push GOKCafe.Commerce.Package/nupkg/GOKCafe.Commerce.1.0.0.nupkg \
  --source "https://nuget.pkg.github.com/huythinh2507/index.json" \
  --api-key YOUR_GITHUB_TOKEN

# Anyone can install:
dotnet add package GOKCafe.Commerce
```

---

## 📝 What to Tell Your Friend

"I've created a NuGet package with all the backend commerce logic. Here's what you get:"

✅ **Shopping Cart API** - Complete cart management  
✅ **Order Management API** - Full order processing  
✅ **Odoo Integration** - Product sync (handles millions)  
✅ **All DTOs** - Request/response models  
✅ **Easy Setup** - One line of code to register  

"Just install my package and call the APIs from your Umbraco frontend!"

---

## 💻 How Your Friend Uses It

### 1. Install Package
```bash
dotnet add package GOKCafe.Commerce --version 1.0.0
```

### 2. Add One Line to Program.cs
```csharp
builder.Services.AddGOKCafeCommerce(builder.Configuration);
```

### 3. Call APIs from Frontend
```javascript
// In their Umbraco frontend
fetch('/api/v1/cart/items', {
  method: 'POST',
  body: JSON.stringify({ productId, quantity })
})
```

Done! 🎉

---

## 📚 Files to Share

Send your friend:
1. `GOKCafe.Commerce.1.0.0.nupkg` - The package
2. `PACKAGE_USAGE_GUIDE.md` - Full documentation
3. This file - Quick reference

---

## 🔄 Updating the Package

When you make changes:

1. Update version in `GOKCafe.Commerce.Package.csproj`:
```xml
<Version>1.0.1</Version>
```

2. Rebuild:
```bash
cd GOKCafe.Commerce.Package
dotnet pack -c Release -o ./nupkg
```

3. Share new file with your friend

---

## ✅ Success Checklist

- [x] Package created
- [x] Build succeeded
- [x] Package files generated
- [x] Documentation written
- [ ] Share with friend
- [ ] Friend tests integration
- [ ] Deploy to production

---

## 🆘 Quick Troubleshooting

**Package not found?**
```bash
dotnet nuget list source  # Check sources
dotnet nuget add source "path" -n "name"  # Add again
```

**Version conflict?**
```bash
dotnet list package  # See installed versions
dotnet add package GOKCafe.Commerce --version 1.0.0  # Specify version
```

**Need help?**
- See: `PACKAGE_USAGE_GUIDE.md`
- GitHub: https://github.com/huythinh2507/GOK_cafe

---

That's it! You're ready to share your package! 🚀
