using UnityEditor;
using UnityEngine;

public static class TDFactoryProjectSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        SettingsProvider provider =
            new SettingsProvider(
                "Project/2DFactory",
                SettingsScope.Project
            );

        provider.label =
            "2DFactory";

        provider.guiHandler =
            searchContext =>
            {
                TDFactoryProjectSettings settings =
                    TDFactoryProjectSettings.instance;

                float previousLabelWidth =
                    EditorGUIUtility.labelWidth;

                EditorGUIUtility.labelWidth =
                    300.0f;

                EditorGUI.BeginChangeCheck();

                settings.defaultMaterial =
                    (Material)EditorGUILayout.ObjectField(
                        new GUIContent(
                            "Default Material for Created Sprite Renderer",
                            "Material assigned to the SpriteRenderer " +
                            "of newly created 2DFactory prefabs. " +
                            "If no material is assigned, 2DFactory " +
                            "leaves the SpriteRenderer material unchanged."
                        ),
                        settings.defaultMaterial,
                        typeof(Material),
                        false
                    );

                settings.separateSheetName =
                    EditorGUILayout.TextField(
                        new GUIContent(
                            "Separate Sheet Name",
                            "Name used for the Animator Controller " +
                            "and Prefab created by Separate Sheet Mode. " +
                            "Use {Folder} to automatically use the name " +
                            "of the selected folder."
                        ),
                        settings.separateSheetName
                    );

                settings.maxRecursiveScanDepth =
                    EditorGUILayout.IntField(
                        new GUIContent(
                            "Max Recursive Scan Depth",
                            "Maximum folder depth that Separate Sheet Mode " +
                            "will scan when searching for JSON files."
                        ),
                        settings.maxRecursiveScanDepth
                    );

                settings.mainTextureFilterMode =
                    (FilterMode)EditorGUILayout.EnumPopup(
                        new GUIContent(
                            "Main Texture Filter Mode",
                            "Filter mode applied to the main sprite texture " +
                            "when it is imported by 2DFactory. " +
                            "Point filtering is recommended for pixel art."
                        ),
                        settings.mainTextureFilterMode
                    );

                settings.mainTextureCompression =
                    (TextureImporterCompression)
                        EditorGUILayout.EnumPopup(
                            new GUIContent(
                                "Main Texture Compression",
                                "Texture compression setting applied to the " +
                                "main sprite texture when it is imported by " +
                                "2DFactory. Uncompressed is recommended when " +
                                "preserving pixel-art image quality is important."
                            ),
                            settings.mainTextureCompression
                        );
						
				settings.applyTextureSettingsToSecondaryTextures =
					EditorGUILayout.Toggle(
						new GUIContent(
							"Apply the Same to Secondary Textures",
							"Applies the Main Texture Filter Mode and Main Texture " +
							"Compression settings to secondary textures: " +
							"(_N, _AO, _R, and _E textures)."
						),
						settings.applyTextureSettingsToSecondaryTextures
					);

                settings.createSocketReticle =
                    EditorGUILayout.Toggle(
                        new GUIContent(
                            "Create Socket Reticle",
                            "Creates a visual reticle for newly created socket objects " +
                            "when 2DFactory creates or updates the prefab."
                        ),
                        settings.createSocketReticle
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    settings.SaveSettings();
                }

                EditorGUIUtility.labelWidth =
                    previousLabelWidth;
            };

        return provider;
    }
}