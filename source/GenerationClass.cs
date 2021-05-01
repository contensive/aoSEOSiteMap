
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
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
                // -- all links
                string linksql = "select l.name,p.ModifiedDate from ccLinkAliases l left join ccPageContent p on p.id=l.pageid where (p.blockcontent=0) and (p.blockpage=0) and (p.allowmetacontentnofollow=0)";

                using (CPCSBaseClass cs = cp.CSNew()) {
                    if (cs.OpenSQL(linksql)) {
                        do {
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
                            locNode.InnerText = "https://" + cp.Site.DomainPrimary + cs.GetText("name");
                            //
                            // -- last mod date
                            DateTime lastmod = cs.GetDate("ModifiedDate");
                            lastmodNode.InnerText = lastmod.Year + "-" + lastmod.Month.ToString().PadLeft(2, '0') + "-" + lastmod.Month.ToString().PadLeft(2, '0');
                            //
                            cs.GoNext();
                        } while (cs.OK());
                    }
                }
                //
                // -- all catalog products
                int shopPageId = cp.Site.GetInteger("shopping-cart-pageId");
                if (shopPageId > 0) {
                    string cartPathFilename = cp.Content.GetLinkAliasByPageID(shopPageId, "", "") + "?viewid=detail";
                    if (cp.Content.IsField("items", "id")) {
                        using (CPCSBaseClass cs = cp.CSNew()) {
                            if (cs.Open("items", "isInCatalog>0")) {
                                do {
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
                                    locNode.InnerText = "https://" + cp.Site.DomainPrimary + cartPathFilename + "&item=" + cs.GetInteger("id");
                                    //
                                    // -- last mod date
                                    DateTime lastmod = cs.GetDate("ModifiedDate");
                                    lastmodNode.InnerText = lastmod.Year + "-" + lastmod.Month.ToString().PadLeft(2, '0') + "-" + lastmod.Month.ToString().PadLeft(2, '0');
                                    //
                                    cs.GoNext();
                                } while (cs.OK());
                            }
                        }
                    }
                }
                //
                xmlDoc.Save(cp.CdnFiles.PhysicalFilePath + "SeoSiteMap/SeoSiteMap.xml");
                //
                // -- update the robots.txt section of this addon
                cp.Db.ExecuteNonQuery("update ccaggregatefunctions set robotstxt='sitemap: https://" + cp.Site.DomainPrimary + "/sitemap.xml' where ccguid='{E036074B-6B71-4698-BA22-C34F7E9449E9}'");
                return "";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                throw;
            }
        }
    }
}
