VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "GenerationClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Const MapFileName = "seoSiteMap.xml"

Private isAliasing As Boolean
Private PageNotFoundID As Long
Private DefaultTemplateID As Long
Private PrimaryDomain As String
Private DefaultPage As String

Private csv As Object
'
'========================================================================
'   v3.3 Process Compatibility
'       To make an Add-on that works the same in v3.3 and v3.4, use this adapter instead of the execute above
'========================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    Execute = Main(CsvObject, OptionString)
End Function

Public Function Main(csvObj As Object, OptionString As String) As String
    On Error GoTo ErrorTrap
    '
    Set csv = csvObj
    '
    Dim sql As String
    Dim node As String
    Dim Stream As String
    Dim CS As Long
    Dim objFS As New kmaFileSystem3.FileSystemClass
    Dim Temp As String
    Dim Domain() As String
    Dim templateId As Long
    Dim rootPageId As Long

    isAliasing = kmaEncodeBoolean(csv.GetSiteProperty("ALLOWLINKALIAS", "", 0))
    PageNotFoundID = kmaEncodeInteger(csv.GetSiteProperty("PAGENOTFOUNDPAGEID", "", 0))
    DefaultPage = csv.GetSiteProperty("SERVERPAGEDEFAULT", "", 0)
    
    If InStr(1, csv.DomainName, ",", vbTextCompare) <> 0 Then
        Domain = Split(csv.DomainName, ",", , vbTextCompare)
        PrimaryDomain = Domain(0)
    Else
        PrimaryDomain = csv.DomainName
    End If
    
    CS = csv.OpenCSContent("Page Templates", "Name='Default'", , , , , , "ID")
    If csv.IsCSOK(CS) Then
        DefaultTemplateID = csv.GetCSInteger(CS, "ID")
    End If
    Call csv.CloseCS(CS)
    
    CS = csv.OpenCSContent("Site Sections", "((HideMenu=0)or(HideMenu is null)) AND ((BlockSection=0)or(BlockSection is null))")
    Do While csv.IsCSOK(CS)
        rootPageId = csv.GetCSInteger(CS, "RootPageID")
        templateId = csv.GetCSInteger(CS, "TemplateID")
        node = GetPageTree(rootPageId, False, templateId)
        Stream = Stream & node
        Call csv.NextCSRecord(CS)
    Loop
    Call csv.CloseCS(CS)
    
    Call objFS.SaveFile(csv.PhysicalFilePath & MapFileName, GetXMLWrapper(Stream))
    '
    ' update the robots.txt field in the collection
    '
Dim domainNameList As String
    domainNameList = csv.DomainName
Dim pos As Long
    pos = InStr(1, domainNameList, ",")
    If pos > 0 Then
        domainNameList = Mid(domainNameList, 1, pos - 1)
    End If
    sql = "update ccaggregatefunctions set robotstxt='sitemap: http://" & domainNameList & "/SearchEngineSiteMap' where ccguid='{E036074B-6B71-4698-BA22-C34F7E9449E9}'"
    Call csv.executeSQL("", sql)
    '
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

Private Function GetXMLWrapper(Pagelist As String) As String
    On Error GoTo ErrorTrap

    Dim Stream As String

    Stream = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
    Stream = Stream & "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">" & vbCrLf
    
    Stream = Stream & Pagelist & vbCrLf
    
    Stream = Stream & "</urlset>" & vbCrLf
    
    GetXMLWrapper = Stream

    Exit Function
