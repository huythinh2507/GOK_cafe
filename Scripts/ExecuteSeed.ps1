$connectionString = "Server=tcp:gok-cafe-server.database.windows.net,1433;Initial Catalog=GOKCafeDB;Persist Security Info=False;User ID=gok_admin;Password=G0kC@f3#2024;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"

try {
    Write-Host "Connecting to database..."
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()

    Write-Host "Reading SQL script..."
    $sqlScript = Get-Content "$PSScriptRoot\SeedEvents.sql" -Raw

    Write-Host "Executing SQL script..."
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $command.CommandTimeout = 300
    $result = $command.ExecuteNonQuery()

    Write-Host "SQL script executed successfully!"
    Write-Host "Rows affected: $result"

    $connection.Close()
    Write-Host "Connection closed."
}
catch {
    Write-Host "Error executing SQL script: $_"
    Write-Host $_.Exception.Message
    exit 1
}
