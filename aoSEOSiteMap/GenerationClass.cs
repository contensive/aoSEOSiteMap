using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoSEOSiteMap
{
    public class GenerationClass : AddonBaseClass
    {
        public override object Execute(CPBaseClass cp)
        {
            try
            {
                CPCSBaseClass cs = cp.CSNew();
                isAliasing = cp.Doc.GetBoolean("ALLOWLINKALIAS", string.Empty);
                pageNotFoundId = cp.Doc.GetInteger("PAGENOTFOUNDPAGEID", string.Empty);
                defaultPage = cp.Doc.GetProperty("SERVERPAGEDEFAULT", string.Empty);
                xmlContent = "";
                if (cp.Site.Domain.IndexOf(",", 1, StringComparison.CurrentCulture) != 0)
                {
                    domain = cp.Site.Domain.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    primaryDomain = domain[0];
                }
                else
                {
                    primaryDomain = cp.Site.Domain;
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
                    cs.Close();
                }
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
            return "function output";
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
                            sbSql = new StringBuilder();
                            if (cs.OpenSQL(sql, "default", PageNumber: 1))
                            {
                                // while(cs.OK())
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

        private string getDateString(DateTime givenDate)
        {
            if (givenDate.TimeOfDay != new TimeSpan(12, 00, 00))
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
        private CPFileBaseClass cf;
        private long pos, cs, templateId, rootPageId, siteTemplateId;
        private const string crlf = "\n";
        public const string cr = "\t";
        private const string cr2 = "\n\t";
        private const string cr3 = "\n\t\t\t";
        private StringBuilder sbSql = new StringBuilder();


    }
}
