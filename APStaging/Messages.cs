using PX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APStaging
{
    [PXLocalizable]
    public class Messages
    {
        public const string APBillCreated = "AP Bill created successfully.";
        public const string APBillCreationSuccess = "Successfully created an AP Bill";
        public const string APBillCreationFailed = "Failed to create AP Bill";
        public const string NoStagingRecordSelected = "No staging record selected.";
        public const string ButtonWorking = "Button working! Implementation coming in next steps.";
        public const string PreferencesNotSetup = "AP Staging Preferences have not been configured. Please set them up before proceeding.";
        public const string BaseURLNotSet = "Base URL is not set. Please configure the Base URL in the AP Staging Preferences.";
        public const string TokenFailed = "Token retrieval failed. Please check your API credentials and try again.";
        public const string BillEndpointNotSet = "Bill endpoint is not set. Please configure the Bill endpoint in the AP Staging Preferences.";
        public const string RecordAlreadyProcessed = "This staging record has already been processed and an AP Bill has been created.";
        public const string StatusUpdatedToProcessed = "Processing status updated to Processed.";

    }
}
