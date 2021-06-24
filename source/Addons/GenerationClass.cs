
using Contensive.Addons.SeoSiteMap.Models;
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Contensive.Addons.SeoSiteMap {
    //
    //====================================================================================================
    //
    public class GenerationClass : AddonBaseClass {
        //
        //====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                //
                // -- gather all links
                var unSortedList = new List<SiteMapUrl>();
                {
                    //
                    // -- add link alias entries
                    using (CPCSBaseClass cs = cp.CSNew()) {
                        if (cs.OpenSQL("select l.name,l.ModifiedDate from ccLinkAliases l left join ccPageContent p on p.id=l.pageid where (p.blockcontent=0) and (p.blockpage=0) and (p.allowmetacontentnofollow=0)")) {
                            do {
                                unSortedList.Add(new SiteMapUrl {
                                    lastMod = cs.GetDate("ModifiedDate"),
                                    pathPage = cs.GetText("name")
                                });
                                cs.GoNext();
                            } while (cs.OK());
                        }
                    }
                }
                {
                    //
                    // -- add catalog products
                    if (cp.Content.IsField("items", "id")) {
                        int shopPageId = cp.Site.GetInteger("shopping-cart-pageId");
                        if (shopPageId > 0) {
                            string cartPathFilename = cp.Content.GetLinkAliasByPageID(shopPageId, "", "") + "?viewid=detail";
                            using (CPCSBaseClass cs = cp.CSNew()) {
                                if (cs.Open("items", "isInCatalog>0","id",true, "ModifiedDate,id")) {
                                    do {
                                        unSortedList.Add(new SiteMapUrl {
                                            lastMod = cs.GetDate("ModifiedDate"),
                                            pathPage = cartPathFilename + "&item=" + cs.GetInteger("id")
                                        });
                                        cs.GoNext();
                                    } while (cs.OK());
                                }
                            }
                        }
                    }
                }
                //
                // -- sort urlList by modifiedDate newest first
                List<SiteMapUrl> sortedList = unSortedList.OrderByDescending(o => o.lastMod).ToList();
                //
                // -- create xml doc
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode docNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(docNode);
                //
                // -- create urlset node
                XmlNode urlsetNode = xmlDoc.CreateElement("urlset");
                xmlDoc.AppendChild(urlsetNode);
                XmlAttribute urlsetAttr = xmlDoc.CreateAttribute("xmlns");
                urlsetAttr.InnerText = "http://www.sitemaps.org/schemas/sitemap/0.9";
                urlsetNode.Attributes.Append(urlsetAttr);
                //
                // now add the sortedList
                foreach (var item in sortedList) {
                    //
                    // -- build xml nodes
                    XmlNode urlNode = xmlDoc.CreateElement("url");
                    XmlNode locNode = xmlDoc.CreateElement("loc");
                    XmlNode lastmodNode = xmlDoc.CreateElement("lastmod");
                    urlNode.AppendChild(locNode);
                    urlNode.AppendChild(lastmodNode);
                    urlsetNode.AppendChild(urlNode);
                    //
                    // -- page link
                    locNode.InnerText = "https://" + cp.Site.DomainPrimary + item.pathPage;
                    //
                    // -- last mod date
                    lastmodNode.InnerText = item.lastMod.Year + "-" + item.lastMod.Month.ToString().PadLeft(2, '0') + "-" + item.lastMod.Month.ToString().PadLeft(2, '0');

                }
                //
                // -- write file from xmldoc (physical write), so first verify folders with a read, then after, call copylocaltoremote
                string tmp = cp.CdnFiles.Read(Constants.cdnPathFilenameSitemapFile);
                xmlDoc.Save(cp.CdnFiles.PhysicalFilePath + Constants.cdnPathFilenameSitemapFile);
                cp.CdnFiles.CopyLocalToRemote(Constants.cdnPathFilenameSitemapFile);
                //
                // -- update the robots.txt section of this addon
                cp.Db.ExecuteNonQuery("update ccaggregatefunctions set robotstxt='sitemap: https://" + cp.Site.DomainPrimary + "/sitemap.xml' where ccguid=" + cp.Db.EncodeSQLText(Constants.guidAddonGenerator));
                return "";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                throw;
            }
        }
    }
}
