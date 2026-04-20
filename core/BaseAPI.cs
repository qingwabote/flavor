using System.Threading.Tasks;

namespace Flavor
{
    public class BaseAPI
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            RegisterShowModalCallback(OnShowModalCallback);
            RegisterLoadSubpackageCallback(OnLoadSubpackageCallback);
        }

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_minigame")]
        public static extern bool Minigame();

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_applyUpdate")]
        public static extern void ApplyUpdate();

                [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_restart")]
        public static extern void Restart();

        private static int s_ShowModalHandleId;
        private static readonly System.Collections.Generic.Dictionary<int, TaskCompletionSource<bool>> s_ShowModalHandles = new();

        private delegate void ShowModalCallback(int handleId, int type);

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_registerShowModalCallback")]
        private static extern void RegisterShowModalCallback(ShowModalCallback callback);

        [AOT.MonoPInvokeCallback(typeof(ShowModalCallback))]
        private static void OnShowModalCallback(int handleId, int type)
        {
            if (!s_ShowModalHandles.Remove(handleId, out var task))
            {
                return;
            }
            task.TrySetResult(type != 0);
        }

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_showModal")]
        private static extern void ShowModal(int handleId, string title, string content, int showCancel);

        public static Task<bool> ShowModal(string title, string content, bool showCancel)
        {
            var handleId = ++s_ShowModalHandleId;
            var handle = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            s_ShowModalHandles[handleId] = handle;

            ShowModal(handleId, title, content, showCancel ? 1 : 0);
            return handle.Task;
        }


        private static int s_LoadSubpackageHandleId;
        private static readonly System.Collections.Generic.Dictionary<int, TaskCompletionSource<string>> s_LoadSubpackageHandles = new();

        private delegate void LoadSubpackageCallback(int handleId, System.IntPtr errMsgPtr);

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_loadSubpackage")]
        private static extern void LoadSubpackage(int handleId, string name);

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_registerLoadSubpackageCallback")]
        private static extern void RegisterLoadSubpackageCallback(LoadSubpackageCallback callback);

        [AOT.MonoPInvokeCallback(typeof(LoadSubpackageCallback))]
        private static void OnLoadSubpackageCallback(int handleId, System.IntPtr errMsgPtr)
        {
            if (!s_LoadSubpackageHandles.Remove(handleId, out var task))
            {
                return;
            }
            task.TrySetResult(System.Runtime.InteropServices.Marshal.PtrToStringUTF8(errMsgPtr));
        }

        public static Task<string> LoadSubpackage(string name)
        {
            var handleId = ++s_LoadSubpackageHandleId;
            var handle = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            s_LoadSubpackageHandles[handleId] = handle;

            LoadSubpackage(handleId, name);
            return handle.Task;
        }
#else
        public static bool Minigame() { return false; }

        public static void ApplyUpdate() { }

        public static void Restart() { }

        public static Task<bool> ShowModal(string title, string content, bool showCancel) { return Task.FromResult(true); }

        public static Task<string> LoadSubpackage(string name) { return Task.FromResult(string.Empty); }
#endif
    }
}
