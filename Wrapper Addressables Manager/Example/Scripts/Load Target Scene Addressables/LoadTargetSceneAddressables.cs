using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// Обертка над логкикой загрузки сцены
/// (идея в том, что тут в инспекторе уст. настройки для загрузки сцены,
/// а ключ определяю где то из вне)
///
/// Так же имеет логику авто удаления данных об сцене из оперативки,
/// когда уйдем с загруженной сцены
/// </summary>
public class LoadTargetSceneAddressables : MonoBehaviour
{
    public event Action OnInit
    {
        add
        {
            _loadSceneAddressables.OnInit += value;
        }

        remove
        {
            _loadSceneAddressables.OnInit -= value;
        }
    }

    public bool IsInit => _loadSceneAddressables.IsInit;
    
    [SerializeField]
    private AbsCallbackGetSceneAddressables _loadSceneAddressables;

    /// Т.К key являеться object и не сериализ. По этому ключ сериализ. отдельно, а ост. данные сразу в нем сериализую
    /// </summary>
    [SerializeField]
    private DataSceneLoadAddressable _sceneLoadSettings;

    private AsyncOperationHandle<SceneInstance> _handleScene;

    /// <summary>
    /// Производит ли автоматически удаление данных сцены из опер. памяти при переходе на другую сцену
    /// </summary>
    [SerializeField]
    private bool _isAutoUnloadScene = true;

    public GetServerRequestData<AsyncOperationHandle<SceneInstance>> StartLoadScene(object keyScene)
    {
        if (_isAutoUnloadScene == true) 
        {
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
        
        _sceneLoadSettings = new DataSceneLoadAddressable(keyScene, _sceneLoadSettings.LoadMode, _sceneLoadSettings.ActivateOnLoad, _sceneLoadSettings.Priority, _sceneLoadSettings.ReleaseMode);
        var callback = _loadSceneAddressables.GetData(_sceneLoadSettings);

        if (_isAutoUnloadScene == true)
        {
            if (callback.IsGetDataCompleted == false)
            {
                callback.OnGetDataCompleted += OnGetDataCompleted;
            }
            else
            {
                GetDataCompleted();
            }

            void OnGetDataCompleted()
            {
                if (callback.IsGetDataCompleted == true)
                {
                    callback.OnGetDataCompleted -= OnGetDataCompleted;
                    GetDataCompleted();
                }
            }

            void GetDataCompleted()
            {
                //Нужен т.к в случае ошибки, handle будет автоматически уничтожен и тут ловить больше нечего
                if (callback.StatusServer == StatusCallBackServer.Ok)
                {
                    _handleScene = callback.GetData;
                    StartAutoUnloadScene();
                }
                else
                {
                    Destroy(this);
                }
            }
            
        }


        return callback;
    }
    
    public GetServerRequestData<AsyncOperationHandle<SceneInstance>> StartLoadScene(DataSceneLoadAddressable sceneLoadSettings)
    {
        if (_isAutoUnloadScene == true) 
        {
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.transform.parent = null;
        }
        
        var callback = _loadSceneAddressables.GetData(sceneLoadSettings);

        if (_isAutoUnloadScene == true)
        {
            if (callback.IsGetDataCompleted == false)
            {
                callback.OnGetDataCompleted += OnGetDataCompleted;
            }
            else
            {
                GetDataCompleted();
            }

            void OnGetDataCompleted()
            {
                if (callback.IsGetDataCompleted == true)
                {
                    callback.OnGetDataCompleted -= OnGetDataCompleted;
                    GetDataCompleted();
                }
            }

            void GetDataCompleted()
            {
                //Нужен т.к в случае ошибки, handle будет автоматически уничтожен и тут ловить больше нечего
                if (callback.StatusServer == StatusCallBackServer.Ok)
                {
                    _handleScene = callback.GetData;
                    StartAutoUnloadScene();    
                }
                else
                {
                    Destroy(this);
                }
            }
            
        }


        return callback;
    }
    
    // т.к при загрузку сцены сразу и переходим на неё, то вызыв OnDestroy будет осуществлен в момент перехода с текущей сцена на сцену котор загружаем,
    // а значит если в этот момент вызову Addressables.UnloadSceneAsync, то получиться, что я загружаю сцену XXXX и сразу же эту сцену(XXXX) уничтожаю 
    // что мне не подходит
    //     
    //По этому задумка в след. До момента перехода на сцену XXXX(которую загружаем), мы делаем GM на котором находиться этот скрипт DontDestroy
    //И когда перейдем на сцену XXXX, то этот GM переместим с DontDestroy на сцену XXXX. Тем самым сделаем так, что бы метод OnDestroy сработал, когда будем уходить со сцены,
    //а значит и в этом OnDestroy мы можем выгрузить(удалить) сцену из оперативки
 
    private void StartAutoUnloadScene()
    {
        if (_handleScene.IsDone == false) 
        {
            _handleScene.Completed += OnCheckIsDoneLoadScene;
            
        }
        else
        {
            CheckIsDoneLoadScene();
        }
    }

    private void OnCheckIsDoneLoadScene(AsyncOperationHandle<SceneInstance> obj)
    {
        if (_handleScene.IsDone == true) 
        {
            _handleScene.Completed -= OnCheckIsDoneLoadScene;
            CheckIsDoneLoadScene();
        }
    }

    private void CheckIsDoneLoadScene()
    {
        if (_handleScene.Result.Scene.name != SceneManager.GetActiveScene().name)
        {
            SceneManager.sceneLoaded += OnCheckSceneLoaded;
        }
        else
        {
            MoveCurrentScriptInCurrentScene();
        }
    }

    private void OnCheckSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (_handleScene.Result.Scene.name == SceneManager.GetActiveScene().name)
        {
            SceneManager.sceneLoaded -= OnCheckSceneLoaded;
            MoveCurrentScriptInCurrentScene();
        }
    }

    private void MoveCurrentScriptInCurrentScene()
    {
        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (_handleScene.IsValid() == true) 
        {
            Addressables.UnloadSceneAsync(_handleScene);
        }
    }
    
}
