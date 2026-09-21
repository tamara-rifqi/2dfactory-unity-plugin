using System;
using System.Collections.Generic;
using System.IO;

public class TDFactoryDirScanner
{
    public List<string> ScanJson(
        string scanPath,
        int maxDepth = 5)
    {
        List<string> allJson = new List<string>();

        if (!Directory.Exists(scanPath))
        {
            return allJson;
        }

        ScanJsonRecursive(
            scanPath,
            allJson,
            0,
            maxDepth
        );

        allJson.Sort();

        return allJson;
    }


    private void ScanJsonRecursive(
        string directoryPath,
        List<string> allJson,
        int currentDepth,
        int maxDepth)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        string[] files;

        try
        {
            files = Directory.GetFiles(directoryPath);
        }
        catch
        {
            return;
        }


        // ---------------------------------------------------------
        // FILES
        // ---------------------------------------------------------

        foreach (string filePath in files)
        {
            if (
                string.Equals(
                    Path.GetExtension(filePath),
                    ".json",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                allJson.Add(filePath);
            }
        }


        // ---------------------------------------------------------
        // MAX DEPTH
        // ---------------------------------------------------------

        if (currentDepth >= maxDepth)
        {
            return;
        }


        // ---------------------------------------------------------
        // SUBDIRECTORIES
        // ---------------------------------------------------------

        string[] directories;

        try
        {
            directories =
                Directory.GetDirectories(directoryPath);
        }
        catch
        {
            return;
        }

        foreach (string subDirectoryPath in directories)
        {
            ScanJsonRecursive(
                subDirectoryPath,
                allJson,
                currentDepth + 1,
                maxDepth
            );
        }
    }
}