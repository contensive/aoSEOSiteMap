using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoSEOSiteMap
{
    public class SiteMap2Class : AddonBaseClass
    {   
        private const string MapFileName = "seoSiteMap.xml";
        public override object Execute(CPBaseClass cp)
        {
            try
            {
                CPCSBaseClass cs = cp.CSNew();
                CPFileBaseClass cpFile = cp.File;
                cp.Response.Clear();
                cp.Response.SetType("text/XML");
                return cp.File.Read(cp.Site.PhysicalFilePath + MapFileName);
             }
            catch
            {
                cp.Site.ErrorReport("SiteMap2Class");
                return string.Empty;
            }
         }

     }
}
