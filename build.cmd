@echo Off
set EnableNuGetPackageRestore=true

set config=%1
if "%config%" == "" (
   set config=Debug
)
set target=%2
if "%target%" == "" (
   set target=Test
) 
if not "%3" == "" (
   set target="%target%;%3"
) 
if not "%4" == "" (
   set target="%target%;%4"
) 
if not "%5" == "" (
   set target="%target%;%5"
) 
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild build.proj /p:Configuration="%config%" "/t:%target%"