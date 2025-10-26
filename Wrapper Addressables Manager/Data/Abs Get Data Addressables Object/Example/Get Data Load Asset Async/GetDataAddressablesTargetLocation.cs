using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Будет искать указ обьект(можно передать Key, GUID, AssetReference, IResourceLocation и т.д) в опр месте (через LoadResourceLocationsAsync)
/// (пример нужно искать обьект только локально или только на сервере)
/// </summary>
public class GetDataAddressablesTargetLocation : AbsCallbackGetDataTAddressables
{
    /// <summary>
    /// Путь к фаилу, котор. и будем искать
    /// Пример,
    /// к серверу путь нач http или https
    /// к локалке путь нач Assets (пока не собрано, а после сборки обычно нач с file)
    /// </summary>
    /// (при работе в инспекторе)
    [SerializeField] 
    private string _pathTargetEditor = "http / Assets";
    /// (при работе в собраном проекте)
    [SerializeField] 
    private string _pathTargetBuild = "http / file";
    /// <summary>
    /// Если будет true, будут выбраны все элементы кроме тех, что совпадают с указанным именем
    /// если false, будет выбран только элемент с указанным именем
    /// </summary>
    [SerializeField] 
    private bool _excludeTarget = false;

    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
       
    /// <summary>
    /// Список Id callback, которые сейчас в ожидании
    /// (сериализован просто для удобного отслеживания в инспекторе)
    /// </summary>
    [SerializeField]
    private List<int> _idCallback = new List<int>();

    private void Awake()
    {
        _idCallback.Clear();
    
        _isInit = true;
        OnInit?.Invoke();
    }

