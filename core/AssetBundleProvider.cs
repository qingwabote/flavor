using System.ComponentModel;
using System.IO;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Flavor
{
    [DisplayName("Flavor AssetBundle Provider")]
    public class AssetBundleProvider : UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider
    {
        public override async void Provide(ProvideHandle provideHandle)
        {
            if (!BaseAPI.Minigame())
            {
                base.Provide(provideHandle);
                return;
            }

            var name = Path.GetFileNameWithoutExtension(provideHandle.Location.InternalId);
            var err = await BaseAPI.LoadSubpackage(name);
            if (err.Length > 0)
            {
                provideHandle.Complete<AssetBundleResource>(null, status: false, new($"[{typeof(AssetBundleProvider).FullName} Provide] [{typeof(BaseAPI).FullName} LoadSubpackageAsync {name}] {err}"));
                return;
            }

            new AssetBundleResource().Start(provideHandle);
        }

        public override void Release(IResourceLocation location, object asset)
        {
            if (!BaseAPI.Minigame())
            {
                base.Release(location, asset);
                return;
            }

            (asset as AssetBundleResource).GetAssetBundle().Unload(true);
        }
    }
}
