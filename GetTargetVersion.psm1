function Get-TargetVersion {
    param (
        $NuGetVersioningDllPath
    )
    $lastTag = git describe --abbrev=0 --tags
    if ([String]::IsNullOrWhiteSpace($lastTag)) {
        $lastTag = 'v0.1.0'
        $comitCount = git rev-list HEAD --count
    }
    else {
        $comitCount = git rev-list "$lastTag..HEAD" --count
    }
    if ($lastTag.StartsWith('v') -or $lastTag.StartsWith('V')) {
        $lastTag = $lastTag.SubString(1)
    }
    $null = [System.Reflection.Assembly]::LoadFrom($NuGetVersioningDllPath)
    [ref]$lastVer = [NuGet.Versioning.SemanticVersion]::Parse('0.1.0')
    if ([NuGet.Versioning.SemanticVersion]::TryParse($lastTag, $lastVer)) {
        if ($lastVer.Value.IsPrerelease) {
            $targetVer = "$lastVer-dev.$comitCount"
        }
        else {
            $targetVer = "$($lastVer.Value.Major).$($lastVer.Value.Minor).$($lastVer.Value.Pathch+1)-dev.$comitCount"
        }
    }
    else {
        $targetVer = "$lastVer-dev.$comitCount"
    }
    Write-Output $targetVer
}