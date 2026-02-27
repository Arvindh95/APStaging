using Newtonsoft.Json.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace APStaging
{
    public class APInvoiceStagingMaint : PXGraph<APInvoiceStagingMaint, APInvoiceStaging>
    {
        // Main header view — Fluent BQL
        public SelectFrom<APInvoiceStaging>.View APStaging;

        // Detail view — Fluent BQL
        public SelectFrom<APInvoiceStagingDetail>
            .Where<APInvoiceStagingDetail.stagingID.IsEqual<APInvoiceStaging.stagingID.FromCurrent>>
            .View Details;

        public SelectFrom<APInvoiceStagingPayment>
            .Where<APInvoiceStagingPayment.stagingID.IsEqual<APInvoiceStaging.stagingID.FromCurrent>>
            .View PaymentInfo;

        public PXSetup<APStagingPreferences> Setup;

        private APStagingPreferences Prefs =>
            Setup.Current ?? throw new PXSetupNotEnteredException(Messages.PreferencesNotSetup, typeof(APStagingPreferences));

        // Toolbar actions
        public PXSave<APInvoiceStaging> Save;
        public PXCancel<APInvoiceStaging> Cancel;
        public PXDelete<APInvoiceStaging> Delete;
        public PXInsert<APInvoiceStagingDetail> InsertDetail;
        public PXDelete<APInvoiceStagingDetail> DeleteDetail;

        public PXAction<APInvoiceStaging> createAPBill;

        private JObject BuildAPBillPayload()
        {
            var current = APStaging.Current;
            if (current == null) return null;

            // Get vendor code — Fluent BQL
            string vendorCode = null;
            if (current.VendorID != null)
            {
                var vendor = SelectFrom<Vendor>
                    .Where<Vendor.bAccountID.IsEqual<@P.AsInt>>
                    .View.Select(this, current.VendorID);
                vendorCode = ((Vendor)vendor)?.AcctCD;
            }

            var payload = new JObject
            {
                ["Vendor"]    = new JObject { ["value"] = vendorCode },
                ["VendorRef"] = new JObject { ["value"] = current.InvoiceNbr },
                ["Date"]      = new JObject { ["value"] = current.DocDate?.ToString("yyyy-MM-dd") }
            };

            var detailsArray = new JArray();
            var details = Details.Select().RowCast<APInvoiceStagingDetail>();

            foreach (var detail in details)
            {
                // Get account code — Fluent BQL
                string accountCode = null;
                if (detail.AccountID != null)
                {
                    var account = SelectFrom<Account>
                        .Where<Account.accountID.IsEqual<@P.AsInt>>
                        .View.Select(this, detail.AccountID);
                    accountCode = ((Account)account)?.AccountCD;
                }

                // Get subaccount code — Fluent BQL
                string subaccountCode = null;
                if (detail.SubID != null)
                {
                    var sub = SelectFrom<PX.Objects.GL.Sub>
                        .Where<PX.Objects.GL.Sub.subID.IsEqual<@P.AsInt>>
                        .View.Select(this, detail.SubID);
                    subaccountCode = ((PX.Objects.GL.Sub)sub)?.SubCD;
                }

                // Get branch code — Fluent BQL
                string branchCode = null;
                if (detail.BranchID != null)
                {
                    var branch = SelectFrom<Branch>
                        .Where<Branch.branchID.IsEqual<@P.AsInt>>
                        .View.Select(this, detail.BranchID);
                    branchCode = ((Branch)branch)?.BranchCD;
                }

                // Amount is now persisted (PXDBDecimal), so detail.Amount is reliable
                detailsArray.Add(new JObject
                {
                    ["Amount"]               = new JObject { ["value"] = detail.Amount },
                    ["Subaccount"]           = new JObject { ["value"] = subaccountCode },
                    ["TransactionDescription"] = new JObject { ["value"] = detail.TransactionDescr },
                    ["Account"]              = new JObject { ["value"] = accountCode },
                    ["Qty"]                  = new JObject { ["value"] = detail.Qty },
                    ["UnitCost"]             = new JObject { ["value"] = detail.UnitCost },
                    ["Branch"]               = new JObject { ["value"] = branchCode }
                });
            }

            payload["Details"] = detailsArray;
            return payload;
        }

        private static bool CreateAPBillViaApi(JObject payload, APStagingPreferences prefs)
        {
            string token = null;
            try
            {
                PXTrace.WriteInformation("Starting AP Bill creation process...");
                token = GetAcumaticaToken(prefs);

                var handler = new HttpClientHandler { UseProxy = false };
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(3) })
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var baseUrl  = prefs.BaseUrl?.TrimEnd('/') ?? throw new PXException(Messages.BaseURLNotSet);
                    var endpoint = prefs.EndpointBill?.Trim()  ?? throw new PXException(Messages.BillEndpointNotSet);
                    var apiUrl   = $"{baseUrl}{endpoint}";

                    var jsonString = payload.ToString();
                    var content    = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                    PXTrace.WriteInformation($"Making AP Bill API call to: {apiUrl}");

                    var response        = client.PutAsync(apiUrl, content).GetAwaiter().GetResult();
                    var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    PXTrace.WriteInformation($"AP Bill API Response Status: {response.StatusCode}");
                    PXTrace.WriteInformation($"AP Bill API Response Body: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                        PXTrace.WriteError($"AP Bill API call failed: {response.StatusCode} - {responseContent}");

                    return response.IsSuccessStatusCode;
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(token))
                    LogoutFromAPI(token, prefs);
            }
        }

        private static string GetAcumaticaToken(APStagingPreferences prefs)
        {
            PXTrace.WriteInformation("Starting token request...");
            var baseUrl  = prefs.BaseUrl?.TrimEnd('/') ?? throw new PXException(Messages.BaseURLNotSet);
            var tokenUrl = $"{baseUrl}/identity/connect/token";

            var handler = new HttpClientHandler { UseProxy = false };
            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(3) })
            {
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type",    "password"),
                    new KeyValuePair<string,string>("client_id",     prefs.TokenClientId),
                    new KeyValuePair<string,string>("client_secret", prefs.TokenClientSecret),
                    new KeyValuePair<string,string>("username",      prefs.TokenUsername),
                    new KeyValuePair<string,string>("password",      prefs.TokenPassword),
                    new KeyValuePair<string,string>("scope",         string.IsNullOrWhiteSpace(prefs.TokenScope) ? "api" : prefs.TokenScope)
                });

                var response = client.PostAsync(tokenUrl, form).GetAwaiter().GetResult();
                var body     = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                    throw new PXException(Messages.TokenFailed);

                return JObject.Parse(body)["access_token"]?.ToString();
            }
        }

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create AP Bill")]
        public virtual IEnumerable CreateAPBill(PXAdapter adapter)
        {
            var current = APStaging.Current;
            if (current == null)
                throw new PXException(Messages.NoStagingRecordSelected);

            if (current.ProcessingStatus == "P")
                throw new PXException(Messages.RecordAlreadyProcessed);

            var payload = BuildAPBillPayload();
            if (payload == null)
                throw new PXException(Messages.APBillCreationFailed);

            PXTrace.WriteInformation($"AP Bill Payload: {payload.ToString(Newtonsoft.Json.Formatting.Indented)}");

            // Save current changes before launching background operation
            Actions.PressSave();

            var stagingID = current.StagingID;
            var prefs     = Prefs;

            // Use PXLongOperation so the HTTP call runs off the web request thread
            PXLongOperation.StartOperation(this, () =>
            {
                bool success = false;
                try
                {
                    success = CreateAPBillViaApi(payload, prefs);
                }
                catch (Exception ex)
                {
                    throw new PXException(Messages.APBillCreationFailed + " " + ex.Message, ex);
                }

                if (!success)
                    throw new PXException(Messages.APBillCreationFailed);

                // Update processing status inside the long operation using a fresh graph instance
                var graph = PXGraph.CreateInstance<APInvoiceStagingMaint>();
                APInvoiceStaging record = (APInvoiceStaging)graph.APStaging
                    .Search<APInvoiceStaging.stagingID>(stagingID);
                if (record != null)
                {
                    record.ProcessingStatus = "P";
                    graph.APStaging.Update(record);
                    graph.Save.Press();
                }

                PXTrace.WriteInformation(Messages.APBillCreationSuccess);
            });

            return adapter.Get();
        }

        private static void LogoutFromAPI(string token, APStagingPreferences prefs)
        {
            try
            {
                PXTrace.WriteInformation("Attempting to logout from API...");

                var baseUrl   = prefs.BaseUrl?.TrimEnd('/');
                if (string.IsNullOrEmpty(baseUrl)) return;

                // Logout URL uses the identity endpoint consistent with token endpoint
                var logoutUrl = $"{baseUrl}/identity/connect/endsession";

                var handler = new HttpClientHandler { UseProxy = false };
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) })
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = client.PostAsync(logoutUrl, null).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                        PXTrace.WriteInformation("Successfully logged out from API");
                    else
                        PXTrace.WriteWarning($"Logout response: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"Logout failed: {ex.Message}");
                // Don't throw — logout failure must not disrupt the main flow
            }
        }

        // FieldUpdated — use SetValueExt so downstream field events fire correctly
        protected void _(Events.FieldUpdated<APInvoiceStaging, APInvoiceStaging.vendorID> e)
        {
            var row = e.Row;
            if (row?.VendorID == null) return;

            // Fluent BQL — targeted query instead of loading all records
            var location = SelectFrom<Location>
                .Where<Location.bAccountID.IsEqual<@P.AsInt>
                    .And<Location.isDefault.IsEqual<True>>>
                .View.Select(this, row.VendorID);
            if (location != null)
                e.Cache.SetValueExt<APInvoiceStaging.vendorLocationID>(row, ((Location)location).LocationID);

            var vendor = SelectFrom<Vendor>
                .Where<Vendor.bAccountID.IsEqual<@P.AsInt>>
                .View.Select(this, row.VendorID);
            if (vendor != null)
            {
                e.Cache.SetValueExt<APInvoiceStaging.termsID>(row, ((Vendor)vendor).TermsID);
                e.Cache.SetValueExt<APInvoiceStaging.curyID>(row,  ((Vendor)vendor).CuryID);
            }
        }

        // RowPersisting — efficient targeted BQL instead of loading all vendors
        protected void _(Events.RowPersisting<APInvoiceStaging> e)
        {
            var row = e.Row;
            if (row == null || row.VendorID != null || string.IsNullOrWhiteSpace(row.VendorName))
                return;

            // 1. Try exact match via SQL
            var vendor = (Vendor)SelectFrom<Vendor>
                .Where<Vendor.acctName.IsEqual<@P.AsString>>
                .View.Select(this, row.VendorName);

            // 2. Try LIKE match via SQL — avoids loading all vendors into memory
            if (vendor == null)
            {
                vendor = (Vendor)SelectFrom<Vendor>
                    .Where<Vendor.acctName.IsLike<@P.AsString>>
                    .View.Select(this, $"%{row.VendorName}%");
            }

            if (vendor != null)
                row.VendorID = vendor.BAccountID;
        }

        // Ensure Amount is always calculated before save (covers API/webhook inserts where PXFormula may not fire)
        protected void _(Events.RowPersisting<APInvoiceStagingDetail> e)
        {
            var row = e.Row;
            if (row == null) return;

            if (row.Amount == null || row.Amount == 0m)
            {
                row.Amount = (row.Qty ?? 0m) * (row.UnitCost ?? 0m) - (row.DiscountAmt ?? 0m);
            }
        }
    }
}
