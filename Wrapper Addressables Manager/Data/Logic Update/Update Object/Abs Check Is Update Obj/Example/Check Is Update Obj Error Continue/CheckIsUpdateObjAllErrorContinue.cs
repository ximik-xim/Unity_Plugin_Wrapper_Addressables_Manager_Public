using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Random = UnityEngine.Random;


/// <summary>
/// Нужен для проверки, есть ли обновления для указ фаила, может
/// 1) В случае ошибки при отправке, еще раз сдел. запрос
/// 2) Возможность проверить, нужно ли обновлять указ. фаилы(или фаил)
/// (если общ размер в ответе > 0, то значит есть обновления для фаилов) 
/// </summary>
public class CheckIsUpdateObjAllErrorContinue : AbsCheckIsUpdateObj
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
    
    /// <summary>
    /// Не возращать, интерфеис тех обьектов, которых не надо обновлять
    /// </summary>
    [SerializeField]
    private bool _isIgnoreZeroSize = true;
    
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
    
    
    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> CheckIsUpdateObj(List<IResourceLocation> locatorsObjectUpdate)
    {
        Debug.Log("Запрос на проверку обн. обьектов был отправлен");
        
        //Тут именно скопировать нужно все элементы, т.к список locatorsObjectUpdate очиститься после вызова return
        List<IResourceLocation> copiedListData = new List<IResourceLocation>(locatorsObjectUpdate);
        
        //запрашиваю данные
        var dataCallback = Addressables.GetDownloadSizeAsync(copiedListData);
        
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
        
        void OnCompletedCallback(AsyncOperationHandle<long> data)
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
                Debug.Log("Запрос на проверку обн. обьектов успешен");
                //очищаю список ошибок
                _errorLogic.OnRemoveAllError();
                
                //Если включена фильтрация
                if (_isIgnoreZeroSize == true)
                {
                    if (dataCallback.Result > 0) 
                    {
                        Debug.Log("Обьекту(-там) нужно обновление"); 
                        
                        //заполняю данные для ответа
                        wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                        
                        List<StatusCallbackIResourceLocation> listCallbackData = new List<StatusCallbackIResourceLocation>();
                        foreach (var VARIABLE in copiedListData)
                        {
                            listCallbackData.Add(new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE));
                        }
                        StorageStatusCallbackIResourceLocation storage = new StorageStatusCallbackIResourceLocation(listCallbackData);
                        
                        wrapperCallbackData.Data.GetData = storage;

                        wrapperCallbackData.Data.IsGetDataCompleted = true;
                        wrapperCallbackData.Data.Invoke();

                        _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                        
                        if (dataCallback.IsValid() == true) 
                        {
                            Addressables.Release(dataCallback);
                        }
                        return;
                    }
                    else
                    {
                        //Т.к включена филтрация, то возращаю пустой список(т.к у тех обьектво нет обновлений)
                        Debug.Log("Обьекту(-там) НЕ нужно обновление");
                        
                        
                        //заполняю данные для ответа
                        wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                        
                        StorageStatusCallbackIResourceLocation storage3 = new StorageStatusCallbackIResourceLocation(new List<StatusCallbackIResourceLocation>());
                        
                        wrapperCallbackData.Data.GetData = storage3;

                        wrapperCallbackData.Data.IsGetDataCompleted = true;
                        wrapperCallbackData.Data.Invoke();
                
                        _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                        
                        if (dataCallback.IsValid() == true) 
                        {
                            Addressables.Release(dataCallback);
                        }
                        return;
                        
                    }
                    
                }
                
                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                
                List<StatusCallbackIResourceLocation> listCallbackData2 = new List<StatusCallbackIResourceLocation>();
                foreach (var VARIABLE in copiedListData)
                {
                    listCallbackData2.Add(new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE));
                }
                StorageStatusCallbackIResourceLocation storage2 = new StorageStatusCallbackIResourceLocation(listCallbackData2);
                        
                wrapperCallbackData.Data.GetData = storage2;

                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();
                
                _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                
                if (dataCallback.IsValid() == true) 
                {
                    Addressables.Release(dataCallback);
                }
                return;
            }
            else
            {
                Debug.Log("Запрос на проверку обн. обьектов ошибка. Переотправка");
                //добавляю ошибку
                _errorLogic.OnAddError();
                
                if (dataCallback.IsValid() == true) 
                {
                    Addressables.Release(dataCallback);
                }
                
                //Проверяю, могу ли еще раз отпр. запрос
                if (_errorLogic.IsContinue == true) 
                {
                    //заного отпр. запрос, и по новой 
                    dataCallback = Addressables.GetDownloadSizeAsync(copiedListData);
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
                    Debug.Log("Запрос на проверку обн. обьектов ошибка. Попытки кончились. Возр. ERROR");
                    
                    //если попытки достучаться до сервера закончились, то отпр. все как есть(ошибку)
                    
                    //очищаю список ошибок
                    _errorLogic.OnRemoveAllError();
                    
                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    
                    List<StatusCallbackIResourceLocation> listCallbackData = new List<StatusCallbackIResourceLocation>();
                    foreach (var VARIABLE in copiedListData)
                    {
                        listCallbackData.Add(new StatusCallbackIResourceLocation(StatusCallBackServer.Error, VARIABLE));
                    }
                    StorageStatusCallbackIResourceLocation storage = new StorageStatusCallbackIResourceLocation(listCallbackData);

                    wrapperCallbackData.Data.GetData = storage;
                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();
                    
                    _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
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
