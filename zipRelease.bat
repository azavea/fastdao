@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Assumes it is being run in the root dir of the repository.
rem %1 is a version number.

if "%1" equ "" (
    set version=Dev
) else (
    set version=%1
)
rem Optionally %2 is the root dir of the repository.
if "%2" neq "" (
    pushd "%2"
)

echo Zipping release.
set zipexe=\\lr01\7z\7z.exe
if not exist !zipexe! (
    echo Unable to access !zipexe!
    exit /b 1
)

set releasedir=release_temp
set libdir=!releasedir!\lib
set exampledir=!releasedir!\examples

if exist "!releasedir!" (
    rmdir /s /q !releasedir!
    if %errorlevel% neq 0 (
        echo ERROR: Unable to remove old release dir.
        exit /b 2
    )
)
mkdir "!releasedir!"
mkdir "!libdir!"
mkdir "!exampledir!"
FOR /R "Azavea.Open.DAO\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!"
mkdir !libdir!\CSV
FOR /R "Azavea.Open.DAO.CSV\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\CSV"
mkdir !libdir!\Firebird
FOR /R "Azavea.Open.DAO.Firebird\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\Firebird"
mkdir !libdir!\OleDb
FOR /R "Azavea.Open.DAO.OleDb\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\OleDb"
mkdir !libdir!\PostgreSQL
FOR /R "Azavea.Open.DAO.PostgreSQL\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\PostgreSQL"
mkdir !libdir!\SQLite
FOR /R "Azavea.Open.DAO.SQLite\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\SQLite"
mkdir !libdir!\SQLServer
FOR /R "Azavea.Open.DAO.SQLServer\bin\Release" %%f IN (*.*) DO copy "%%f" "!libdir!\SQLServer"

rem copy "examples\*.sln" "!exampledir!"
FOR /D %%d IN (examples\*) DO (
    mkdir "!exampledir!\%%~nd"
    copy "%%d\*.sln" "!exampledir!\%%~nd"
    copy "%%d\*.cs" "!exampledir!\%%~nd"
    copy "%%d\*.xml" "!exampledir!\%%~nd"
    copy "%%d\*.csproj" "!exampledir!\%%~nd"
    copy "%%d\Properties" "!exampledir!\%%~nd"
)

set filename=FastDAO_!version!.zip
if exist !filename! (
    del !filename!
    if %errorlevel% neq 0 (
        echo ERROR: Unable to delete old release zipfile: !filename!
        exit /b 10
    )
)
"!zipexe!" a "!filename!" "./!releasedir!/*"
if %errorlevel% neq 0 (
    echo ERROR: Unable to zip release binaries.
    exit /b 11
)

:end

if "%2" neq "" (
    popd
)
