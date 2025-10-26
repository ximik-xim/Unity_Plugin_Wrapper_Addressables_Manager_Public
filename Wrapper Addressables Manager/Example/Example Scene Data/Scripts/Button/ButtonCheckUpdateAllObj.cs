using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Тестово запускает обновление для всех элементов
/// </summary>
public class ButtonCheckUpdateAllObj : MonoBehaviour
{
    [SerializeField]
   private Button _button;
       
     [SerializeField]
    private AbsCheckAndDownloadUpdateObject _downloadUpdateObject;

    private void Awake()
    {
        Debug.Log("Иниц. началась");
        
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
            Debug.Log("Иниц. Chekc");
            _downloadUpdateObject.OnInit -= OnInitCheckAndDownloadUpdateObject;
            CheckInit();
        }
    }
    

    private void CheckInit()
    {
        if (_downloadUpdateObject.IsInit == true)
        {
            Debug.Log("Иниц. закончилась");
            Init();
        }
    }

    private void Init()
    {
        _button.onClick.AddListener(ButtonClick);
    }


    private void ButtonClick()
    {
        StartLogic();
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

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(ButtonClick);
    }
}
