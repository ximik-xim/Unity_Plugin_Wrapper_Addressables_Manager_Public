using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

/// <summary>
/// Кастомный Asset Reference, в который можно передать только сцену помеченную как Addressable
/// </summary>
[System.Serializable]
public class AssetReferenceSceneCustom : AssetReference
{
    public AssetReferenceSceneCustom(string guid) : base(guid)
    {
    }
    
#if UNITY_EDITOR
    //Проверка через путь(вызывается при выборе из поиска)
    //Оставляет только сцены, помеченные как Addressable.
    public override bool ValidateAsset(string path)
    {
        // Проверяем, является ли ассет сценой
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path);
        if (asset == null)
        {
            return false;
        }
        
        // Проверяем, помечена ли сцена как Addressable
        var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        
        return UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid) != null;
    }
    
    /// Проверка по объекту(вызывается при drag & drop)
    public override bool ValidateAsset(Object obj)
    {
        // Дополнительная проверка (на всякий случай)
        return obj is UnityEditor.SceneAsset scene && UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings
                   .FindAssetEntry(UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(scene))) != null;
    }
#endif
}
