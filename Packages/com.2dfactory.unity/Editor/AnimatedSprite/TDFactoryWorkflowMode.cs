using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Animations;
using UnityEngine;

public class TDFactoryWorkflowMode
{
	public enum CreationMode
	{
		ClipsOnly,
		Full
	}

    public enum WorkflowType
    {
        SingleSheet,
        SeparateSheet
    }

	public void Execute(
		string scanPath,
		WorkflowType workflowType,
		CreationMode creationMode)
	{
		switch (workflowType)
		{
			case WorkflowType.SingleSheet:
				ExecuteSingleSheet(
					scanPath,
					creationMode
				);
				break;

			case WorkflowType.SeparateSheet:
				ExecuteSeparateSheet(
					scanPath,
					creationMode
				);
				break;
		}
	}
	
	private string ResolveImagePath(
		string jsonFilePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta)
	{
		if (string.IsNullOrEmpty(jsonMeta.image))
		{
			Debug.LogError(
				$"2DFactory: JSON does not contain 'image': {jsonFilePath}"
			);

			return string.Empty;
		}

		string jsonDirectory =
			System.IO.Path.GetDirectoryName(
				jsonFilePath
			);

		string imagePath =
			System.IO.Path.Combine(
				jsonDirectory,
				jsonMeta.image
			);

		imagePath =
			imagePath.Replace(
				'\\',
				'/'
			);

		if (!System.IO.File.Exists(imagePath))
		{
			Debug.LogError(
				$"2DFactory: Sprite sheet not found: {imagePath}"
			);

			return string.Empty;
		}

		return imagePath;
	}
	
	private SpriteRect CreateSpriteRect(
		TDFactoryJsonParser.TDFactoryFrame frame,
		TDFactoryJsonParser.TDFactoryFrameTag tag,
		int textureHeight)
	{
		SpriteRect spriteRect =
			new SpriteRect();

		spriteRect.name =
			frame.name;

		spriteRect.spriteID =
			GUID.Generate();

		float unityY =
			textureHeight
			- frame.frame.y
			- frame.frame.h;

		spriteRect.rect =
			new Rect(
				frame.frame.x,
				unityY,
				frame.frame.w,
				frame.frame.h
			);
		
		spriteRect.alignment = SpriteAlignment.Custom;
		spriteRect.pivot = new Vector2(
			0.5f + tag.pivot.x / frame.frame.w,
			0.5f + tag.pivot.y / frame.frame.h
		);

		return spriteRect;
	}
	
	private void AddSecondaryTextureIfFound(
		List<SecondarySpriteTexture> secondaryTextures,
		string directory,
		string imageBaseName,
		string suffix,
		string secondaryTextureName)
	{
		string texturePath =
			System.IO.Path.Combine(
				directory,
				imageBaseName + suffix + ".png"
			);

		texturePath =
			texturePath.Replace(
				'\\',
				'/'
			);

		Texture2D texture =
			AssetDatabase.LoadAssetAtPath<Texture2D>(
				texturePath
			);

		if (texture == null)
		{
			return;
		}

		for (int i = 0;
			 i < secondaryTextures.Count;
			 i++)
		{
			if (
				secondaryTextures[i].name
				!= secondaryTextureName
			)
			{
				continue;
			}

			SecondarySpriteTexture existingSecondaryTexture =
				secondaryTextures[i];

			existingSecondaryTexture.texture =
				texture;

			secondaryTextures[i] =
				existingSecondaryTexture;

			return;
		}

		SecondarySpriteTexture secondaryTexture =
			new SecondarySpriteTexture();

		secondaryTexture.name =
			secondaryTextureName;

		secondaryTexture.texture =
			texture;

		secondaryTextures.Add(
			secondaryTexture
		);
	}
	
