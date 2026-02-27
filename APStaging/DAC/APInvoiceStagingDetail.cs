using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;

namespace APStaging
{
    [Serializable]
    [PXCacheName("AP Invoice Staging Detail")]
    public class APInvoiceStagingDetail : PXBqlTable, IBqlTable
    {
        #region DetailID
        [PXDBIdentity(IsKey = true)]
        public int? DetailID { get; set; }
        public abstract class detailID : PX.Data.BQL.BqlInt.Field<detailID> { }
        #endregion

        #region StagingID
        [PXDBInt]
        [PXDBDefault(typeof(APInvoiceStaging.stagingID))]
        [PXParent(typeof(Select<APInvoiceStaging, Where<APInvoiceStaging.stagingID, Equal<Current<APInvoiceStagingDetail.stagingID>>>>))]
        [PXUIField(DisplayName = "Staging ID", Visible = false)]
        public virtual int? StagingID { get; set; }
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

        #region TransactionDescr
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Transaction Descr.")]
        public string TransactionDescr { get; set; }
        public abstract class transactionDescr : PX.Data.BQL.BqlString.Field<transactionDescr> { }
        #endregion

        #region Qty
        [PXDBDecimal]
        [PXUIField(DisplayName = "Quantity")]
        public decimal? Qty { get; set; }
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
        #endregion

        #region UnitCost
        [PXDBDecimal]
        [PXUIField(DisplayName = "Unit Price")]
        public decimal? UnitCost { get; set; }
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        #endregion

        #region DiscountAmt
        [PXDBDecimal]
        [PXUIField(DisplayName = "Discount Amount")]
        public decimal? DiscountAmt { get; set; }
        public abstract class discountAmt : PX.Data.BQL.BqlDecimal.Field<discountAmt> { }
        #endregion

        #region Amount
        [PXDBDecimal(2)]
        [PXFormula(typeof(Sub<Mult<qty, unitCost>, discountAmt>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount", Enabled = false)]
        public decimal? Amount { get; set; }
        public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
        #endregion

        #region AccountID
        [Account]
        [PXUIField(DisplayName = "Account")]
        public int? AccountID { get; set; }
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
        #endregion

        #region SubID
        [SubAccount(typeof(accountID))]
        [PXUIField(DisplayName = "Subaccount")]
        public int? SubID { get; set; }
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
        #endregion

        #region ProjectID
        [ProjectBase]
        [PXUIField(DisplayName = "Project")]
        public int? ProjectID { get; set; }
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
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
