@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem %1 - the folder to process.
rem %2 - the output file.

rem Create the remote dir.
echo mkdir %1
rem CD to it both locally and remotely.
echo cd %1
echo lcd %1
rem Put all the files in the dir.
echo mput *

rem Now go into that dir.
pushd "%1"

rem For each subdir, recurse.
for /D %%D in (*) do (
    if "%%D" neq "." (
        call "%~f0" "%%D" "%2"
        if %errorlevel% neq 0 (
            echo ERROR: Unable to recurse into "%%D".
            exit /b 1
        )
    )
)

rem CD back up to the parent dir.
echo cd ..
echo lcd ..

popd
