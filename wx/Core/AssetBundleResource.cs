using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using WeChatWASM;

namespace Flavor
{
    public class AssetBundleResource : IAssetBundleResource
    {
        private AssetBundle m_Bundle;

        public AssetBundle GetAssetBundle()
        {
            return m_Bundle;
        }

        public void Start(ProvideHandle provideHandle)
        {
            var path = $"{provideHandle.ResourceManager.TransformInternalId(provideHandle.Location)}.txt";
            Debug.Log($"[{typeof(AssetBundleResource).FullName}] Start {path}");
            var fs = WX.GetFileSystemManager();
            fs.ReadFile(new()
            {
                filePath = path,
                fail = (e) =>
                {
                    provideHandle.Complete<AssetBundleResource>(null, status: false, new($"[{typeof(AssetBundleResource).FullName}] {e.errMsg}"));
                },
                success = (res) =>
                {
                    m_Bundle = AssetBundle.LoadFromMemory(res.binData);
                    provideHandle.Complete(this, status: true, null);
                }
            });
        }
    }
}