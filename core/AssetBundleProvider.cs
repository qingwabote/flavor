using System.ComponentModel;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Flavor
{
    [DisplayName("Flavor AssetBundle Provider")]
    public class AssetBundleProvider : UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider
    {
        public override void Provide(ProvideHandle provideHandle)
        {
            Debug.Log($"[{typeof(AssetBundleProvider).FullName}] Minigame {CoreAPI.Minigame()}");
            if (!CoreAPI.Minigame())
            {
                base.Provide(provideHandle);
                return;
            }

            new AssetBundleResource().Start(provideHandle);
        }
    }
}