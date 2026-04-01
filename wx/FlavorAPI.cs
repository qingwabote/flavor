using System;
using Flavor.Core;
using UnityEngine;
using WeChatWASM;

namespace Flavor
{
    public class FlavorAPI
    {
        public const bool ShareAvailable = true;

        public const bool HubAvailable = true;

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

        private static WXPageManager s_PageManager;

        public static void OpenHub(string link)
        {
            s_PageManager ??= WX.CreatePageManager();

            s_PageManager.Load(new()
            {
                openlink = link,
                success = (res) =>
                {
                    s_PageManager.Show(new()
                    {
                        fail = (res) =>
                        {
                            Debug.LogError($"PageManager: {res.errMsg}");
                        }
                    });
                },
                fail = (res) =>
                {
                    Debug.LogError($"PageManager: {res.errMsg}");
                }
            });
        }
    }
}
