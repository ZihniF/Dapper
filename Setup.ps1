$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot

try {
    dotnet build Dapper.slnx

    if ($LASTEXITCODE -ne 0) {
        throw 'Proje derlenemedi.'
    }

    dotnet run --project WebUI --no-build -- --initialize

    if ($LASTEXITCODE -ne 0) {
        throw 'SQL Server kurulumu tamamlanamadı. SSMS bağlantınızı ve appsettings.json ayarını kontrol edin.'
    }

    Write-Host 'Kurulum tamamlandı. Uygulamayı başlatmak için:'
    Write-Host 'dotnet run --project WebUI --no-build --launch-profile http'
}
finally {
    Pop-Location
}
