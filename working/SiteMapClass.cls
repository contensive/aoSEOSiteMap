VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "SiteMapClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Const LogFileName = "seoSiteMap.log"

Private Main As Object

Public Sub Init(MainObject As Object)
    On Error GoTo ErrorTrap
    
    Set Main = MainObject
    
    Exit Sub
    
ErrorTrap:
    Call HandleError("SiteMapClass", "Init", Err.Number, Err.Source, Err.Description, True, False)
    End Sub

Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    
    Dim Stream As String
    Dim PCC As Variant
    Dim Count As Long
    Dim Pointer As Long
    Dim ModifiedDate As Date
    Dim objCSV As Object
    Dim ReturnValue As Variant
    Dim PageLink As String
    Dim PageID As Long
    Dim PageNotFoundID As Long
    
    Call Main.ClearStream
    Call Main.SetStreamType("text/XML")
    
    PageNotFoundID = kmaEncodeInteger(Main.GetSiteProperty("PAGENOTFOUNDPAGEID"))
    
    Stream = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
    Stream = Stream & "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">" & vbCrLf
    Set objCSV = CreateObject("ccCSrvr3.ContentServerClass")
    ReturnValue = objCSV.OpenConnection(Main.ApplicationName)
    If ReturnValue.ApplicationStatus = 2 Then
    
        PCC = objCSV.GetPersistentVariant("PCC")
        Set objCSV = Nothing
        If Not IsEmpty(PCC) Then
        
            Count = UBound(PCC, 2)

            For Pointer = 0 To Count
                PageID = KmaEncodeNumber(PCC(PCC_ID, Pointer))
                
                If PageNotFoundID <> PageID Then
                
                    PageLink = Main.EncodeURL(Main.GetPageLink4(PageID, "", True, True))
                    'PageLink = Main.EncodeURL(Main.GetPageLink2(PageID, ""))
                    If InStr(1, "://", PageLink, vbTextCompare) = 0 Then
                        PageLink = "http://" & Main.ServerHost & PageLink
                    End If
                    PageLink = Replace(PageLink, "&", "&amp;", , , vbTextCompare)
                    ModifiedDate = KmaEncodeDate(PCC(PCC_ModifiedDate, Pointer))
                    
                    Stream = Stream & "<url>" & vbCrLf
                    Stream = Stream & "<loc>" & PageLink & "</loc>" & vbCrLf
                    If ModifiedDate <> CDate(0) Then
                        Stream = Stream & "<lastmod>" & GetDate(ModifiedDate) & "</lastmod>" & vbCrLf
                    End If
                    Stream = Stream & "</url>" & vbCrLf
                
                End If
                
            Next
            
        End If
        
    End If
    
    Stream = Stream & "</urlset>" & vbCrLf
    
    GetContent = Stream
    
    Exit Function
ErrorTrap:
    Call HandleError("SiteMapClass", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
Private Function GetDate(GivenDate As Date) As String
    On Error GoTo ErrorTrap
    
    If GivenDate <> CDate(0) Then
        GetDate = Year(GivenDate) & "-" & PadValue(Month(GivenDate), 2) & "-" & PadValue(Day(GivenDate), 2)
    End If
    
    Exit Function
ErrorTrap:
    Call HandleError("SiteMapClass", "GetPageContentName", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function PadValue(Value As String, StringLenghth As Long) As String
    On Error GoTo ErrorTrap

    Dim Stream As String
    Dim Counter As Long
    Dim ValueLenghth As Long

    ValueLenghth = Len(Value)
    Stream = Value
    
    If ValueLenghth < StringLenghth Then
        For Counter = ValueLenghth To StringLenghth - 1
            Stream = "0" & Stream
        Next
    End If
    
    PadValue = Stream
    
    Exit Function
ErrorTrap:
    Call HandleError("SiteMapClass", "PadValue", Err.Number, Err.Source, Err.Description, True, False)
End Function
