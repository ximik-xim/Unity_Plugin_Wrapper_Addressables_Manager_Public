using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

/// <summary>
/// Пример получения обьекта через Label
/// </summary>
public class ExampleGetDataAddressablesAssetLabelReference : MonoBehaviour
{
    public bool IsInit => _isInit;
    private bool _isInit = false;
    public event Action OnInit;

    /// <summary>
    /// Lable обьектов 
    /// </summary>
    [SerializeField] 
    private AssetLabelReference _labelKey;

    // [SerializeField] 
    // private AbsCallbackGetDataAddressables _getDataAddressables;

    private  AsyncOperationHandle<IList<Object>> _localData;
    
    private void Awake()
    {
        InitGetData();
        // if (_getDataAddressables.IsInit == false)
        // {
        //     _getDataAddressables.OnInit += OnInitGetData;
        //     return;
        // }
        //
        // InitGetData();
    }

    // private void OnInitGetData()
    // {
    //     if (_getDataAddressables.IsInit == true)
    //     {
    //         _getDataAddressables.OnInit -= OnInitGetData;
    //         InitGetData();
    //     }
    //
    // }

    private void InitGetData()
    {
        //Дело в том, что если под одним Label наход. несколько обьектов, то исп LoadAssetAsync<T> уже нельзя т.к он вернет лиш 1 элемент(и то фиг пойми какой)
        //По этому нужно исп. метод, котор загружает список элементов по ключу (LoadAssetsAsync<T>), и тут ОБЯЗ! нужно указ помимо ключа, еще и метод, который будет возращать callback каждого элемента
        //И да, пока хорощей обертки для этого метода не сделал, по этому просто в таком виде как пример будет(потом сделаю)
        Debug.Log("Послан запрос на списка обьектов Object (UnityEngine.Object) по указ Label");
        var dataCallback = Addressables.LoadAssetsAsync<Object>(_labelKey, CallbackElementObject);

        if (dataCallback.IsDone == true)
        {
            CompletedGetData();
        }
        else
        {
            dataCallback.Completed += OnCompletedGetData;
        }

        //Это метод срабатывает когда элемент из списка загрузок возвращает callback
        void CallbackElementObject(object obj)
        {
            //Тут какая то спец логика по обработке кажд. элемента(хз, это надо будет вынести отдельно в арг. метода обертки)
        }

        void OnCompletedGetData(AsyncOperationHandle<IList<Object>> data)
        {
            if (dataCallback.IsDone == true)
            {
                dataCallback.Completed -= OnCompletedGetData;
                CompletedGetData();
            }
        }

        void CompletedGetData()
        {
            _localData = dataCallback;
            
            Debug.Log("----- Данные получены ----");
            Debug.Log("Статус запроса = " + dataCallback.Status.ToString());
            Debug.Log("Получен кол - во обьектов = " + dataCallback.Result.Count);

            foreach (var VARIABLE in dataCallback.Result)
            {
                Debug.Log("Вывод данных об обьекте = " + VARIABLE);
            }

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
