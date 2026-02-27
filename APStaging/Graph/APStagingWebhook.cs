using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Api.Webhooks;
using PX.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace APStaging
{
    public class APStagingWebhookHandler : IWebhookHandler
    {
        // Shared HttpClient — avoids socket exhaustion from per-call instantiation
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task HandleAsync(WebhookContext context, CancellationToken cancellation)
        {
            string body;
            string evidenceUrl = null;
            string guid = null;
            string responseMsg = "skipped";
            string documentUrl = null;

            using (var reader = new StreamReader(context.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            PXTrace.WriteInformation("Webhook Body: {0}", body);

            try
            {
                APStagingPreferences prefs = GetPreferences();
                string storecoveToken = prefs?.StorecoveToken ?? throw new Exception("Storecove token not configured");

                var json = JObject.Parse(body);
                string eventType    = json.Value<string>("event_type");
                string eventName    = json.Value<string>("event");
                string documentGuid = json.Value<string>("document_guid");

                if (eventType == "received_document" && eventName == "received" && !string.IsNullOrEmpty(documentGuid))
                {
                    string storecoveBaseUrl = prefs?.StorecoveBaseUrl?.TrimEnd('/') ?? "https://api.storecove.com/api/v2";
                    string receivedDocUrl   = $"{storecoveBaseUrl}/received_documents/{documentGuid}";
                    PXTrace.WriteInformation("Received Document URL: {0}", receivedDocUrl);

                    // Fetch document from Storecove
                    var storecoveRequest = new HttpRequestMessage(HttpMethod.Get, receivedDocUrl);
                    storecoveRequest.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storecoveToken);

                    var response = await _httpClient.SendAsync(storecoveRequest, cancellation);
                    if (!response.IsSuccessStatusCode)
                    {
                        PXTrace.WriteError("Failed to call received_document URL: {0} {1}",
                            (int)response.StatusCode, response.ReasonPhrase);
                        responseMsg = "error";
                    }
                    else
                    {
                        var receivedJson = await response.Content.ReadAsStringAsync();
                        PXTrace.WriteInformation("Received Document JSON: {0}", receivedJson);

                        var storecoveObj  = JObject.Parse(receivedJson);
                        var invoice       = storecoveObj["document"]?["invoice"];
                        var supplier      = invoice?["accounting_supplier_party"]?["party"];
                        var invoiceLines  = invoice?["invoice_lines"] as JArray;

                        var acumaticaPayload = new JObject
                        {
                            ["VendorName"] = new JObject { ["value"] = (string)supplier?["company_name"] },
                            ["Date"]       = new JObject { ["value"] = (string)invoice?["issue_date"] },
                            ["VendorRef"]  = new JObject { ["value"] = (string)invoice?["invoice_number"] },
                            ["InvoiceNbr"] = new JObject { ["value"] = (string)invoice?["invoice_number"] },
                            ["Details"]    = new JArray()
                        };

                        if (invoiceLines != null)
                        {
                            foreach (var line in invoiceLines)
                            {
                                ((JArray)acumaticaPayload["Details"]).Add(new JObject
                                {
                                    ["Quantity"]       = new JObject { ["value"] = (decimal?)line["quantity"] ?? 0 },
                                    ["UnitPrice"]      = new JObject { ["value"] = (decimal?)line["item_price"] ?? 0 },
                                    ["TransDesc"]      = new JObject { ["value"] = (string)line["name"] },
                                    ["DiscountAmount"] = new JObject { ["value"] = 0 }
                                });
                            }
                        }

                        PXTrace.WriteInformation("Acumatica payload: {0}", acumaticaPayload.ToString());

                        // Get Acumatica token and POST the staging record
                        string acumaticaToken = await GetAcumaticaTokenFromPrefsAsync(prefs);

                        var baseUrl  = prefs?.BaseUrl?.TrimEnd('/')   ?? throw new Exception("Base URL not configured");
                        var endpoint = prefs?.EndpointAPStaging?.Trim() ?? throw new Exception("APStaging endpoint not configured");
                        var apiUrl   = $"{baseUrl}{endpoint}";

                        var apiRequest = new HttpRequestMessage(HttpMethod.Put, apiUrl)
                        {
                            Content = new StringContent(acumaticaPayload.ToString(),
                                System.Text.Encoding.UTF8, "application/json")
                        };
                        apiRequest.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", acumaticaToken);

                        var postResp    = await _httpClient.SendAsync(apiRequest, cancellation);
                        var postRespStr = await postResp.Content.ReadAsStringAsync();
                        PXTrace.WriteInformation("Acumatica PUT response ({0}): {1}", (int)postResp.StatusCode, postRespStr);

                        if (!postResp.IsSuccessStatusCode)
                        {
                            PXTrace.WriteError("Acumatica PUT failed: {0} - {1}", (int)postResp.StatusCode, postRespStr);
                            responseMsg = "error";
                        }
                        else
                        {
                            responseMsg = "success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error in webhook: {0}", ex.Message);
                responseMsg = "error";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = 200;
            using (var writer = context.Response.CreateTextWriter())
            {
                writer.Write(JsonConvert.SerializeObject(new
                {
                    result      = responseMsg,
                    guid,
                    evidenceUrl,
                    documentUrl
                }));
            }
        }

        private APStagingPreferences GetPreferences()
        {
            var graph = PXGraph.CreateInstance<PXGraph>();
            var setup = new PXSetup<APStagingPreferences>(graph);
            return setup.Current ?? throw new Exception("AP Staging Preferences not configured");
        }

        private async Task<string> GetAcumaticaTokenFromPrefsAsync(APStagingPreferences prefs)
        {
            var baseUrl  = prefs?.BaseUrl?.TrimEnd('/') ?? throw new Exception("Base URL not configured");
            var tokenUrl = $"{baseUrl}/identity/connect/token";

            var values = new Dictionary<string, string>
            {
                { "grant_type",    "password" },
                { "client_id",     prefs?.TokenClientId     ?? throw new Exception("Client ID not configured") },
                { "client_secret", prefs?.TokenClientSecret ?? throw new Exception("Client secret not configured") },
                { "username",      prefs?.TokenUsername      ?? throw new Exception("Username not configured") },
                { "password",      prefs?.TokenPassword      ?? throw new Exception("Password not configured") },
                { "scope",         string.IsNullOrWhiteSpace(prefs?.TokenScope) ? "api" : prefs.TokenScope }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(values)
            };

            var response   = await _httpClient.SendAsync(request);
            var respString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to login to Acumatica: " + respString);

            return JObject.Parse(respString).Value<string>("access_token");
        }
    }
}
