using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Linq;

namespace APStaging
{
    public class APInvoiceStagingMaint : PXGraph<APInvoiceStagingMaint>
    {
        // Main header view
        public PXSelect<APInvoiceStaging> APStaging;

        // Detail view
        public PXSelect<APInvoiceStagingDetail,
            Where<APInvoiceStagingDetail.stagingID, Equal<Current<APInvoiceStaging.stagingID>>>>
            Details;

        public PXSelect<APInvoiceStagingPayment,
            Where<APInvoiceStagingPayment.stagingID, Equal<Current<APInvoiceStaging.stagingID>>>> 
            PaymentInfo;


        // Save/cancel buttons for header
        public PXSave<APInvoiceStaging> Save;
        public PXCancel<APInvoiceStaging> Cancel;
        public PXDelete<APInvoiceStaging> Delete;

        // Save/cancel for detail grid
        public PXInsert<APInvoiceStagingDetail> InsertDetail;
        public PXDelete<APInvoiceStagingDetail> DeleteDetail;

        // Default vendor behavior
        protected void _(Events.FieldUpdated<APInvoiceStaging, APInvoiceStaging.vendorID> e)
        {
            var row = e.Row as APInvoiceStaging;
            if (row?.VendorID == null) return;

            var location = PXSelect<Location,
                Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                      And<Location.isDefault, Equal<True>>>>
                .Select(this, row.VendorID);
            if (location != null)
                row.VendorLocationID = ((Location)location).LocationID;

            var vendor = PXSelect<Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                .Select(this, row.VendorID);
            if (vendor != null)
            {
                row.TermsID = ((Vendor)vendor).TermsID;
                row.CuryID = ((Vendor)vendor).CuryID;
            }
        }

        protected void _(Events.RowPersisting<APInvoiceStaging> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (row.VendorID == null && !string.IsNullOrWhiteSpace(row.VendorName))
            {
                // Get all vendors
                var vendors = PXSelect<Vendor>.Select(this).RowCast<Vendor>();

                // 1. Try exact match (case-insensitive)
                var vendor = vendors
                    .FirstOrDefault(v => string.Equals(v.AcctName, row.VendorName, StringComparison.OrdinalIgnoreCase));

                // 2. Try partial/fuzzy match (contains, case-insensitive)
                if (vendor == null)
                {
                    vendor = vendors
                        .FirstOrDefault(v => v.AcctName != null &&
                                             v.AcctName.IndexOf(row.VendorName, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (vendor != null)
                {
                    row.VendorID = vendor.BAccountID;
                }
                // else: leave blank, user can pick in the UI
            }
        }

    }
}
