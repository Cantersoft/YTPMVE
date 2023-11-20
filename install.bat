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

echo Enter the version number for your Vegas installation (e.g. 18)
set /p vegasversion=

SET "var="&for /f "delims=0123456789" %%i in ("%vegasversion%") do set var=%%i
if defined var (echo %vegasversion% is not a number! && pause && exit) else (echo Version: %vegasversion%)

echo Y | xcopy YTPMVE.cs "%ProgramFiles%\VEGAS\VEGAS Pro %vegasversion%.0\Script Menu\"

REM --> Modify YTPMVE.cs if Vegas is a version using the Sony namespace.
if %vegasversion% LSS 13 (
	FindAndReplace.vbs "%ProgramFiles%\VEGAS\VEGAS Pro %vegasversion%.0\Script Menu\YTPMVE.cs" "using ScriptPortal.Vegas;" "using Sony.Vegas;"
)

rmdir /s /q "%ProgramFiles%\VEGAS\YTPMVE"
mkdir "%ProgramFiles%\VEGAS\YTPMVE"

echo Y | xcopy YTPMVE.py "%ProgramFiles%\VEGAS\YTPMVE\"
echo Y | xcopy YTPMVE.exe "%ProgramFiles%\VEGAS\YTPMVE\"
echo Done! If Vegas was open, please rescan the script menu folder.
pause