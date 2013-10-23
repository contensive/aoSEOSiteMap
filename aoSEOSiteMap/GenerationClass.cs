using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;
using System.Collections;
using Microsoft.VisualBasic;

namespace aoSEOSiteMap
{
    public class GenerationClass : AddonBaseClass
    {
        public override object Execute(CPBaseClass cp)
        {
            try
            {
                return "Hello GenerationClass";
                CPCSBaseClass cs = cp.CSNew();
                CPFileBaseClass cf = cp.File;
                isAliasing = cp.Doc.GetBoolean("ALLOWLINKALIAS", string.Empty);
                pageNotFoundId = cp.Doc.GetInteger("PAGENOTFOUNDPAGEID", string.Empty);
                defaultPage = cp.Doc.GetProperty("SERVERPAGEDEFAULT", string.Empty);
                xmlContent = "";
                if (cp.Site.DomainList.IndexOf(",", 1, StringComparison.CurrentCulture) != 0)
                {
                    domain = cp.Site.DomainList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    primaryDomain = domain[0];
                }
                else
                {
                    primaryDomain = cp.Site.DomainList;
                }

                if (cs.Open("Page Templates", SQLCriteria: "Name='Default", SelectFieldList: "ID"))
                {
                    siteTemplateId = cp.Doc.GetInteger("ID");
                }
                cs.Close();

                if (cs.Open("Site Sections", SQLCriteria: "((HideMenu=0)or(HideMenu is null)) AND ((BlockSection=0)or(BlockSection is null))"))
                {
                    do
                    {
                        rootPageId = cp.Doc.GetInteger("RootPageId");
                        templateId = cp.Doc.GetInteger("TemplateId");
                        if (templateId == 0)
                        {
                            templateId = siteTemplateId;
                        }
                        xmlContent += getXmlNodeList(cp, cs, rootPageId, false, templateId, primaryDomain, ref usedUrlList);
                        cs.GoNext();
                    } while (cs.OK());
                }
                cs.Close();

                xmlContent = "" + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + crlf + "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">" +
                    + kmIndent+
                             "" + crlf + "</urlset>" + "";
                cf.Save(cp.Site.PhysicalFilePath + mapFileName, xmlContent);
                domainNameList = cp.Site.DomainList;
                int index = domainNameList.IndexOf(",", 1, StringComparison.CurrentCulture);
                if (index > 0)
                {
                    domainNameList = Strings.Mid(domainNameList, 1, index - 1);
                }
                sql = "update ccaggregatefunctions set robotstxt='sitemap: http://" + domainNameList + "/SearchEngineSiteMap' where ccguid='{E036074B-6B71-4698-BA22-C34F7E9449E9}'";
                return Convert.ToString(cp.Db.ExecuteSQL(sql));

            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }

        private string getXmlNodeList(CPBaseClass cp, CPCSBaseClass cs, long nodePageID, bool parentFlag, long defaultTemplateId, string primaryDomain, ref string usedUrlList)
        {
            try
            {
                usedUrlList = string.Empty;
                string xmlNodeList = string.Empty, criteria, pageLink = string.Empty, sql;
                long pageID, pageTemplateID;
                DateTime modifiedDate = new DateTime();

                if (nodePageID != pageNotFoundId)
                {
                    if (parentFlag)
                    {
                        criteria = "(ParentID=" + nodePageID + ")";
                    }
                    else
                    {
                        criteria = "(ID=" + nodePageID + ")";
                    }
                    criteria += " AND ((BlockContent=0) OR (BlockContent is Null)) AND ((BlockPage=0) OR (BlockPage is Null))";

                    if (cs.Open("Page Content", SQLCriteria: criteria, SortFieldList: "ID", PageNumber: 0, SelectFieldList: "ID,ModifiedDate,BlockContent,BlockPage,TemplateID,link"))
                    {
                        do
                        {
                            pageTemplateID = cp.Doc.GetInteger("TemplateID");
                            if (pageTemplateID == 0)
                            {
                                pageTemplateID = defaultTemplateId;
                            }
                            pageID = cp.Doc.GetInteger("ID");
                            if (cp.Doc.GetText("link") == "")
                            {
                                if (usedUrlList.IndexOf(pageLink, 1, StringComparison.CurrentCulture) == 0)
                                {
                                    usedUrlList = usedUrlList + crlf + pageLink;
                                    modifiedDate = cp.Doc.GetDate("ModifiedDate");
                                }
                            }

                            sbSql.Append("select l.name from cclinkaliases l left join ccpagecontent p on p.id=l.pageid where ( l.id in (");
                            sbSql.Append(" select distinct max(id) from cclinkaliases where ");
                            sbSql.Append(string.Format("(pageid={0})and(querystringsuffix is not null) group by querystringsuffix))", pageID));
                            sql = sbSql.ToString();
                            //
                            cp.Site.TestPoint("Link Alias SQL: " + sql);
                            //
                            sbSql = new StringBuilder();
                            if (cs.OpenSQL(sql, "default", PageNumber: 1))
                            {
                                do
                                {
                                    pageLink = "http://" + primaryDomain + cp.Doc.GetText("name");
                                    if (usedUrlList.IndexOf(pageLink, 1, StringComparison.CurrentCulture) == 0)
                                    {
                                        usedUrlList = usedUrlList + crlf + pageLink;
                                        xmlNodeList = xmlNodeList + getXmlNode(cp, cs, pageLink, modifiedDate);
                                    }
                                    cs.GoNext();
                                } while (cs.OK());
                                cs.Close();
                            }
                            xmlNodeList = xmlNodeList + getXmlNodeList(cp, cs, pageID, true, pageTemplateID, primaryDomain, ref usedUrlList);
                            cs.GoNext();
                        } while (cs.OK());
                        cs.Close();
                    }

                }
                return xmlNodeList;
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }

        private string getXmlNode(CPBaseClass cp, CPCSBaseClass cs, string pageLink, DateTime modifiedDate)
        {
            try
            {
                string returnXml, dateNode;
                dateNode = string.Empty;
                dateNode = cr2 + "<lastmode>" + getDateString(modifiedDate) + "</lastmod>";
                returnXml = cr + "<url>" + cr2 + "<loc>" + pageLink + "</loc>" + dateNode + cr + "</url";
                return returnXml;

            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }

        private string GetPageLink(CPBaseClass cp, CPCSBaseClass cs, int pageID, int templateId)
        {
            string stream = "";
            if (isAliasing)
            {
                if (cs.Open("Link Aliases", SQLCriteria: "(PageID=" + pageID + ")and(queryStringSuffix is null)",
                                            SelectFieldList: "ID", SortFieldList: "id desc"))
                {
                    if (cs.OK())
                    {
                        stream = "http://" + primaryDomain + cp.Doc.GetText("Name");
                    }
                    else
                    {
                        stream = getPageLinkByTemplate(cp, cs, pageID, templateId);
                    }
                }
                cs.Close();
            }
            else
            {
                stream = getPageLinkByTemplate(cp, cs, pageID, templateId);
            }
            return stream;
        }

        private string getPageLinkByTemplate(CPBaseClass cp, CPCSBaseClass cs, int pageId, int templateId)
        {
            string stream = string.Empty, pageLink = string.Empty;
            if (cs.Open("Page Templates", SQLCriteria: "ID=" + templateId, SelectFieldList: "Link"))
            {
                if (cs.OK())
                {
                    pageLink = cp.Doc.GetText("Link");
                }
            }
            cs.Close();
            if (pageLink != "")
            {
                if (pageLink.PadLeft(1) != "/")
                {
                    pageLink = "/" + pageLink;
                }
                stream = pageLink + "?bid=" + pageId;
            }
            else
            {
                if (defaultPage.PadLeft(1) != "/")
                {
                    defaultPage = "/" + defaultPage;
                }
                else
                {
                    stream = defaultPage + "?bid=" + pageId;
                }
            }
            if (stream.IndexOf(primaryDomain, 1, StringComparison.CurrentCulture) == 0)
            {
                stream = primaryDomain + stream;
            }

            if (stream.IndexOf("://", 1, StringComparison.CurrentCulture) == 0)
            {
                stream = "http://" + stream;
            }
            return stream.Replace("&", "&amp;");
        }

        private string getDateString(DateTime givenDate)
        {
            if (givenDate != Microsoft.VisualBasic.DateAndTime.DateValue("0"))
            {
                return givenDate.Year + "-" + padValue(givenDate.Month.ToString(), 2) + "-" + padValue(givenDate.Day.ToString(), 2);
            }
            return string.Empty;
        }

        private string padValue(string value, long stringLength)
        {
            string stream = string.Empty;
            long counter, valueLength;
            valueLength = value.Length;
            if (valueLength < stringLength)
            {
                for (counter = valueLength; counter < stringLength - 1; counter++)
                {
                    stream += "0";
                }
            }
            return stream;
        }

        private bool isAliasing;
        private long pageNotFoundId;
        private string primaryDomain, defaultPage, domainNameList, sql, xmlNode, xmlContent;
        private string temp, usedUrlList, pageLink;
        private string[] domain;
       
        private long pos, cs, templateId, rootPageId, siteTemplateId;
        private const string crlf = "\n";
        public const string cr = "\t";
        private const string cr2 = "\n\t";
        private const string cr3 = "\n\t\t\t";
        private StringBuilder sbSql = new StringBuilder();
        private const string mapFileName = "seoSiteMap.xml";
        private int kmIndent;

    }
}
