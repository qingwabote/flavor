using System;
using Flavor.Core;
using WeChatWASM;

namespace Flavor
{
    public class FlavorAPI
    {
        public const bool ShareAvailable = true;

        public class ShareInfo
        {
            public Action Success;
            public Action Failure;
            public Action Cancel;
        }

        public static void ShareAppMessage(ShareInfo info)
        {
            WX.ShareAppMessage(new());
            info.Success?.Invoke();
        }

        public static void ShareAppMessage(ShareInfo info, string eventKey, int eventDim = 0)
        {
            var e = ReportEvents.Instance.Get(eventKey);
            WX.ShareAppMessage(new()
            {
                query = $"wxgamebranchid={e.ID}&wxgamebranchdim={eventDim}"
            });
            info.Success?.Invoke();
        }

        public static void OnShareAppMessage(Action action)
        {
            var param = new WXShareAppMessageParam();
            WX.OnShareAppMessage(param, (p) => { action(); });
        }

        public static void ReportEvent(string key, int type, int dim = 0)
        {
            var e = ReportEvents.Instance.Get(key, type);
            WX.ReportUserBehaviorBranchAnalytics(new()
            {
                branchId = e.ID,
                eventType = type,
                branchDim = dim.ToString()
            });
        }
    }
}
