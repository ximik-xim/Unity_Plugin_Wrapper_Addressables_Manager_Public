using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Пока для тестов заглушка
/// (потом подум. как лучше брать ID с каталогов)
/// </summary>
public class TestGetListCatalogID : AbsGetListCatalogID
{
    public override bool IsInit => true;
    public override event Action OnInit;

    [SerializeField]
    private bool _isBlockList = false;

    [SerializeField]
    private List<string> _catalogID;
    
    private void Awake()
    {
        OnInit?.Invoke();
    }

    public override bool IsBlockList()
    {
        return _isBlockList;
    }

    public override List<string> GetCatalogID()
    {
        return _catalogID;
    }
    
    
    
    
    
    
    
    
    
//     // Update is called once per frame
//     void Update()
//     {
//
//
//
//
//
//         В1) если знаю путь до фаила
//         string json = System.IO.File.ReadAllText(catalogPath);
//         var dict = JsonUtility.FromJson<SerializableDict>(json);
//         Debug.Log("Catalog ID: " + dict.m_LocatorId);
//
//         [System.Serializable]
//         class SerializableDict
//         {
//
//     public string m_LocatorId;
// }
//
// В2) Вручную залесть в фаил и глянуть 



}