	private void AddingSecondaryTextures(
		string imagePath,
		TextureImporter importer)
	{
		if (importer == null)
		{
			return;
		}

		string directory =
			System.IO.Path.GetDirectoryName(
				imagePath
			);

		string imageBaseName =
			System.IO.Path.GetFileNameWithoutExtension(
				imagePath
			);

		List<SecondarySpriteTexture> secondaryTextures =
			new List<SecondarySpriteTexture>(
				importer.secondarySpriteTextures
			);

		AddSecondaryTextureIfFound(
			secondaryTextures,
			directory,
			imageBaseName,
			"_N",
			"_NormalMap"
		);

		AddSecondaryTextureIfFound(
			secondaryTextures,
			directory,
			imageBaseName,
			"_R",
			"_RoughnessMap"
		);

		AddSecondaryTextureIfFound(
			secondaryTextures,
			directory,
			imageBaseName,
			"_AO",
			"_AOMap"
		);

		AddSecondaryTextureIfFound(
			secondaryTextures,
			directory,
			imageBaseName,
			"_E",
			"_EmissionMap"
		);

		importer.secondarySpriteTextures =
			secondaryTextures.ToArray();
	}
	
	private void PrepareSecondaryTextureConfiguration(
		string imagePath)
	{
		string directory =
			System.IO.Path.GetDirectoryName(
				imagePath
			);

		string imageBaseName =
			System.IO.Path.GetFileNameWithoutExtension(
				imagePath
			);

		ConfigureSecondaryTextureImporter(
			System.IO.Path.Combine(
				directory,
				imageBaseName + "_N.png"
			).Replace('\\', '/'),
			"_N"
		);

		ConfigureSecondaryTextureImporter(
			System.IO.Path.Combine(
				directory,
				imageBaseName + "_R.png"
			).Replace('\\', '/'),
			"_R"
		);

		ConfigureSecondaryTextureImporter(
			System.IO.Path.Combine(
				directory,
				imageBaseName + "_AO.png"
			).Replace('\\', '/'),
			"_AO"
		);

		ConfigureSecondaryTextureImporter(
			System.IO.Path.Combine(
				directory,
				imageBaseName + "_E.png"
			).Replace('\\', '/'),
			"_E"
		);
	}
	
	private void ConfigureSecondaryTextureImporter(
		string texturePath,
		string suffix)
	{
		TextureImporter importer =
			AssetImporter.GetAtPath(
				texturePath
			) as TextureImporter;

		if (importer == null)
		{
			return;
		}

		if (suffix == "_N")
		{
			importer.textureType =
				TextureImporterType.NormalMap;

			importer.sRGBTexture =
				false;
		}
		else if (
			suffix == "_R" ||
			suffix == "_AO"
		)
		{
			importer.textureType =
				TextureImporterType.Default;

			importer.sRGBTexture =
				false;
		}
		else if (suffix == "_E")
		{
			importer.textureType =
				TextureImporterType.Default;

			importer.sRGBTexture =
				true;
		}
		
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;
		
		if (settings.applyTextureSettingsToSecondaryTextures)
		{
			importer.filterMode =
				settings.mainTextureFilterMode;

			importer.textureCompression =
				settings.mainTextureCompression;
		}

		EditorUtility.SetDirty(
			importer
		);

		AssetDatabase.ImportAsset(
			texturePath,
			ImportAssetOptions.ForceUpdate
		);
	}
	
	private bool CreateSpritesFromJson(
		string imagePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta)
	{
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;
		
		string assetPath =
			imagePath.Replace(
				'\\',
				'/'
			);

		TextureImporter importer =
			AssetImporter.GetAtPath(
				assetPath
			) as TextureImporter;

		if (importer == null)
		{
			Debug.LogError(
				$"2DFactory: Could not get TextureImporter: {assetPath}"
			);

			return false;
		}
		
		importer.filterMode =
			settings.mainTextureFilterMode;

		importer.textureCompression =
			settings.mainTextureCompression;

		importer.spriteImportMode =
			SpriteImportMode.Multiple;

		AssetDatabase.ImportAsset(
			assetPath,
			ImportAssetOptions.ForceUpdate
		);

		Texture2D texture =
			AssetDatabase.LoadAssetAtPath<Texture2D>(
				assetPath
			);

		if (texture == null)
		{
			Debug.LogError(
				$"2DFactory: Could not load texture: {assetPath}"
			);

			return false;
		}

		SpriteDataProviderFactories factories =
			new SpriteDataProviderFactories();

		factories.Init();

		ISpriteEditorDataProvider dataProvider =
			factories.GetSpriteEditorDataProviderFromObject(
				texture
			);

		if (dataProvider == null)
		{
			Debug.LogError(
				$"2DFactory: Could not obtain Sprite data provider: {assetPath}"
			);

			return false;
		}

		dataProvider.InitSpriteEditorDataProvider();

		List<SpriteRect> spriteRects =
			new List<SpriteRect>();

		foreach (
			KeyValuePair<
				string,
				TDFactoryJsonParser.TDFactoryFrameTag
			> tagPair
			in jsonMeta.frameTags)
		{
			TDFactoryJsonParser.TDFactoryFrameTag tag =
				tagPair.Value;

			foreach (
				TDFactoryJsonParser.TDFactoryFrame frame
				in tag.frames
			)
			{
				SpriteRect spriteRect =
					CreateSpriteRect(
						frame,
						tag,
						texture.height
					);

				spriteRects.Add(
					spriteRect
				);
			}
		}

		dataProvider.SetSpriteRects(
			spriteRects.ToArray()
		);

		dataProvider.Apply();
		
		AddingSecondaryTextures(
			assetPath,
			importer
		);
		
		PrepareSecondaryTextureConfiguration(
			assetPath
		);

		importer.SaveAndReimport();

		Debug.Log(
			$"2DFactory: Created {spriteRects.Count} Sprites: {assetPath}"
		);

		return true;
	}
	