    public override GetServerRequestData<AsyncOperationHandle<T>> GetData<T>(object data)
    {
        Debug.Log("Запрос на загр. обьекта по указ. пути был отправлен");
        
        int id = GetUniqueId();
        CallbackRequestDataWrapperT<AsyncOperationHandle<T>> wrapperCallbackData = new CallbackRequestDataWrapperT<AsyncOperationHandle<T>>(id);
        _idCallback.Add(id);

        AsyncOperationHandle<IList<IResourceLocation>> dataCallbackLoadResource;
        
        if (data is IResourceLocation resourceLocation)
        {
            Debug.Log($"Обноружена ФИГНЯ !!! В логику загрузки LoadResourceLocationsAsync был отправлен интерфеис IResourceLocation. Операция была запущена по ключ = {resourceLocation.PrimaryKey} ");
             dataCallbackLoadResource = Addressables.LoadResourceLocationsAsync(resourceLocation.PrimaryKey);
        }
        else
        {
             dataCallbackLoadResource = Addressables.LoadResourceLocationsAsync(data);
        }

        //проверяю готовы ли данные 
        if (dataCallbackLoadResource.IsDone == true)
        {
            //да готовы, начинаю обработку
            CompletedCallbackLoadResource();
        }
        else
        {
            //не, неготовы, начинаю ожидание, пока прийдут
            dataCallbackLoadResource.Completed += OnCompletedCallbackLoadResource;
        }


        void OnCompletedCallbackLoadResource(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            //Если данные пришли
            if (dataCallbackLoadResource.IsDone == true)
            {
                dataCallbackLoadResource.Completed -= OnCompletedCallbackLoadResource;
                //начинаю обработку данных
                CompletedCallbackLoadResource();
            }

        }

        void CompletedCallbackLoadResource()
        {
            if (dataCallbackLoadResource.Status == AsyncOperationStatus.Succeeded && dataCallbackLoadResource.Result != null && dataCallbackLoadResource.Result.Count > 0)
            {
                //получили список всех путей для этого обьекта(как локальных(local), так и пути к серверу(Remote))
                var resourceAllLocation = dataCallbackLoadResource.Result;

                IResourceLocation _resLocation = null;

                //путь к фаилу
                string patchTarget = "";

#if UNITY_EDITOR
                patchTarget = _pathTargetEditor;
#else
                patchTarget = _pathTargetBuild;
#endif

#if UNITY_EDITOR
                //чисто для тестов
                foreach (var VARIABLE in resourceAllLocation)
                {
                    Debug.Log("Найденный обьект Id = " + VARIABLE.InternalId);
                }
#endif
                //ищем подход. путь
                foreach (var VARIABLE in resourceAllLocation)
                {
                    if (_excludeTarget == true) 
                    {
                        if (VARIABLE.InternalId.StartsWith(patchTarget) == false)
                        {
                            _resLocation = VARIABLE;
                            break;
                        }
                    }
                    else
                    {
                        if (VARIABLE.InternalId.StartsWith(patchTarget) == true)
                        {
                            _resLocation = VARIABLE;
                            break;
                        }
                    }
                }

                //если нашли похдход. путь к обьекту, то нач. его загр
                if (_resLocation != null)
                {
                    StartLoadAsset(_resLocation);    
                }
                else
                {
                    Debug.Log("Запрос на загр. обьекта по указ. пути. Ошибка, ничего не найдено");
                    
                    //а если такого пути нету отвеч. Error
                    
                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    wrapperCallbackData.Data.GetData = default;

                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();
                    
                    _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                    
                    if (dataCallbackLoadResource.IsValid() == true) 
                    {
                        Addressables.Release(dataCallbackLoadResource);   
                    }
                }
                
                
                void StartLoadAsset(IResourceLocation resourceLocation)
                {
                    Debug.Log("Запрос на загр. обьекта по указ. пути. Получение обьекта");
                    var dataCallbackLoadAsset = Addressables.LoadAssetAsync<T>(resourceLocation);

                    //проверяю готовы ли данные 
                    if (dataCallbackLoadAsset.IsDone == true)
                    {
                        //да готовы, начинаю обработку
                        CompletedCallbackLoadAsset();
                    }
                    else
                    {
                        //не, неготовы, начинаю ожидание, пока прийдут
                        dataCallbackLoadAsset.Completed += OnCompletedCallbackLoadAsset;
                    }

                    void OnCompletedCallbackLoadAsset(AsyncOperationHandle<T> obj)
                    {
                        //Если данные пришли
                        if (dataCallbackLoadAsset.IsDone == true)
                        {
                            dataCallbackLoadAsset.Completed -= OnCompletedCallbackLoadAsset;
                            //начинаю обработку данных
                            CompletedCallbackLoadAsset();
                        }
                    }

                    void CompletedCallbackLoadAsset()
                    {
                        if (dataCallbackLoadAsset.Status == AsyncOperationStatus.Succeeded)
                        {
                            Debug.Log("Запрос на загр. обьекта по указ. пути. Обьект получен");
                            wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                            wrapperCallbackData.Data.GetData = dataCallbackLoadAsset;

                            wrapperCallbackData.Data.IsGetDataCompleted = true;
                            wrapperCallbackData.Data.Invoke();

                            _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                            
                            
                            if (dataCallbackLoadResource.IsValid() == true) 
                            {
                                Addressables.Release(dataCallbackLoadResource);   
                            }
                            return;
                        }
                        else
                        {
                            Debug.Log("Запрос на загр. обьекта по указ. пути. Ошибка при получ. обьекта");
                            //заполняю данные для ответа
                            wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                            wrapperCallbackData.Data.GetData = default;

                            wrapperCallbackData.Data.IsGetDataCompleted = true;
                            wrapperCallbackData.Data.Invoke();

                            _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                            
                                            
                            if (dataCallbackLoadAsset.IsValid() == true) 
                            {
                                Addressables.Release(dataCallbackLoadAsset);   
                            }
                            
                            if (dataCallbackLoadResource.IsValid() == true) 
                            {
                                Addressables.Release(dataCallbackLoadResource);   
                            }
                            return;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Запрос на загр. обьекта по указ. пути. Ошибка");
                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                wrapperCallbackData.Data.GetData = default;

                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();

                _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                
                            
                if (dataCallbackLoadResource.IsValid() == true) 
                {
                    Addressables.Release(dataCallbackLoadResource);   
                }
                return;
            }

        }

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
