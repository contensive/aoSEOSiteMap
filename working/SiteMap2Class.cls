VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "SiteMap2Class"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit

Const MapFileName = "seoSiteMap.xml"

Private Main As Object

Public Sub Init(MainObject As Object)
    On Error GoTo ErrorTrap
    
    Set Main = MainObject
    
    Exit Sub
    
ErrorTrap:
    Call HandleError("SiteMap2Class", "Init", Err.Number, Err.Source, Err.Description, True, False)
    End Sub

Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    
    Dim objFS As New kmaFileSystem3.FileSystemClass
    
    Call Main.ClearStream
    Call Main.SetStreamType("text/XML")
    
    GetContent = objFS.ReadFile(Main.PhysicalFilePath & MapFileName)
    
    Exit Function
ErrorTrap:
    Call HandleError("SiteMap2Class", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
    End Function

