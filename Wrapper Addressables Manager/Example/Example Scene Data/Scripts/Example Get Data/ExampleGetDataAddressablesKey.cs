using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Пример получения обьекта через Key
/// </summary>
public class ExampleGetDataAddressablesKey : MonoBehaviour
{
   public bool IsInit => _isInit;
   private bool _isInit = false;
   public event Action OnInit;

   /// <summary>
   /// ключ привязанный к обьекту
   /// </summary>
   [SerializeField] 
   private string _key;

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
      Debug.Log("Послан запрос на получения данных GameObject");
      var dataCallback = _getDataAddressables.GetData<GameObject>(_key);

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
