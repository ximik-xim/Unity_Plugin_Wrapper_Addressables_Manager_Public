using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Делает запрос, к хранилещу с обьектами(которые или разрешено или запрещено брать)
/// и сравнивает обьекты по интерфеис IResourceLocation. Это самй лучший вариант для сравнения
/// (т.к иначе может возникнуть ситуация когда Key заблокирован, а обьект нет, и в итоге можно будет скачать обьект)
/// НО тут нужно указ, где ссылку(IResourceLocation) на фаил(локально или для сервера)
/// </summary>
public class IsGetAddressablesObjectSOStorageIResourceLocation : AbsBoolIsGetAddressablesObject
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
    
    
    public override bool IsInit => _storageBool.IsInit;
    public override event Action OnInit
    {
        add
        {
            _storageBool.OnInit += value;
        }

        remove
        {
            _storageBool.OnInit -= value;
        }
    }

    [SerializeField] 
    private SOStorageBoolIsGetAddressablesObject _storageBool;

    public override GetServerRequestData<bool> IsGet(object obj)
    {
        CallbackDataBool wrapperCallbackData = new CallbackDataBool(0);
        
        
        AsyncOperationHandle<IList<IResourceLocation>> callback;
        
        if (obj is IResourceLocation resourceLocation)
        {
            Debug.Log($"Обноружена ФИГНЯ !!! В логику Проверки пути до фаила LoadResourceLocationsAsync был отправлен интерфеис IResourceLocation. Операция была запущена по ключ = {resourceLocation.PrimaryKey} ");
            callback = Addressables.LoadResourceLocationsAsync(resourceLocation.PrimaryKey);
        }
        else
        {
            callback = Addressables.LoadResourceLocationsAsync(obj);
        }

        if (callback.IsDone == true)
        {
            Complited();
        }
        else
        {
            callback.Completed += OnComplited;
        }


        void OnComplited(AsyncOperationHandle<IList<IResourceLocation>> data)
        {
            if (callback.IsDone == true)
            {
                callback.Completed -= OnComplited;
                Complited();
            }
        }

        void Complited()
        {
            IResourceLocation _resLocation = null;

            //путь к фаилу
            string patchTarget = "";

#if UNITY_EDITOR
            patchTarget = _pathTargetEditor;
#else
                patchTarget = _pathTargetBuild;
#endif

            //ищем подход. путь
            foreach (var VARIABLE in callback.Result)
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


            bool isGet = false;
            
            if (_resLocation != null)
            {
                isGet = _storageBool.IsGetObjectIRes(_resLocation);
            }

            wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
            wrapperCallbackData.Data.GetData = isGet;
            wrapperCallbackData.Data.IsGetDataCompleted = true;
            wrapperCallbackData.Data.Invoke();
            
            if (callback.IsValid() == true) 
            {
                Addressables.Release(callback); 
            }
        }

        return wrapperCallbackData.DataGet;
    }
}
