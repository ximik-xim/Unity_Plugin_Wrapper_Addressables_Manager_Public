using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранилеще со списком статусов запросов
/// </summary>
public class StorageStatusCallbackIResourceLocation
{
    public StorageStatusCallbackIResourceLocation(List<StatusCallbackIResourceLocation> listCallbackStatus)
    {
        _listCallbackStatus = listCallbackStatus;
        
        if (listCallbackStatus.Count > 0)
        {
            bool isAllError = true;
            bool isError = false;
            
            foreach (var VARIABLE in listCallbackStatus)
            {
                if (VARIABLE.StatusCallBack == StatusCallBackServer.Ok)
                {
                    isAllError = false;
                }
         
                if (VARIABLE.StatusCallBack == StatusCallBackServer.Error)
                {
                    isError = true;
                }
            }

            if (isAllError == true)
            {
                _statusAllCallBack = TypeStorageStatusCallbackIResourceLocator.AllError;
                return;
            }
      
            if (isError == true)
            {
                _statusAllCallBack = TypeStorageStatusCallbackIResourceLocator.PartialError;
                return;
            }
        }
        
        _statusAllCallBack = TypeStorageStatusCallbackIResourceLocator.Ok;
    }
   
    public StorageStatusCallbackIResourceLocation(List<StatusCallbackIResourceLocation> listCallbackStatus, TypeStorageStatusCallbackIResourceLocator statusAllCallBack)
    {
        _listCallbackStatus = listCallbackStatus;
        _statusAllCallBack = statusAllCallBack;
    }
   
    private TypeStorageStatusCallbackIResourceLocator _statusAllCallBack;
    public TypeStorageStatusCallbackIResourceLocator StatusAllCallBack => _statusAllCallBack;

    private List<StatusCallbackIResourceLocation> _listCallbackStatus;
    public IReadOnlyList<StatusCallbackIResourceLocation> ListCallbackStatus => _listCallbackStatus;

}

/// <summary>
/// Ok - Все запросы успешны
/// PartialError - У некоторых запросов есть ошибки
/// AllError - Все запросы пришли с ошибками
/// </summary>
public enum TypeStorageStatusCallbackIResourceLocator
{
    Ok,
    PartialError,
    AllError,
}