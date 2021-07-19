
using Contensive.BaseClasses;
using System;

namespace Contensive.Addons.SeoSiteMap {
    //
    //====================================================================================================
    //
    public class GetSiteMapClass : AddonBaseClass {
        //
        //====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                cp.Response.Clear();
                cp.Response.SetType("application/xml");
                return cp.CdnFiles.Read(Constants.cdnPathFilenameSitemapFile);
            } catch(Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
