using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

/// <summary>
/// Нужен что бы проверить, есть ли обновления для каталогов
/// (вернет все ID каталогов, которым нужно обновление)
/// </summary>
public class CheckUpdateCatalogs : MonoBehaviour
{
    public bool IsInit => _isInit;
    private bool _isInit = false;
    public event Action OnInit;
    
    [SerializeField]
    private LogicErrorCallbackRequest _errorLogic;

    /// <summary>
    /// Время ожидание, до переотправки запроса
    /// (нужно, т.к иначе, если сразу сделаю переотправку запроса, получу ERROR Attempting to use an invalid operation handle)
    /// </summary>
    [SerializeField]
    private float _timeSecondWait = 0.5f;

    /// <summary>
    /// Буду ли исп. для ожидания асинхронную операцию
    /// (если нет, то буду исп. корутину)
    /// </summary>
    [SerializeField]
    private bool _useWaitAsycn = true;
    
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
    
    public GetServerRequestData<List<string>> StartCheckUpdateCatalog()
    {
        Debug.Log("Запрос на проверку обн. каталогов был отправлен");
        //запрашиваю данные
        var dataCallback = Addressables.CheckForCatalogUpdates();
        
        int id = GetUniqueId();
        //делаю обертку т.к могу несколько раз делать запросы на данные, а верну лиш 1 итог. результат 
        CallbackListCatalogIDAddressablesWrapper wrapperCallbackData = new CallbackListCatalogIDAddressablesWrapper(id);
        _idCallback.Add(id);

        //проверяю готовы ли данные 
        if (dataCallback.IsDone == true)
        {
            //да готовы, начинаю обработку
            CompletedCallback();
        }
        else
        {
            //не, неготовы, начинаю ожидание, пока прийдут
            dataCallback.Completed += OnCompletedCallback;
        }
        
        void OnCompletedCallback(AsyncOperationHandle<List<string>> obj)
        {
            //Если данные пришли
            if (dataCallback.IsDone == true)
            {
                dataCallback.Completed -= OnCompletedCallback;
                //начинаю обработку данных
                CompletedCallback();    
            }
        }

        void CompletedCallback()
        {
            //Если успешно получил данные
            if (dataCallback.Status == AsyncOperationStatus.Succeeded)
            {
                
                Debug.Log("Запрос на проверку обн. каталогов успешен");
                
                //очищаю список ошибок
                _errorLogic.OnRemoveAllError();
                
                //заполняю данные для ответа
                wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
                wrapperCallbackData.Data.GetData = dataCallback.Result;
                wrapperCallbackData.Data.IsGetDataCompleted = true;
                wrapperCallbackData.Data.Invoke();
                
                _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
                
                //Тут не надо вручную удалять данные из оперативки, если вручну не выключели авто очистки у метода CheckForCatalogUpdates( тут передать false)
                //иначе метод ломаеться(ля приколы)
                // if (dataCallback.IsValid() == true) 
                // {
                //     Addressables.Release(dataCallback);
                // }
                
                return;
            }
            else
            {
                //добавляю ошибку
                _errorLogic.OnAddError();
                
                //Тут не надо вручную удалять данные из оперативки, если вручну не выключели авто очистки у метода CheckForCatalogUpdates( тут передать false)
                //иначе метод ломаеться(ля приколы)
                // if (dataCallback.IsValid() == true) 
                // {
                //     Addressables.Release(dataCallback);
                // }
                
                //Проверяю, могу ли еще раз отпр. запрос
                if (_errorLogic.IsContinue == true) 
                {
                    Debug.Log("Запрос на проверку обн. каталогов ошибка. Подготовка к переотправки");
                    
                    //Тут пришлось сделать ожид. перед отпр. след запрос т.к иначе(если сразу дел. след. запрос) получаю ERROR Attempting to use an invalid operation handle) 
                    if (_useWaitAsycn == true) 
                    {
                        //Асинхронное ожидание
                        Debug.Log("Асинхрон. Ожид. перед переотправкой");
                        WaitRequestAsync();
                    }
                    else
                    {
                        //Ожидание через корутину
                        Debug.Log("Ожид. через корутину перед переотправкой");
                        StartCoroutine(WaitRequestCoroutine());
                    }
                    
                    async Task WaitRequestAsync()
                    {
                        // Ждём указ. кол - во секунд, перед переотправкой
                        await Task.Delay((int)(_timeSecondWait * 1000));

                        NextPushRequest();
                    }
                    
                    IEnumerator WaitRequestCoroutine()
                    {
                        // Ждём указ. кол - во секунд, перед переотправкой
                        yield return new WaitForSeconds(_timeSecondWait);

                        NextPushRequest();
                    }
                    
                    
                    void NextPushRequest()
                    {
                        Debug.Log("Запрос на проверку обн. каталогов ошибка. Переотправка начата");
                        
                        //заного отпр. запрос, и по новой 
                        dataCallback = Addressables.CheckForCatalogUpdates();
                        if (dataCallback.IsDone == true)
                        {
                            CompletedCallback();
                        }
                        else
                        {
                            dataCallback.Completed -= OnCompletedCallback;
                            dataCallback.Completed += OnCompletedCallback;
                        }
                    }
                    
                    
                    return;
                }
                else
                {
                    Debug.Log("Запрос на проверку обн. каталогов ошибка. Попытки кончились. Возр. ERROR");
                    
                    //если попытки достучаться до сервера закончились, то отпр. все как есть(ошибку)
                    
                    //очищаю список ошибок
                    _errorLogic.OnRemoveAllError();
                    
                    //заполняю данные для ответа
                    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Error;
                    wrapperCallbackData.Data.GetData = dataCallback.Result;
                    wrapperCallbackData.Data.IsGetDataCompleted = true;
                    wrapperCallbackData.Data.Invoke();
                    
                    _idCallback.Remove(wrapperCallbackData.DataGet.IdMassage);
                    
                    return;
                }
                
            }
        }

        //возр. обертку с callback, когда данные будут готовы(и с неё же получ. данные)
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
