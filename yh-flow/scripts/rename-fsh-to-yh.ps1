#!/usr/bin/env pwsh
# rename-fsh-to-yh.ps1
# Renames all FSH (FullStackHero) namespaces and project references to YH (YH.Flow).
# This script is idempotent and can be re-run safely.
#
# Usage: pwsh scripts/rename-fsh-to-yh.ps1
# Run from the yh-flow/ root directory.

$ErrorActionPreference = "Stop"
$baseDir = Split-Path -Parent $PSScriptRoot
if (-not $baseDir) { $baseDir = "." }

Write-Host "=== FSH -> YH Rename Script ===" -ForegroundColor Cyan
Write-Host "Base directory: $baseDir"
Write-Host ""

# -------------------------------------------------------
# Phase 1: csproj file content replacements
# -------------------------------------------------------
Write-Host "[Phase 1] Replacing csproj file contents..." -ForegroundColor Yellow

$csprojFiles = Get-ChildItem -Path "$baseDir/src" -Filter "*.csproj" -Recurse
foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # General namespace prefix: FSH. -> YH.
    $content = $content -replace 'FSH\.', 'YH.'

    # Specific Host project renames (already covered by FSH. -> YH. above,
    # but listed here for clarity and idempotency)
    # FSH.Starter.Api -> YH.Flow.Api (covered by next phase)
    # FSH.Starter.DbMigrator -> YH.Flow.DbMigrator
    # FSH.Starter.AppHost -> YH.Flow.AppHost
    # FSH.Starter.Migrations.PostgreSQL -> YH.Flow.Migrations.PostgreSQL

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }
}

# Now fix the Starter -> Flow mapping in all csproj files (Phase 1b)
$csprojFiles2 = Get-ChildItem -Path "$baseDir/src" -Filter "*.csproj" -Recurse
foreach ($file in $csprojFiles2) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # YH.Starter.* -> YH.Flow.* (since Phase 1 already changed FSH -> YH)
    $content = $content -replace 'YH\.Starter\.', 'YH.Flow.'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated (Starter->Flow): $($file.FullName)"
    }
}

Write-Host "[Phase 1] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 2: C# source file namespace replacements
# -------------------------------------------------------
Write-Host "[Phase 2] Replacing C# source file namespaces..." -ForegroundColor Yellow

$csFiles = Get-ChildItem -Path "$baseDir/src" -Filter "*.cs" -Recurse
foreach ($file in $csFiles) {
    # Skip obj/ and bin/ directories
    if ($file.FullName -match '[\\/](obj|bin)[\\/]') { continue }

    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # namespace FSH. -> namespace YH.
    $content = $content -replace 'namespace FSH\.', 'namespace YH.'
    # using FSH. -> using YH.
    $content = $content -replace 'using FSH\.', 'using YH.'
    # typeof(FSH. -> typeof(YH.
    $content = $content -replace 'typeof\(FSH\.', 'typeof(YH.'
    # FSH.Starter. -> YH.Flow. (for any remaining string references)
    $content = $content -replace 'FSH\.Starter\.', 'YH.Flow.'
    # FSH. -> YH. (catch-all for any other FSH namespace references)
    $content = $content -replace 'FSH\.', 'YH.'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }
}

Write-Host "[Phase 2] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 3: Configuration file replacements
# -------------------------------------------------------
Write-Host "[Phase 3] Replacing configuration files..." -ForegroundColor Yellow

$configFiles = Get-ChildItem -Path "$baseDir/src" -Include "appsettings*.json","launchSettings.json","*.json" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' -and $_.Extension -eq '.json' }

foreach ($file in $configFiles) {
    if ($file.FullName -match '[\\/](obj|bin)[\\/]') { continue }

    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # Specific migration assembly references
    $content = $content -replace 'FSH\.Starter\.Migrations\.PostgreSQL', 'YH.Flow.Migrations.PostgreSQL'
    # General FSH. -> YH.
    $content = $content -replace 'FSH\.', 'YH.'
    # FSH-Starter -> YH-Flow (for any hyphenated references)
    $content = $content -replace 'FSH-Starter', 'YH-Flow'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }
}

