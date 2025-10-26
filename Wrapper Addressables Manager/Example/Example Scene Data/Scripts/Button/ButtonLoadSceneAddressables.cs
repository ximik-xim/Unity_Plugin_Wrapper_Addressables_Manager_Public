using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// По нажатию на кнопку запустит получение(загрузку) указ сцене
/// </summary>
public class ButtonLoadSceneAddressables : MonoBehaviour
{
    public bool IsInit => _isInit;
    private bool _isInit = false;
    public event Action OnInit;

    [SerializeField]
    private Button _button;

    [SerializeField]
    private AssetReferenceSceneCustom _keyNameScene;

    [SerializeField]
    private LoadTargetSceneAddressables _loadTargetSceneAddressables;

    
    private void Awake()
    {
        if (_loadTargetSceneAddressables.IsInit == false)
        {
            _loadTargetSceneAddressables.OnInit += OnInitStorageIsGetScene;
        }


        CheckInit();
    }

    private void OnInitStorageIsGetScene()
    {
        if (_loadTargetSceneAddressables.IsInit == true)
        {
            _loadTargetSceneAddressables.OnInit -= OnInitStorageIsGetScene;
            CheckInit();
        }
    }

    private void CheckInit()
    {
        if (_loadTargetSceneAddressables.IsInit == true)
        {
            Init();

            _isInit = true;
            OnInit?.Invoke();
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
        _loadTargetSceneAddressables.StartLoadScene(_keyNameScene);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(ButtonClick);
    }
}