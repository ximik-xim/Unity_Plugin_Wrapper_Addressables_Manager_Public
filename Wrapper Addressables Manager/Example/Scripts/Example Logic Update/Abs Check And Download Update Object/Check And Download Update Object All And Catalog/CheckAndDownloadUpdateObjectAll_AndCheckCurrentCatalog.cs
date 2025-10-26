using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Отвечает за проверку и загрузку обновлений как для каталога, так и ВСЕХ элементов, которые могут быть обновлены
/// (нужен в случае если каталог был обновлен, а сами фаилы по какой либо причине не были обновлены)
/// (желательно использовать в момент запуска(загрузки) игры)
/// </summary>
public class CheckAndDownloadUpdateObjectAll_AndCheckCurrentCatalog : AbsCheckAndDownloadUpdateObject
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
    
    /// <summary>
    /// Проверка, есть ли обновление для каталогов
    /// </summary>
    [SerializeField]
    private CheckUpdateCatalogs _checkUpdateCatalog;

    /// <summary>
    /// Запуск скачивания обновлений для каталогов
    /// </summary>
    [SerializeField]
    private AbsUpdateCatalogs _updateCatalog;
    
    /// <summary>
    /// Получение списка ID каталогов, котор. можно обновлять
    /// </summary>
    [SerializeField] 
    private AbsGetListCatalogID _getCatalogUpdateID;
    
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
    ///  Список обьектов, которые можно обновлять
    /// </summary>
    [SerializeField] 
    private SOStorageBoolIsGetAddressablesObject _storageIsGetObject;
    

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
        if (Addressables.ResourceLocators != null)
        {
            Debug.Log("Addressables Вроде иниц.");
            StartInit();
        }
        else
        {
            Debug.Log("Addressables Вроде НЕ иниц.");
            StartInitAddressables();
        }
    }
    
    private void StartInitAddressables()
    {
        //!!! ТУТ ОБЯЗ. НУЖНО ИНИЦ Addressables (перед запросом каталогов)
        var callback  = Addressables.InitializeAsync();

        if (callback.IsDone == true)
        {
            ComplitedInitAddressables();
        }
        else
        {
            callback.Completed += OnInitComplitedInitAddressables;
        }
        

        void OnInitComplitedInitAddressables(AsyncOperationHandle<IResourceLocator> obj)
        {
            if (callback.IsDone == true)
            {
                callback.Completed -= OnInitComplitedInitAddressables;
                ComplitedInitAddressables();
            }
        }
        
        void ComplitedInitAddressables()
        {
            if (callback.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables инициализирован");    
            }
            else
            {
                Debug.LogError("Не удалось инициализировать Addressables");    
            }

            StartInit();
        }
    }
    
    private void StartInit()
    {
        if (_checkUpdateCatalog.IsInit == false)
        {
            Debug.Log("Check Init _checkUpdateCatalog");
            _checkUpdateCatalog.OnInit += OnInitCheckUpdateCatalog;
        }

        if (_updateCatalog.IsInit == false)
        {
            Debug.Log("Check Init _updateCatalog");
            _updateCatalog.OnInit += OnInitUpdateCatalog;
        }

        if (_getCatalogUpdateID.IsInit == false)
        {
            Debug.Log("Check Init _getCatalogUpdateID");
            _getCatalogUpdateID.OnInit += OnInitGetCatalogUpdateID;
        }
        
        if (_absCheckIsUpdateObj.IsInit == false)
        {
            Debug.Log("Check Init _absCheckIsUpdateObj");
            _absCheckIsUpdateObj.OnInit += OnInitCheckIsUpdateObj;
        }
        

        if (_absDownloadUpdateObj.IsInit == false)
        {
            Debug.Log("Check Init _absDownloadUpdateObj");
            _absDownloadUpdateObj.OnInit += OnInitDownloadUpdateObj;
        }

        
        if (_storageIsGetObject.IsInit == false)
        {
            Debug.Log("Check Init _storageIsGetObject");
            _storageIsGetObject.OnInit += OnInitStorageIsGetObject;
        }

        CheckInit();
    }

    private void OnInitCheckUpdateCatalog()
    {
        if (_checkUpdateCatalog.IsInit == true)
        {
            Debug.Log("Init _checkUpdateCatalog");
            _checkUpdateCatalog.OnInit -= OnInitCheckUpdateCatalog;
            CheckInit();
        }
    }
    
    private void OnInitUpdateCatalog()
    {
        if (_updateCatalog.IsInit == true)
        {
            Debug.Log("Init _updateCatalog");
            _updateCatalog.OnInit -= OnInitUpdateCatalog;
            CheckInit();
        }
    }

    private void OnInitGetCatalogUpdateID()
    {
        if (_getCatalogUpdateID.IsInit == true)
        {
            Debug.Log("Init _getCatalogUpdateID");
            _getCatalogUpdateID.OnInit -= OnInitGetCatalogUpdateID;
            CheckInit();
        }
    }

    private void OnInitCheckIsUpdateObj()
    {
        if (_absCheckIsUpdateObj.IsInit == true)
        {
            Debug.Log("Init _absCheckIsUpdateObj");
            _absCheckIsUpdateObj.OnInit -= OnInitCheckIsUpdateObj;
            CheckInit();
        }
    }
    
    private void OnInitDownloadUpdateObj()
    {
        if (_absDownloadUpdateObj.IsInit == true)
        {
            Debug.Log("Init _absDownloadUpdateObj");
            _absDownloadUpdateObj.OnInit -= OnInitDownloadUpdateObj;
            CheckInit();
        }
    }

    private void OnInitStorageIsGetObject()
    {
        if (_storageIsGetObject.IsInit == true)
        {
            Debug.Log("Init _storageIsGetObject");
            _storageIsGetObject.OnInit -= OnInitStorageIsGetObject;
            CheckInit();
        }
    }

    private void CheckInit()
    {
        if (_isInit == false)
        {
            if (_checkUpdateCatalog.IsInit == true && _updateCatalog.IsInit == true && _getCatalogUpdateID.IsInit == true && _absDownloadUpdateObj.IsInit == true && _storageIsGetObject.IsInit == true && _absCheckIsUpdateObj.IsInit == true) 
            {
                _isInit = true;
                OnInit?.Invoke();
            }
        }
    }
        
    
    /// <summary>
    /// Запуск логики обновление фаилов
    /// (сначало делаем запрос для проверки обн. у каталогов)
    /// </summary>
    /// <returns></returns>
    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> StartCheckUpdateCatalog()
    {
        if (_isBlock == false)
        {
            _isBlock = true;
            
            _wrapperCallbackData = new CallbackStorageStatusIResourceLocationAddressablesWrapper(0);
        
            //Проверка, есть ли обновл. у катологов
            var dataCallback = _checkUpdateCatalog.StartCheckUpdateCatalog();
        
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
                    Debug.Log($"Получил {dataCallback.GetData.Count} котолога нуждающихся в обновлении");
                    
                    //Если сервер ответил(а знач получили список каталогов(ID), которых надо обновить)
                    FilteringCatalogID(dataCallback.GetData);
                }
                else
                {
                    //Если сервер не ответил, вызываем ошибку
                    CallbackError();
                }
            }
        }

        return _wrapperCallbackData.DataGet;
    }

    /// <summary>
    /// Фильтрация каталогов
    /// Тут сравниваем, разрешено ли обновл. этот каталог
    /// (может по каким то причинам нельзя)
    /// </summary>
    /// <param name="idCatalogUpdate"></param>
    private void FilteringCatalogID(List<string> idCatalogUpdate)
    {
        //пока просто получаю список ID каталогов, с этим списком буду сравнивать получ. список ID каталогов, котор. надо обновить
        var listCatalog = _getCatalogUpdateID.GetCatalogID();

        //список ID элементов, которых буду в итоге обновлять
        List<string> updateCatalogID = new List<string>();
        
        //Если это список заблокированных элементов
        if (_getCatalogUpdateID.IsBlockList() == true)
        {
            foreach (var VARIABLE in idCatalogUpdate)
            {
                //проверяю, есть ли ID в списке заблокированных
                if (listCatalog.Contains(VARIABLE) == false)
                {
                    //если нет, то добавляю его, в список ID для обновл. каталогов 
                    updateCatalogID.Add(VARIABLE);
                }
            }
            
        }
        else
        {
            foreach (var VARIABLE in idCatalogUpdate)
            {
                //проверяю, есть ли ID в списке разрешенных
                if (listCatalog.Contains(VARIABLE) == true)
                {
                    //если да, то добавляю его, в список ID для обновл. каталогов 
                    updateCatalogID.Add(VARIABLE);
                }
            }
        }

        //Если после фильтрации еще ост. каталоги(их ID) для обновл., то вызываю их обновл. 
        if (updateCatalogID.Count > 0)
        {
            Debug.Log($"По итогу фильтрации вызываю обновл у {updateCatalogID.Count} каталогов");
            StartUpdateCatalog(updateCatalogID);    
        }
        else
        {
            Debug.Log("По итогу фильтрации нет обновлений у каталогов");


            StartCheckUpdateAllElementCurrentCatalog();
        }
        
    }


    /// <summary>
    /// Начинаю логику проверки наличия обновлений для всех фаилов в тек. каталоге
    /// (бывает, что при обновл происход. ошибка,по этому это нужно проверять)
    /// </summary>
    private void StartCheckUpdateAllElementCurrentCatalog()
    {
        Debug.Log("Начинаю проверку на наличие обновлений всех обьектов текущего каталога");
        
        List<IResourceLocation> listObjectCatalog = new List<IResourceLocation>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var VARIABLE in locator.AllLocations)
            {
                listObjectCatalog.Add(VARIABLE);
            }
        }
        
        //Проверка, есть ли обновл. у обьекта
        var dataCallback = _absCheckIsUpdateObj.CheckIsUpdateObj(listObjectCatalog);

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
                    List<IResourceLocation> listUpdateObject = new List<IResourceLocation>();
                    foreach (var VARIABLE in dataCallback.GetData.ListCallbackStatus)
                    {
                        listUpdateObject.Add(VARIABLE.ResourceLocator);
                    }

                    Debug.Log($"Получено обьектов {listUpdateObject.Count} для обновлений");
                    
                    StartUpdateObject(listUpdateObject);
                    return;
                }

                Debug.Log("Ошибка, не все элементы удалось проверить");
            }
        
            CallbackError();
        }
    
    }


    /// <summary>
    /// Запускаю само обновление каталогов
    /// </summary>
    private void StartUpdateCatalog(List<string> idCatalogUpdate)
    {
        var dataCallback = _updateCatalog.StartUpdateCatalog(idCatalogUpdate);
        
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
                FilteringObject(dataCallback.GetData);
            }
            else
            {
                CallbackError();
            }
        }
        
    }
    
    /// <summary>
    /// Эта часть отвеч. за филтрацию обьектов, для которых можно скачивать обновл(а для каких нет)
    /// И так же, проверяет, что бы если была разбивка на части(по запросам), в итогде дошли все запросы(а иначе ошибка)
    /// </summary>
    private void FilteringObject(StorageStatusCallbackIResourceLocation storageLocation)
    {
        List<IResourceLocation> _listLocation = new List<IResourceLocation>();
            
        //Тут нужна именно доп проверка этого статуса, по тому как, если разбиваем запросы, то может произойти так, что один из запросов не дойдет
        if (storageLocation.StatusAllCallBack == TypeStorageStatusCallbackIResourceLocator.Ok)
        {
            Debug.Log($"Обновление каталогов прошло успешно, начинаю фильтрацию для {storageLocation.ListCallbackStatus.Count} обьектов");
            
            foreach (var VARIABLE in storageLocation.ListCallbackStatus)
            {
                if (_storageIsGetObject.IsGetObjectIRes(VARIABLE.ResourceLocator) == true) 
                {
                    _listLocation.Add(VARIABLE.ResourceLocator);    
                }
            }

            Debug.Log($"После фильтрации осталось {_listLocation.Count} обьектов, начинаю обновление");
            
            StartUpdateObject(_listLocation);
            return;
        }
        else
        {
            CallbackError();
        }
    }

    /// <summary>
    /// Тут уже начинаю непосредственно скачивать обновления для обьектов
    /// </summary>
    private void StartUpdateObject(List<IResourceLocation> locations)
    {
        var dataCallback = _absDownloadUpdateObj.DownloadUpdateObj(locations);
        
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
                    //удалит старые не используемые бандлы
                    Addressables.CleanBundleCache();
                    
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
