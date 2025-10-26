using System;
using UnityEngine;

/// <summary>
/// Пример запуска обновления для всего(каталогов и обьектов)
/// </summary>
public class ExampleStartUpdateAll : MonoBehaviour
{
    /// <summary>
    /// Логика обновления каталогов и обьектов(все разом)
    /// </summary>
    [SerializeField]
    private AbsCheckAndDownloadUpdateObject _downloadUpdateObject;

    private void Awake()
    {
        if (_downloadUpdateObject.IsInit == false)
        {
            _downloadUpdateObject.OnInit += OnInitCheckAndDownloadUpdateObject;
        }
        

        CheckInit();
    }

    private void OnInitCheckAndDownloadUpdateObject()
    {
        if (_downloadUpdateObject.IsInit == true)
        {
            _downloadUpdateObject.OnInit -= OnInitCheckAndDownloadUpdateObject;
            CheckInit();
        }
    }
    

    private void CheckInit()
    {
        if (_downloadUpdateObject.IsInit == true)
        {
            StartLogic();
        }
    }


    private void StartLogic()
    {
        //Запускаем логику обновления для каталогов и обьектов
        var dataCallback = _downloadUpdateObject.StartCheckUpdateCatalog();

        if (dataCallback.IsGetDataCompleted == true)
        {
            CompletedCallback();
        }
        else
        {
            dataCallback.OnGetDataCompleted -= OnCompletedCallback;
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
            if (dataCallback.StatusServer == StatusCallBackServer.Ok)
            {
                if (dataCallback.GetData != null)
                {
                    if (dataCallback.GetData.StatusAllCallBack == TypeStorageStatusCallbackIResourceLocator.Ok)
                    {
                        Debug.Log("УХУ Обнова удалась УХУ");
                        
                    }
                    else
                    {
                        Debug.Log("Увы, что то сдохло, надо идти чинить");
                    }
                    
                    return;
                }
                else
                {
                    Debug.Log("УХУ Обнова удалась УХУ");
                    return;
                }
            }

            Debug.Log("Увы, что то сдохло, надо идти чинить");
        }
    }
}
