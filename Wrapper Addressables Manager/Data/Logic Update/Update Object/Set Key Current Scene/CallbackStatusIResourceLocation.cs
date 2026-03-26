using UnityEngine;

/// <summary>
/// Обертка нужна, для получения хранилеща со списком IResourceLocator(и статусом запроса) обьектов
/// </summary>
public class CallbackStatusIResourceLocation : AbsServerRequestDataWrapper<StorageStatusCallbackIResourceLocation>
{
    public CallbackStatusIResourceLocation(int id) : base(id)
    {
    }
}
