using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

/// <summary>
/// Содержит список вариантов, откудова можно взять обьект.
/// (при ERROR от сервера, будет переключаться на след. вариант)
/// Будет поочереди перебирать все варианты, пока кто то не вернет статус OK, или пока не закончатся варианты
/// </summary>
public class GetDataAddressablesVariantsListT : AbsCallbackGetDataTAddressables
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;

    /// <summary>
    /// Буду ли переключать на следующий вариант, если статус OK, НО данные при этом Null
    /// </summary>
    [SerializeField]
    private bool _moveNextVariantDataNull;

    /// <summary>
    /// Список вариантов, откуда можно взять обьект
    /// </summary>
    [SerializeField]
    private List<AbsCallbackGetDataTAddressables> _variants;

    /// <summary>
    /// Список Id callback, которые сейчас в ожидании
    /// (сериализован просто для удобного отслеживания в инспекторе)
    /// </summary>
    [SerializeField]
    private List<int> _idCallback = new List<int>();

    /// <summary>
    /// сериализую, что бы видить через инспектор, какой вариант не иниц.
    /// </summary>
    [SerializeField]
    private List<AbsCallbackGetDataTAddressables> _bufferInit = new List<AbsCallbackGetDataTAddressables>();

    private void Awake()
    {
        _idCallback.Clear();
        StartInitLocal();
    }

    void StartInitLocal()
    {
        bool _isStart = false;

        StartAction();

        void StartAction()
        {
            // просто блокировать проверку готовности задачи пока у всех не подпишусь
            _isStart = true;

            foreach (var VARIABLE in _variants)
            {
                if (VARIABLE.IsInit == false)
                {
                    _bufferInit.Add(VARIABLE);
                    VARIABLE.OnInit += CheckCompletedInit;
                }

            }

            _isStart = false;

            CheckCompletedInit();

        }

        void CheckCompletedInit()
        {
            if (_isStart == false)
            {
                int targetCount = _bufferInit.Count;
                for (int i = 0; i < targetCount; i++)
                {
                    if (_bufferInit[i].IsInit == true)
                    {
                        _bufferInit[i].OnInit -= CheckCompletedInit;
                        _bufferInit.RemoveAt(i);
                        i--;
                        targetCount--;
                    }
                }

                if (_bufferInit.Count == 0)
                {
                    Init();
                }
            }
        }
    }

    private void Init()
    {
        _isInit = true;
        OnInit?.Invoke();
    }

    public override GetServerRequestData<AsyncOperationHandle<T>> GetData<T>(object data)
    {
        int id = GetUniqueId();

        CallbackRequestDataWrapperT<AsyncOperationHandle<T>> wrapperCallbackData = new CallbackRequestDataWrapperT<AsyncOperationHandle<T>>(id);
        _idCallback.Add(id);

        if (_variants.Count > 0)
        {
            int lastId = 0;

            NextElement();

            void NextElement()
            {
                if (_variants.Count >= lastId + 1)
                {
                    var requestData = _variants[lastId];
                    var dataCallback = requestData.GetData<T>(data);

                    if (dataCallback.IsGetDataCompleted == true)
                    {
                        CompletedCallback();
                    }
                    else
                    {
                        dataCallback.OnGetDataCompleted -= OnCompletedCallback;
                        dataCallback.OnGetDataCompleted += OnCompletedCallback;
                    }

                    return;


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
                        //Если успешно получил данные
                        if (dataCallback.StatusServer == StatusCallBackServer.Ok)
                        {
                            //если вклучена проверка на null, и пришел null в данных(хотя статус и OK)
                            if (_moveNextVariantDataNull == true && dataCallback.GetData.Result == null)
                            {
                                Debug.Log("Переключ. получ статус ОК, но пришел null, переключ. на след. вариант");

                                dataCallback.OnGetDataCompleted -= OnCompletedCallback;

                                                            
                                if (dataCallback.GetData.IsValid() == true) 
                                {
                                    Addressables.Release(dataCallback.GetData);   
                                }
                                
                                //переключ на след. вариант
                                lastId++;
                                NextElement();
                            }
                            else
                            {
                                Debug.Log("Переключ. получ статус ОК");
                                //заполняю данные для ответа
                                wrapperCallbackData.Data.StatusServer = dataCallback.StatusServer;
                                wrapperCallbackData.Data.GetData = dataCallback.GetData;

                                wrapperCallbackData.Data.IsGetDataCompleted = true;
                                wrapperCallbackData.Data.Invoke();

                                _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                                return;
                            }
                        }
                        else
                        {
                            Debug.Log("Переключ. получ статус ERROR, переключ. на след. вариант");

                            dataCallback.OnGetDataCompleted -= OnCompletedCallback;

                            if (dataCallback.GetData.IsValid() == true) 
                            {
                                Addressables.Release(dataCallback.GetData);   
                            }
                            
                            lastId++;
                            //Если же, этот способ получить данные вернул ERROR, то переключаюсь на следующий способ
                            NextElement();
                        }
                    }

                }
                else
                {
                    Debug.Log("Переключ. элементы в списке закончились. Возрв. ERROR");
                    //Если элементы в списке закончились, значит оправляю Error
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    wrapperCallbackData.Data.GetData = default;

                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();

                    _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
                    return;
                }

            }
        }
        else
        {
            Debug.Log("Переключ. нет элементов в списке. Возрв. ERROR");
            //Если элементы в списке тупо нет, значит оправляю Error
            wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
            wrapperCallbackData.Data.GetData = default;

            wrapperCallbackData.Data.IsGetDataCompleted = true;
            wrapperCallbackData.Data.Invoke();

            _idCallback.Remove(wrapperCallbackData.Data.IdMassage);
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