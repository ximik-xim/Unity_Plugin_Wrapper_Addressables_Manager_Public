using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Проверяет и загружает обновление для списка обьектов(точнее их интерфеиса IResourceLocation)
/// </summary>
public class CheckAndDownloadUpdateObjectTargetListObj : MonoBehaviour
{
    public bool IsInit => _isInit;
    private bool _isInit = false;
    public event Action OnInit;
    
    /// <summary>
    /// Проверит можно ли обновить этот обьект и есть ли для него обновления
    /// </summary>
    [SerializeField]
    private AbsCheckIsUpdateObj _absCheckIsUpdateObj;
    
    /// <summary>
    ///  Загрузка обновлений у обьектов
    /// </summary>
    [SerializeField]
    private AbsDownloadUpdateObj _absDownloadUpdateObj;
    
    /// <summary>
    /// Обертка над Callback, для возможности выполнить цепочку операции и вернуть в конце результат
    /// </summary>
    private CallbackStorageStatusIResourceLocationAddressablesWrapper _wrapperCallbackData;
    
    /// <summary>
    /// Блокировка, т.к не подразумаеваться многораз. использ.(подрят) этого класса
    /// </summary>
    public bool IsBlock => _isBlock;
    private bool _isBlock;
    public event Action OnUpdateStatusBlock;
    
    private void Awake()
    {
        if (_absCheckIsUpdateObj.IsInit == false)
        {
            _absCheckIsUpdateObj.OnInit += OnInitAbsCheckIsUpdateObj;
        }

        if (_absDownloadUpdateObj.IsInit == false)
        {
            _absDownloadUpdateObj.OnInit += OnInitAbsDownloadUpdateObj;
        }

        CheckInit();
    }


    private void OnInitAbsCheckIsUpdateObj()
    {
        if (_absCheckIsUpdateObj.IsInit == true)
        {
            _absCheckIsUpdateObj.OnInit -= OnInitAbsCheckIsUpdateObj;
            CheckInit();
        }
    }

    private void OnInitAbsDownloadUpdateObj()
    {
        if (_absDownloadUpdateObj.IsInit == true)
        {
            _absDownloadUpdateObj.OnInit -= OnInitAbsDownloadUpdateObj;
            CheckInit();
        }
    }

    private void CheckInit()
    {
        if (_isInit == false)
        {
            if (_absCheckIsUpdateObj.IsInit == true && _absDownloadUpdateObj.IsInit == true) 
            {
                _isInit = true;
                OnInit?.Invoke();
            }
        }
    }
    
    public GetServerRequestData<StorageStatusCallbackIResourceLocation> StartCheckAndUpdateObject(List<IResourceLocation> locationObject)
    {
        if (_isBlock == false)
        {
            _isBlock = true;
            
            _wrapperCallbackData = new CallbackStorageStatusIResourceLocationAddressablesWrapper(0);
            
            //Проверка, есть ли обновл. у обьекта
            var dataCallback = _absCheckIsUpdateObj.CheckIsUpdateObj(locationObject);

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
                    if (dataCallback.GetData.StatusAllCallBack == TypeStorageStatusCallbackIResourceLocator.Ok)
                    {
                        Debug.Log($"Получил {dataCallback.GetData.ListCallbackStatus.Count} обьектов нуждающихся в обновлении");

                        //Debug.Log($"Получил {dataCallback.GetData.ListCallbackStatus[0].ResourceLocator.InternalId}");
                        StartUpdateData(dataCallback.GetData);
                        return;
                    }

                }

                CallbackError();
            }
        }

        return _wrapperCallbackData.DataGet;
    }


    private void StartUpdateData(StorageStatusCallbackIResourceLocation locationObject)
    {
        if (locationObject.ListCallbackStatus.Count > 0)
        {
            List<IResourceLocation> listResourceLocation = new List<IResourceLocation>();
            foreach (var VARIABLE in locationObject.ListCallbackStatus)
            {
                listResourceLocation.Add(VARIABLE.ResourceLocator);
            }
            
            //Запускаю загрузку обновлении для обьектов(которые были отфильтрованы)
            var dataCallback = _absDownloadUpdateObj.DownloadUpdateObj(listResourceLocation);
            
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
                    if (dataCallback.GetData.StatusAllCallBack == TypeStorageStatusCallbackIResourceLocator.Ok) 
                    {
                    
                        Debug.Log("Все обновления для обьектов были успешно установлены");
                
                        _wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                        _wrapperCallbackData.Data.GetData = dataCallback.GetData;
                        _wrapperCallbackData.Data.IsGetDataCompleted = true;
                        _wrapperCallbackData.Data.Invoke();

                        _isBlock = false;
                        OnUpdateStatusBlock?.Invoke();
                
                        return;
                    }
                
                }
            
                CallbackError();
                return;
            }
        }
        else
        {
            //А раз нет обновл. для каталогов, значит все прошло успешно
            _wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
            _wrapperCallbackData.Data.GetData = null;
            _wrapperCallbackData.Data.IsGetDataCompleted = true;
            _wrapperCallbackData.Data.Invoke();

            _isBlock = false;
            OnUpdateStatusBlock?.Invoke();
        }
    }
    
    private void CallbackError()
    {
        Debug.Log("Что то пошло не так, и обновление сломалось (УВЫ И АХ УХУ УХУ)");
        
        _wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
        _wrapperCallbackData.Data.GetData = null;
        _wrapperCallbackData.Data.IsGetDataCompleted = true;
        _wrapperCallbackData.Data.Invoke();

        _isBlock = false;
        OnUpdateStatusBlock?.Invoke();
    }
    
}
