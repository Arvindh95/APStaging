using PX.Data;

namespace APStaging
{
    public class APStagingSetupMaint : PXGraph<APStagingSetupMaint>
    {
        public PXSetup<APStagingPreferences> Setup;

        public PXSave<APStagingPreferences>   Save;
        public PXCancel<APStagingPreferences> Cancel;
    }
}
