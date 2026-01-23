using System;
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

        public static void OnShareAppMessage(Action action)
        {
            var param = new WXShareAppMessageParam();
            WX.OnShareAppMessage(param, (p) => { action(); });
        }
    }
}
