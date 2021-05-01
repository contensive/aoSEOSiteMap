
rem echo off

rem build and deliver to deployment folder

call build.cmd

rem upload to contensive application
c:
cd %collectionPath%
cc -a %appName% --installFile "%collectionName%.zip"
cd ..\..\scripts