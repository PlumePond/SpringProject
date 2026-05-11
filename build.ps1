# ============================================================
#  MonoGame Multi-Platform Build Script
#  Usage:  ./build.ps1
#          ./build.ps1 -Version 2.1.0
# ============================================================

param (
    [string]$Version    = "",
    [string]$CsprojName = "SpringProject",   # matches SpringProject.csproj
    [string]$ExportName = "spring_project",  # used in output folder/zip names
    [string]$ProjectDir = "."
)

# -- 1. Resolve version --------------------------------------
$VersionFile = Join-Path $PSScriptRoot "version.txt"

if ($Version -ne "") {
    $ResolvedVersion = $Version.Trim()
} elseif (Test-Path $VersionFile) {
    $ResolvedVersion = (Get-Content $VersionFile -Raw).Trim()
} else {
    Write-Error "No version specified and version.txt not found."
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Yellow
Write-Host "  Building $ExportName $ResolvedVersion" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow
Write-Host ""

# -- 2. Platform RIDs to build -------------------------------
$RIDs = @(
    "win-x64",
    "linux-x64",
    "linux-arm64",
    "osx-x64",
    "osx-arm64"
)

$csprojPath = Join-Path $ProjectDir "$CsprojName.csproj"
if (-not (Test-Path $csprojPath)) {
    Write-Error "Could not find $csprojPath - check -CsprojName and -ProjectDir"
    exit 1
}

$Results = @()

foreach ($RID in $RIDs) {
    # e.g. "spring_project 1.0.0 win-x64"
    $FolderName = "$ExportName $ResolvedVersion $RID"
    $PublishDir = Join-Path $ProjectDir "bin/Release/net9.0/$FolderName"

    Write-Host ">> $RID..." -ForegroundColor White

    dotnet publish "$csprojPath" -c Release -r $RID --self-contained true /p:PublishSingleFile=true /p:Version=$ResolvedVersion -o "$PublishDir"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "   FAILED (code $LASTEXITCODE)" -ForegroundColor Red
        $Results += [PSCustomObject]@{ RID=$RID; Status="FAILED"; Output="-" }
        continue
    }

    # Zip the publish folder, naming the zip to match
    $ZipPath = Join-Path $ProjectDir "bin/Release/net9.0/$FolderName.zip"
    if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }

    Compress-Archive -Path "$PublishDir/*" -DestinationPath $ZipPath -CompressionLevel Optimal

    Write-Host "   OK: $FolderName.zip" -ForegroundColor Green
    $Results += [PSCustomObject]@{ RID=$RID; Status="OK"; Output="$FolderName.zip" }
}

# -- 3. Summary ----------------------------------------------
Write-Host ""
Write-Host "======================================" -ForegroundColor Yellow
Write-Host "  Summary" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow
$Results | Format-Table -AutoSize

$Failed = $Results | Where-Object { $_.Status -eq "FAILED" }
if ($Failed) {
    Write-Host "WARNING: $($Failed.Count) platform(s) failed." -ForegroundColor Red
    exit 1
} else {
    Write-Host "All platforms built successfully!" -ForegroundColor Green
}