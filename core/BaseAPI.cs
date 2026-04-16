using System.Threading.Tasks;

namespace Flavor
{
    public class BaseAPI
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            s_LoadSubpackageHandleId = 0;
            s_LoadSubpackageHandles.Clear();
            RegisterLoadSubpackageCallback(OnLoadSubpackageCallback);
        }

        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_minigame")]
        public static extern bool Minigame();

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

        public static Task<string> LoadSubpackage(string name) { return Task.FromResult(string.Empty); }
#endif
    }
}
