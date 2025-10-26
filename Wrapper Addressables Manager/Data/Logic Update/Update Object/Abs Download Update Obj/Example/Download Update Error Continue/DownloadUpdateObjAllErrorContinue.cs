using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Random = UnityEngine.Random;


/// <summary>
/// Нужен для скачивания обновлений для указ. обьектов(или обьекта)
/// В случае ошибки при отправке, еще раз сдел. запрос
/// </summary>
public class DownloadUpdateObjAllErrorContinue : AbsDownloadUpdateObj
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
    
    /// <summary>
    /// Список Id callback, которые сейчас в ожидании
    /// (сериализован просто для удобного отслеживания в инспекторе)
    /// </summary>
    [SerializeField]
    private List<int> _idCallback = new List<int>();

    [SerializeField]
    private LogicErrorCallbackRequest _errorLogic;

    private void Awake()
    {
        if (_errorLogic.IsInit == false)
        {
            _errorLogic.OnInit += OnInitErrorLogic;
        }

        CheckInit();
    }

    private void OnInitErrorLogic()
    {
        _errorLogic.OnInit -= OnInitErrorLogic;
        CheckInit();
    }

    private void CheckInit()
    {
        if (_isInit == false)
        {
            if (_errorLogic.IsInit == true)
            {
                _isInit = true;
                OnInit?.Invoke();
            }
        }
    }
    
    
    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> DownloadUpdateObj(List<IResourceLocation> locatorsObjectUpdate)
    {
        //Тут именно скопировать нужно все элементы, т.к список locatorsObjectUpdate очиститься после вызова return
        List<IResourceLocation> copiedListData = new List<IResourceLocation>(locatorsObjectUpdate);
        
        Debug.Log("Запрос на загр. обн. обьекта был отправлен");
        //запрашиваю данные
        var dataCallback = Addressables.DownloadDependenciesAsync(copiedListData);
        
        int id = GetUniqueId();
        //делаю обертку т.к могу несколько раз делать запросы на данные, а верну лиш 1 итог. результат 
        CallbackStorageStatusIResourceLocationAddressablesWrapper wrapperCallbackData = new CallbackStorageStatusIResourceLocationAddressablesWrapper(id);
        _idCallback.Add(id);

        //проверяю готовы ли данные 
        if (dataCallback.IsDone == true)
        {
            //да готовы, начинаю обработку
            CompletedCallback();
        }
        else
        {
            //не, неготовы, начинаю ожидание, пока прийдут
            dataCallback.Completed += OnCompletedCallback;
        }
        
        void OnCompletedCallback(AsyncOperationHandle obj)
        {
            //Если данные пришли
            if (dataCallback.IsDone == true)
            {
                dataCallback.Completed -= OnCompletedCallback;
                //начинаю обработку данных
                CompletedCallback();    
            }
        }

        void CompletedCallback()
        {
            //Если успешно получил данные
            if (dataCallback.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Запрос на загр. обн. обьекта успешен");
                
                //очищаю список ошибок
                _errorLogic.OnRemoveAllError();
                
                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;

                List<StatusCallbackIResourceLocation> listStatusCallback = new List<StatusCallbackIResourceLocation>();
                foreach (var VARIABLE in copiedListData)
                {
                    StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE);
                    listStatusCallback.Add(statusCallback);
                }

                StorageStatusCallbackIResourceLocation statusAll = new StorageStatusCallbackIResourceLocation(listStatusCallback, TypeStorageStatusCallbackIResourceLocator.Ok);
                
                wrapperCallbackData.Data.GetData = statusAll;
                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();
                
                _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
                if (dataCallback.IsValid() == true) 
                {
                    Addressables.Release(dataCallback);
                }
                return;
            }
            else
            {
                //добавляю ошибку
                _errorLogic.OnAddError();
                
                if (dataCallback.IsValid() == true) 
                {
                    Addressables.Release(dataCallback);
                }
                
                //Проверяю, могу ли еще раз отпр. запрос
                if (_errorLogic.IsContinue == true) 
                {
                    Debug.Log("Запрос на загр. обн. обьекта ошибка. Переотправка");
                    
                    //заного отпр. запрос, и по новой 
                    dataCallback = Addressables.DownloadDependenciesAsync(copiedListData);
                    if (dataCallback.IsDone == true)
                    {
                        CompletedCallback();
                    }
                    else
                    {
                        dataCallback.Completed -= OnCompletedCallback;
                        dataCallback.Completed += OnCompletedCallback;
                    }
                    
                    return;
                }
                else
                {
                    Debug.Log("Запрос на загр. обн. обьекта ошибка. Попытки кончились. Возр. ERROR");
                    
                    //если попытки достучаться до сервера закончились, то отпр. все как есть(ошибку)
                    
                    //очищаю список ошибок
                    _errorLogic.OnRemoveAllError();
                    
                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    
                    List<StatusCallbackIResourceLocation> listStatusCallback = new List<StatusCallbackIResourceLocation>();
                    foreach (var VARIABLE in copiedListData)
                    {
                        StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Error, VARIABLE);
                        listStatusCallback.Add(statusCallback);
                    }

                    StorageStatusCallbackIResourceLocation statusAll = new StorageStatusCallbackIResourceLocation(listStatusCallback, TypeStorageStatusCallbackIResourceLocator.AllError);

                    wrapperCallbackData.Data.GetData = statusAll;
                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();
                    
                    _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
                    
                    return;
                }
                
            }
        }

        //возр. обертку с callback, когда данные будут готовы(и с неё же получ. данные)
        return wrapperCallbackData.DataGet;
    }
    
    private int GetUniqueId()
    {
        int id = 0;
        while (true)
        {
            id = Random.Range(0, Int32.MaxValue - 1);
            if (_idCallback.Contains(id) == false)
            {
                break;
            }
        }

        return id;
    }
}
