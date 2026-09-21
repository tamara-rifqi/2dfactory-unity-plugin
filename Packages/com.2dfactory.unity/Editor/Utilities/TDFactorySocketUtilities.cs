using UnityEngine;

public static class TDFactorySocketUtility
{
    public static void ApplySocketTransform(
        Transform socketTransform,
        TDFactorySocketData socketData,
        Vector2 pivot,
        Vector2 cropShift,
        float pixelsPerUnit)
    {
        if (socketTransform == null)
        {
            return;
        }

        if (socketData == null)
        {
            return;
        }

        if (pixelsPerUnit <= 0.0f)
        {
            return;
        }

        float localX =
            (
                socketData.x
                - (
                    pivot.x
                    - cropShift.x
                )
            ) / pixelsPerUnit;

        float localY =
            (
                socketData.y
                - (
                    pivot.y
                    - cropShift.y
                )
            ) / pixelsPerUnit;

        float localZ =
            socketData.z / pixelsPerUnit;

        socketTransform.localPosition =
            new Vector3(
                localX,
                localY,
                localZ
            );

        socketTransform.localRotation =
            Quaternion.Euler(
                0.0f,
                0.0f,
                socketData.rz
            );

        socketTransform.localScale =
            new Vector3(
                socketData.sx,
                socketData.sy,
                socketData.sz
            );
    }
}