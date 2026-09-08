@echo off
rem Puts a shortcut to Statistle.exe (in this folder) on the desktop. No admin rights needed.
setlocal
set "HERE=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$s = (New-Object -ComObject WScript.Shell).CreateShortcut([IO.Path]::Combine([Environment]::GetFolderPath('Desktop'), 'Statistle.lnk'));" ^
  "$s.TargetPath = '%HERE%Statistle.exe'; $s.WorkingDirectory = '%HERE%'; $s.Description = 'Statistle - Statistics for engineers'; $s.Save();" ^
  "Write-Host 'Shortcut created on the desktop.'"
pause
