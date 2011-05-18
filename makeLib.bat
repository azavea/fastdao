@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Assumes it is being run in the root dir of the repository.
rem Optionally %1 is the root dir of the repository.
if "%1" neq "" (
    pushd "%1"
)

echo Copying binaries for the lib dir.

set libdir=lib

if exist "!libdir!" (
    rmdir /s /q !libdir!
    if %errorlevel% neq 0 (
        echo ERROR: Unable to remove old lib dir.
        exit /b 2
    )
)
mkdir "!libdir!"
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

:end

if "%1" neq "" (
    popd
)
