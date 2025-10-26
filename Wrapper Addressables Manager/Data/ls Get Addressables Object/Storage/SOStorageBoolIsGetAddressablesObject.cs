using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Используется
/// 1) как список обектов которы разрешено(или запрещено) обновлять(или загружать)
/// 2) так и список обьектов которых надо обновить
/// </summary>
[CreateAssetMenu(menuName = "Storage Is Get Addressables Object")]
public class SOStorageBoolIsGetAddressablesObject : ScriptableObject, IInitScripObj
{
    public event Action OnInit;
    public bool IsInit => _isInit;
    private bool _isInit = false;

    //Нужна, т.к до момента иниц. идет время(т.к асинхроность), и одного IsInit тут не хватит
    private bool _isStart = false;

    /// <summary>
    /// это списоки разрешенных или запрещенных к загр. обьектов 
    /// </summary>
    [SerializeField] 
    private bool _isBlockList;
    public bool IsBlockList => _isBlockList;
    
    [SerializeField] 
    private List<DataAssetReferenceAndFilterInternalId> _refObj;
    
    [SerializeField] 
    private List<DataKeyAddressablesAndFilterInternalId>  _key;
    
    /// <summary>
    /// Интерфеисы ваше указ. обьектов
    /// </summary>
    private List<IResourceLocation> _resourceLocations = new List<IResourceLocation>();
    
    public void InitScripObj()
    {
#if UNITY_EDITOR
        
        EditorApplication.playModeStateChanged -= OnUpdateStatusPlayMode;
        EditorApplication.playModeStateChanged += OnUpdateStatusPlayMode;

        //На случай если event playModeStateChanged не отработает при входе в режим PlayModeStateChange.EnteredPlayMode (такое может быть, и как минимум по этому нужна пер. bool _init)
        if (EditorApplication.isPlaying == true)
        {
            if (_isInit == false)
            {
                Awake();
            }
        }
        else
        {
            //Нужен, что бы сбросить переменную при запуске проекта(т.к при выходе(закрытии) из проекта, переменная не факт что будет сброшена)
            _isInit = false;
            _isStart = false;
        }
#endif
    }
        
#if UNITY_EDITOR
    private void OnUpdateStatusPlayMode(PlayModeStateChange obj)
    {
        //При выходе из Play Mode произвожу очистку данных(тем самым эмулирую что при след. запуске(вхождение в Play Mode) данные будут обнулены)
        if (obj == PlayModeStateChange.ExitingPlayMode)
        {
            if (_isInit == true)
            {
                _isInit = false;
                _isStart = false;
            }
        }
        
        // При запуске игры эмулирую иниц. SO(По идеи не совсем верно, т.к Awake должен произойти немного раньше, но пофиг)(как показала практика метод может не сработать)
        if (obj == PlayModeStateChange.EnteredPlayMode)
        {
            if (_isInit == false)
            {
                Awake();
            }
            
        }
    }
#endif
    
    private void Awake()
    {
        if (_isInit == false && _isStart == false) 
        {
            _isStart = true;
            _resourceLocations.Clear();
            StartGetIResourceLocation();
        }
    }

    private void OnEnable()
    {
        if (_isStart == false)
        {
            Awake();    
        }
    }

    /// <summary>
    /// Этот метод ищет этот обьект, среди списокв выше
    /// </summary>
    public bool IsGetObject(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        // AssetReference
        if (obj is AssetReference assetRef)
        {
            //Если это список разрешенных обьектов
            if (_isBlockList == false)
            {
                foreach (var VARIABLE in _refObj)
                {
                    if (VARIABLE.RefObj == assetRef) 
                    {
                        return true;
                    }
                }
                
                return false;
            }

            //Если это список запрещенных обьектов
            foreach (var VARIABLE in _refObj)
            {
                if (VARIABLE.RefObj == assetRef) 
                {
                    return false;
                }
            }

            return true;

        }
        
        // Key
        if (obj is string strKey)
        {
            //Если это список разрешенных обьектов
            if (_isBlockList == false)
            {
                foreach (var VARIABLE in _key)
                {
                    if (VARIABLE.KeyAddressables == strKey) 
                    {
                        return true;
                    }
                }

                return false;
            }

            //Если это список запрещенных обьектов
            foreach (var VARIABLE in _key)
            {
                if (VARIABLE.KeyAddressables == strKey) 
                {
                    return false;
                }
            }

            return true;

        }


        // IResourceLocation
        if (obj is IResourceLocation iResourceLocation)
        {
            //Если это список разрешенных обьектов

            return IsGetObjectIRes(iResourceLocation);
        }
        
        return false;
    }
    