	private void SetAnimationLoop(
		AnimationClip animationClip,
		string loopValue)
	{
		AnimationClipSettings settings =
			AnimationUtility.GetAnimationClipSettings(
				animationClip
			);

		settings.loopTime =
			string.Equals(
				loopValue,
				"true",
				System.StringComparison.OrdinalIgnoreCase
			);

		AnimationUtility.SetAnimationClipSettings(
			animationClip,
			settings
		);
	}
	
	private void CreateAnimationClip(
		string directory,
		string animationName,
		TDFactoryJsonParser.TDFactoryFrameTag tag,
		Dictionary<string, Sprite> sprites)
	{
		if (tag.frames == null ||
			tag.frames.Count == 0)
		{
			Debug.LogWarning(
				$"2DFactory: Animation '{animationName}' has no frames."
			);

			return;
		}

		List<ObjectReferenceKeyframe> keyframes =
			new List<ObjectReferenceKeyframe>();

		float currentTime = 0.0f;
		
		//Direction Playback Implementation
		List<int> orderedFrameIndices =
			new List<int>();

		int frameCount =
			tag.frames.Count;

		string direction =
			tag.direction ?? "forward";

		direction =
			direction.ToLowerInvariant();

		switch (direction)
		{
			case "reverse":

				for (
					int i = frameCount - 1;
					i >= 0;
					i--
				)
				{
					orderedFrameIndices.Add(i);
				}

				break;

			case "pingpong":

				for (
					int i = 0;
					i < frameCount;
					i++
				)
				{
					orderedFrameIndices.Add(i);
				}

				if (frameCount > 1)
				{
					for (
						int i = frameCount - 2;
						i >= 0;
						i--
					)
					{
						orderedFrameIndices.Add(i);
					}
				}

				break;

			case "forward":
			default:

				for (
					int i = 0;
					i < frameCount;
					i++
				)
				{
					orderedFrameIndices.Add(i);
				}

				break;
		}
		
		foreach (
			int frameIndex
			in orderedFrameIndices
		)
		{
			TDFactoryJsonParser.TDFactoryFrame frame =
				tag.frames[frameIndex];

			if (!sprites.TryGetValue(
					frame.name,
					out Sprite sprite))
			{
				Debug.LogError(
					$"2DFactory: Sprite '{frame.name}' " +
					$"was not found for animation '{animationName}'."
				);

				continue;
			}

			ObjectReferenceKeyframe keyframe =
				new ObjectReferenceKeyframe();

			keyframe.time =
				currentTime;

			keyframe.value =
				sprite;

			keyframes.Add(
				keyframe
			);

			currentTime +=
				frame.duration / 1000.0f;
		}

		if (keyframes.Count == 0)
		{
			return;
		}

		string clipPath =
			System.IO.Path.Combine(
				directory,
				animationName + ".anim"
			);

		clipPath =
			clipPath.Replace(
				'\\',
				'/'
			);

		AnimationClip animationClip =
			AssetDatabase.LoadAssetAtPath<AnimationClip>(
				clipPath
			);

		bool isNew =
			animationClip == null;

		if (isNew)
		{
			animationClip =
				new AnimationClip();

			animationClip.name =
				animationName;

			AssetDatabase.CreateAsset(
				animationClip,
				clipPath
			);
		}

		EditorCurveBinding binding =
			EditorCurveBinding.PPtrCurve(
				"",
				typeof(SpriteRenderer),
				"m_Sprite"
			);

		AnimationUtility.SetObjectReferenceCurve(
			animationClip,
			binding,
			keyframes.ToArray()
		);

		SetAnimationLoop(
			animationClip,
			tag.loop
		);

		EditorUtility.SetDirty(
			animationClip
		);

		AssetDatabase.SaveAssets();

		if (isNew)
		{
			Debug.Log(
				$"2DFactory: Created AnimationClip: {clipPath}"
			);
		}
		else
		{
			Debug.Log(
				$"2DFactory: Updated AnimationClip: {clipPath}"
			);
		}
	}
	