Write-Host "[Phase 3] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 4: Solution file replacements
# -------------------------------------------------------
Write-Host "[Phase 4] Replacing solution files..." -ForegroundColor Yellow

$slnxFiles = Get-ChildItem -Path "$baseDir/src" -Filter "*.slnx" -Recurse
foreach ($file in $slnxFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # FSH.Starter -> YH.Flow (project names and paths)
    $content = $content -replace 'FSH\.Starter', 'YH.Flow'
    # FSH_Starter -> YH_Flow (underscored references if any)
    $content = $content -replace 'FSH_Starter', 'YH_Flow'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }

    # Rename the solution file itself
    if ($file.Name -match 'FSH') {
        $newName = $file.Name -replace 'FSH\.Starter', 'YH.Flow'
        $newPath = Join-Path $file.DirectoryName $newName
        if (-not (Test-Path $newPath)) {
            Rename-Item -Path $file.FullName -NewName $newName
            Write-Host "  Renamed: $($file.Name) -> $newName"
        }
    }
}

Write-Host "[Phase 4] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 5: Build property replacements (Directory.Build.props)
# -------------------------------------------------------
Write-Host "[Phase 5] Replacing build properties..." -ForegroundColor Yellow

$buildProps = Get-ChildItem -Path "$baseDir/src" -Filter "Directory.Build.props" -Recurse
foreach ($file in $buildProps) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # Authors and Company
    $content = $content -replace 'Mukesh Murugan', 'YHFlow'
    $content = $content -replace 'FullStackHero', 'YHFlow'
    # Package tags
    $content = $content -replace 'FSH;FullStackHero', 'YH;YHFlow'
    # Any remaining FSH references
    $content = $content -replace 'FSH', 'YH'
    # Repository URLs
    $content = $content -replace 'fullstackhero/dotnet-starter-kit', 'akinix-plane/yh-flow'
    $content = $content -replace 'fullstackhero\.net', 'yhflow.dev'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }
}

Write-Host "[Phase 5] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 6: AppHost.cs special handling
# -------------------------------------------------------
Write-Host "[Phase 6] Special handling for AppHost.cs..." -ForegroundColor Yellow

$appHostFiles = Get-ChildItem -Path "$baseDir/src/Host" -Filter "AppHost.cs" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' }

foreach ($file in $appHostFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $original = $content

    # Aspire generated project references (dot -> underscore in type names)
    $content = $content -replace 'Projects\.FSH_Starter_Api', 'Projects.YH_Flow_Api'
    $content = $content -replace 'Projects\.FSH_Starter_DbMigrator', 'Projects.YH_Flow_DbMigrator'
    $content = $content -replace 'Projects\.FSH_Starter_AppHost', 'Projects.YH_Flow_AppHost'
    $content = $content -replace 'Projects\.FSH_Starter_Migrations_PostgreSQL', 'Projects.YH_Flow_Migrations_PostgreSQL'

    # Resource names
    $content = $content -replace '"fsh-db"', '"yhflow-db"'
    $content = $content -replace '"fsh-uploads"', '"yhflow-uploads"'
    $content = $content -replace '"fsh-cache"', '"yhflow-cache"'

    # Comment references
    $content = $content -replace 'FSH\.Starter\.AppHost -> fsh-starter', 'YH.Flow.AppHost -> yh-flow'
    $content = $content -replace 'multiple FSH apps', 'multiple YH apps'

    # General FSH. -> YH. for any remaining references
    $content = $content -replace 'FSH\.Starter\.', 'YH.Flow.'
    $content = $content -replace 'FSH\.', 'YH.'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Updated: $($file.FullName)"
    }
}

Write-Host "[Phase 6] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Phase 7: Physical directory renames
# -------------------------------------------------------
Write-Host "[Phase 7] Renaming Host project directories..." -ForegroundColor Yellow

