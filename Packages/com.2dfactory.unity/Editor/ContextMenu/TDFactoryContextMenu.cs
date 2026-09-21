using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class TDFactoryContextMenu
{
    // =========================================================
    // CREATE SPRITES ASSET (2DF)
    // =========================================================
	[MenuItem(
		"Assets/Create Sprites Asset (2DF)/Single Sheet Mode/Clips Only",
		false,
		110
	)]
	private static void CreateSingleSheetClipsOnly()
	{
		ExecuteWorkflow(
			TDFactoryWorkflowMode.WorkflowType.SingleSheet,
			TDFactoryWorkflowMode.CreationMode.ClipsOnly
		);
	}

	[MenuItem(
		"Assets/Create Sprites Asset (2DF)/Single Sheet Mode/Full",
		false,
		100
	)]
	private static void CreateSingleSheetFull()
	{
		ExecuteWorkflow(
			TDFactoryWorkflowMode.WorkflowType.SingleSheet,
			TDFactoryWorkflowMode.CreationMode.Full
		);
	}

	[MenuItem(
		"Assets/Create Sprites Asset (2DF)/Separate Sheet Mode/Clips Only",
		false,
		130
	)]
	private static void CreateSeparateSheetClipsOnly()
	{
		ExecuteWorkflow(
			TDFactoryWorkflowMode.WorkflowType.SeparateSheet,
			TDFactoryWorkflowMode.CreationMode.ClipsOnly
		);
	}

	[MenuItem(
		"Assets/Create Sprites Asset (2DF)/Separate Sheet Mode/Full",
		false,
		120
	)]
	private static void CreateSeparateSheetFull()
	{
		ExecuteWorkflow(
			TDFactoryWorkflowMode.WorkflowType.SeparateSheet,
			TDFactoryWorkflowMode.CreationMode.Full
		);
	}

    // =========================================================
    // WORKFLOW EXECUTION
    // =========================================================
	private static void ExecuteWorkflow(
		TDFactoryWorkflowMode.WorkflowType workflowType,
		TDFactoryWorkflowMode.CreationMode creationMode)
	{
		string selectedPath =
			GetSelectedFolderPath();

		if (string.IsNullOrEmpty(selectedPath))
		{
			Debug.LogWarning(
				"2DFactory: No valid folder is selected."
			);

			return;
		}

		TDFactoryWorkflowMode workflowMode =
			new TDFactoryWorkflowMode();

		workflowMode.Execute(
			selectedPath,
			workflowType,
			creationMode
		);
	}


    // =========================================================
    // GET SELECTED FOLDER
    // =========================================================
    private static string GetSelectedFolderPath()
    {
        // -----------------------------------------------------
        // FIRST: Try normal Unity selection.
        // This works when right-clicking in the right pane.
        // -----------------------------------------------------

        Object selectedObject =
            Selection.activeObject;

        if (selectedObject != null)
        {
            string selectedPath =
                AssetDatabase.GetAssetPath(
                    selectedObject
                );

            if (
                !string.IsNullOrEmpty(selectedPath) &&
                AssetDatabase.IsValidFolder(selectedPath)
            )
            {
                return selectedPath;
            }
        }


        // -----------------------------------------------------
        // SECOND: Get the active Project Browser folder.
        // This handles the left-side folder tree.
        // -----------------------------------------------------

        string activeFolderPath;

        if (TryGetActiveFolderPath(
            out activeFolderPath))
        {
            if (IsValidAssetsFolder(
                activeFolderPath))
            {
                return activeFolderPath;
            }
        }

        return string.Empty;
    }


    // =========================================================
    // REFLECTION: PROJECT WINDOW UTIL
    // =========================================================
    private static bool TryGetActiveFolderPath(
        out string path)
    {
        path = string.Empty;

        MethodInfo method =
            typeof(ProjectWindowUtil).GetMethod(
                "TryGetActiveFolderPath",
                BindingFlags.Static |
                BindingFlags.NonPublic
            );

        if (method == null)
        {
            Debug.LogWarning(
                "2DFactory: Could not find " +
                "ProjectWindowUtil.TryGetActiveFolderPath()."
            );

            return false;
        }

        object[] arguments =
        {
            null
        };

        bool result =
            (bool)method.Invoke(
                null,
                arguments
            );

        if (!result)
        {
            return false;
        }

        path =
            arguments[0] as string;

        return !string.IsNullOrEmpty(path);
    }


    // =========================================================
    // ASSETS FOLDER VALIDATION
    // =========================================================
    private static bool IsValidAssetsFolder(
        string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            return false;
        }

        if (path == "Assets")
        {
            return true;
        }

        return path.StartsWith("Assets/");
    }


    // =========================================================
    // MENU VALIDATION
    // =========================================================
    [MenuItem(
        "Assets/Create Sprites Asset (2DF)/Single Sheet Mode/Clips Only",
        true,
        110
    )]
    [MenuItem(
        "Assets/Create Sprites Asset (2DF)/Single Sheet Mode/Full",
        true,
        100
    )]
    [MenuItem(
        "Assets/Create Sprites Asset (2DF)/Separate Sheet Mode/Clips Only",
        true,
        130
    )]
    [MenuItem(
        "Assets/Create Sprites Asset (2DF)/Separate Sheet Mode/Full",
        true,
        120
    )]
    private static bool ValidateFolderSelection()
    {
        string selectedPath =
            GetSelectedFolderPath();

        return !string.IsNullOrEmpty(
            selectedPath
        );
    }
}