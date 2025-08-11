using System;
using PX.Data;

namespace APStaging
{
    [Serializable]
    [PXCacheName("AP Staging Preferences")]
    public class APStagingPreferences : PXBqlTable, IBqlTable
    {
        #region BaseUrl (e.g. https://yourdomain/saga)
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Acumatica Base URL")]
        public string BaseUrl { get; set; }
        public abstract class baseUrl : PX.Data.BQL.BqlString.Field<baseUrl> { }
        #endregion

        #region TokenClientId
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Client ID")]
        public string TokenClientId { get; set; }
        public abstract class tokenClientId : PX.Data.BQL.BqlString.Field<tokenClientId> { }
        #endregion

        #region TokenClientSecret
        [PXRSACryptString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Client Secret")]
        public string TokenClientSecret { get; set; }
        public abstract class tokenClientSecret : PX.Data.BQL.BqlString.Field<tokenClientSecret> { }
        #endregion

        #region TokenUsername
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Username")]
        public string TokenUsername { get; set; }
        public abstract class tokenUsername : PX.Data.BQL.BqlString.Field<tokenUsername> { }
        #endregion

        #region TokenPassword
        [PXRSACryptString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Password")]
        public string TokenPassword { get; set; }
        public abstract class tokenPassword : PX.Data.BQL.BqlString.Field<tokenPassword> { }
        #endregion

        #region TokenScope
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Scope")]
        public string TokenScope { get; set; }
        public abstract class tokenScope : PX.Data.BQL.BqlString.Field<tokenScope> { }
        #endregion

        #region EndpointAPStaging (e.g. /entity/APStaging/24.200.001/APStaging)
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Entity Endpoint (APStaging)")]
        public string EndpointAPStaging { get; set; }
        public abstract class endpointAPStaging : PX.Data.BQL.BqlString.Field<endpointAPStaging> { }
        #endregion

        #region EndpointBill (e.g. /entity/APStaging/24.200.001/Bill)
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Action Endpoint (Bill)")]
        public string EndpointBill { get; set; }
        public abstract class endpointBill : PX.Data.BQL.BqlString.Field<endpointBill> { }
        #endregion

        #region StorecoveBaseUrl (e.g. https://api.storecove.com/api/v2)
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Storecove Base URL")]
        public string StorecoveBaseUrl { get; set; }
        public abstract class storecoveBaseUrl : PX.Data.BQL.BqlString.Field<storecoveBaseUrl> { }
        #endregion

        #region StorecoveToken
        [PXRSACryptString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Storecove Token")]
        public string StorecoveToken { get; set; }
        public abstract class storecoveToken : PX.Data.BQL.BqlString.Field<storecoveToken> { }
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
