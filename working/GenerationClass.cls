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
'Private siteTemplateID As Long
Private primaryDomain As String
Private DefaultPage As String
'
Private Const crlf = vbCrLf
Private Const cr = vbCrLf & vbTab
Private Const cr2 = vbCrLf & vbTab & vbTab
Private Const cr3 = vbCrLf & vbTab & vbTab & vbTab

Private csv As Object
'
'========================================================================
'   v3.3 Process Compatibility
'       To make an Add-on that works the same in v3.3 and v3.4, use this adapter instead of the execute above
'========================================================================
'
Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    On Error GoTo ErrorTrap
    '
    Set csv = CsvObject
    '
    Dim domainNameList As String
    Dim Pos As Long
    Dim sql As String
    Dim xmlNode As String
    Dim xmlContent As String
    Dim cs As Long
    Dim objFS As New kmaFileSystem3.FileSystemClass
    Dim Temp As String
    Dim Domain() As String
    Dim templateId As Long
    Dim rootPageId As Long
    Dim siteTemplateID As Long
    Dim usedUrlList As String
    Dim pageLink As String
    '
    isAliasing = kmaEncodeBoolean(csv.GetSiteProperty("ALLOWLINKALIAS", "", 0))
    PageNotFoundID = kmaEncodeInteger(csv.GetSiteProperty("PAGENOTFOUNDPAGEID", "", 0))
    DefaultPage = csv.GetSiteProperty("SERVERPAGEDEFAULT", "", 0)
    xmlContent = ""
    '
    If InStr(1, csv.DomainName, ",", vbTextCompare) <> 0 Then
        Domain = Split(csv.DomainName, ",", , vbTextCompare)
        primaryDomain = Domain(0)
    Else
        primaryDomain = csv.DomainName
    End If
    '
    cs = csv.OpenCSContent("Page Templates", "Name='Default'", , , , , , "ID")
    If csv.iscsok(cs) Then
        siteTemplateID = csv.GetCSInteger(cs, "ID")
    End If
    Call csv.closecs(cs)
    '
    ' get all links from sections
    '
    cs = csv.OpenCSContent("Site Sections", "((HideMenu=0)or(HideMenu is null)) AND ((BlockSection=0)or(BlockSection is null))")
    Do While csv.iscsok(cs)
        rootPageId = csv.GetCSInteger(cs, "RootPageID")
        templateId = csv.GetCSInteger(cs, "TemplateID")
        If templateId = 0 Then
            templateId = siteTemplateID
        End If
        xmlContent = xmlContent & getXmlNodeList(rootPageId, False, templateId, usedUrlList, primaryDomain)
        Call csv.NextCSRecord(cs)
    Loop
    Call csv.closecs(cs)
'    '
'    ' get all links from link alias
'    '
'    sql = "" _
'        & " select l.name" _
'        & " from" _
'        & " cclinkaliases l" _
'        & " left join ccpagecontent p on p.id=l.pageid" _
'        & " where" _
'        & " (l.id in (" _
'        & " select distinct max(id) from cclinkaliases group by pageid,querystringsuffix" _
'        & " ))" _
'        & " and(p.id is not null)" _
'        & " AND((p.BlockContent=0) OR (p.BlockContent is Null))" _
'        & " AND((p.BlockPage=0) OR (p.BlockPage is Null))" _
'        & " AND((p.AllowInChildLists=1) OR (p.AllowInMenus=1))" _
'        & ""
'    cs = csv.openCsSql("default", sql, 1)
'    Do While csv.iscsok(cs)
'        pageLink = "http://" & PrimaryDomain & csv.getcsText(cs, "name")
'        If InStr(1, usedUrlList, pageLink, vbTextCompare) = 0 Then
'            usedUrlList = usedUrlList & crlf & pageLink
'            xmlContent = xmlContent & getXmlNode(pageLink, CDate(0))
'        End If
'        Call csv.NextCSRecord(cs)
'    Loop
'    Call csv.closecs(cs)
    '
    xmlContent = "" _
        & "<?xml version=""1.0"" encoding=""UTF-8""?>" _
        & crlf & "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">" _
        & kmaIndent(xmlContent) _
        & crlf & "</urlset>" _
        & ""
    Call objFS.SaveFile(csv.PhysicalFilePath & MapFileName, xmlContent)
    '
    ' update the robots.txt field in the collection
    '
    domainNameList = csv.DomainName
    Pos = InStr(1, domainNameList, ",")
    If Pos > 0 Then
        domainNameList = Mid(domainNameList, 1, Pos - 1)
    End If
    sql = "update ccaggregatefunctions set robotstxt='sitemap: http://" & domainNameList & "/SearchEngineSiteMap' where ccguid='{E036074B-6B71-4698-BA22-C34F7E9449E9}'"
    Call csv.executeSQL("", sql)
    '
    Execute = ""
    Exit Function
