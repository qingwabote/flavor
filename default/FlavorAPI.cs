using System;

namespace Flavor
{
    public class FlavorAPI
    {
        public const bool ShareAvailable = false;

        public class ShareInfo
        {
            public Action Success;
            public Action Failure;
            public Action Cancel;
        }

        public static void ShareAppMessage(ShareInfo info) { }
    }
}
