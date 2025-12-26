# Run SQL scripts through the application's database context
param(
    [string]$ScriptPath = "CreateEventTables.sql"
)

Write-Host "Reading SQL script: $ScriptPath"
$sqlContent = Get-Content "$PSScriptRoot\$ScriptPath" -Raw

# Split by GO statements
$batches = $sqlContent -split '\r?\nGO\r?\n'

Write-Host "Found $($batches.Count) SQL batches to execute"

# Change to API directory
Set-Location "$PSScriptRoot\..\GOKCafe.API"

foreach ($batch in $batches) {
    if ($batch.Trim() -ne "") {
        Write-Host "`nExecuting batch..."
        $escapedSql = $batch.Replace('"', '\"').Replace("'", "''")

        # Use dotnet ef to execute SQL
        $command = "dotnet ef dbcontext info --project ..\GOKCafe.Infrastructure --startup-project ."
        Invoke-Expression $command

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Connection successful"
            break
        }
    }
}

Write-Host "`nDone!"
