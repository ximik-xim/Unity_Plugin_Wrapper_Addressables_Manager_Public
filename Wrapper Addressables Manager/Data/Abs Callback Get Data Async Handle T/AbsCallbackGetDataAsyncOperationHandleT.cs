using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Абстракция которая вернет обьект T типа обернутый в AsyncOperationHandle. Тип указывается при обращении к методу.
/// Аргумент метода определяется при наследовании
/// (в основном используется для получения обьектов через  Addressables.LoadAssetAsync<T>)
/// </summary>
public abstract class AbsCallbackGetDataAsyncOperationHandleT<ArgData> : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;
    public abstract GetServerRequestData<AsyncOperationHandle<T>> GetData<T>(ArgData data);
}
