using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Делает запрос, к хранилещу с обьектами(которые или разрешено или запрещено брать)
/// ! И СРАВНИВАЕТ ИМЕЕНО С ОБЬЕКТАМИ в списках у хранилеща (а не как интерфеис IResourceLocation)
/// ЭТО МОЖЕТ ПРИВЕСТИ, К ТОМУ, ЧТО ОБЬЕКТ БУДЕТ ЗАПРЕЩЕН, А КЛЮЧ РАЗРЕШЕН (ИСПОЛЬЗОВАТЬ С УМОМ !!)
/// </summary>
public class IsGetAddressablesObjectSOStorageObject : AbsBoolIsGetAddressablesObject
{
    public override bool IsInit => _storageBool.IsInit;
    public override event Action OnInit
	{
		add
		{
			_storageBool.OnInit += value;
		}

		remove
		{
			_storageBool.OnInit -= value;
		}
	}

    [SerializeField] 
    private SOStorageBoolIsGetAddressablesObject _storageBool;

    public override GetServerRequestData<bool> IsGet(object data)
    {
	    CallbackDataBool wrapperCallbackData = new CallbackDataBool(0);

	    bool isGet = false;
	    
	    wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;

	    if (_storageBool.IsGetObject(data) == true) 
	    {
		    isGet = true;
	    }
	    
	    wrapperCallbackData.Data.GetData = isGet;
	    wrapperCallbackData.Data.IsGetDataCompleted = true;
	    wrapperCallbackData.Data.Invoke();

	    return wrapperCallbackData.DataGet;
    }
}
