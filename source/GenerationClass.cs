
using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;
using System.Text;

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
                var result = new StringBuilder();
                result.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                result.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                //
                // -- all links
                var linkList = DbBaseModel.createList<LinkAliasModel>(cp);
                foreach (LinkAliasModel linkAlias in linkList) {
                    result.Append("<url>");
                    result.Append("<loc>https://" + cp.Site.DomainPrimary + linkAlias.name + "</loc>");
                    var page = DbBaseModel.create<PageContentModel>(cp,linkAlias.pageId);
                    if ( page != null) {
                        if (page.modifiedDate != null) {
                            DateTime lastmod = ((DateTime)linkAlias.modifiedDate);
                            result.Append("<lastmod>" + lastmod.Year + "-" + lastmod.Month.ToString().PadLeft(2, '0') + "-" + lastmod.Month.ToString().PadLeft(2, '0') + "</lastmod>");
                        }
                    }
                    //result.Append("<changefreq>monthly</changefreq>");
                    //result.Append("<priority>0.5</priority>");
                    result.Append("</url>");
                }
                //
                // -- all catalog products
                //
                // -- save the site map
                result.Append("</urlset>");
                cp.CdnFiles.Save("SeoSiteMap/SeoSiteMap.xml", result.ToString());
                //
                // -- update the robots.txt section of this addon
                cp.Db.ExecuteNonQuery("update ccaggregatefunctions set robotstxt='sitemap: https://" + cp.Site.DomainPrimary + "/SearchEngineSiteMap' where ccguid='{E036074B-6B71-4698-BA22-C34F7E9449E9}'");
                return "";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                throw;
            }
        }
    }
}
