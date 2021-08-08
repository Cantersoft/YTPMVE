set v=18

pyinstaller --onefile .\YTPMVE.py
move .\dist\YTPMVE.exe .
xcopy /y *.cs "%ProgramFiles%\VEGAS\VEGAS Pro %v%.0\Script Menu\YTPMVE"
xcopy /y YTPMVE.py "%ProgramFiles%\VEGAS\VEGAS Pro %v%.0\Script Menu\YTPMVE"
xcopy /y YTPMVE.exe "%ProgramFiles%\VEGAS\VEGAS Pro %v%.0\Script Menu\YTPMVE"