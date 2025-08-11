using Newtonsoft.Json.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;  // Add this for Account
using PX.Objects.PM;  // Add this for PMProject
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        // ADD THIS LINE - Action declaration
        public PXAction<APInvoiceStaging> createAPBill;

        private JObject BuildAPBillPayload()
        {
            var current = APStaging.Current;
            if (current == null) return null;

            // Get vendor code from VendorID
            string vendorCode = null;
            if (current.VendorID != null)
            {
                var vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                    .Select(this, current.VendorID);
                vendorCode = vendor != null ? ((Vendor)vendor).AcctCD : null;
            }

            // Build simplified header payload - only these 3 fields
            var payload = new JObject
            {
                ["Vendor"] = new JObject { ["value"] = vendorCode },
                ["VendorRef"] = new JObject { ["value"] = current.InvoiceNbr },
                ["Date"] = new JObject { ["value"] = current.DocDate?.ToString("yyyy-MM-dd") }
            };

            // Build simplified details array - now with 7 fields including Branch
            var detailsArray = new JArray();
            var details = Details.Select().RowCast<APInvoiceStagingDetail>();

            foreach (var detail in details)
            {
                // Get account code
                string accountCode = null;
                if (detail.AccountID != null)
                {
                    var account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>
                        .Select(this, detail.AccountID);
                    accountCode = account != null ? ((Account)account).AccountCD : null;
                }

                // Get subaccount code
                string subaccountCode = null;
                if (detail.SubID != null)
                {
                    var subaccount = PXSelect<PX.Objects.GL.Sub, Where<PX.Objects.GL.Sub.subID, Equal<Required<PX.Objects.GL.Sub.subID>>>>
                        .Select(this, detail.SubID);
                    subaccountCode = subaccount != null ? ((PX.Objects.GL.Sub)subaccount).SubCD : null;
                }

                // Get branch code for detail
                string branchCode = null;
                if (detail.BranchID != null)
                {
                    var branch = PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
                        .Select(this, detail.BranchID);
                    branchCode = branch != null ? ((Branch)branch).BranchCD : null;
                }

                // Include these 7 fields (added Branch)
                var detailObj = new JObject
                {
                    ["Amount"] = new JObject { ["value"] = detail.Amount },
                    ["Subaccount"] = new JObject { ["value"] = subaccountCode },
                    ["Description"] = new JObject { ["value"] = detail.TransactionDescr },
                    ["Account"] = new JObject { ["value"] = accountCode },
                    ["Qty"] = new JObject { ["value"] = detail.Qty },
                    ["UnitCost"] = new JObject { ["value"] = detail.UnitCost },
                    ["Branch"] = new JObject { ["value"] = branchCode }
                };

                detailsArray.Add(detailObj);
            }

            payload["Details"] = detailsArray;

            return payload;
        }

        private bool CreateAPBillSync(JObject payload)  // Remove async, change return type
        {
            try
            {
                // LOG: Starting API call process
                PXTrace.WriteInformation("Starting AP Bill creation process...");

                // Get Acumatica API token
                string token = GetAcumaticaTokenSync();

                // Use same pattern - disable proxy and set timeout
                var handler = new HttpClientHandler { UseProxy = false };
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(3) })
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var apiUrl = "https://hopelessly-noted-jay.ngrok-free.app/saga/entity/APStaging/24.200.001/Bill";
                    var jsonString = payload.ToString();
                    var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                    // LOG: Making AP Bill API call
                    PXTrace.WriteInformation($"Making AP Bill API call to: {apiUrl}");
                    PXTrace.WriteInformation($"Request payload size: {jsonString.Length} characters");

                    // Use .Result instead of await
                    var response = client.PutAsync(apiUrl, content).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    // LOG: API response details
                    PXTrace.WriteInformation($"AP Bill API Response Status: {response.StatusCode}");
                    PXTrace.WriteInformation($"AP Bill API Response Headers: {response.Headers}");
                    PXTrace.WriteInformation($"AP Bill API Response Body: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        PXTrace.WriteInformation("AP Bill created successfully via API");
                    }
                    else
                    {
                        PXTrace.WriteError($"AP Bill API call failed: {response.StatusCode} - {responseContent}");
                    }

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError($"Exception in CreateAPBillSync: {ex.Message}");
                PXTrace.WriteError($"Exception StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        private string GetAcumaticaTokenSync()
        {
            PXTrace.WriteInformation("Starting token request...");

            string tokenUrl = "https://hopelessly-noted-jay.ngrok-free.app/saga/identity/connect/token";

            // Use same pattern as your AuthService - disable proxy and set timeout
            var handler = new HttpClientHandler { UseProxy = false };
            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(3) })
            {
                PXTrace.WriteInformation($"Making token request to: {tokenUrl}");

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("grant_type", "password"),
                    new KeyValuePair<string,string>("client_id", "EF2EADF8-F569-7975-21FB-DE7BF43509E2@IYRES"),
                    new KeyValuePair<string,string>("client_secret", "5T3MpqMJE5vQnhgYc5CVdQ"),
                    new KeyValuePair<string,string>("username", "apiuser1"),
                    new KeyValuePair<string,string>("password", "Abc@1234"),
                    new KeyValuePair<string,string>("scope", "api")
                });

                // Use .Result instead of async/await
                HttpResponseMessage response = client.PostAsync(tokenUrl, form).Result;
                string body = response.Content.ReadAsStringAsync().Result;

                PXTrace.WriteInformation($"Token response status: {response.StatusCode}");
                PXTrace.WriteInformation($"Token response body: {body}");

                if (!response.IsSuccessStatusCode)
                {
                    PXTrace.WriteError($"Token request failed: {response.StatusCode} - {body}");
                    throw new Exception($"Failed to get token: {body}");
                }

                var json = JObject.Parse(body);
                string token = json["access_token"]?.ToString();

                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Access token not found in response");
                }

                PXTrace.WriteInformation("✅ Token obtained successfully");
                return token;
            }
        }


        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Create AP Bill")]
        public virtual IEnumerable CreateAPBill(PXAdapter adapter)
        {
            // Get current staging record
            var current = APStaging.Current;
            if (current == null)
            {
                throw new PXException(Messages.NoStagingRecordSelected);
            }

            try
            {
                // Build the JSON payload
                var payload = BuildAPBillPayload();
                if (payload == null)
                {
                    throw new PXException(Messages.APBillCreationFailed);
                }

                // LOG THE PAYLOAD - Pretty formatted (keep for debugging)
                string payloadString = payload.ToString(Newtonsoft.Json.Formatting.Indented);
                PXTrace.WriteInformation($"AP Bill Payload: {payloadString}");

                // MAKE THE ACTUAL API CALL
                var result = CreateAPBillSync(payload);

                if (result)
                {
                    // Success - show message but don't throw exception
                    PXTrace.WriteInformation(Messages.APBillCreationSuccess);
                    // Or use: this.Actions.PressSave(); // to show success in UI
                }
                else
                {
                    throw new PXException(Messages.APBillCreationFailed);
                }
            }
            catch (Exception ex)
            {
                throw new PXException(Messages.APBillCreationFailed);
            }

            return adapter.Get();
        }


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
