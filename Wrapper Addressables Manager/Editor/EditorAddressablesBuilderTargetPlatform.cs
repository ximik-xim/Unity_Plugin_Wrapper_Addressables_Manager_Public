using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
/// <summary>
/// Нужен что бы делать сборки обнов Addressable под разные платформы
/// (сам переключает на нужную платформу и делает сборку обнов,
/// есть возможность преключ на ту платформу, которая была до старта сборки)
/// </summary>
public class EditorAddressablesBuilderTargetPlatform 
{
    /// <summary>
    /// Сохраняем настройки сборки которые были до переключ платформы
    /// </summary>
    private static BuildTargetGroup _lastGroup;
    private static BuildTarget _lastTarget;

    /// <summary>
    /// Переключ. на указ платформу с сохр. тек. настроек сборки
    /// </summary>
    private static void SwitchPlatform(BuildTargetGroup group, BuildTarget target)
    {
        _lastGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        _lastTarget = EditorUserBuildSettings.activeBuildTarget;

        if (_lastTarget != target)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        }
    }

    /// <summary>
    /// Возвращаем настройки выбранной платформы, которые были до запуска сборки Addressable
    /// </summary>
    private static void RestorePlatform()
    {
        if (_lastTarget != EditorUserBuildSettings.activeBuildTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(_lastGroup, _lastTarget);
        }
    }

    /// <summary>
    /// Запускаем логику переключения платформы и сборку обновл 
    /// </summary>
    private static void BuildAddressablesFor(BuildTargetGroup group, BuildTarget target, bool isRemoveLastPlatform)
    {
        SwitchPlatform(group, target);

        // Build Addressables
        AddressableAssetSettings.BuildPlayerContent();

        if (isRemoveLastPlatform == true)
        {
            RestorePlatform();    
        }
        
        EditorUtility.DisplayDialog("Сборка обновлений Addressables закончена", $"Addressables сборка под платформу: {target}", "ОК");
    }

    
    /// <summary>
    /// Сборка под андроид
    /// с переключением на платформу котор. была до запуска сборки обновл.
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Remove Current Platform/Android")]
    public static void BuildForAndroidRemoveCurrentPlatform()
    {
        BuildAddressablesFor(BuildTargetGroup.Android, BuildTarget.Android, true);
    }

    /// <summary>
    /// Сборка под Windows
    /// с переключением на платформу котор. была до запуска сборки обновл.
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Remove Current Platform/Windows")]
    public static void BuildForWindowsRemoveCurrentPlatform()
    {
        BuildAddressablesFor(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, true);
    }

    /// <summary>
    /// Сборка под IOS
    /// с переключением на платформу котор. была до запуска сборки обновл.
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Remove Current Platform/iOS")]
    public static void BuildForIOSRemoveCurrentPlatform()
    {
        BuildAddressablesFor(BuildTargetGroup.iOS, BuildTarget.iOS, true);
    }
    
    /// <summary>
    /// Сборка под WebGL
    /// с переключением на платформу котор. была до запуска сборки обновл.
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Remove Current Platform/WebGL")]
    public static void BuildForWebGLRemoveCurrentPlatform()
    {
        BuildAddressablesFor(BuildTargetGroup.WebGL, BuildTarget.WebGL, true);
    }
    
    /// <summary>
    /// Сборка под андроид
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Select Platform/Android")]
    public static void BuildForAndroid()
    {
        BuildAddressablesFor(BuildTargetGroup.Android, BuildTarget.Android, false);
    }

    /// <summary>
    /// Сборка под Windows
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Select Platform/Windows")]
    public static void BuildForWindows()
    {
        BuildAddressablesFor(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, false);
    }

    /// <summary>
    /// Сборка под IOS
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Select Platform/iOS")]
    public static void BuildForIOS()
    {
        BuildAddressablesFor(BuildTargetGroup.iOS, BuildTarget.iOS, false);
    }
    
    /// <summary>
    /// Сборка под WebGL
    /// </summary>
    [MenuItem("Wrapper Addressables Manager/Build And Select Platform/WebGL")]
    public static void BuildForWebGL()
    {
        BuildAddressablesFor(BuildTargetGroup.WebGL, BuildTarget.WebGL, false);
    }
}