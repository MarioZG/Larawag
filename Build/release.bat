"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" build.xml /fl /target:ReleaseLatest -restore
echo off
if %errorlevel% gtr 0 goto error
echo on
"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" build.xml /target:CleanUp -restore
:error
pause
