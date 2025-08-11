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
        public async Task HandleAsync(WebhookContext context, CancellationToken cancellation)
        {
            string body;
            //string eventType = null;
            string evidenceUrl = null;
            string guid = null;
            string responseMsg = "skipped";
            string documentUrl = null;

            // Read the webhook body
            using (var reader = new StreamReader(context.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            PXTrace.WriteInformation("Webhook Body: {0}", body);

            // Save payload to file
            string appData = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\WebhookLogs\\";
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            string filename = $"payload_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid().ToString().Substring(0, 8)}.txt";
            var filePath = Path.Combine(appData, filename);
            File.WriteAllText(filePath, body);

            // Parse and extract data
            try
            {
                var json = JObject.Parse(body);
                string eventType = json.Value<string>("event_type");
                string eventName = json.Value<string>("event");
                string documentGuid = json.Value<string>("document_guid");
                string storecoveToken = "6Yulb4v6ojlARzex4XZ9Wfu4TQkX6DcIa2-SogY730I"; // Or from config


                if (eventType == "received_document" && eventName == "received" && !string.IsNullOrEmpty(documentGuid))
                {
                    string receivedDocUrl = $"https://api.storecove.com/api/v2/received_documents/{documentGuid}";
                    PXTrace.WriteInformation("Received Document URL: {0}", receivedDocUrl);

                    // Save the URL to the log file
                    File.AppendAllText(filePath, Environment.NewLine + "Received Document URL: " + receivedDocUrl);

                    // --- Call Storecove received_document URL and log the payload ---
                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        // Add your Storecove Bearer token here!
                        httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storecoveToken);

                        var response = await httpClient.GetAsync(receivedDocUrl, cancellation);
                        if (response.IsSuccessStatusCode)
                        {
                            var receivedJson = await response.Content.ReadAsStringAsync();

                            // Log the JSON payload
                            PXTrace.WriteInformation("Received Document JSON: {0}", receivedJson);

                            // Save to a file (for auditing)
                            var receivedJsonFilename = Path.GetFileNameWithoutExtension(filePath) + "_received.json";
                            var receivedJsonPath = Path.Combine(appData, receivedJsonFilename);
                            File.WriteAllText(receivedJsonPath, receivedJson);
                            PXTrace.WriteInformation("Saved received document JSON to: {0}", receivedJsonPath);
                            // Parse Storecove JSON (already as string: receivedJson)
                            var storecoveObj = JObject.Parse(receivedJson);
                            var invoice = storecoveObj["document"]?["invoice"];
                            var supplier = invoice?["accounting_supplier_party"]?["party"];
                            var invoiceLines = invoice?["invoice_lines"] as JArray;

                            // Build Acumatica REST payload in memory
                            var acumaticaPayload = new JObject
                            {
                                ["VendorName"] = new JObject { ["value"] = (string)supplier?["company_name"] },
                                ["Date"] = new JObject { ["value"] = (string)invoice?["issue_date"] },
                                ["VendorRef"] = new JObject { ["value"] = (string)invoice?["invoice_number"] },
                                ["Details"] = new JArray()
                            };

                            foreach (var line in invoiceLines)
                            {
                                var detail = new JObject
                                {
                                    ["Quantity"] = new JObject { ["value"] = (decimal?)line["quantity"] ?? 0 },
                                    ["UnitPrice"] = new JObject { ["value"] = (decimal?)line["item_price"] ?? 0 },
                                    ["TransDesc"] = new JObject { ["value"] = (string)line["name"] },
                                    ["DiscountAmount"] = new JObject { ["value"] = 0 }
                                };
                                ((JArray)acumaticaPayload["Details"]).Add(detail);
                            }

                            // (OPTIONAL) Log it for debugging
                            PXTrace.WriteInformation("Acumatica payload: {0}", acumaticaPayload.ToString());
                            File.AppendAllText(filePath, Environment.NewLine + "Acumatica Payload (pretty):" + Environment.NewLine + acumaticaPayload.ToString(Newtonsoft.Json.Formatting.Indented));

                            // (1) Get Acumatica API token
                            string acumaticaToken = await GetAcumaticaTokenAsync(filePath);

                            // (2) POST to Acumatica endpoint
                            using (var apiClient = new System.Net.Http.HttpClient())
                            {
                                apiClient.DefaultRequestHeaders.Authorization =
                                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", acumaticaToken);

                                var jsonString = acumaticaPayload.ToString();
                                var apiUrl = "https://hopelessly-noted-jay.ngrok-free.app/saga/entity/APStaging/24.200.001/APStaging";
                                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                                var postResp = await apiClient.PutAsync(apiUrl, content);   

                                var postRespStr = await postResp.Content.ReadAsStringAsync();
                                PXTrace.WriteInformation("Acumatica PUT response: " + postRespStr);

                                // (Optional) Also log to file
                                File.AppendAllText(filePath, Environment.NewLine + "Acumatica PUT response:" + Environment.NewLine + postRespStr);
                            }



                            responseMsg = "success";
                        }
                        else
                        {
                            PXTrace.WriteError("Failed to call received_document URL: {0} {1}", (int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PXTrace.WriteError("Error parsing JSON or received_document response: {0}", ex.Message);
                responseMsg = "error";
            }




            // Respond with result + (optionally) the evidence/document URLs if event=succeeded
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            using (var writer = context.Response.CreateTextWriter())
            {
                writer.Write(JsonConvert.SerializeObject(new
                {
                    result = responseMsg,
                    guid,
                    evidenceUrl,
                    documentUrl
                }));
            }


        }

        public async Task<string> GetAcumaticaTokenAsync(string filePath)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var tokenUrl = "https://hopelessly-noted-jay.ngrok-free.app/saga/identity/connect/token";
                var values = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    //{ "client_id", "1738D622-97CE-8848-3D14-AE0D6EED9FFF@Publisher" },
                    { "client_id", "EF2EADF8-F569-7975-21FB-DE7BF43509E2@IYRES" },
                    //{ "client_secret", "1jEmrW2FE-Eh3gHA60R_NQ" },
                    { "client_secret", "5T3MpqMJE5vQnhgYc5CVdQ" },
                    //{ "username", "apiuser" },
                    { "username", "apiuser1" },
                    { "password", "Abc@1234" },
                    { "scope", "api" }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(tokenUrl, content);
                var respString = await response.Content.ReadAsStringAsync();

                // Log the response to file
                File.AppendAllText(filePath, Environment.NewLine + "Acumatica login response:" + Environment.NewLine + respString);

                if (!response.IsSuccessStatusCode)
                {
                    File.AppendAllText(filePath, Environment.NewLine + "Acumatica login FAILED.");
                    throw new Exception("Failed to login to Acumatica: " + respString);
                }

                var obj = JObject.Parse(respString);
                File.AppendAllText(filePath, Environment.NewLine + "Acumatica login SUCCESS. Access token received.");
                return obj.Value<string>("access_token");
            }
        }




    }
}
