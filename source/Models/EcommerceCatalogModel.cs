using Contensive.BaseClasses;
using Contensive.Models.Db;
using System;

namespace Contensive.Addons.SeoSiteMap.Models {
    //
    /// <summary>
    /// from ecommerce collection
    /// </summary>
    internal class EcommerceCatalogModel : Contensive.Models.Db.DbBaseModel {
        public static DbBaseTableMetadataModel tableMetadata = new DbBaseTableMetadataModel("ecommerce catalogs", "ecEcommerceCatalogs", "default", false);
        //
        public static int getPrimaryCatalogLastPageId( CPBaseClass cp ) {
            try {
                if(!cp.Content.IsField("ecommerce catalogs", "lastPageId")) {
                    //
                    // -- legacy page id
                    return cp.Site.GetInteger("shopping-cart-pageId");
                } else {
                    //
                    // -- ecommerce > 22.12.10.0
                    using (var dt = cp.Db.ExecuteQuery("select top 1 lastPageId from ecEcommerceCatalogs order by id")) {
                        if (dt?.Rows == null || dt.Rows.Count == 0) { return 0; }
                        return (int)dt.Rows[0][0];
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
