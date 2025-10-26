using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Абстракция которая вернет обьект T типа обернутый в AsyncOperationHandle. Тип T указывается при наследовании, как и принимаемый аргумент
/// </summary>
public abstract class AbsCallbackGetDataAsyncOperationHandle<GetType, ArgData> : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;
    public abstract GetServerRequestData<AsyncOperationHandle<GetType>> GetData(ArgData data);
}
