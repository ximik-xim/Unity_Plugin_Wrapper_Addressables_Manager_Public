using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// Эта абстракция нужна, что бы можно было реализовать разные способы получения сцены через Addressables
/// - В аргументы передаются настройки для загрузки сцен
/// </summary>
public abstract class AbsCallbackGetSceneAddressables : AbsCallbackGetDataAsyncOperationHandle<SceneInstance, DataSceneLoadAddressable>
{
    
}
