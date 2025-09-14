"MSBuild.exe" build.xml /fl /target:ReleaseLatest -restore
echo off
if %errorlevel% gtr 0 goto error
echo on
"MSBuild.exe" build.xml /target:CleanUp -restore
:error
pause
