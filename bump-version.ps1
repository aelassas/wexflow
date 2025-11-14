param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Version
)

# Convert short version to full versions
$FullVersion = if ($Version -match "^\d+\.\d+$") { "$Version.0" } else { $Version }
$FileVersion = if ($Version -match "^\d+\.\d+$") { "$Version.0.0" } else { "$Version.0.0" }

Write-Host "Updating Wexflow version to $Version..."

# Base directory of the script
$BaseDir = $PSScriptRoot

# ---------------------------------------------------------
# 1. Update all *.csproj files
# ---------------------------------------------------------

Get-ChildItem -Path $BaseDir -Recurse -Filter *.csproj |
Where-Object {
    # Exclude:
    #   src\admin\**
    #   src\net\**
    #   samples\**
    #   tests\net\**
    $_.FullName -notmatch '\\src\\admin\\' -and
    $_.FullName -notmatch '\\src\\net\\' -and
    $_.FullName -notmatch '\\samples\\' -and
    $_.FullName -notmatch '\\tests\\net\\'
} | ForEach-Object {

    $csproj = $_.FullName
    $content = [System.IO.File]::ReadAllText($csproj)

    $content = [regex]::Replace(
        $content,
        '(<Version>)[^<]*(</Version>)',
        { param($m) "$($m.Groups[1].Value)$FullVersion$($m.Groups[2].Value)" }
    )

    [System.IO.File]::WriteAllText($csproj, $content)
    Write-Host "Updated $csproj"
}

# ---------------------------------------------------------
# 2. Update Swagger manifests
# ---------------------------------------------------------
$swaggerManifests = @(
    (Join-Path $BaseDir "src\netcore\Wexflow.Server\swagger-ui\swagger.yaml"),
    (Join-Path $BaseDir "src\net\Wexflow.Server\swagger-ui\swagger.yaml")
)

$swaggerManifests | ForEach-Object {
    $swaggerManifest = $_
    $content = [System.IO.File]::ReadAllText($swaggerManifest)

    $content = [regex]::Replace(
        $content,
        'version:\s*(\d+\.\d+)',
        { param($m) "version: $Version" }
    )

    [System.IO.File]::WriteAllText($swaggerManifest, $content)
    Write-Host "Updated $swaggerManifest"
}

# ---------------------------------------------------------
# 3. Update all .bat scripts
# ---------------------------------------------------------
$scripts = @(
    (Join-Path $BaseDir "setup\setup-windows-netcore.bat"),
    (Join-Path $BaseDir "setup\setup-linux-netcore.bat"),
    (Join-Path $BaseDir "setup\setup-macos-netcore.bat")
)

$scripts | ForEach-Object {
    $script = $_
    $content = [System.IO.File]::ReadAllText($script)

    $content = [regex]::Replace(
        $content,
        'set version=(\d+\.\d+)',
        { param($m) "set version=$Version" }
    )

    [System.IO.File]::WriteAllText($script, $content)
    Write-Host "Updated $script"
}

# ---------------------------------------------------------
# 4. Update AssemblyInfo.cs
# ---------------------------------------------------------
Get-ChildItem -Path $BaseDir -Recurse -Filter AssemblyInfo.cs |
Where-Object {
    # Exclude:
    #   src\admin
    #   src\netcore\**
    #   samples\**
    $_.FullName -notmatch '\\src\\admin\\' -and
    $_.FullName -notmatch '\\src\\netcore\\' -and
    $_.FullName -notmatch '\\samples\\'
} | ForEach-Object {
    $assemblyInfo = $_.FullName
    $content = [System.IO.File]::ReadAllText($assemblyInfo)

    $content = [regex]::Replace(
        $content,
        '(\[assembly:\s*AssemblyVersion\(")[^"]*("\)\])',
        { param($m) "$($m.Groups[1].Value)$FileVersion$($m.Groups[2].Value)" }
    )

    $content = [regex]::Replace(
        $content,
        '(\[assembly:\s*AssemblyFileVersion\(")[^"]*("\)\])',
        { param($m) "$($m.Groups[1].Value)$FileVersion$($m.Groups[2].Value)" }
    )

    [System.IO.File]::WriteAllText($assemblyInfo, $content)
    Write-Host "Updated $assemblyInfo"
}

# ---------------------------------------------------------
# 5. Update Wexflow Manager .resx
# ---------------------------------------------------------
$managerResx = (Join-Path $BaseDir "src\net\Wexflow.Clients.Manager\Manager.resx")
$managerContent = [System.IO.File]::ReadAllText($managerResx)

$managerContent = [regex]::Replace(
    $managerContent,
    'Version\s*(\d+\.\d+)\s*\r?\n?©',
    { param($m) "Version $Version`r`n©" }
)

[System.IO.File]::WriteAllText($managerResx, $managerContent)
Write-Host "Updated $managerResx"

# ---------------------------------------------------------
# 6. Update Inno Setup .iss files
# ---------------------------------------------------------
$innoScripts = @(
    (Join-Path $BaseDir "setup\setup-windows-x64.iss"),
    (Join-Path $BaseDir "setup\setup-windows-x86.iss")
)

$innoScripts | ForEach-Object {
    $innoScript = $_
    $content = [System.IO.File]::ReadAllText($innoScript)

    $content = [regex]::Replace(
        $content,
        '#define MyAppVersion "(\d+\.\d+)"',
        { param($m) "#define MyAppVersion `"$Version`"" }
    )

    [System.IO.File]::WriteAllText($innoScript, $content)
    Write-Host "Updated $innoScript"
}

Write-Host "All version updates complete."
