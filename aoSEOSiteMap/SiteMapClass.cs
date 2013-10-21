using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoSEOSiteMap
{
    public class siteMapClass : AddonBaseClass
    {
        public override object Execute(CPBaseClass cp)
        {
            try
            {
                return "";
            //    CPCSBaseClass cs = cp.CSNew();
            //    cp.Response.Clear();
            //    cp.Response.SetType("text/XML");
            //    pageNotFoundID = cp.Doc.GetInteger("PAGENOTFOUNDPAGEID");
            //    stream = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
            //    stream += "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n";
            //    returnValue = cp.Site.Name;
            //    CPContentBaseClass cc = cp.Content;
                       
            //    for (pointer = 0; pointer <= count; pointer++)
            //    {
            //        //PageID = KmaEncodeNumber(PCC(PCC_ID, Pointer))//VB6 Code
            //        // pageID= cp.Utils.EncodeNumber(PCC(PCC_ID, pointer));//C# Code
            //        if (pageNotFoundID != pageID)
            //        {
            //            pageLink = cp.Utils.EncodeUrl(cc.GetPageLink((int)pageID, "", true));
            //            if (pageLink.IndexOf("://", 1, StringComparison.CurrentCultureIgnoreCase) == 0)
            //            {
            //                pageLink = "http://" + cp.Request.Host + pageLink;
            //            }
            //            pageLink = pageLink.Replace("&", "&amp;");
            //            // ModifiedDate = KmaEncodeDate(PCC(PCC_ModifiedDate, Pointer))//VB6 Code
            //            //modifiedDate = cp.Utils.EncodeDate(PCC(PCC_ModifiedDate, pointer));//C# Code
            //            stream += "<url>\n";
            //            stream += "<loc>" + pageLink + "</loc>\n";
            //            if (modifiedDate.TimeOfDay != new TimeSpan(12, 00, 00))
            //            {
            //                stream += "<lastmod>" + GetDate(cp, modifiedDate) + "</lastmod>\n";
            //            }
            //            stream += "</url>\n";
            //        }
                //}

                //stream += "</urlset>\n";
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }

        }

        
        private string GetDate(CPBaseClass cp, DateTime givenDate)
        {
            try
            {
                if (givenDate.TimeOfDay != new TimeSpan(12, 00, 00))
                {
                    return givenDate.Year + "-" + padValue(cp, givenDate.Month.ToString(), 2) + "-" + padValue(cp, givenDate.Day.ToString(), 2);
                }
            }
            catch (System.Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }

            return string.Empty;
        }

        private string padValue(CPBaseClass cp, string value, long stringLength)
        {
            string stream = string.Empty;

            long counter, valueLength;
            try
            {
                valueLength = value.Length;
                if (valueLength < stringLength)
                {
                    for (counter = valueLength; counter <= stringLength - 1; counter++)
                    {
                        stream += "0";
                    }
                }
                return stream;
            }
            catch (System.Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }


        private const string logFileName = "seoSitemap.log";
        private string stream, pageLink;
        private long pointer, count, pageID, pageNotFoundID;
        private DateTime modifiedDate;
        private object returnValue;
        private object PCC;
        private object PCC_ModifiedDate;
        private object PCC_ID;

    }
}
