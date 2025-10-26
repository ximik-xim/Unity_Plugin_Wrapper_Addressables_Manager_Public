using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Обертка нужна, для получения списка ID каталогов
/// </summary>
public class CallbackListCatalogIDAddressablesWrapper : AbsServerRequestDataWrapper<List<string>>
{
    public CallbackListCatalogIDAddressablesWrapper(int id) : base(id)
    {
    }
}

