using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TDFactorySocketController))]
public class TDFactorySocketControllerEditor :
    Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button(
            "Populate from a Selected JSON"
        ))
        {
            string jsonPath =
                EditorUtility.OpenFilePanel(
                    "Select 2DFactory JSON",
                    Application.dataPath,
                    "json"
                );

            if (string.IsNullOrEmpty(jsonPath))
            {
                return;
            }

            string projectPath =
                Application.dataPath;

            if (!jsonPath.StartsWith(
                    projectPath))
            {
                EditorUtility.DisplayDialog(
                    "2DFactory",
                    "The selected JSON must be inside the Unity project.",
                    "OK"
                );

                return;
            }

            string assetPath =
                "Assets" +
                jsonPath.Substring(
                    projectPath.Length
                );

            assetPath =
                assetPath.Replace(
                    '\\',
                    '/'
                );

            TDFactoryJsonParser parser =
                new TDFactoryJsonParser();

            if (!parser.ParseJsonFile(assetPath))
            {
                EditorUtility.DisplayDialog(
                    "2DFactory",
                    "Failed to parse JSON.",
                    "OK"
                );

                return;
            }

            TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta =
                parser.JsonMeta;

            List<TDFactoryAnimationSocketData> socketData =
                TDFactorySocketDataBuilder.BuildSocketData(
                    assetPath,
                    jsonMeta
                );

            TDFactorySocketController controller =
                (TDFactorySocketController)target;

            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "2DFactory",
                    "The current controller is null.",
                    "OK"
                );

                return;
            }

            controller.UpdateSocketData(
                socketData
            );

            EditorUtility.SetDirty(
                controller
            );
        }
		if (GUILayout.Button(
			"Update Sockets Transform to Current Sprite"
		))
		{
			TDFactorySocketController controller =
                (TDFactorySocketController)target;
				
            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "2DFactory",
                    "The current controller is null.",
                    "OK"
                );

                return;
            }
			
			controller.UpdateSocketsToCurrentSprite();
			
			EditorUtility.SetDirty(
				controller
			);

			PrefabUtility.RecordPrefabInstancePropertyModifications(
				controller
			);

			AssetDatabase.SaveAssets();

			Debug.Log(
				"2DFactory: Transform of socket objects for sprite \"" +
				controller.GetComponent<SpriteRenderer>().sprite.name +
				"\" has been set."
			);
		}
		
		
    }
}