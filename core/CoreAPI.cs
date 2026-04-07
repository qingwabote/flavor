namespace Flavor
{
    public class CoreAPI
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal", EntryPoint = "flavor_minigame")]
        public static extern bool Minigame();
#else
        public static bool Minigame() { return false; }
#endif
    }
}