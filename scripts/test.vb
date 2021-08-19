'@INCLUDE=vbs/GetTaniumDir.vbs
'@INCLUDE=vbs/ReadFindingsDatesFile.vbs
'@START_INCLUDES_HERE
'------------ INCLUDES START - Do not edit between this line and INCLUDE ENDS -----
'- Begin file: vbs/GetTaniumDir.vbs
'@SAFELIBINCLUDE
Function GetTaniumDir(strSubDir, sh)
    On Error Resume Next
    GetTaniumDir=Eval("sh.Environment(""Process"")(""TANIUM_CLIENT_ROOT"")") : Err.Clear
    If GetTaniumDir="" Then GetTaniumDir=Eval("sh.RegRead(""HKLM\Software\Tanium\Tanium Client\Path"")") : Err.Clear
    If GetTaniumDir="" Then GetTaniumDir=Eval("sh.RegRead(""HKLM\Software\Wow6432Node\Tanium\Tanium Client\Path"")") : Err.Clear
    If GetTaniumDir="" Then GetTaniumDir=Eval("sh.RegRead(""HKLM\Software\McAfee\Real Time"")") : Err.Clear
    If GetTaniumDir="" Then GetTaniumDir=Eval("sh.RegRead(""HKLM\Software\Wow6432Node\McAfee\Real Time\Path"")") : Err.Clear
    If GetTaniumDir="" Then Err.Clear: Exit Function
    On Error Goto 0

    If Not Right(GetTaniumDir, 1) = "\" Then
        GetTaniumDir = GetTaniumDir & "\"
    End If

    GetTaniumDir = GetTaniumDir & strSubDir & "\"
    GetTaniumDir = Replace(GetTaniumDir, "\\", "\")
End Function

Function GetComplyDir(strSubDir, sh)
    GetComplyDir = GetTaniumDir("Tools\Comply\" & strSubDir, sh)
End Function

Function GetComplyPath(strSubPath, sh)
    GetComplyPath = GetTaniumDir("Tools\Comply\", sh) & strSubPath
End Function
'- End file: vbs/GetTaniumDir.vbs
'- Begin file: vbs/ReadFindingsDatesFile.vbs
' Expects Set fs = CreateObject("Scripting.FileSystemObject") to have been called
Function FormatTime(timeString)
    If timeString = "" Then
        FormatTime = ""
    Else
        FormatTime = Mid(timeString, 1, 4) & "-" & Mid(timeString, 5, 2) & "-" & Mid(timeString, 7, 2)
    End If
End Function

Function ReadFindingsDatesFile(findingsDatesFilePath)
    Dim findingsDates
    Set findingsDates = CreateObject("Scripting.Dictionary")
    If fs.FileExists(findingsDatesFilePath) Then
        Dim inputFile, fileReadLine, findingId, firstFound, lastFound
        Set inputFile = fs.OpenTextFile(findingsDatesFilePath)
        ' Read input, split on | and populate two-element arrays with first found/last found.  Use findingsId as key to
        ' the dictionary.
        Do While Not inputFile.AtEndOfStream
            fileReadLine = Split(inputFile.ReadLine, "|")
            findingId = fileReadLine(0)
            firstFound = fileReadLine(1)
            lastFound = fileReadLine(2)
            findingsDates.Add findingId, Array(firstFound, lastFound)
        Loop
        inputFile.Close
    End If
    Set ReadFindingsDatesFile = findingsDates
End Function
'- End file: vbs/ReadFindingsDatesFile.vbs
'------------ INCLUDES END - Do not edit above this line and INCLUDE STARTS -----

Const ForReading = 1
Dim fs, sh, complianceFile, vulnerabilityFile, findingsDatesFile

Set fs = CreateObject("Scripting.FileSystemObject")
Set sh = CreateObject("WScript.Shell")
complianceFile = GetTaniumDir("Tools\Comply", sh) & "results\compliance_unified.txt"
vulnerabilityFile = GetTaniumDir("Tools\Comply", sh) & "results\vulnerability_unified.txt"
findingsDatesFile = GetTaniumDir("Tools\Comply", sh) & "results\findings_dates.txt"

Sub PrintFindings(fs, filePath, findingsDatesDict)
  If fs.FileExists(filePath) Then
    Dim findingsFile, findingsLine, splitLine, findingsId, dateArr
    Set findingsFile = fs.OpenTextFile(filePath, ForReading)
    Do While Not findingsFile.AtEndOfStream
      findingsLine = findingsFile.ReadLine
      splitLine = Split(findingsLine, "|")
      findingsId = splitLine(0)
      If splitLine(2) = "nonvulnerable" Then
        j = 2
      Else
        If findingsDatesDict.Exists(findingsId) Then
          dateArr = findingsDatesDict.Item(findingsId)
          WScript.Echo findingsLine & "|" & dateArr(0) & "|" & dateArr(1)
        Else
          WScript.Echo findingsLine & "||"
        End If
      End If
    Loop
    findingsFile.Close
  End If
End Sub

If fs.FileExists(complianceFile) Or fs.FileExists(vulnerabilityFile) Then
  Dim findingsDatesDict: Set findingsDatesDict = ReadFindingsDatesFile(findingsDatesFile)
  PrintFindings fs, complianceFile, findingsDatesDict
  PrintFindings fs, vulnerabilityFile, findingsDatesDict
Else
  WScript.Echo "Findings not found"
End If
' Copyright 2021, Tanium Inc.
