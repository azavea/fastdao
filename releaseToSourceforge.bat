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

echo Creating .htaccess file to make Index.html work as the default page for docs.
rem Hack: Sandcastle creates "Index.html" but sourceforge only uses "index.html"
rem       as a default.  So override with a .htaccess file.
if exist "%apidir%\.htaccess" del "%apidir%\.htaccess"
if %errorlevel% neq 0 (
    echo ERROR: Unable to remove old .htaccess file.
    exit /b 3
)
echo DirectoryIndex Index.html >> "%apidir%\.htaccess"

echo Executing scp upload of the Sandcastle docs...
echo yes | \\lr01\putty\pscp.exe -i "C:\Documents and Settings\hudson\.ssh\id_rsa.ppk" -r %apidir% azaveaci,fastdao@web.sourceforge.net:htdocs
if %errorlevel% neq 0 (
    echo ERROR: Unable to copy up API docs to sourceforge.
    exit /b 3
)

echo Adding extra copy of presentation.css with correct capitalization.
rem Hack: Sandcastle creates "Presentation.css" but references "presentation.css".
rem       On Sourceforge (case sensitive file system) that doesn't work.  Copy it
rem       in case it is used both ways.
echo yes | \\lr01\putty\pscp.exe -i "C:\Documents and Settings\hudson\.ssh\id_rsa.ppk" %apidir%\styles\Presentation.css azaveaci,fastdao@web.sourceforge.net:htdocs\%apidir%\styles\presentation.css
if %errorlevel% neq 0 (
    echo ERROR: Unable to copy up presentation.css to sourceforge.
    exit /b 3
)

echo Executing scp upload of the release zipfile...
echo yes | \\lr01\putty\pscp.exe -i "C:\Documents and Settings\hudson\.ssh\id_rsa.ppk" FastDAO_%version%.zip azaveaci,fastdao@web.sourceforge.net:/home/frs/project/f/fa/fastdao

echo Creating release tag.
set releasetag=release_%version%
git remote rm origin
git remote add origin ssh://azaveaci@fastdao.git.sourceforge.net/gitroot/fastdao/fastdao
if %errorlevel% neq 0 (
    echo ERROR: Unable to set ssh origin to sourceforge.
    exit /b 9
)
git tag %releasetag%
if %errorlevel% neq 0 (
    echo ERROR: Unable to create new release tag.
    exit /b 8
)
git push
if %errorlevel% neq 0 (
    echo ERROR: Unable to push release tag up to sourceforge.
    exit /b 9
)

if "%2" neq "" (
    popd
)
