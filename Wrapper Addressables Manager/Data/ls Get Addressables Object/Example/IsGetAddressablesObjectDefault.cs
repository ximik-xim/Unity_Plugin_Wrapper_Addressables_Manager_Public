using System;
using UnityEngine;

/// <summary>
/// На случай, если вручную будем задовать доступность обьекта(через инспектор)
/// </summary>
public class IsGetAddressablesObjectDefault : AbsBoolIsGetAddressablesObject
{
    [SerializeField] 
    private bool _isGet;

    public override bool IsInit => true;
    public override event Action OnInit;

    private void Awake()
    {
        OnInit?.Invoke();
    }

    public override GetServerRequestData<bool> IsGet(object data)
    {
        CallbackDataBool wrapperCallbackData = new CallbackDataBool(0);
	    
        wrapperCallbackData.Data.StatusServer = StatusCallBackServer.Ok;
        wrapperCallbackData.Data.GetData = _isGet;
        wrapperCallbackData.Data.IsGetDataCompleted = true;
        wrapperCallbackData.Data.Invoke();

        return wrapperCallbackData.DataGet;
    }
}
