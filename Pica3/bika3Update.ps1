try {
    $ErrorActionPreference = 'Stop'
    $ProgressPreference = 'SilentlyContinue'
    Write-Host "哔咔 3 开发版更新脚本"
    Write-Host "注意：开发版基于最新的代码，不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。" -ForegroundColor Yellow
    Write-Host "当前路径：$(Get-Location)"
    if(![System.IO.File]::Exists('./bika3.exe')) {
        Write-Host "没有找到已安装的版本" -ForegroundColor Yellow
        $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
        if($archi -eq 5) {
            $fn = 'bika3_latest_arm64.7z'
        } else {
            $fn = 'bika3_latest_x64.7z'
        }
        $result = Invoke-WebRequest "https://os.scighost.com/pica3/build/$fn" -UseBasicParsing -Method HEAD
        $version = $result.Headers['x-oss-meta-version']
        $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
        Write-Host "下载安装包 `(v$version`, $size MB`)"
        Write-Host "是否显示下载进度条（显示进度条会减慢下载速度）"
        Write-Host '[Y] 是(Y) ' -NoNewline
        Write-Host ' [N] 否(N)' -NoNewline -ForegroundColor Yellow
        Write-Host ' (默认值为 N ):' -NoNewline
        $read = Read-Host
        if($read -eq 'Y' -or $read -eq 'y') {
            $ProgressPreference = 'Continue'
        }
        Write-Host "下载中，请稍等。。。"
        Invoke-WebRequest "https://os.scighost.com/pica3/build/$fn" -UseBasicParsing -OutFile "./$fn"
        Write-Host "下载完成，文件名：$fn"
        Write-Host "任意键退出"
        [Console]::ReadKey()
        Exit
    }
    if(![System.IO.File]::Exists('./temp/bika3_latest.7z')) {
        Write-Host "安装包不存在" -ForegroundColor Yellow
        $null = New-Item "./temp" -ItemType "Directory" -Force
        $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
        if($archi -eq 5) {
            $url = 'https://os.scighost.com/pica3/build/bika3_latest_arm64.7z'
        } else {
            $url = 'https://os.scighost.com/pica3/build/bika3_latest_x64.7z'
        }
        $result = Invoke-WebRequest -Uri $url -UseBasicParsing -Method HEAD
        $version = $result.Headers['x-oss-meta-version']
        $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
        Write-Host "下载安装包 `(v$version`, $size MB`)"
        Write-Host "是否显示下载进度条（显示进度条会减慢下载速度）"
        Write-Host '[Y] 是(Y) ' -NoNewline
        Write-Host ' [N] 否(N)' -NoNewline -ForegroundColor Yellow
        Write-Host ' (默认值为 N ):' -NoNewline
        $read = Read-Host
        if($read -eq 'Y' -or $read -eq 'y') {
            $ProgressPreference = 'Continue'
        }
        Write-Host "下载中，请稍等。。。"
        Invoke-WebRequest -Uri $url -UseBasicParsing -OutFile "./temp/bika3_latest.7z"
    }
    if(![System.IO.File]::Exists('./7Zip4Powershell/7Zip4Powershell.psd1')) {
        Write-Host "下载解压模块"
        Invoke-WebRequest "https://os.scighost.com/common/tool/7Zip4Powershell.zip" -UseBasicParsing -OutFile "./7Zip4Powershell.zip"
        Expand-Archive -Path "./7Zip4Powershell.zip" -DestinationPath "./" -Force
        Remove-Item -Path "./7Zip4Powershell.zip" -Force -Recurse
    }
    Import-Module -Name "./7Zip4Powershell/7Zip4Powershell.psd1" -Force
    Write-Host "开始解压"
    Expand-7Zip -ArchiveFileName "./temp/bika3_latest.7z" -TargetPath "./temp/"
    try {
        $null = Get-Process -Name bika3
        Write-Host "bika3.exe 正在运行，等待进程退出" -ForegroundColor Yellow
        Wait-Process -Name bika3
        Start-Sleep -Seconds 1
    } catch { }
    Write-Host "替换文件"
    Get-ChildItem -Path "./temp/bika3/*" -File | Move-Item -Destination "./" -Force
    Copy-Item -Path "./temp/bika3/*" -Destination "./" -Force -Recurse
    Write-Host "更新完成"
    Invoke-Item -Path "./bika3.exe"
    Write-Host "清理安装包"
    Remove-Item -Path "./temp" -Force -Recurse
    Start-Sleep -Seconds 1
} catch {
    Write-Host $_.Exception -ForegroundColor Red -BackgroundColor Black
    $archi = (Get-WmiObject WIN32_PROCESSOR).Architecture
    if($archi -eq 5) {
        $url = 'https://os.scighost.com/pica3/build/bika3_latest_arm64.7z'
    } else {
        $url = 'https://os.scighost.com/pica3/build/bika3_latest_x64.7z'
    }
    Write-Host "`n更新失败，请解压并替换「安装路径/temp」文件夹中的安装包，或重新下载最新开发版：" -ForegroundColor Yellow
    Write-Host "$url`n" -ForegroundColor Yellow
    Write-Host "任意键退出"
    [Console]::ReadKey()
}