	private void PrepareAnimationClips(
		string imagePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta,
		string animationOutputDirectory = null)
	{
		if (string.IsNullOrEmpty(animationOutputDirectory))
		{
			animationOutputDirectory =
				System.IO.Path.GetDirectoryName(imagePath);
		}
		
		string assetPath =
			imagePath.Replace('\\', '/');

		UnityEngine.Object[] assets =
			AssetDatabase.LoadAllAssetsAtPath(
				assetPath
			);

		Dictionary<string, Sprite> sprites =
			new Dictionary<string, Sprite>();

		foreach (UnityEngine.Object asset in assets)
		{
			Sprite sprite =
				asset as Sprite;

			if (sprite == null)
			{
				continue;
			}

			sprites[sprite.name] =
				sprite;
		}

		if (sprites.Count == 0)
		{
			Debug.LogError(
				$"2DFactory: No Sprite assets found: {assetPath}"
			);

			return;
		}

		foreach (
			KeyValuePair<
				string,
				TDFactoryJsonParser.TDFactoryFrameTag
			> tagPair
			in jsonMeta.frameTags
		)
		{
			string animationName =
				tagPair.Key;

			TDFactoryJsonParser.TDFactoryFrameTag tag =
				tagPair.Value;

			CreateAnimationClip(
				animationOutputDirectory,
				animationName,
				tag,
				sprites
			);
		}
	}
	
	private void PrepareAnimatorController(
		string imagePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta,
		string AnimControllerOutputDirectory = null,
		string outputName = null)
	{
		string assetPath =
			imagePath.Replace('\\', '/');
		
		if (string.IsNullOrEmpty(AnimControllerOutputDirectory))
		{
			AnimControllerOutputDirectory =
				System.IO.Path.GetDirectoryName(assetPath);
		}
		
		if (string.IsNullOrEmpty(outputName))
		{
			outputName =
				System.IO.Path.GetFileNameWithoutExtension(
					assetPath
				);
		}

		string controllerPath =
			System.IO.Path.Combine(
				AnimControllerOutputDirectory,
				outputName + "_Animator.controller"
			);

		controllerPath =
			controllerPath.Replace(
				'\\',
				'/'
			);

		AnimatorController controller =
			AssetDatabase.LoadAssetAtPath<AnimatorController>(
				controllerPath
			);

		bool isNew =
			controller == null;

		if (isNew)
		{
			controller =
				AnimatorController.CreateAnimatorControllerAtPath(
					controllerPath
				);
		}

		AnimatorStateMachine stateMachine =
			controller.layers[0].stateMachine;

		bool hasDefaultState =
			stateMachine.defaultState != null;

		foreach (
			KeyValuePair<
				string,
				TDFactoryJsonParser.TDFactoryFrameTag
			> tagPair
			in jsonMeta.frameTags
		)
		{
			string animationName =
				tagPair.Key;

			string clipPath =
				System.IO.Path.Combine(
					AnimControllerOutputDirectory,
					animationName + ".anim"
				);

			clipPath =
				clipPath.Replace(
					'\\',
					'/'
				);

			AnimationClip animationClip =
				AssetDatabase.LoadAssetAtPath<AnimationClip>(
					clipPath
				);

			if (animationClip == null)
			{
				Debug.LogError(
					$"2DFactory: Could not load AnimationClip: " +
					$"{clipPath}"
				);

				continue;
			}

			AnimatorState state =
				null;

			foreach (
				ChildAnimatorState childState
				in stateMachine.states
			)
			{
				if (
					childState.state.name
					== animationName
				)
				{
					state =
						childState.state;

					break;
				}
			}

			bool stateIsNew =
				state == null;

			if (stateIsNew)
			{
				state =
					stateMachine.AddState(
						animationName
					);
			}

			state.motion =
				animationClip;

			if (
				isNew &&
				!hasDefaultState
			)
			{
				stateMachine.defaultState =
					state;

				hasDefaultState =
					true;
			}
		}

		EditorUtility.SetDirty(
			controller
		);

		AssetDatabase.SaveAssets();

		if (isNew)
		{
			Debug.Log(
				$"2DFactory: Created Animator Controller: " +
				$"{controllerPath}"
			);
		}
		else
		{
			Debug.Log(
				$"2DFactory: Updated Animator Controller: " +
				$"{controllerPath}"
			);
		}
	}
	