$hostDir = "$baseDir/src/Host"

$renames = @{
    "FSH.Starter.Api" = "YH.Flow.Api"
    "FSH.Starter.DbMigrator" = "YH.Flow.DbMigrator"
    "FSH.Starter.AppHost" = "YH.Flow.AppHost"
    "FSH.Starter.Migrations.PostgreSQL" = "YH.Flow.Migrations.PostgreSQL"
}

foreach ($oldName in $renames.Keys) {
    $oldPath = Join-Path $hostDir $oldName
    $newName = $renames[$oldName]
    $newPath = Join-Path $hostDir $newName

    if (Test-Path $oldPath) {
        if (Test-Path $newPath) {
            Write-Host "  SKIP (target exists): $oldName -> $newName" -ForegroundColor DarkYellow
        } else {
            Rename-Item -Path $oldPath -NewName $newName
            Write-Host "  Renamed: $oldName -> $newName"
        }
    } else {
        Write-Host "  SKIP (source not found): $oldName" -ForegroundColor DarkYellow
    }
}

# Also rename the .csproj files inside the renamed directories to match
Write-Host ""
Write-Host "[Phase 7b] Renaming csproj files to match directory names..." -ForegroundColor Yellow

$hostCsprojRenames = @{
    "YH.Flow.Api/FSH.Starter.Api.csproj" = "YH.Flow.Api/YH.Flow.Api.csproj"
    "YH.Flow.DbMigrator/FSH.Starter.DbMigrator.csproj" = "YH.Flow.DbMigrator/YH.Flow.DbMigrator.csproj"
    "YH.Flow.AppHost/FSH.Starter.AppHost.csproj" = "YH.Flow.AppHost/YH.Flow.AppHost.csproj"
    "YH.Flow.Migrations.PostgreSQL/FSH.Starter.Migrations.PostgreSQL.csproj" = "YH.Flow.Migrations.PostgreSQL/YH.Flow.Migrations.PostgreSQL.csproj"
}

foreach ($oldRel in $hostCsprojRenames.Keys) {
    $oldPath = Join-Path $hostDir $oldRel
    $newRel = $hostCsprojRenames[$oldRel]
    $newPath = Join-Path $hostDir $newRel

    if (Test-Path $oldPath) {
        if (Test-Path $newPath) {
            Write-Host "  SKIP (target exists): $oldRel -> $newRel" -ForegroundColor DarkYellow
        } else {
            Rename-Item -Path $oldPath -NewName (Split-Path $newRel -Leaf)
            Write-Host "  Renamed: $oldRel -> $newRel"
        }
    } else {
        Write-Host "  SKIP (source not found): $oldRel" -ForegroundColor DarkYellow
    }
}

Write-Host "[Phase 7] Done." -ForegroundColor Green
Write-Host ""

# -------------------------------------------------------
# Summary
# -------------------------------------------------------
Write-Host "=== Rename Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Verifying no residual FSH references..." -ForegroundColor Yellow

$residualCs = Get-ChildItem -Path "$baseDir/src" -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } |
    Select-String -Pattern 'FSH\.' -SimpleMatch

$residualCsproj = Get-ChildItem -Path "$baseDir/src" -Filter "*.csproj" -Recurse |
    Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } |
    Select-String -Pattern 'FSH\.' -SimpleMatch

if ($residualCs) {
    Write-Host "  WARNING: Found FSH. references in .cs files:" -ForegroundColor Red
    $residualCs | ForEach-Object { Write-Host "    $_" }
} else {
    Write-Host "  OK: No FSH. references in .cs files" -ForegroundColor Green
}

if ($residualCsproj) {
    Write-Host "  WARNING: Found FSH. references in .csproj files:" -ForegroundColor Red
    $residualCsproj | ForEach-Object { Write-Host "    $_" }
} else {
    Write-Host "  OK: No FSH. references in .csproj files" -ForegroundColor Green
}

Write-Host ""
Write-Host "Script completed successfully!" -ForegroundColor Green
