using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoSEOSiteMap
{
    public class SiteMapClass : AddonBaseClass
    {
        private int PageNotFoundID;
        public override object Execute(CPBaseClass CP)
        {
            CPCSBaseClass cs = CP.CSNew();
            CP.Response.Clear();
            CP.Response.SetType("text/XML");
            PageNotFoundID = CP.Utils.EncodeInteger(CP.Site.GetText("PAGENOTFOUNDPAGEID"));
            //String stream = "<?xml version=""1.0"" encoding=""UTF-8""?>"  + System.Environment.NewLine;
            //stream = stream + "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">" + System.Environment.NewLine;
            CP.Site.Name("ccCSrvr3.ContentServerClass");
            CP.Site.Name(CP.Response.SetType);
            if (CP.Response.SetStatus = 2)
            { 

           }
        }
    }
}