	private Sprite GetInitialSprite(
		string imagePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta)
	{
		string assetPath =
			imagePath.Replace('\\', '/');
		
		if (jsonMeta.frameTags == null ||
			jsonMeta.frameTags.Count == 0)
		{
			return null;
		}

		TDFactoryJsonParser.TDFactoryFrameTag firstTag =
			null;

		foreach (
			KeyValuePair<
				string,
				TDFactoryJsonParser.TDFactoryFrameTag
			> tagPair
			in jsonMeta.frameTags
		)
		{
			firstTag =
				tagPair.Value;

			break;
		}

		if (firstTag == null ||
			firstTag.frames == null ||
			firstTag.frames.Count == 0)
		{
			return null;
		}

		string firstFrameName =
			firstTag.frames[0].name;

		UnityEngine.Object[] assets =
			AssetDatabase.LoadAllAssetsAtPath(
				assetPath
			);

		foreach (UnityEngine.Object asset in assets)
		{
			Sprite sprite =
				asset as Sprite;

			if (sprite == null)
			{
				continue;
			}

			if (sprite.name == firstFrameName)
			{
				return sprite;
			}
		}

		Debug.LogWarning(
			$"2DFactory: Initial Sprite not found: " +
			$"{firstFrameName}"
		);

		return null;
	}
	
	private void SyncSocketGameObjects(
		GameObject prefabRoot,
		List<TDFactoryAnimationSocketData> socketData,
		Sprite initialSprite)
	{
		if (initialSprite == null)
		{
			return;
		}

		HashSet<string> socketNames =
			new HashSet<string>();

		foreach (
			TDFactoryAnimationSocketData animationData
			in socketData
		)
		{
			foreach (
				TDFactoryFrameSocketData frameData
				in animationData.frames
			)
			{
				foreach (
					TDFactorySocketData socket
					in frameData.sockets
				)
				{
					if (string.IsNullOrEmpty(socket.name))
					{
						continue;
					}

					socketNames.Add(
						socket.name
					);
				}
			}
		}
		
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;

		Sprite socketReticle =
			AssetDatabase.LoadAssetAtPath<Sprite>(
				"Packages/com.2dfactory.unity/Resources/socket_reticle.png"
			);

		float pixelsPerUnit =
			initialSprite.pixelsPerUnit;

		foreach (string socketName in socketNames)
		{
			Transform socketTransform =
				prefabRoot.transform.Find(
					socketName
				);

			GameObject socketObject;

			if (socketTransform == null)
			{
				socketObject =
					new GameObject(
						socketName
					);

				socketObject.transform.SetParent(
					prefabRoot.transform,
					false
				);
				
				if (settings.createSocketReticle)
				{
					SpriteRenderer spriteRenderer =
						socketObject.AddComponent<SpriteRenderer>();

					spriteRenderer.sprite =
						socketReticle;
				}
			}
			else
			{
				socketObject =
					socketTransform.gameObject;
			}

			foreach (
				TDFactoryAnimationSocketData animationData
				in socketData
			)
			{
				foreach (
					TDFactoryFrameSocketData frameData
					in animationData.frames
				)
				{
					if (
						frameData.frameName
						!= initialSprite.name
					)
					{
						continue;
					}

					foreach (
						TDFactorySocketData socket
						in frameData.sockets
					)
					{
						if (socket.name != socketName)
						{
							continue;
						}

						TDFactorySocketUtility.ApplySocketTransform(
							socketObject.transform,
							socket,
							animationData.pivot,
							animationData.cropShift,
							pixelsPerUnit
						);

						break;
					}

					break;
				}
			}
		}
	}
	
