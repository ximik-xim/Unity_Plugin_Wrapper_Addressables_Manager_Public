
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Пример запуска обновлений обьектов, через список(список разреш. обьектов для обн)
/// </summary>
public class ExampleStartUpdateListObject : MonoBehaviour
{
    /// <summary>
    ///  Список обьектов, которые можно обновлять
    /// </summary>
    [SerializeField] 
    private SOStorageBoolIsGetAddressablesObject _storageIsGetObject;

    /// <summary>
    /// Логика для обновления указ обьектов
    /// (с проверкой, а нужно ли обновл. указанным обьектам)
    /// </summary>
    [SerializeField]
    private CheckAndDownloadUpdateObjectTargetListObj _checkAndDownloadUpdateObject;

    private void Awake()
    {
        if (_storageIsGetObject.IsInit == false)
        {
            _storageIsGetObject.OnInit += OnInitStorageIsGetObject;
        }

        if (_checkAndDownloadUpdateObject.IsInit == false)
        {
            _checkAndDownloadUpdateObject.OnInit += OnInitСheckAndDownloadUpdateObject;
        }

        CheckInit();
    }

    private void OnInitStorageIsGetObject()
    {
        if (_storageIsGetObject.IsInit == true)
        {
            _storageIsGetObject.OnInit -= OnInitStorageIsGetObject;
            CheckInit();
        }
    }

    private void OnInitСheckAndDownloadUpdateObject()
    {
        if (_checkAndDownloadUpdateObject.IsInit == true)
        {
            _checkAndDownloadUpdateObject.OnInit -= OnInitСheckAndDownloadUpdateObject;
            CheckInit();
        }
    }

    private void CheckInit()
    {
        if (_storageIsGetObject.IsInit == true && _checkAndDownloadUpdateObject.IsInit == true)
        {
            StartLogic();
        }
    }

    private void StartLogic()
    {
        List<IResourceLocation> listResourceLocation = new List<IResourceLocation>();
        foreach (var VARIABLE in _storageIsGetObject.GetAllObject())
        {
            listResourceLocation.Add(VARIABLE);
        }
        
        //Запускаем логику обновления для обьектов(взятых из списка с рашен. обьектами для обновл)
        var dataCallback = _checkAndDownloadUpdateObject.StartCheckAndUpdateObject(listResourceLocation);

        if (dataCallback.IsGetDataCompleted == true)
        {
            CompletedCallback();
        }
        else
        {
            dataCallback.OnGetDataCompleted -= OnCompletedCallback;
            dataCallback.OnGetDataCompleted += OnCompletedCallback;
        }

        void OnCompletedCallback()
        {
            //Если данные пришли
            if (dataCallback.IsGetDataCompleted == true)
            {
                dataCallback.OnGetDataCompleted -= OnCompletedCallback;
                //начинаю обработку данных
                CompletedCallback();
            }
        }

        void CompletedCallback()
        {
            if (dataCallback.StatusServer == StatusCallBackServer.Ok)
            {
                if (dataCallback.GetData != null)
                {
                    if (dataCallback.GetData.StatusAllCallBack == TypeStorageStatusCallbackIResourceLocator.Ok)
                    {
                        Debug.Log("УХУ Обнова удалась УХУ");
                        
                    }
                    else
                    {
                        Debug.Log("Увы, что то сдохло, надо идти чинить");
                    }
                    
                    return;
                }
                else
                {
                    Debug.Log("УХУ Обнова удалась УХУ");
                    return;
                }
            }

            Debug.Log("Увы, что то сдохло, надо идти чинить");
        }
    }
}
