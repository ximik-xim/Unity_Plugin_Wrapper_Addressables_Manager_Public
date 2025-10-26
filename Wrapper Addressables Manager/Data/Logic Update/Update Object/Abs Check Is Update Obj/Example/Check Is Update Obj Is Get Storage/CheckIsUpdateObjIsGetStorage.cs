using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using Random = UnityEngine.Random;


/// <summary>
/// Нужен, что бы свериться со списком, и проверить, какие обьекты разрешены, а какие запрещены к обновлению
/// </summary>
public class CheckIsUpdateObjIsGetStorage : AbsCheckIsUpdateObj
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
    private SOStorageBoolIsGetAddressablesObject _storageIsGet;

    /// <summary>
    /// Дальше передаю получ. список разрешенных обьектов
    /// </summary>
    [SerializeField]
    private AbsCheckIsUpdateObj _absCheckIsUpdateObj;

    private void Awake()
    {
        _idCallback.Clear();

        if (_storageIsGet.IsInit == false)
        {
            _storageIsGet.OnInit += OnInitStorageIsGet;
        }
        
        if (_absCheckIsUpdateObj.IsInit == false)
        {
            _absCheckIsUpdateObj.OnInit += OnInitCheckIsUpdateObj;
        }

        CheckInit();
        
    }
    
    private void OnInitStorageIsGet()
    {
        if (_storageIsGet.IsInit == true)
        {
            _storageIsGet.OnInit -= OnInitStorageIsGet;
            CheckInit();
        }
    }
    
    
    private void OnInitCheckIsUpdateObj()
    {
        if (_absCheckIsUpdateObj.IsInit == true)
        {
            _absCheckIsUpdateObj.OnInit -= OnInitCheckIsUpdateObj;
            CheckInit();
        }
    }
    
    private void CheckInit()
    {
        if (_isInit == false)
        {
            _isInit = true;
            OnInit?.Invoke();    
        }
    }

    public override GetServerRequestData<StorageStatusCallbackIResourceLocation> CheckIsUpdateObj(List<IResourceLocation> locatorsObjectUpdate)
    {
        int id = GetUniqueId();
        CallbackStorageStatusIResourceLocationAddressablesWrapper wrapperCallbackData = new CallbackStorageStatusIResourceLocationAddressablesWrapper(id);
        _idCallback.Add(id);
        
        List<IResourceLocation> listLocator = new List<IResourceLocation>();
        
        foreach (var VARIABLE in locatorsObjectUpdate)
        {
            if (_storageIsGet.IsGetObjectIRes(VARIABLE) == true) 
            {
                listLocator.Add(VARIABLE);
            }
        }
        
        var dataCallback= _absCheckIsUpdateObj.CheckIsUpdateObj(listLocator);
        
        //проверяю готовы ли данные 
        if (dataCallback.IsGetDataCompleted == true)
        {
            //да готовы, начинаю обработку
            CompletedCallback();
        }
        else
        {
            //не, неготовы, начинаю ожидание, пока прийдут
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
            wrapperCallbackData.Data.StatusServer = dataCallback.StatusServer;
            wrapperCallbackData.Data.GetData = dataCallback.GetData;
           
            wrapperCallbackData.Data.IsGetDataCompleted = true;
            wrapperCallbackData.Data.Invoke();
            
            _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
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