	private void PreparePrefab(
		string imagePath,
		Sprite initialSprite,
		List<TDFactoryAnimationSocketData> socketData,
		string prefabOutputDirectory = null,
		string outputName = null)
	{
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;
			
		string assetPath =
			imagePath.Replace('\\', '/');
			
		if (string.IsNullOrEmpty(prefabOutputDirectory))
		{
			prefabOutputDirectory =
				System.IO.Path.GetDirectoryName(assetPath);
		}
		
		if (string.IsNullOrEmpty(outputName))
		{
			outputName =
				System.IO.Path.GetFileNameWithoutExtension(
					assetPath
				);
		}

		string controllerPath =
			System.IO.Path.Combine(
				prefabOutputDirectory,
				outputName + "_Animator.controller"
			);

		controllerPath =
			controllerPath.Replace('\\', '/');

		RuntimeAnimatorController animatorController =
			AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
				controllerPath
			);

		if (animatorController == null)
		{
			Debug.LogError(
				$"2DFactory: Could not load Animator Controller: " +
				$"{controllerPath}"
			);

			return;
		}

		string prefabPath =
			System.IO.Path.Combine(
				prefabOutputDirectory,
				outputName + ".prefab"
			);

		prefabPath =
			prefabPath.Replace('\\', '/');

		bool prefabExists =
			AssetDatabase.LoadAssetAtPath<GameObject>(
				prefabPath
			) != null;

		GameObject prefabRoot;

		if (prefabExists)
		{
			prefabRoot =
				PrefabUtility.LoadPrefabContents(
					prefabPath
				);
		}
		else
		{
			prefabRoot =
				new GameObject(
					outputName
				);
		}

		SpriteRenderer spriteRenderer =
			prefabRoot.GetComponent<SpriteRenderer>();

		if (spriteRenderer == null)
		{
			spriteRenderer =
				prefabRoot.AddComponent<SpriteRenderer>();
				
			if (settings.defaultMaterial != null)
			{
				spriteRenderer.material =
					settings.defaultMaterial;
			}
		}

		Animator animator =
			prefabRoot.GetComponent<Animator>();

		if (animator == null)
		{
			animator =
				prefabRoot.AddComponent<Animator>();
		}

		animator.runtimeAnimatorController =
			animatorController;

		if (initialSprite != null)
		{
			spriteRenderer.sprite =
				initialSprite;
		}

		TDFactorySocketController controller =
			prefabRoot.GetComponent<TDFactorySocketController>();

		if (controller == null)
		{
			controller =
				prefabRoot.AddComponent<TDFactorySocketController>();
			controller.SetSocketData(
				socketData
			);
		}
		else
		{
			controller.UpdateSocketData(
				socketData
			);
		}

		SyncSocketGameObjects(
			prefabRoot,
			socketData,
			initialSprite
		);

		GameObject prefab =
			PrefabUtility.SaveAsPrefabAsset(
				prefabRoot,
				prefabPath
			);

		if (prefabExists)
		{
			PrefabUtility.UnloadPrefabContents(
				prefabRoot
			);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(
				prefabRoot
			);
		}

		if (prefab == null)
		{
			Debug.LogError(
				$"2DFactory: Failed to save prefab: " +
				$"{prefabPath}"
			);

			return;
		}

		AssetDatabase.SaveAssets();

