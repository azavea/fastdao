@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Assumes it is being run in the trunk dir.
rem %1 is a version number.

if "%1" equ "" (
    set version=Dev
) else (
    set version=%1
)

echo Zipping bin/Release dir.
set zipexe=\\lr02\7z\7z.exe
if not exist !zipexe! (
    echo Unable to access !zipexe!
    exit /b 1
)

if exist "releasebin" (
    rmdir /s /q releasebin
    if %errorlevel% neq 0 (
        echo ERROR: Unable to remove old release dir.
        exit /b 2
    )
)
mkdir releasebin
FOR /R "Azavea.Open.DAO\bin\Release" %%f IN (*.*) DO copy %%f "releasebin"
mkdir releasebin\CSV
FOR /R "Azavea.Open.DAO.CSV\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\CSV"
mkdir releasebin\Firebird
FOR /R "Azavea.Open.DAO.Firebird\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\Firebird"
mkdir releasebin\OleDb
FOR /R "Azavea.Open.DAO.OleDb\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\OleDb"
mkdir releasebin\PostgreSQL
FOR /R "Azavea.Open.DAO.PostgreSQL\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\PostgreSQL"
mkdir releasebin\SQLite
FOR /R "Azavea.Open.DAO.SQLite\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\SQLite"
mkdir releasebin\SQLServer
FOR /R "Azavea.Open.DAO.SQLServer\bin\Release" %%f IN (*.*) DO copy %%f "releasebin\SQLServer"


set filename=fastdao_!version!.zip
if exist !filename! (
    del !filename!
    if %errorlevel% neq 0 (
        echo ERROR: Unable to delete old release zipfile: !filename!
        exit /b 10
    )
)
"!zipexe!" a "!filename!" "./releasebin/*"
if %errorlevel% neq 0 (
    echo ERROR: Unable to zip release binaries.
    exit /b 11
)

:end
