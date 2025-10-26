using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Логика переотправки запроса несколько раз(в случ. ошибки) совмещенная с логикой вызова обновлен у каталогов 
/// </summary>
public class UpdateCatalogsAllErrorContinue : AbsUpdateCatalogs
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
    
    [SerializeField]
    private LogicErrorCallbackRequest _errorLogic;
    
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
    
    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> StartUpdateCatalog(List<string> idCatalogUpdate)
    {
        //Тут именно скопировать нужно все элементы, т.к список idCatalogUpdate очиститься после вызова return
        List<string> copiedListData = new List<string>(idCatalogUpdate);
        
        Debug.Log("Запрос на загр. обн. каталогов был отправлен");
        //запрашиваю данные
        var dataCallback = Addressables.UpdateCatalogs(copiedListData);
        
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
        
        void OnCompletedCallback(AsyncOperationHandle<List<IResourceLocator>> obj)
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
                Debug.Log("Запрос на обн. каталогов успешен");
                
                //очищаю список ошибок
                _errorLogic.OnRemoveAllError();
                
                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;

                List<StatusCallbackIResourceLocation> listStatusCallback = new List<StatusCallbackIResourceLocation>();
                foreach (var VARIABLE in dataCallback.Result)
                {
#if ENABLE_JSON_CATALOG
                    foreach (var VARIABLE2 in VARIABLE.Keys)
                    {
                        VARIABLE.Locate(VARIABLE2, typeof(UnityEngine.Object), out IList<IResourceLocation> list);
                        
                        if (list != null)
                        {
                            foreach (var VARIABLE3 in list)
                            {
                                StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE3); 
                                listStatusCallback.Add(statusCallback);
                            }
                        }
                    }       
                    
                    foreach (var VARIABLE2 in VARIABLE.Keys)
                    {
                        VARIABLE.Locate(VARIABLE2, typeof(object), out IList<IResourceLocation> list);
                        
                        if (list != null)
                        {
                            foreach (var VARIABLE3 in list)
                            {
                                StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE3); 
                                listStatusCallback.Add(statusCallback);
                            }
                        }
                    }       
                    
#else
                     foreach (var VARIABLE2 in VARIABLE.AllLocations)
                    {
                        StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE2);
                        listStatusCallback.Add(statusCallback);
                    }
#endif
                }

                StorageStatusCallbackIResourceLocation statusAll = new StorageStatusCallbackIResourceLocation(listStatusCallback, TypeStorageStatusCallbackIResourceLocator.Ok);
                
                wrapperCallbackData.Data.GetData = statusAll;
                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();
                
                _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);

                //Тут не надо вручную удалять данные из оперативки, если вручну не выключели авто очистки у метода UpdateCatalogs( тут передать false)
                //иначе метод ломаеться(ля приколы)
                // if (dataCallback.IsValid() == true) 
                // {
                //     Addressables.Release(dataCallback);
                // }
                return;
            }
            else
            {
                //добавляю ошибку
                _errorLogic.OnAddError();
                
                //Проверяю, могу ли еще раз отпр. запрос
                if (_errorLogic.IsContinue == true) 
                {
                    Debug.Log("Запрос на обн. каталогов ошибка. Переотправка");
                    
                    //Тут не надо вручную удалять данные из оперативки, если вручну не выключели авто очистки у метода UpdateCatalogs( тут передать false)
                    //иначе метод ломаеться(ля приколы)
                    // if (dataCallback.IsValid() == true) 
                    // {
                    //     Addressables.Release(dataCallback);
                    // }
                    
                    //заного отпр. запрос, и по новой 
                    dataCallback = Addressables.UpdateCatalogs(copiedListData);
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
                    Debug.Log("Запрос на обн. каталогов ошибка. Попытки кончились. Возр. ERROR");
                    
                    //если попытки достучаться до сервера закончились, то отпр. все как есть(ошибку)
                    
                    //очищаю список ошибок
                    _errorLogic.OnRemoveAllError();
                    
                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    
                    List<StatusCallbackIResourceLocation> listStatusCallback = new List<StatusCallbackIResourceLocation>();
                    foreach (var VARIABLE in dataCallback.Result)
                    {
                        
#if ENABLE_JSON_CATALOG
                        foreach (var VARIABLE2 in VARIABLE.Keys)
                        {
                            VARIABLE.Locate(VARIABLE2, typeof(UnityEngine.Object), out IList<IResourceLocation> list);
                        
                            if (list != null)
                            {
                                foreach (var VARIABLE3 in list)
                                {
                                    StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE3); 
                                    listStatusCallback.Add(statusCallback);
                                }
                            }
                        }       
                    
                        foreach (var VARIABLE2 in VARIABLE.Keys)
                        {
                            VARIABLE.Locate(VARIABLE2, typeof(object), out IList<IResourceLocation> list);
                        
                            if (list != null)
                            {
                                foreach (var VARIABLE3 in list)
                                {
                                    StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE3); 
                                    listStatusCallback.Add(statusCallback);
                                }
                            }
                        }       
                    
#else
                     foreach (var VARIABLE2 in VARIABLE.AllLocations)
                    {
                        StatusCallbackIResourceLocation statusCallback = new StatusCallbackIResourceLocation(StatusCallBackServer.Ok, VARIABLE2);
                        listStatusCallback.Add(statusCallback);
                    }
#endif
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
