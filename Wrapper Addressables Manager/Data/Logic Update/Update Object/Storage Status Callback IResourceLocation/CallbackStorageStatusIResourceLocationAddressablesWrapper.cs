using UnityEngine;

/// <summary>
/// Обертка нужна, для получения хранилеща со списком IResourceLocator(и статусом запроса) обьектов
/// </summary>
public class CallbackStorageStatusIResourceLocationAddressablesWrapper : AbsServerRequestDataWrapper<StorageStatusCallbackIResourceLocation>
{
    public CallbackStorageStatusIResourceLocationAddressablesWrapper(int id) : base(id)
    {
    }
}
