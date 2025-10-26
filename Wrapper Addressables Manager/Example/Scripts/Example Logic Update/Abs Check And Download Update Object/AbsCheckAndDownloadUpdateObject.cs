using System;
using UnityEngine;

/// <summary>
/// Нужна, что бы реализовать разные подходы к обновлению данных об обьектах
/// </summary>
public abstract class AbsCheckAndDownloadUpdateObject : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;

    public abstract GetServerRequestData<StorageStatusCallbackIResourceLocation> StartCheckUpdateCatalog();
}
