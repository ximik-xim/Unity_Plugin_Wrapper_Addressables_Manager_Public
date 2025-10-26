using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// По нажатию на кнопку запустит получение обьекта, по указ ключу
/// (ключом будет AssetReference)
/// </summary>
public class ButtonLoadTargetObj : MonoBehaviour
{
    [SerializeField]
    private Button _button;
        
    [SerializeField]
    private GameObject _parent;
        
    /// <summary>
    /// ссылка на обьект
    /// </summary>
    [SerializeField] 
    private AssetReference _assetReference;
   
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
        _button.onClick.AddListener(ButtonClick);
    }


    private void ButtonClick()
    {
        StartLogic();
    }
   
    private void StartLogic()
    {
        if (_localData.IsValid() == true) 
        {
            Addressables.Release(_localData);   
        }
        
        Debug.Log("Послан запрос на получения данных GameObject");
        var dataCallback = _getDataAddressables.GetData<GameObject>(_assetReference);

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
            Debug.Log("Получен обьект = " + dataCallback.GetData);
            Debug.Log("Проверка на null = " + (dataCallback.GetData.Result == null));

            Instantiate(dataCallback.GetData.Result, _parent.transform);
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
