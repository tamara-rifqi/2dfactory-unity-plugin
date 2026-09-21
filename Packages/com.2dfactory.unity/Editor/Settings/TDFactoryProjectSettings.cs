using UnityEditor;
using UnityEngine;

[FilePath(
    "ProjectSettings/2DFactorySettings.asset",
    FilePathAttribute.Location.ProjectFolder
)]
public class TDFactoryProjectSettings :
    ScriptableSingleton<TDFactoryProjectSettings>
{
    public Material defaultMaterial;

    public string separateSheetName =
        "{FOLDER}";

    public int maxRecursiveScanDepth =
        5;

    public FilterMode mainTextureFilterMode =
        FilterMode.Point;

    public TextureImporterCompression mainTextureCompression =
        TextureImporterCompression.Uncompressed;
		
	public bool applyTextureSettingsToSecondaryTextures =
		true;

    public bool createSocketReticle =
        true;

    public void SaveSettings()
    {
        Save(true);
    }
}