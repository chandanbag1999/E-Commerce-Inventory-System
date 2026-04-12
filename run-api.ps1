# Kill any existing dotnet processes on port 5275
$port = 5275
$pid = (Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue).OwningProcess
if ($pid) {
    Write-Host "Killing existing process on port $port (PID: $pid)..."
    Stop-Process -Id $pid -Force
    Start-Sleep -Seconds 1
}

# Build and run
Write-Host "Starting EIVMS API..."
dotnet run --project src/EIVMS.API
