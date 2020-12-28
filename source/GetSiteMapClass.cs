
using Contensive.BaseClasses;
using System;

namespace Contensive.Addons.SeoSiteMap {
    //
    //====================================================================================================
    //
    public class GetSiteMapClass : AddonBaseClass {
        private const string MapFileName = "SeoSiteMap/SeoSiteMap.xml";
        //
        //====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                cp.Response.Clear();
                cp.Response.SetType("text/XML");
                return cp.CdnFiles.Read(MapFileName);
            } catch(Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
