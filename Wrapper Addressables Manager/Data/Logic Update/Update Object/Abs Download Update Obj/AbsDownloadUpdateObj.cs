using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Нужна, что бы скачать обновление для указ. обьектов
/// </summary>
public abstract class AbsDownloadUpdateObj : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;

    public abstract GetServerRequestData<StorageStatusCallbackIResourceLocation> DownloadUpdateObj(List<IResourceLocation> locatorsObjectUpdate);
}
