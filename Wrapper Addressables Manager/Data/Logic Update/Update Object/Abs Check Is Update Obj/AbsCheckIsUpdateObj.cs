using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Нужна что бы проверить, можно ли обновлять обьект
/// (пример.
/// обьекту не нужно обновление
/// или запрещено обновлять данный обьект)
/// </summary>
public abstract class AbsCheckIsUpdateObj : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;

    public abstract GetServerRequestData<StorageStatusCallbackIResourceLocation> CheckIsUpdateObj(List<IResourceLocation> locatorsObjectUpdate);


}
