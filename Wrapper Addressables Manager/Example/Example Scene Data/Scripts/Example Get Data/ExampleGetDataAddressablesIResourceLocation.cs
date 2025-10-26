using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Пример получения обьекта через IResourceLocation
/// </summary>
public class ExampleGetDataAddressablesIResourceLocation : MonoBehaviour
{
   public bool IsInit => _isInit;
   private bool _isInit = false;
   public event Action OnInit;

   /// <summary>
   /// А тут он нужен, что бы побыстрому получить интерфеис IResourceLocation
   /// </summary>
   [SerializeField]
   private AssetReference _refObj;
   
   /// <summary>
   /// интерфеис с информ. об полож обьекта
   /// </summary>
   private IResourceLocation _resourceLocation;

   [SerializeField] 
   private AbsCallbackGetDataTAddressables _getDataAddressables;

   private AsyncOperationHandle<GameObject> _localData;
   
   private void Awake()
   {
      if (_getDataAddressables.IsInit == false)
      {
         _getDataAddressables.OnInit += OnInitGetData;
         return;
      }

      InitGetData();
   }

   private void OnInitGetData()
   {
      if (_getDataAddressables.IsInit == true)
      {
         _getDataAddressables.OnInit -= OnInitGetData;
         InitGetData();
      }
   }

   private void InitGetData()
   {
      StartGetIResourceLocation();
   }

   private void StartGetIResourceLocation()
   {
      var dataCallback = Addressables.LoadResourceLocationsAsync(_refObj.RuntimeKey);
      if (dataCallback.IsDone == true)
      {
         CompletedCallback(dataCallback);
      }
      else
      {
         dataCallback.Completed += OnCompletedCallback;
      }
      void OnCompletedCallback(AsyncOperationHandle<IList<IResourceLocation>> callbackLoadData)
      {
         if (callbackLoadData.IsDone == true)
         {
            callbackLoadData.Completed -= OnCompletedCallback;
            CompletedCallback(callbackLoadData);    
         }
      }
      
      void CompletedCallback(AsyncOperationHandle<IList<IResourceLocation>> callbackLoadData)
      {
         if (callbackLoadData.Status == AsyncOperationStatus.Succeeded && callbackLoadData.Result.Count > 0) 
         {
            _resourceLocation = callbackLoadData.Result[0];
            StartGetObject();
         }
         else
         {
            Debug.Log("Ресурс(обьект) не был найден, увы");
         }
         
         if (callbackLoadData.IsValid() == true) 
         {
            Addressables.Release(callbackLoadData);   
         }
      }

   }
   private void StartGetObject()
   {
      Debug.Log("Послан запрос на получения данных GameObject");
      var dataCallback = _getDataAddressables.GetData<GameObject>(_resourceLocation);

      if (dataCallback.IsGetDataCompleted == true)
      {
         CompletedGetData();
      }
      else
      {
         dataCallback.OnGetDataCompleted += OnCompletedGetData;
      }

      void OnCompletedGetData()
      {
         if (dataCallback.IsGetDataCompleted == true)
         {
            dataCallback.OnGetDataCompleted -= OnCompletedGetData;
            CompletedGetData();
         }
      }

      void CompletedGetData()
      {
         _localData = dataCallback.GetData;
         
         Debug.Log("----- Данные получены ----");
         Debug.Log("Статус запроса = " + dataCallback.StatusServer.ToString());
         Debug.Log("Получен обьект = " + dataCallback.GetData.Result);
         Debug.Log("Проверка на null = " + (dataCallback.GetData.Result == null));

         _isInit = true;
         OnInit?.Invoke();
      }
      
   }

   private void OnDestroy()
   {
      if (_localData.IsValid() == true) 
      {
         Addressables.Release(_localData);   
      }
   }
}
