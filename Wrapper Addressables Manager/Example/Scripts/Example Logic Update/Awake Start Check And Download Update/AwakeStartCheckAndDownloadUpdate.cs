using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Отвечает за запуск обновление
/// - каталогов
/// - обьектов
/// (желательно использовать в момент запуска(загрузки) игры)
/// </summary>
public class AwakeStartCheckAndDownloadUpdate : MonoBehaviour
{
   [SerializeField]
   private AbsCheckAndDownloadUpdateObject _checkAndDownloadUpdate;
   
    private void Awake()
    {
        if (_checkAndDownloadUpdate.IsInit == false)
        {
            _checkAndDownloadUpdate.OnInit += OnInitCheckAndDownloadUpdate;
        }
        
        CheckInit();
    }
    
    private void OnInitCheckAndDownloadUpdate()
    {
        if (_checkAndDownloadUpdate.IsInit == true)
        {
            _checkAndDownloadUpdate.OnInit -= OnInitCheckAndDownloadUpdate;
            CheckInit();
        }
    }
    
    private void CheckInit()
    {
        if (_checkAndDownloadUpdate.IsInit == true)
        {
            StartCheckUpdateCatalog();
        }
    }

    private void StartCheckUpdateCatalog()
    {
        _checkAndDownloadUpdate.StartCheckUpdateCatalog();
    }
}
