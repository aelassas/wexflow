function Touch-CsprojFiles {
    param (
        [Parameter(Mandatory = $true)]
        [string]$ProjectRoot
    )

    if (-Not (Test-Path $ProjectRoot)) {
        Write-Error "Path '$ProjectRoot' does not exist."
        return
    }

    Get-ChildItem -Path $ProjectRoot -Directory | ForEach-Object {
        $csproj = Get-ChildItem -Path $_.FullName -Filter *.csproj -File
        if ($csproj) {
            Write-Host "Touching $($csproj.FullName)..."
            $content = Get-Content $csproj.FullName
            Set-Content $csproj.FullName $content
        } else {
            Write-Warning "No .csproj found in $($_.FullName)"
        }
    }
}

# Reload Intellisense for all .csproj files in the .NET 4.8 projects
Touch-CsprojFiles -ProjectRoot ".\net"