ErrorTrap:
    Call HandleError("geneationClass", "execute", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'
'
Private Function getXmlNodeList(nodePageID As Long, ParentFlag As Boolean, defaultTemplateId As Long, ByRef usedUrlList As String, primaryDomain As String) As String
    On Error GoTo ErrorTrap
    '
    Dim xmlNodeList As String
    '
    Dim cs As Long
    Dim cs2 As Long
    Dim Criteria As String
    Dim RecordID As Long
    Dim pageLink As String
    Dim modifiedDate As Date
    Dim PageID As Long
    Dim PageTemplateID As Long
    Dim sql As String
    '
    If nodePageID <> PageNotFoundID Then
        If ParentFlag Then
            Criteria = "(ParentID=" & nodePageID & ")"
        Else
            Criteria = "(ID=" & nodePageID & ")"
        End If
        Criteria = Criteria & " AND ((BlockContent=0) OR (BlockContent is Null)) AND ((BlockPage=0) OR (BlockPage is Null))"
        'Criteria = Criteria & " AND ((BlockContent=0) OR (BlockContent is Null)) AND ((BlockPage=0) OR (BlockPage is Null)) AND ((AllowInChildLists=1) OR (AllowInMenus=1))"
        cs = csv.OpenCSContent("Page Content", Criteria, "ID", , 0, , , "ID,ModifiedDate,BlockContent,BlockPage,TemplateID,link")
        Do While csv.iscsok(cs)
            PageTemplateID = csv.GetCSInteger(cs, "TemplateID")
            If PageTemplateID = 0 Then
                PageTemplateID = defaultTemplateId
            End If
            PageID = csv.GetCSInteger(cs, "ID")
            If csv.getcsText(cs, "link") = "" Then
                pageLink = GetPageLink(PageID, PageTemplateID)
                If InStr(1, usedUrlList, pageLink, vbTextCompare) = 0 Then
                    usedUrlList = usedUrlList & crlf & pageLink
                    modifiedDate = KmaEncodeDate(csv.GetCSDate(cs, "ModifiedDate"))
                    xmlNodeList = xmlNodeList & getXmlNode(pageLink, modifiedDate)
                End If
            End If
            '
            ' get addon-content links from link aliases
            '
            sql = "" _
                & " select l.name" _
                & " from" _
                & " cclinkaliases l" _
                & " left join ccpagecontent p on p.id=l.pageid" _
                & " where" _
                & " (l.id in (" _
                & " select distinct max(id) from cclinkaliases where (pageid=" & PageID & ")and(querystringsuffix is not null) group by querystringsuffix" _
                & " ))" _
                & ""
            cs2 = csv.openCsSql("default", sql, 1)
            Do While csv.iscsok(cs2)
                pageLink = "http://" & primaryDomain & csv.getcsText(cs2, "name")
                If InStr(1, usedUrlList, pageLink, vbTextCompare) = 0 Then
                    usedUrlList = usedUrlList & crlf & pageLink
                    xmlNodeList = xmlNodeList & getXmlNode(pageLink, CDate(0))
                End If
                Call csv.NextCSRecord(cs2)
            Loop
            Call csv.closecs(cs2)
            '
            ' get child nodes
            '
            xmlNodeList = xmlNodeList & getXmlNodeList(PageID, True, PageTemplateID, usedUrlList, primaryDomain)
            Call csv.NextCSRecord(cs)
        Loop
        Call csv.closecs(cs)
    End If
    '
    getXmlNodeList = xmlNodeList
    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "getXmlNodeList", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'Private Function getXmlNodeByCS(cs As Long, defaultTemplateId As Long) As String
'    On Error GoTo ErrorTrap
'    Dim xmlNode As String
'    '
'    Dim pageLink As String
'    Dim modifiedDate As Date
'    Dim PageID As Long
'    Dim PageTemplateID As Long
'
'    If csv.IsCSOK(cs) Then
'        PageTemplateID = csv.GetCSInteger(cs, "TemplateID")
'        If PageTemplateID = 0 Then
'            PageTemplateID = defaultTemplateId
'        End If
'        PageID = csv.GetCSInteger(cs, "ID")
'        If csv.getcstext(cs, "link") = "" Then
'            pageLink = GetPageLink(PageID, PageTemplateID)
'            modifiedDate = KmaEncodeDate(csv.GetCSDate(cs, "ModifiedDate"))
'            xmlNode = xmlNode & getXmlNode(pageLink, modifiedDate)
'        End If
'        xmlNode = xmlNode & getXmlNodeList(PageID, True, PageTemplateID)
'    End If
'    '
'    getXmlNodeByCS = xmlNode
'    Exit Function
'ErrorTrap:
'    Call HandleError("GenerationClass", "getXmlNodeByCS", Err.Number, Err.Source, Err.Description, True, False)
'End Function
'
'
'
Private Function getXmlNode(pageLink As String, modifiedDate As Date) As String
    Dim returnXml As String
    Dim dateNode As String
    '
    dateNode = ""
    If modifiedDate <> CDate(0) Then
        dateNode = cr2 & "<lastmod>" & getDateString(modifiedDate) & "</lastmod>"
    End If
    returnXml = "" _
        & cr & "<url>" _
        & cr2 & "<loc>" & pageLink & "</loc>" _
        & dateNode _
        & cr & "</url>" _
        & ""
    '
    getXmlNode = returnXml
End Function
'
'
'
Private Function GetPageLink(PageID, templateId) As String
    Dim Stream As String
    Dim cs As Long
    '
    On Error GoTo ErrorTrap
    If isAliasing Then
        cs = csv.OpenCSContent("Link Aliases", "(PageID=" & PageID & ")and(queryStringSuffix is null)", "id desc", , , , , "Name")
        If csv.iscsok(cs) Then
            Stream = "http://" & primaryDomain & csv.GetCS(cs, "Name")
        Else
            Stream = GetPageLinkByTemplate(PageID, templateId)
        End If
        Call csv.closecs(cs)
    Else
        Stream = GetPageLinkByTemplate(PageID, templateId)
    End If

    GetPageLink = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("GenerationClass", "GetPageLink", Err.Number, Err.Source, Err.Description, True, False)
End Function

Private Function GetPageLinkByTemplate(PageID, templateId) As String
    On Error GoTo ErrorTrap

    Dim Stream As String
    Dim cs As Long
    Dim pageLink As String
    
    cs = csv.OpenCSContent("Page Templates", "ID=" & templateId, , , , , , "Link")
    If csv.iscsok(cs) Then
        pageLink = csv.GetCS(cs, "Link")
    End If
    Call csv.closecs(cs)
    
    If pageLink <> "" Then
        If Left(pageLink, 1) <> "/" Then
            pageLink = "/" & pageLink
        End If
        Stream = pageLink & "?bid=" & PageID
    Else
        If Left(DefaultPage, 1) <> "/" Then
            DefaultPage = "/" & DefaultPage
        End If
        Stream = DefaultPage & "?bid=" & PageID
    End If
    
    If InStr(1, Stream, primaryDomain, vbTextCompare) = 0 Then
        Stream = primaryDomain & Stream
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
'
Private Function getDateString(GivenDate As Date) As String
    On Error GoTo ErrorTrap
    
    If GivenDate <> CDate(0) Then
        getDateString = Year(GivenDate) & "-" & PadValue(Month(GivenDate), 2) & "-" & PadValue(Day(GivenDate), 2)
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
'
Public Function Main(obj As Object, str As String) As String
End Function

