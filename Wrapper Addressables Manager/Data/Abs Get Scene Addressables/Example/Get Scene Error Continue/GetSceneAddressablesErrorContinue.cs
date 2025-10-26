using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>
/// Это обертка нужна, что бы сделать переотправку запроса несколько раз(в случ. ошибки)
/// </summary>
public class GetSceneAddressablesErrorContinue : AbsCallbackGetSceneAddressables
{
    public override bool IsInit => _isInit;
    private bool _isInit = false;
    public override event Action OnInit;
    
    [SerializeField]
    private AbsCallbackGetSceneAddressables _absGetData;

    [SerializeField]
    private LogicErrorCallbackRequest _errorLogic;

    private void Awake()
    {
        if (_errorLogic.IsInit == false)
        {
            _errorLogic.OnInit += OnInitErrorLogic;
        }
       
        if (_absGetData.IsInit == false)
        {
            _absGetData.OnInit += OnInitGetData;
        }

        CheckInit();
    }

    private void OnInitErrorLogic()
    {
        _errorLogic.OnInit -= OnInitErrorLogic;
        CheckInit();
    }
    
    private void OnInitGetData()
    {
        _absGetData.OnInit -= OnInitGetData;
        CheckInit();
    }

    private void CheckInit()
    {
        if (_isInit == false)
        {
            if (_errorLogic.IsInit == true && _absGetData.IsInit == true)
            {
                _isInit = true;
                OnInit?.Invoke();
            }
        }
    }

    public override GetServerRequestData<AsyncOperationHandle<SceneInstance>> GetData(DataSceneLoadAddressable data)
    {
        Debug.Log("Запрос на загр. сцены был отправлен");
        //Тут именно скопировать нужно
        DataSceneLoadAddressable copiedData = data;

        //запрашиваю загузку сцены 
        var dataCallback = _absGetData.GetData(copiedData);
        //делаю обертку т.к могу несколько раз делать запросы на данные, а верну лиш 1 итог. результат 
        CallbackRequestDataWrapperT<AsyncOperationHandle<SceneInstance>> wrapperCallbackData = new CallbackRequestDataWrapperT<AsyncOperationHandle<SceneInstance>>(dataCallback.IdMassage);

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
            //Если успешно получил данные
            if (dataCallback.StatusServer == StatusCallBackServer.Ok)
            {
                Debug.Log("Запрос на загр. сцены успешен");
                //очищаю список ошибок
                _errorLogic.OnRemoveAllError();

                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = dataCallback.StatusServer;
                wrapperCallbackData.Data.GetData = dataCallback.GetData;

                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();
                return;
            }
            else
            {
                //добавляю ошибку
                _errorLogic.OnAddError();

                //Проверяю, могу ли еще раз отпр. запрос
                if (_errorLogic.IsContinue == true)
                {
                    Debug.Log("Запрос на загр. сцены ошибка. Переотправка");
                    
                    if (dataCallback.GetData.IsValid() == true) 
                    {
                        //Addressables.Release(dataCallback);
                        Addressables.UnloadSceneAsync(dataCallback.GetData);
                    }
                    
                    //заного отпр. запрос, и по новой 
                    dataCallback = _absGetData.GetData(copiedData);
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
                }
                else
                {
                    Debug.Log("Запрос на загр. сцены ошибка. Попытки кончились. Возр. ERROR");
                    //если попытки достучаться до сервера закончились, то отпр. все как есть(ошибку)

                    //очищаю список ошибок
                    _errorLogic.OnRemoveAllError();

                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = dataCallback.StatusServer;
                    wrapperCallbackData.Data.GetData = dataCallback.GetData;

                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();

                    return;
                }

            }
        }

        //возр. обертку с callback, когда данные будут готовы(и с неё же получ. данные)
        return wrapperCallbackData.DataGet;
    }
}