ErrorTrap:
    Call HandleError("SiteMapClass", "PadValue", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function GetPageTree(PageID As Long, ParentFlag As Boolean, templateId As Long) As String
    On Error GoTo ErrorTrap

    Dim CS As Long
    Dim Criteria As String
    Dim Stream As String
    Dim RecordID As Long
    
    If PageID <> PageNotFoundID Then
    
        If ParentFlag Then
            Criteria = "(ParentID=" & PageID & ")"
        Else
            Criteria = "(ID=" & PageID & ")"
        End If
        
        Criteria = Criteria & " AND ((BlockContent=0) OR (BlockContent is Null)) AND ((BlockPage=0) OR (BlockPage is Null)) AND ((AllowInChildLists=1) OR (AllowInMenus=1))"
        
        CS = csv.OpenCSContent("Page Content", Criteria, "ID", , 0, , , "ID,ModifiedDate,BlockContent,BlockPage,AllowInChildLists,AllowInMenus,TemplateID,link")
        Do While csv.IsCSOK(CS)
            'If csv.getcstext(CS, "link") = "" Then
                Stream = Stream & GetXMLNodeSet(CS, templateId)
            'End If
            Call csv.NextCSRecord(CS)
        Loop
        Call csv.CloseCS(CS)

    End If
    
    GetPageTree = Stream

    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "GetPageTree", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function GetXMLNodeSet(Pointer As Long, GivenTemplateID As Long) As String
    On Error GoTo ErrorTrap

    Dim Stream As String
    Dim PageLink As String
    Dim ModifiedDate As Date
    Dim PageID As Long
    Dim PageTemplateID As Long
    
    If csv.IsCSOK(Pointer) Then
        PageTemplateID = csv.GetCSInteger(Pointer, "TemplateID")
        
        If PageTemplateID = 0 Then
            If GivenTemplateID <> 0 Then
                PageTemplateID = GivenTemplateID
            Else
                PageTemplateID = DefaultTemplateID
            End If
        End If
        
        PageID = csv.GetCSInteger(Pointer, "ID")
        If csv.getcstext(Pointer, "link") = "" Then
            '
            ' only include pages that do not have Menu Link Override (causes a redirect and is not real content)
            '
            PageLink = GetPageLink(PageID, PageTemplateID)
            ModifiedDate = KmaEncodeDate(csv.GetCSDate(Pointer, "ModifiedDate"))
            '
            Stream = Stream & "<url>" & vbCrLf
            Stream = Stream & "<loc>" & PageLink & "</loc>" & vbCrLf
            If ModifiedDate <> CDate(0) Then
                Stream = Stream & "<lastmod>" & GetDate(ModifiedDate) & "</lastmod>" & vbCrLf
            End If
            Stream = Stream & "</url>" & vbCrLf
        End If
        
        Stream = Stream & GetPageTree(PageID, True, PageTemplateID)
        
    End If
    
    GetXMLNodeSet = Stream

    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "GetXMLNodeSet", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function GetPageLink(PageID, templateId) As String
    On Error GoTo ErrorTrap

    Dim Stream As String
    Dim CS As Long
    
    If isAliasing Then
        CS = csv.OpenCSContent("Link Aliases", "PageID=" & PageID, , , , , , "Name")
        If csv.IsCSOK(CS) Then
            Stream = "http://" & PrimaryDomain & csv.GetCS(CS, "Name")
        Else
            Stream = GetPageLinkByTemplate(PageID, templateId)
        End If
        Call csv.CloseCS(CS)
    Else
        Stream = GetPageLinkByTemplate(PageID, templateId)
    End If

    GetPageLink = Stream

    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "GetPageLink", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function GetPageLinkByTemplate(PageID, templateId) As String
    On Error GoTo ErrorTrap

    Dim Stream As String
    Dim CS As Long
    Dim PageLink As String
    
    CS = csv.OpenCSContent("Page Templates", "ID=" & templateId, , , , , , "Link")
    If csv.IsCSOK(CS) Then
        PageLink = csv.GetCS(CS, "Link")
    End If
    Call csv.CloseCS(CS)
    
    If PageLink <> "" Then
        If Left(PageLink, 1) <> "/" Then
            PageLink = "/" & PageLink
        End If
        Stream = PageLink & "?bid=" & PageID
    Else
        If Left(DefaultPage, 1) <> "/" Then
            DefaultPage = "/" & DefaultPage
        End If
        Stream = DefaultPage & "?bid=" & PageID
    End If
    
    If InStr(1, Stream, PrimaryDomain, vbTextCompare) = 0 Then
        Stream = PrimaryDomain & Stream
    End If
    
    If InStr(1, Stream, "://", vbTextCompare) = 0 Then
        Stream = "http://" & Stream
    End If
    
    Stream = Replace(Stream, "&", "&amp;", , , vbTextCompare)
    
    GetPageLinkByTemplate = Stream

    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "GetPageLinkByTemplate", Err.Number, Err.Source, Err.Description, True, False)
End Function
