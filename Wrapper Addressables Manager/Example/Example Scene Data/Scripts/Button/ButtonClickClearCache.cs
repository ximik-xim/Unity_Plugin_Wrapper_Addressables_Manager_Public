using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

/// <summary>
/// Очистит кэш
/// (удалит загр. бандлы, но не загр. данные об каталоге, данные каталога наход. отдельно)
/// </summary>
public class ButtonClickClearCache : MonoBehaviour
{
    [SerializeField]
    private Button _button;
    
    private void Awake()
    {
        Init();
    }

    
    private void Init()
    {
        _button.onClick.AddListener(ButtonClick);
    }


    private void ButtonClick()
    {
        StartLogic();
    }

    private void StartLogic()
    {
#if !UNITY_EDITOR
        //удалит старые не используемые бандлы(не исп. бандлы могут появиться после обновления)
        Addressables.CleanBundleCache();
#endif
        
        //получаем все ключи(IResourceLocation) обьектов, котор есть в тек. каталоге
        List<IResourceLocation> listObjectCatalog = new List<IResourceLocation>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var VARIABLE in locator.AllLocations)
            {
                listObjectCatalog.Add(VARIABLE);
            }
        }

        foreach (var VARIABLE in listObjectCatalog)
        {
            //удалит загруженный ресурсы(бандл), по его ключу
            Addressables.ClearDependencyCacheAsync(VARIABLE);
        }
        
        //очистка кэша всей игры(удалит загр. бандлы)
        //Caching.ClearCache();
        
        //это если бы знали все имен бандлов
        //Caching.ClearAllCachedVersions
        
        //удалит папку где наход. загруженный каталог с сервера
        var path = System.IO.Path.Combine(Application.persistentDataPath, "com.unity.addressables");
        if (System.IO.Directory.Exists(path) == true)
        {
            System.IO.Directory.Delete(path, true);
        }
            
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(ButtonClick);
    }
}
