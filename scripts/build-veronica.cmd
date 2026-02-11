
rem echo off

rem build and deliver to deployment folder

set appName=veronica

call build.cmd

rem upload to application
c:
cd %collectionPath%
cc -a %appName% --installFile "%collectionName%.zip"
cd ..\..\scripts