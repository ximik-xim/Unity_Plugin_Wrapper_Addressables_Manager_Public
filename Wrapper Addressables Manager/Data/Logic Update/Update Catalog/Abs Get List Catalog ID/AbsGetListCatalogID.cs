using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Нужна, что бы получить список ID каталогов
/// (исп. что бы понять, можно ли обн. этот каталог или нет) 
/// </summary>
public abstract class AbsGetListCatalogID : MonoBehaviour
{
    public abstract bool IsInit { get; }
    public abstract event Action OnInit;
    public abstract bool IsBlockList();

    public abstract List<string> GetCatalogID();
    
    
}


