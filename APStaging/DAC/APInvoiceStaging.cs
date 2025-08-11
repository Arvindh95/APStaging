using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;

namespace APStaging
{
    [Serializable]
    [PXCacheName("AP Invoice Staging")]              // links to SQL table
    public class APInvoiceStaging : PXBqlTable, IBqlTable
    {
        #region StagingID (Primary Key)
        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Staging ID", Enabled = false)]
        public int? StagingID { get; set; }
        public abstract class stagingID : PX.Data.BQL.BqlInt.Field<stagingID> { }
        #endregion

        #region DocType
        [PXDBString(3, IsUnicode = true, IsFixed = true)]
        [PXDefault("INV")]
        [PXStringList(new string[] { "INV", "ACR", "ADR", "PPM" }, new string[] { "Invoice", "Credit Adj", "Debit Adj", "Prepayment" })]
        [PXUIField(DisplayName = "Doc Type")]
        public virtual string DocType { get; set; }
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion

        #region VendorID
        [Vendor]                                             // PXSelector under the hood
        [PXUIField(DisplayName = "Vendor")]
        public int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region VendorName
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Vendor Name")]
        public string VendorName { get; set; }
        public abstract class vendorName : PX.Data.BQL.BqlString.Field<vendorName> { }
        #endregion

        #region DocDate
        [PXDBDate]
        [PXUIField(DisplayName = "Doc. Date")]
        public DateTime? DocDate { get; set; }
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
        #endregion

        #region InvoiceNbr
        [PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Invoice Nbr.")]
        public string InvoiceNbr { get; set; }
        public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
        #endregion

        #region Description
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public string Description { get; set; }
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        #endregion

        #region TermsID
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<Terms.termsID>), DescriptionField = typeof(Terms.descr))]
        [PXUIField(DisplayName = "Terms")]
        public string TermsID { get; set; }
        public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
        #endregion

        #region DueDate
        [PXDBDate]
        [PXUIField(DisplayName = "Due Date")]
        public DateTime? DueDate { get; set; }
        public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
        #endregion

        #region DiscDate
        [PXDBDate]
        [PXUIField(DisplayName = "Discount Date")]
        public DateTime? DiscDate { get; set; }
        public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }
        #endregion

        #region CuryID
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXSelector(typeof(Search<Currency.curyID>))]
        [PXDefault(typeof(AccessInfo.baseCuryID))]
        [PXUIField(DisplayName = "Currency")]
        public string CuryID { get; set; }
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        #endregion

        #region BranchID
        [PXDBInt]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(
            typeof(Branch.branchID),
            SubstituteKey = typeof(Branch.branchCD),
            DescriptionField = typeof(Branch.acctName)
        )]
        public int? BranchID { get; set; }
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion

        #region APAccountID
        [Account(DisplayName = "AP Account")]
        public int? APAccountID { get; set; }
        public abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
        #endregion

        #region APSubID
        [SubAccount(DisplayName = "AP Subaccount")]
        public int? APSubID { get; set; }
        public abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
        #endregion

        #region PayDate
        [PXDBDate]
        [PXUIField(DisplayName = "Pay Date")]
        public DateTime? PayDate { get; set; }
        public abstract class payDate : PX.Data.BQL.BqlDateTime.Field<payDate> { }
        #endregion

        #region VendorLocationID
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<vendorID>>>),
            DisplayName = "Vendor Location")]
        public int? VendorLocationID { get; set; }
        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        #endregion

        #region FinPeriodID
        [FinPeriodID]
        [PXUIField(DisplayName = "Fin. Period")]
        public string FinPeriodID { get; set; }
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        #endregion

        #region Status
        [PXDBString(1, IsFixed = true)]
        [PXDefault("H")]
        [PXStringList(
            new[] { "H", "B", "V", "S", "N", "C", "P", "K", "E", "R", "Z" },
            new[] { "On Hold", "Balanced", "Voided", "Scheduled", "Open", "Closed",
                    "Printed", "Pre-Released", "Pending Approval", "Rejected", "Reserved" })]
        [PXUIField(DisplayName = "Status")]
        public string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region APRefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "AP Ref. Nbr.")]
        public string APRefNbr { get; set; }
        public abstract class aPRefNbr : PX.Data.BQL.BqlString.Field<aPRefNbr> { }
        #endregion

        #region APDocType
        [PXDBString(3, IsUnicode = true)]
        [PXUIField(DisplayName = "AP Doc. Type")]
        public string APDocType { get; set; }
        public abstract class aPDocType : PX.Data.BQL.BqlString.Field<aPDocType> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion

        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
