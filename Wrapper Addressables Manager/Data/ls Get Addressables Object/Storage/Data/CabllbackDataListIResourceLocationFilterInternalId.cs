using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


public class CabllbackDataListIResourceLocationFilterInternalId
{
    public CabllbackDataListIResourceLocationFilterInternalId(AsyncOperationHandle<IList<IResourceLocation>> callbackData, string filterInternalId_IResourceLocation)
    {
        _callbackData = callbackData;
        _filterInternalId_IResourceLocation = filterInternalId_IResourceLocation;
    }
    
    public AsyncOperationHandle<IList<IResourceLocation>> CallbackData => _callbackData;
    private AsyncOperationHandle<IList<IResourceLocation>> _callbackData;

    /// <summary>
    /// Нужен что бы указать, какой именно IResourceLocation добавлять в список
    /// (локальный, тогда InternalId это путь до обьекта, пример Assets/Addressable Test/Example/Data Example/Example Object.prefab
    /// или путь который ведеть к серверу (тогда нач с http))
    /// ----------------------------------------------------------------------------
    /// (если пустой, то просто возмет все IResourceLocation(и локальный и для сервера)) 
    /// </summary>
    public string FilterInternalId_IResourceLocation => _filterInternalId_IResourceLocation;
    private string _filterInternalId_IResourceLocation;
}