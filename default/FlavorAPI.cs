namespace Flavor
{
    public class FlavorAPI : BaseAPI
    {
        public const bool ShareAvailable = false;

        public const bool HubAvailable = false;

        public static void ShareAppMessage(ShareInfo info) { }

        public static void ShareAppMessage(ShareInfo info, string eventKey, int eventDim = 0) { }

        public static void ReportEvent(string key, int type, int dim = 0) { }

        public static void ReportAwake() { }

        public static void OpenHub(string v) { }
    }
}
