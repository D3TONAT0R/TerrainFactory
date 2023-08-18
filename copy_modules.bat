:: %1 = $(TargetDir)
:: %2 = $(SolutionDir)
:: %3 = $(Configuration)
:: %4 = $(ProjectName)

set target=%1
set solution=%2
set config=%3
set name=%4
set dest=%solution%TerrainFactoryConsole\bin\%config%\netcoreapp3.1\%name%

echo Copying DLLs for module %name%...
echo Destination is %dest%
xcopy "%target%\*.*" %dest% /Y /I /E

echo Generating module.info file...
@echo off
echo %name% > %dest%\module.info