try {
    Write-Host "哔咔 3 开发版下载脚本"
    Write-Host "注意：开发版基于最新的代码，不是正式发布版，可能存在功能缺失，Bug 频出的问题。" -ForegroundColor Yellow
    Write-Host "`n`n`n"
    Write-Host "当前路径：$(Get-Location)"
    $null = New-Item ./temp -ItemType Directory -Force -ErrorAction Stop
    $result = Invoke-WebRequest https://os.scighost.com/pica3/build/bika3_latest_x64.zip -Method HEAD -ErrorAction Stop
    $version = $result.Headers['x-oss-meta-version']
    $size = ($result.Headers['Content-Length']/1024/1024).ToString('F2')
    Write-Host "下载安装包（版本：$version，大小：$size MB）"
    Invoke-WebRequest https://os.scighost.com/pica3/build/bika3_latest_x64.zip -OutFile ./temp/bika3_latest.zip -ErrorAction Stop
    Write-Host "开始解压"
    Expand-Archive -Path ./temp/bika3_latest.zip -DestinationPath ./temp/ -Force -ErrorAction Stop
    try {
        $null = Get-Process -Name bika3 -ErrorAction Stop
        Write-Host "bika3.exe 正在运行，等待进程退出" -ForegroundColor Yellow
        Wait-Process -Name bika3 -ErrorAction Stop
        Start-Sleep -Seconds 1
    } catch { }
    Write-Host "替换文件"
    Get-ChildItem -Path ./temp/bika3/* -File | Move-Item -Destination ./ -Force -ErrorAction Stop
    Copy-Item -Path ./temp/bika3/* -Destination ./ -Force -Recurse -ErrorAction Stop
    Write-Host "清理安装包"
    Remove-Item -Path ./temp -Force -Recurse -ErrorAction Stop
    Write-Host "更新完成"
    Invoke-Item -Path ./bika3.exe -ErrorAction Stop
    Start-Sleep -Seconds 2
} catch {
    Write-Host $_.Exception -ForegroundColor Red -BackgroundColor Black
    Write-Host "任意键退出"
    [Console]::ReadKey()
}