    private void StartGetIResourceLocation()
    {
        List<CabllbackDataListIResourceLocationFilterInternalId> listCallback = new List<CabllbackDataListIResourceLocationFilterInternalId>();


        foreach (var VARIABLE in _refObj)
        {
            var callback = Addressables.LoadResourceLocationsAsync(VARIABLE.RefObj);
            
            CabllbackDataListIResourceLocationFilterInternalId callbackData = new CabllbackDataListIResourceLocationFilterInternalId(callback, VARIABLE.FilterInternalId_IResourceLocation);
            listCallback.Add(callbackData);
        }

        foreach (var VARIABLE in _key)
        {
            var callback = Addressables.LoadResourceLocationsAsync(VARIABLE.KeyAddressables);

            CabllbackDataListIResourceLocationFilterInternalId callbackData = new CabllbackDataListIResourceLocationFilterInternalId(callback, VARIABLE.FilterInternalId_IResourceLocation);
            listCallback.Add(callbackData);
        }

        Check();

        void Check()
        {
            int targetCount = listCallback.Count;

            for (int i = 0; i < targetCount; i++)
            {
                //Если этот callbak закончил работу, обрабатываю данные
                if (listCallback[i].CallbackData.IsDone == true)
                {
                    //Те данные, что пришли добавляю в общ список данных, получ. с сервера
                    foreach (var VARIABLE in listCallback[i].CallbackData.Result)
                    {
                        //Если у обьекта есть фильтрация по IResourceLocation(через место нахождение обьекта)
                        if (listCallback[i].FilterInternalId_IResourceLocation != "" && listCallback[i].FilterInternalId_IResourceLocation != String.Empty)
                        {
                            //Если начало пути (или весь путь целиком) до обьекта совпадает с указ в фильтре
                            if (VARIABLE.InternalId.StartsWith(listCallback[i].FilterInternalId_IResourceLocation) == true) 
                            {
                                //Добавляю интерфеис IResourceLocation в список
                                _resourceLocations.Add(VARIABLE);    
                            }
                            
                        }
                        else
                        {
                            //Если фильтрации для IResourceLocation нету, то добовляю все IResourceLocation найденные для этого обьекта
                            _resourceLocations.Add(VARIABLE);    
                        }
                    }
                    
                    //Отписывась от проверки 
                    listCallback[i].CallbackData.Completed -= OnCheck;
                    
                    if (listCallback[i].CallbackData.IsValid() == true) 
                    {
                        Addressables.Release(listCallback[i].CallbackData);
                    }
                    
                    //Удаляю Callback с сервера
                    listCallback.RemoveAt(i);
                    i--;
                    targetCount--;
                }
                else
                {
                    //Тут отписка обяз. Т.к сюда буду несколько раз заходиться
                    listCallback[i].CallbackData.Completed -= OnCheck;
                    listCallback[i].CallbackData.Completed += OnCheck;
                }
            }

            if (listCallback.Count == 0)
            {
                StartInit();
            }
        }

        void OnCheck(AsyncOperationHandle<IList<IResourceLocation>> data)
        {
            Check();
        }

    }

    private void StartInit()
    {
        if (_isInit == false)
        {
            _isInit = true;
            OnInit?.Invoke();
        }
    }

    public bool IsGetObjectIRes(IResourceLocation iResourceLocation)
    {
        if (_isBlockList == false)
        {
            foreach (var VARIABLE in _resourceLocations)
            {
                if (VARIABLE.PrimaryKey == iResourceLocation.PrimaryKey && VARIABLE.InternalId == iResourceLocation.InternalId && VARIABLE.ResourceType == iResourceLocation.ResourceType)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            foreach (var VARIABLE in _resourceLocations)
            {
                if (VARIABLE.PrimaryKey == iResourceLocation.PrimaryKey && VARIABLE.InternalId == iResourceLocation.InternalId && VARIABLE.ResourceType == iResourceLocation.ResourceType)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public IReadOnlyList<IResourceLocation> GetAllObject()
    {
        return _resourceLocations;
    }
}





