@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Assumes it is being run in the trunk dir.
rem %1 is a version number.

if "%1" equ "" (
    echo You cannot release a dev build to sourceforge, only a real version.
    exit /b 1
)
set version=%1

rem Optionally %2 is the trunk directory.
if "%2" neq "" (
    pushd "%2"
)

set apidir=api_%version%

rem Hack: This just says "y" to the "I don't recognize this key" warning and quits.
\\lr01\putty\psftp.exe azavea@web.sourceforge.net < yes.txt
if %errorlevel% neq 0 (
    echo ^^^^^^ Ignore those errors, that was just to eliminate the key warning.
    %COMSPEC% /c
)

rem Hack: This just says "y" to the "I don't recognize this key" warning and quits.
\\lr01\putty\psftp.exe azavea@frs.sourceforge.net < yes.txt
if %errorlevel% neq 0 (
    echo ^^^^^^ Ignore those errors, that was just to eliminate the key warning.
    %COMSPEC% /c
)


echo Creating .htaccess file to make Index.html work as the default page for docs.
rem Hack: Sandcastle creates "Index.html" but sourceforge only uses "index.html"
rem       as a default.  So override with a .htaccess file.
if exist "%apidir%\.htaccess" del "%apidir%\.htaccess"
if %errorlevel% neq 0 (
    echo ERROR: Unable to remove old .htaccess file.
    exit /b 3
)
echo DirectoryIndex Index.html >> "%apidir%\.htaccess"

echo Building sftp command to put all the api docs.
echo Removing sftp command file if it exists...
if exist put.txt del put.txt
if %errorlevel% neq 0 (
    echo ERROR: Unable to remove old upload docs command file: put.txt
    exit /b 2
)

echo cd htdocs >> put.txt
call buildRecursivePutCommand.bat %apidir% put.txt >> put.txt
if %errorlevel% neq 0 (
    echo ERROR: Unable to build put command to put api docs.
    exit /b 4
)

echo Adding extra copy of presentation.css with correct capitalization.
rem Hack: Sandcastle creates "Presentation.css" but references "presentation.css".
rem       On Sourceforge (case sensitive file system) that doesn't work.  Copy it
rem       in case it is used both ways.
echo cd %apidir% >> put.txt
echo cd styles >> put.txt
echo lcd %apidir% >> put.txt
echo lcd styles >> put.txt
rem no copy in sftp, have to put again.
echo mv Presentation.css presentation.css >> put.txt
echo put Presentation.css >> put.txt

echo quit >> put.txt

echo Executing sftp upload...
\\lr01\putty\psftp.exe %sf_user%,fastdao@web.sourceforge.net -pw %sf_password% < put.txt

echo Building sftp command to put the binary release.
echo Removing sftp command file if it exists...
if exist put.txt del put.txt
if %errorlevel% neq 0 (
    echo ERROR: Unable to remove old upload docs command file: put.txt
    exit /b 2
)
echo cd /home/frs/project/f/fa/fastdao >> put.txt
echo cd releases >> put.txt
echo put FastDAO_%version%.zip >> put.txt
echo quit >> put.txt
echo Executing sftp upload...
\\lr01\putty\psftp.exe %sf_user%,fastdao@web.sourceforge.net -pw %sf_password% < put.txt

echo Creating release branch.
set releasebranch=release_%version%
git checkout -b %releasebranch%
if %errorlevel% neq 0 (
    echo ERROR: Unable to create new release branch.
    exit /b 8
)
echo %sf_password% | git push --repo=ssh://%sf_user%@fastdao.git.sourceforge.net/gitroot/fastdao/fastdao %releasebranch%
if %errorlevel% neq 0 (
    echo ERROR: Unable to push release branch up to sourceforge.
    exit /b 9
)

if "%2" neq "" (
    popd
)
