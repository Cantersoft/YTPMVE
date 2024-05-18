@echo off

:-------------------------------------
REM  --> Check for permissions
    IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
) ELSE (
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
)

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params= %*
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params:"=""%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------    

set /p vegasversion= Enter the version number for your Vegas installation (e.g. 18): 

SET "var="&for /f "delims=0123456789" %%i in ("%vegasversion%") do set var=%%i
if defined var (echo %vegasversion% is not a number! && pause && exit) else (echo Version: %vegasversion%)

set scripts_installation_directory= "%ProgramFiles%\VEGAS\VEGAS Pro %vegasversion%.0\Script Menu\YTPMVE_VEGAS"
set engine_installation_directory= "%ProgramFiles%\VEGAS\YTPMVE"

rmdir /s /q %scripts_installation_directory%
mkdir %scripts_installation_directory%

rmdir /s /q %engine_installation_directory%
mkdir %engine_installation_directory%

echo Y | xcopy YTPMVE.cs %scripts_installation_directory%
echo Y | xcopy YTPMVE.cs.config %scripts_installation_directory%
echo Y | xcopy Newtonsoft.Json.dll %scripts_installation_directory%

REM --> Modify YTPMVE.cs if Vegas is a version not supporting PitchSemis.
REM --> Modify YTPMVE.cs if Vegas is a version using the Sony namespace.

if %vegasversion% LSS 15 (
    echo:
    echo Warning! PitchSemis not supported in Vegas %vegasversion%.
    echo:
    FindAndReplace.vbs "%scripts_installation_directory%\YTPMVE.cs" "/*PitchSemis NOT SUPPORTED IN VEGAS 14*/" "/*"
    FindAndReplace.vbs "%scripts_installation_directory%\YTPMVE.cs" "/*END PitchSemis NOT SUPPORTED IN VEGAS 14*/" "*/"      

    if %vegasversion% LSS 13 (
        echo Changing namespace to Sony.Vegas.
        FindAndReplace.vbs "%scripts_installation_directory%\YTPMVE.cs" "using ScriptPortal.Vegas;" "using Sony.Vegas;"
    )    
)


echo Y | xcopy YTPMVE.py %engine_installation_directory%
echo Y | xcopy YTPMVE_UI.py %engine_installation_directory%
echo Y | xcopy hoof.ico %engine_installation_directory%
echo Y | xcopy YTPMVE_UI.exe %engine_installation_directory%
echo Done! If Vegas was open, please rescan the Script Menu.
pause