@echo off

SET major=0
SET minor=0
SET build=0
SET revision=0

FOR /F "tokens=1-4 delims=." %%A IN ("%1") DO (
    IF NOT [%%A] == [] SET major=%%A
    IF NOT [%%B] == [] SET minor=%%B
    IF NOT [%%C] == [] SET build=%%C
    IF NOT [%%D] == [] SET revision=%%D
)

set version=%major%.%minor%.%build%.%revision%
echo Setting assembly version to %version%

call batchSubstitute.bat "@ASSEMBLYVERSION@" "%version%" Azavea.Open.DAO\templates\ProductAssemblyInfo.cs > Azavea.Open.DAO\temp
call batchSubstitute.bat "@YEAR@" "%DATE:~-4%" Azavea.Open.DAO\temp > Azavea.Open.DAO\ProductAssemblyInfo.cs
del Azavea.Open.DAO\temp
