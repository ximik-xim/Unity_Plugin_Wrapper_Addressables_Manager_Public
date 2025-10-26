using UnityEngine;
using UnityEngine.AddressableAssets;


[System.Serializable]
public class DataAssetReferenceAndFilterInternalId
{
    public AssetReference RefObj => _refObj;
    [SerializeField]
    private AssetReference _refObj;
    
    /// <summary>
    /// Нужен что бы указать, какой имеенно IResourceLocation добовлять в список
    /// (локальный, тогда InternalId это путь до обьекта, пример Assets/Addressable Test/Example/Data Example/Example Object.prefab
    /// или путь который ведеть к серверу (тогда нач с http))
    /// ----------------------------------------------------------------------------
    /// (если пустой, то просто возмет все IResourceLocation(и локальный и для сервера)) 
    /// </summary>
    public string FilterInternalId_IResourceLocation => _filterInternalId_IResourceLocation;
    [SerializeField]
    private string _filterInternalId_IResourceLocation = "";
}
