using System;
using TTSDK;

namespace Flavor
{
    public class FlavorAPI
    {
        // public struct Layout
        // {
        //     public double width;
        //     public double height;
        //     public double top;
        //     public double right;
        //     public double bottom;
        //     public double left;
        // }

        // public Layout GetMenuButtonLayout()
        // {
        //     var res = TT.GetMenuButtonLayout();
        //     return new()
        //     {
        //         width = (double)res["width"],
        //         height = (double)res["height"],
        //         top = (double)res["top"],
        //         right = (double)res["right"],
        //         bottom = (double)res["bottom"],
        //         left = (double)res["left"]
        //     };
        // }

        public const bool ShareAvailable = true;

        public class ShareInfo
        {
            public Action Success;
            public Action Failure;
            public Action Cancel;
        }

        public static void ShareAppMessage(ShareInfo info)
        {
            TT.ShareAppMessage(TTSDK.UNBridgeLib.LitJson.JsonMapper.ToObject(@"{}"), (p) => { info.Success?.Invoke(); }, (p) => { info.Failure?.Invoke(); }, () => { info.Cancel?.Invoke(); });
        }

        public static void OnShareAppMessage(Func<ShareInfo> callback)
        {
            TT.OnShareAppMessage((p) =>
            {
                var info = callback();
                return new(new(), (p) => { info.Success?.Invoke(); }, (p) => { info.Failure?.Invoke(); }, () => { info.Cancel?.Invoke(); });
            });
        }
    }
}
