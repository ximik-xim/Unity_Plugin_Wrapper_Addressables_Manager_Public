using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;


/// <summary>
/// Нужен что бы соотнести интерфеис IResourceLocator
/// И статус запроса к серверу
/// </summary>
public class StatusCallbackIResourceLocation
{
    public StatusCallbackIResourceLocation(StatusCallBackServer statusCallBack, IResourceLocation resourceLocation)
    {
        _statusCallBack = statusCallBack;
        _resourceLocation = resourceLocation;
    }
   
    private StatusCallBackServer _statusCallBack;
    public StatusCallBackServer StatusCallBack => _statusCallBack; 
      
    private IResourceLocation _resourceLocation;
    public IResourceLocation ResourceLocator => _resourceLocation;
}