$apiPid = 7096
$allDotnet = Get-Process dotnet -ErrorAction SilentlyContinue
foreach ($p in $allDotnet) {
    if ($p.Id -ne $apiPid) {
        Stop-Process -Id $p.Id -Force
        Write-Host "Killed PID: $($p.Id)"
    }
}
Write-Host "Done. Kept PID $apiPid (EIVMS API)"
