using System.ComponentModel;
using System.IO;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Flavor
{
    [DisplayName("Flavor AssetBundle Provider")]
    public class AssetBundleProvider : UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider
    {
        public override async void Provide(ProvideHandle provideHandle)
        {
            await BaseAPI.LoadSubpackage(Path.GetFileNameWithoutExtension(provideHandle.Location.InternalId));
            base.Provide(provideHandle);
        }
    }
}
