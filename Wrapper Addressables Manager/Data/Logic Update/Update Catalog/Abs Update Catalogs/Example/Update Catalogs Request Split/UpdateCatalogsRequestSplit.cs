using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using Random = UnityEngine.Random;

/// <summary>
/// Этот скрипт нужен, что бы список элементов которые надо обновить разбить
/// на несколько запросов(групп) по N элементов (да хоть группу по 1 элементу) 
/// </summary>
public class UpdateCatalogsRequestSplit : AbsUpdateCatalogs
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
    

    [SerializeField] 
    private AbsUpdateCatalogs _absUpdateCatalogs;
    
    /// <summary>
    /// Кол-во элементов в запросе к серверу
    /// </summary>
    [SerializeField]
    private int _countElementRequest = 1;


    /// <summary>
    /// Список Id callback, которые сейчас в ожидании
    /// (сериализован просто для удобного отслеживания в инспекторе)
    /// </summary>
    [SerializeField]
    private List<int> _idCallback = new List<int>();
    
    private void Awake()
    {
        _idCallback.Clear();

        if (_absUpdateCatalogs.IsInit == false)
        {
            _absUpdateCatalogs.OnInit += OnInitAbsUpdateCatalogs;
            return;
        }

        InitAbsUpdateCatalogs();
        
    }
    
    private void OnInitAbsUpdateCatalogs()
    {
        if (_absUpdateCatalogs.IsInit == true)
        {
            _absUpdateCatalogs.OnInit -= OnInitAbsUpdateCatalogs;
            InitAbsUpdateCatalogs();
        }
    }
    
    private void InitAbsUpdateCatalogs()
    {
        if (_isInit == false)
        {
            _isInit = true;
            OnInit?.Invoke();    
        }
    }



    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> StartUpdateCatalog(List<string> idCatalogUpdate)
    {
        //Список с callback запросами к серверу 
        List<GetServerRequestData<StorageStatusCallbackIResourceLocation>> bufferCallback = new List<GetServerRequestData<StorageStatusCallbackIResourceLocation>>();

        int id = GetUniqueId();
        //обертка, для возможности венуть данные когда они будут готовы
        CallbackStorageStatusIResourceLocationAddressablesWrapper wrapperCallbackData = new CallbackStorageStatusIResourceLocationAddressablesWrapper(id);
        _idCallback.Add(id);
        
        //Список с получ. данными от всех запросов к серверу
        List<StatusCallbackIResourceLocation> listStatusCallback = new List<StatusCallbackIResourceLocation>();

        
        int targetCount = idCatalogUpdate.Count;
        //Это список ID каталогов, которые буду отправлять(по группом из N элементов)
        List<string> bufferListRequestID = new List<string>();

        //Тут нарезаю весь список с ID каталогов на группы по N элемнтов 
        for (int i = 0; i < targetCount; i++)
        {
            bufferListRequestID.Add(idCatalogUpdate[i]);
            idCatalogUpdate.RemoveAt(i);

            i--;
            targetCount--;

            if (bufferListRequestID.Count == _countElementRequest)
            {
                //Делаю отпр. запроса на обновление католгов 
                var callbackData = _absUpdateCatalogs.StartUpdateCatalog(bufferListRequestID);
                bufferCallback.Add(callbackData);

                bufferListRequestID.Clear();
            }
        }

        //Если все элементы не удалось разбить на равные группы, то в конце ост. группа с меньшим кол-во элементов
        //И её тоже надо отправить
        if (bufferListRequestID.Count > 0)
        {
            var callbackData = _absUpdateCatalogs.StartUpdateCatalog(bufferListRequestID);
            bufferCallback.Add(callbackData);
        }

        Check();
        
        void Check()
        {
            int targetCount = bufferCallback.Count;
            
            for (int i = 0; i < targetCount; i++)
            {
                //Если этот callbak закончил работу, обрабатываю данные
                if (bufferCallback[i].IsGetDataCompleted == true)
                {
                    //Те данные, что пришли добавляю в общ список данных, получ. с сервера
                    foreach (var VARIABLE in bufferCallback[i].GetData.ListCallbackStatus)
                    {
                        listStatusCallback.Add(VARIABLE);
                    }

                    //Отписывась от проверки 
                    bufferCallback[i].OnGetDataCompleted -= OnCheck;
                    //Удаляю Callback с сервера
                    bufferCallback.RemoveAt(i);
                    i--;
                    targetCount--;
                }
                else
                {
                    //Тут отписка обяз. Т.к сюда буду несколько раз заходиться
                    bufferCallback[i].OnGetDataCompleted -= OnCheck;
                    bufferCallback[i].OnGetDataCompleted += OnCheck;
                }
            }

            if (bufferCallback.Count == 0)
            {
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;

                StorageStatusCallbackIResourceLocation statusAll = new StorageStatusCallbackIResourceLocation(listStatusCallback);

                wrapperCallbackData.Data.GetData = statusAll;
                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();

                _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
            }
        }

        void OnCheck()
        {
            Check();
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
