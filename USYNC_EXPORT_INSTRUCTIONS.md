# uSync Export Instructions - Add Document Types to Package

We need to export your existing document types from GOKCafe.Web so they can be included in the Gotik.Commerce package v1.1.0.

## Step 1: Run GOKCafe.Web

```bash
cd d:\GOK_Cafe_BE\GOK_cafe\GOKCafe.Web
dotnet run
```

## Step 2: Access uSync Dashboard

1. Open browser: **https://localhost:44317/umbraco**
2. Log in to Umbraco backoffice
3. Look for **Settings** section in the left menu
4. Find **uSync** dashboard (should be there now that we installed it)

## Step 3: Export Document Types

In the uSync dashboard:

1. Click on **"Export"** or **"Full Export"**
2. Select these items to export:
   - ✅ **Document Types** (all product-related)
   - ✅ **Data Types**
   - ✅ **Templates** (if any)
3. Click **"Export"** button

This will create files in: `GOKCafe.Web/uSync/v9/`

## Step 4: Alternative - Manual uSync Command

If uSync dashboard doesn't appear, you can trigger export via URL:

```
https://localhost:44317/umbraco/backoffice/uSync/uSyncDashboardApi/Export
```

Or check if there's a uSync section in Settings.

## What Will Be Exported

Based on your screenshot, these document types should be exported:
- Product Page
- Product List Page
- Category Page
- Homepage (if needed)
- Any related data types

## After Export

Once you've exported, the files will be in:
```
GOKCafe.Web/uSync/v9/DocumentTypes/
GOKCafe.Web/uSync/v9/DataTypes/
GOKCafe.Web/uSync/v9/Templates/
```

Then I can:
1. Copy these to Gotik.Commerce/uSync/
2. Update package to v1.1.0
3. Rebuild and republish to NuGet

---

## Quick Check After Export

Run this command to verify files were created:

```bash
ls -la GOKCafe.Web/uSync/v9/DocumentTypes/
```

You should see `.config` files for each document type!

Let me know when you've completed the export and I'll continue with the rest of the process.
