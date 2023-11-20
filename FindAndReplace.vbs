' Get command line arguments
Set objArgs = WScript.Arguments
inputFile = objArgs(0)
findText = objArgs(1)
replaceText = objArgs(2)

' Read the input file
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.OpenTextFile(inputFile, 1)
fileContent = objFile.ReadAll
objFile.Close

' Perform find and replace
newContent = Replace(fileContent, findText, replaceText)

' Write the updated content back to the file
Set objFile = objFSO.OpenTextFile(inputFile, 2)
objFile.Write newContent
objFile.Close