		if (prefabExists)
		{
			Debug.Log(
				$"2DFactory: Updated Prefab: {prefabPath}"
			);
		}
		else
		{
			Debug.Log(
				$"2DFactory: Created Prefab: {prefabPath}"
			);
		}
	}
	
	private void ProcessSingleSheet(
		string jsonFilePath,
		TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta,
		CreationMode creationMode)
	{
		// 1. Resolve sprite sheet.
		string imagePath =
			ResolveImagePath(
				jsonFilePath,
				jsonMeta
			);

		if (string.IsNullOrEmpty(imagePath))
		{
			return;
		}

		// 2. TextureImporter settings will be handled later.

		// 3. Create Unity Sprite assets.
		if (!CreateSpritesFromJson(
				imagePath,
				jsonMeta))
		{
			Debug.LogError(
				"2DFactory: Failed to create Sprite assets."
			);

			return;
		}

		// 4. AnimationClips
		PrepareAnimationClips(
			imagePath,
			jsonMeta
		);
		
		if (creationMode == CreationMode.ClipsOnly)
		{
			return;
		}
		
		// 5. AnimationController
		PrepareAnimatorController(
			imagePath,
			jsonMeta
		);
		
		// 6. Create Prefab
		Sprite initialSprite =
			GetInitialSprite(
				imagePath,
				jsonMeta
			);
		
		List<TDFactoryAnimationSocketData> socketData =
			TDFactorySocketDataBuilder.BuildSocketData(
				jsonFilePath,
				jsonMeta
			);
		
		PreparePrefab(
			imagePath,
			initialSprite,
			socketData
		);
	}
	
	private void ExecuteSingleSheet(
		string scanPath,
		CreationMode creationMode)
	{
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;
		
		TDFactoryDirScanner dirScanner =
			new TDFactoryDirScanner();

		List<string> jsonPaths =
			dirScanner.ScanJson(
				scanPath,
				settings.maxRecursiveScanDepth
			);

		if (jsonPaths.Count == 0)
		{
			Debug.Log(
				"2DFactory: No JSON files found."
			);

			return;
		}

		Debug.Log(
			$"2DFactory: JSON files found: {jsonPaths.Count}"
		);

		foreach (string jsonFilePath in jsonPaths)
		{
			TDFactoryJsonParser parser =
				new TDFactoryJsonParser();

			if (!parser.ParseJsonFile(jsonFilePath))
			{
				Debug.LogError(
					"2DFactory: JSON parsing failed."
				);

				continue;
			}

			TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta =
				parser.JsonMeta;

			Debug.Log(
				$"2DFactory: Processing JSON: {jsonFilePath}"
			);

			ProcessSingleSheet(
				jsonFilePath,
				jsonMeta,
				creationMode
			);
		}

		Debug.Log(
			"=== SINGLE SHEET PROCESSING COMPLETED ==="
		);
	}
	
	private string GetSeparateSheetOutputName(
		string scanPath
	)
	{
		string outputName =
			TDFactoryProjectSettings.instance
				.separateSheetName;

		if (string.IsNullOrWhiteSpace(outputName))
		{
			outputName = "{FOLDER}";
		}

		string folderName =
			new DirectoryInfo(scanPath).Name;

		outputName =
			outputName.Replace(
				"{FOLDER}",
				folderName
			);

		char[] invalidCharacters =
			Path.GetInvalidFileNameChars();

		bool hasInvalidCharacters =
			outputName.IndexOfAny(
				invalidCharacters
			) >= 0;

		if (hasInvalidCharacters)
		{
			Debug.LogWarning(
				"2DFactory: Separate Sheet output name " +
				"\"" +
				outputName +
				"\" contains invalid filename characters. " +
				"They will be replaced with '_'."
			);

			foreach (
				char invalidCharacter
				in invalidCharacters
			)
			{
				outputName =
					outputName.Replace(
						invalidCharacter,
						'_'
					);
			}
		}

		const int maxOutputNameLength = 64;

		if (outputName.Length >
			maxOutputNameLength)
		{
			Debug.LogWarning(
				"2DFactory: Separate Sheet output name is too long. " +
				"It will be truncated to " +
				maxOutputNameLength +
				" characters."
			);

			outputName =
				outputName.Substring(
					0,
					maxOutputNameLength
				);
		}

		return outputName;
	}
	
	private class SeparateSheetSource
	{
		public string jsonFilePath;
		public TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta;
	}
	
	private void ExecuteSeparateSheet(
		string scanPath,
		CreationMode creationMode)
	{
		TDFactoryProjectSettings settings =
			TDFactoryProjectSettings.instance;
		
		TDFactoryDirScanner dirScanner =
			new TDFactoryDirScanner();

		List<string> jsonPaths =
			dirScanner.ScanJson(
				scanPath,
				settings.maxRecursiveScanDepth
			);

		if (jsonPaths.Count == 0)
		{
			Debug.Log(
				"2DFactory: No JSON files found."
			);

			return;
		}

		Debug.Log(
			$"2DFactory: JSON files found: {jsonPaths.Count}"
		);
		
		List<SeparateSheetSource> sources =
			new List<SeparateSheetSource>();
		
		// 1. Check for duplicate animation name from all the JSONs.
		HashSet<string> animationNames =
			new HashSet<string>();
			
		foreach (string jsonFilePath in jsonPaths)
		{
			TDFactoryJsonParser parser =
				new TDFactoryJsonParser();

			if (!parser.ParseJsonFile(jsonFilePath))
			{
				Debug.LogError(
					"2DFactory: JSON parsing failed."
				);

				continue;
			}
			TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta =
				parser.JsonMeta;
			
			foreach (var tagPair in jsonMeta.frameTags)
			{
				if (!animationNames.Add(tagPair.Key))
				{
					Debug.LogError(
						"2DFactory: Duplicate animation name found: "
						+ tagPair.Key
						+ " in "
						+ jsonFilePath
					);

					return;
				}
			}
			
			sources.Add(
				new SeparateSheetSource
				{
					jsonFilePath = jsonFilePath,
					jsonMeta = jsonMeta
				}
			);
		}

		// 2. Prepare combined data.
		
		TDFactoryJsonParser.TDFactoryJsonMeta combinedJsonMeta =
			new TDFactoryJsonParser.TDFactoryJsonMeta();

		List<TDFactoryAnimationSocketData> combinedSocketData =
			new List<TDFactoryAnimationSocketData>();

		string defaultImagePath = null;

		TDFactoryJsonParser.TDFactoryJsonMeta defaultJsonMeta =
			null;
		
		// 3. Process every parsed JSON.
		foreach (SeparateSheetSource source in sources)
		{
			string jsonFilePath =
				source.jsonFilePath;
				
			Debug.Log(
				$"2DFactory: Processing JSON: {jsonFilePath}"
			);

			TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta =
				source.jsonMeta;
				
			// 3.1 Resolve sprite sheet.
			string imagePath =
				ResolveImagePath(
					jsonFilePath,
					jsonMeta
				);
				
			// 3.2 Create Unity Sprite assets.
			if (!CreateSpritesFromJson(
					imagePath,
					jsonMeta))
			{
				Debug.LogError(
					"2DFactory: Failed to create Sprite assets."
				);

				return;
			}
			
			// First successfully processed sheet
			// becomes the default visual sprite source.
			if (string.IsNullOrEmpty(defaultImagePath))
			{
				defaultImagePath =
					imagePath;

				defaultJsonMeta =
					jsonMeta;
			}

			// 3.3 Create AnimationClips in scanPath.
			PrepareAnimationClips(
				imagePath,
				jsonMeta,
				scanPath
			);
			
			foreach (var tagPair in jsonMeta.frameTags)
			{
				combinedJsonMeta.frameTags.Add(
					tagPair.Key,
					tagPair.Value
				);
			}
			
			List<TDFactoryAnimationSocketData> socketData =
				TDFactorySocketDataBuilder.BuildSocketData(
					jsonFilePath,
					jsonMeta
				);

			combinedSocketData.AddRange(
				socketData
			);
		}
		
		if (creationMode == CreationMode.ClipsOnly)
		{
			return;
		}
			
		string separateSheetOutputName = 
			GetSeparateSheetOutputName(scanPath);
		
		// 5. Create/update combined Animator Controller.
		PrepareAnimatorController(
			defaultImagePath,
			combinedJsonMeta,
			scanPath,
			separateSheetOutputName
		);
		
		// 6. Determine initial visual sprite.
		Sprite initialSprite =
			GetInitialSprite(
				defaultImagePath,
				defaultJsonMeta
			);
		
		// 7. Create/update combined prefab.
		PreparePrefab(
			defaultImagePath,
			initialSprite,
			combinedSocketData,
			scanPath,
			separateSheetOutputName
		);

		Debug.Log(
			"=== SEPARATE SHEET PROCESSING COMPLETED ==="
		);
	}
	
	
	
}