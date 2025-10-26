using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Эта абстракция нужна, что бы можно было реализовать разные способы получения ресурсов игры через Addressables
/// - В аргумент вместо object можно передать
/// 1) string (key - ключ, который вручную задаем)
/// 2) AssetReference (прям ссылку)
/// 3) IResourceLocation (можно получить через  Addressables.LoadResourceLocationsAsync)
/// ! НЕ работает с
/// 1) GUID (можно получить к примеру черз тот же AssetReference (экземпляр).AssetGUID)
/// </summary>
public abstract class AbsCallbackGetDataTAddressables : AbsCallbackGetDataAsyncOperationHandleT<object>
{
    
}
