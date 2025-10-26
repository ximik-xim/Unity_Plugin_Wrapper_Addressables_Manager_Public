using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;

/// <summary>
/// Отвечает за запуск обновление каталогов
/// </summary>
public abstract class AbsUpdateCatalogs : MonoBehaviour
{
   public abstract bool IsInit { get; }
   public abstract event Action OnInit;
   
   public abstract GetServerRequestData<StorageStatusCallbackIResourceLocation> StartUpdateCatalog(List<string> idCatalogUpdate);
}



