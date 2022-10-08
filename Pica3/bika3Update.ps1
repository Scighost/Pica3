try {
    Write-Host "哔咔 3 开发版下载脚本"
    Write-Host "注意：开发版基于最新的代码，不是正式发布的版本，可能存在功能缺失，Bug 频出的问题。" -ForegroundColor Yellow
    Write-Host "`n`n`n"
    Write-Host "当前路径：$(Get-Location)"
    if(![System.IO.File]::Exists('./7Zip4Powershell/7Zip4Powershell.psd1')) {
        Write-Host "下载解压模块"
        Invoke-WebRequest "https://os.scighost.com/common/tool/7Zip4Powershell.zip" -UseBasicParsing -OutFile "./7Zip4Powershell.zip" -ErrorAction Stop
        Expand-Archive -Path "./7Zip4Powershell.zip" -DestinationPath "./" -Force -ErrorAction Stop
        Remove-Item -Path "./7Zip4Powershell.zip" -Force -Recurse -ErrorAction Stop
    }
    Import-Module -Name "./7Zip4Powershell/7Zip4Powershell.psd1" -Force -ErrorAction Stop
    $null = New-Item "./temp" -ItemType "Directory" -Force -ErrorAction Stop
    $result = Invoke-WebRequest "https://os.scighost.com/pica3/build/bika3_latest_x64.7z" -UseBasicParsing -Method HEAD -ErrorAction Stop
    $version = $result.Headers['x-oss-meta-version']
    $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
    Write-Host "下载安装包 `(v$version`, $size MB`)"
    Invoke-WebRequest "https://os.scighost.com/pica3/build/bika3_latest_x64.7z" -UseBasicParsing -OutFile "./temp/bika3_latest.7z" -ErrorAction Stop
    Write-Host "开始解压"
    Expand-7Zip -ArchiveFileName "./temp/bika3_latest.7z" -TargetPath "./temp/" -ErrorAction Stop
    try {
        $null = Get-Process -Name bika3 -ErrorAction Stop
        Write-Host "bika3.exe 正在运行，等待进程退出" -ForegroundColor Yellow
        Wait-Process -Name bika3 -ErrorAction Stop
        Start-Sleep -Seconds 1
    } catch { }
    Write-Host "替换文件"
    Get-ChildItem -Path "./temp/bika3/*" -File | Move-Item -Destination "./" -Force -ErrorAction Stop
    Copy-Item -Path "./temp/bika3/*" -Destination "./" -Force -Recurse -ErrorAction Stop
    Write-Host "更新完成"
    Invoke-Item -Path "./bika3.exe" -ErrorAction Stop
    Write-Host "清理安装包"
    Remove-Item -Path "./temp" -Force -Recurse -ErrorAction Stop
    Start-Sleep -Seconds 1
} catch {
    Write-Host $_.Exception -ForegroundColor Red -BackgroundColor Black
    Write-Host "`n更新失败，可以从以下链接手动下载最新开发版：" -ForegroundColor Yellow
    Write-Host "https://os.scighost.com/pica3/build/bika3_latest_x64.7z`n" -ForegroundColor Yellow
    Write-Host "任意键退出"
    [Console]::ReadKey()
}