using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;

namespace APStaging
{
    [Serializable]
    [PXCacheName("AP Invoice Staging Payment")]
    public class APInvoiceStagingPayment : PXBqlTable, IBqlTable
    {
        #region StagingID (Primary Key + FK to Header)
        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(APInvoiceStaging.stagingID))]
        [PXParent(typeof(Select<APInvoiceStaging, Where<APInvoiceStaging.stagingID, Equal<Current<stagingID>>>>))]
        public int? StagingID { get; set; }
        public abstract class stagingID : PX.Data.BQL.BqlInt.Field<stagingID> { }
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
        [Account]
        [PXUIField(DisplayName = "AP Account")]
        public int? APAccountID { get; set; }
        public abstract class aPAccountID : PX.Data.BQL.BqlInt.Field<aPAccountID> { }
        #endregion

        #region APSubID
        [SubAccount(typeof(APInvoiceStagingPayment.aPAccountID))]
        [PXUIField(DisplayName = "AP Subaccount")]
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
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<APInvoiceStaging.vendorID>>>), DescriptionField = typeof(Location.descr))]
        [PXUIField(DisplayName = "Payment Location")]
        public int? VendorLocationID { get; set; }
        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime]
        public DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID]
        public Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID]
        public string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime]
        public DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID]
        public Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID]
        public string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region Tstamp
        [PXDBTimestamp]
        public byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion

        #region NoteID
        [PXNote]
        public Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }
}
