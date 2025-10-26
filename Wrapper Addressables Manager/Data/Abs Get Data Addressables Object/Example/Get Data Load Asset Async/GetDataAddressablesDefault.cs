using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Random = UnityEngine.Random;

/// <summary>
/// Обычное получение обьекта через LoadAssetAsync
/// </summary>
[System.Serializable]
public class GetDataAddressablesDefault : AbsCallbackGetDataTAddressables
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

   private void Awake()
   {
       _idCallback.Clear();
    
       _isInit = true;
       OnInit?.Invoke();
   }
       
   public override GetServerRequestData<AsyncOperationHandle<T>> GetData<T>(object data)
   {
       int id = GetUniqueId();
       
       CallbackRequestDataWrapperT<AsyncOperationHandle<T>> wrapperCallbackData = new CallbackRequestDataWrapperT<AsyncOperationHandle<T>>(id);
       _idCallback.Add(id);

       AsyncOperationHandle<T> dataCallback;
       
       //!! Тут ОБЕЗАТЕЛЬНО нужно приведение типа, иначе при передаче IResourceLocation как Object, метод LoadAssetAsync выдает ОШИБКУ !!
       if (data is IResourceLocation resourceLocation)
       {
           dataCallback = Addressables.LoadAssetAsync<T>(resourceLocation);
       }
       else
       {
            dataCallback = Addressables.LoadAssetAsync<T>(data);   
       }

       if (dataCallback.IsDone == true)
       {
           CompletedCallback(dataCallback);
       }
       else
       {
           dataCallback.Completed += OnCompletedCallback;
       }
       
       void OnCompletedCallback(AsyncOperationHandle<T> callbackLoadData)
       {
           if (callbackLoadData.IsDone == true)
           {
               callbackLoadData.Completed -= OnCompletedCallback;
               CompletedCallback(callbackLoadData);    
           }
       }
       
       void CompletedCallback(AsyncOperationHandle<T> callbackLoadData)
       {
           if (callbackLoadData.Status == AsyncOperationStatus.Succeeded)
           {
               wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
           }
           else
           {
               wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
           }
           
           wrapperCallbackData.Data.GetData = dataCallback;
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
