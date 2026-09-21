using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TDFactoryJsonParser
{
    public TDFactoryJsonMeta JsonMeta { get; private set; }


    // =========================================================
    // PARSE JSON FILE
    // =========================================================

    public bool ParseJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError(
                $"2DFactory: Could not open JSON file: {filePath}"
            );

            return false;
        }

        string jsonText;

        try
        {
            jsonText = File.ReadAllText(filePath);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"2DFactory: Could not read JSON file: {filePath}\n" +
                exception.Message
            );

            return false;
        }

        JObject jsonData;

        try
        {
            jsonData = JObject.Parse(jsonText);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"2DFactory: Could not parse JSON: {filePath}\n" +
                exception.Message
            );

            return false;
        }

        JsonMeta = new TDFactoryJsonMeta();

        if (!ParseJsonMetadata(jsonData))
        {
            return false;
        }

        Debug.Log(
            $"2DFactory: Metadata parsed successfully: {filePath}"
        );

        return true;
    }


    // =========================================================
    // PARSE METADATA
    // =========================================================

    private bool ParseJsonMetadata(JObject jsonData)
    {
        JObject meta =
            jsonData["meta"] as JObject;

        if (meta == null)
        {
            Debug.LogError(
                "2DFactory: 'meta' is not an object."
            );

            return false;
        }


        // -----------------------------------------------------
        // IMAGE
        // -----------------------------------------------------

        JsonMeta.image =
            meta.Value<string>("image") ?? "";


        // -----------------------------------------------------
        // SIZE
        // -----------------------------------------------------

        JObject size =
            meta["size"] as JObject;

        if (size != null)
        {
            JsonMeta.size = new Vector2Int(
                size.Value<int?>("w") ?? 0,
                size.Value<int?>("h") ?? 0
            );
        }


        // -----------------------------------------------------
        // SCALE
        // -----------------------------------------------------

        JsonMeta.scale =
            meta.Value<float?>("scale") ?? 1.0f;


        // -----------------------------------------------------
        // FRAME TAGS
        // -----------------------------------------------------

        JArray frameTags =
            meta["frameTags"] as JArray;

        if (frameTags != null)
        {
            foreach (JToken tagToken in frameTags)
            {
                JObject tag =
                    tagToken as JObject;

                if (tag == null)
                    continue;

                string tagName =
                    tag.Value<string>("name") ?? "";

                if (string.IsNullOrEmpty(tagName))
                    continue;

                TDFactoryFrameTag tagData =
                    new TDFactoryFrameTag();

                tagData.from =
                    tag.Value<int?>("from") ?? 0;

                tagData.to =
                    tag.Value<int?>("to") ?? 0;

                tagData.direction =
                    tag.Value<string>("direction")
                    ?? "forward";

                tagData.loop =
                    tag.Value<string>("loop")
                    ?? "false";


                // -------------------------------------------------
                // CROP SHIFT
                // -------------------------------------------------

                JObject cropShift =
                    tag["cropShift"] as JObject;

                if (cropShift != null)
                {
                    tagData.cropShift = new Vector2(
                        cropShift.Value<float?>("x") ?? 0.0f,
                        cropShift.Value<float?>("y") ?? 0.0f
                    );
                }


                // -------------------------------------------------
                // PIVOT
                // -------------------------------------------------

                JObject pivot =
                    tag["pivot"] as JObject;

                if (pivot != null)
                {
                    tagData.pivot = new Vector2(
                        pivot.Value<float?>("x") ?? 0.0f,
                        pivot.Value<float?>("y") ?? 0.0f
                    );
                }


                JsonMeta.frameTags[tagName] =
                    tagData;
            }
        }


        // -----------------------------------------------------
        // FRAMES
        // -----------------------------------------------------

        if (!ParseJsonFrames(jsonData))
        {
            return false;
        }

        return true;
    }


    // =========================================================
    // PARSE FRAMES
    // =========================================================

    private bool ParseJsonFrames(JObject jsonData)
    {
        JObject allFrames =
            jsonData["frames"] as JObject;

        if (allFrames == null)
        {
            Debug.LogError(
                "2DFactory: 'frames' is not an object."
            );

            return false;
        }

        foreach (
            KeyValuePair<string, TDFactoryFrameTag> tagPair
            in JsonMeta.frameTags
        )
        {
            string tagName =
                tagPair.Key;

            TDFactoryFrameTag tagData =
                tagPair.Value;

            foreach (
                JProperty frameProperty
                in allFrames.Properties()
            )
            {
                string frameName =
                    frameProperty.Name;

                if (!frameName.Contains(tagName))
                    continue;

                JObject frameData =
                    frameProperty.Value as JObject;

                if (frameData == null)
                    continue;

                TDFactoryFrame parsedFrame =
                    ParseSingleFrame(
                        frameName,
                        frameData
                    );

                if (parsedFrame == null)
                    continue;

                tagData.frames.Add(parsedFrame);
            }
        }

        return true;
    }


    // =========================================================
    // PARSE SINGLE FRAME
    // =========================================================

    private TDFactoryFrame ParseSingleFrame(
        string frameName,
        JObject frameData)
    {
        TDFactoryFrame parsedFrame =
            new TDFactoryFrame();

        parsedFrame.name =
            frameName;


        // -----------------------------------------------------
        // FRAME RECT
        // -----------------------------------------------------

        JObject frame =
            frameData["frame"] as JObject;

        if (frame != null)
        {
            parsedFrame.frame = new TDFactoryRect(
                frame.Value<int?>("x") ?? 0,
                frame.Value<int?>("y") ?? 0,
                frame.Value<int?>("w") ?? 0,
                frame.Value<int?>("h") ?? 0
            );
        }


        // -----------------------------------------------------
        // SPRITE SOURCE SIZE
        // -----------------------------------------------------

        JObject sourceRect =
            frameData["spriteSourceSize"] as JObject;

        if (sourceRect != null)
        {
            parsedFrame.spriteSourceSize =
                new TDFactoryRect(
                    sourceRect.Value<int?>("x") ?? 0,
                    sourceRect.Value<int?>("y") ?? 0,
                    sourceRect.Value<int?>("w") ?? 0,
                    sourceRect.Value<int?>("h") ?? 0
                );
        }


        // -----------------------------------------------------
        // SOURCE SIZE
        // -----------------------------------------------------

        JObject sourceSize =
            frameData["sourceSize"] as JObject;

        if (sourceSize != null)
        {
            parsedFrame.sourceSize =
                new Vector2Int(
                    sourceSize.Value<int?>("w") ?? 0,
                    sourceSize.Value<int?>("h") ?? 0
                );
        }


        // -----------------------------------------------------
        // FRAME PROPERTIES
        // -----------------------------------------------------

        parsedFrame.duration =
            frameData.Value<int?>("duration") ?? 0;

        parsedFrame.rotated =
            frameData.Value<bool?>("rotated") ?? false;

        parsedFrame.trimmed =
            frameData.Value<bool?>("trimmed") ?? false;


        // -----------------------------------------------------
        // SOCKETS
        // -----------------------------------------------------

        JObject sockets =
            frameData["sockets"] as JObject;

        if (sockets != null)
        {
            foreach (
                JProperty socketProperty
                in sockets.Properties()
            )
            {
                JObject socketData =
                    socketProperty.Value as JObject;

                if (socketData == null)
                    continue;

                TDFactorySocket socket =
                    new TDFactorySocket();

                socket.x =
                    socketData.Value<float?>("x") ?? 0.0f;

                socket.y =
                    socketData.Value<float?>("y") ?? 0.0f;

                socket.z =
                    socketData.Value<float?>("z") ?? 0.0f;

                socket.rx =
                    socketData.Value<float?>("rx") ?? 0.0f;

                socket.ry =
                    socketData.Value<float?>("ry") ?? 0.0f;

                socket.rz =
                    socketData.Value<float?>("rz") ?? 0.0f;

                socket.sx =
                    socketData.Value<float?>("sx") ?? 1.0f;

                socket.sy =
                    socketData.Value<float?>("sy") ?? 1.0f;

                socket.sz =
                    socketData.Value<float?>("sz") ?? 1.0f;

                parsedFrame.sockets[
                    socketProperty.Name
                ] = socket;
            }
        }

        return parsedFrame;
    }


    // =========================================================
    // DATA TYPES
    // =========================================================

    public class TDFactoryJsonMeta
    {
        public string image = "";
        public Vector2Int size = Vector2Int.zero;
        public float scale = 1.0f;

        public Dictionary<string, TDFactoryFrameTag>
            frameTags =
                new Dictionary<string, TDFactoryFrameTag>();
    }


    public class TDFactoryFrameTag
    {
        public int from = 0;
        public int to = 0;
        public string direction = "forward";
        public string loop = "false";

        public Vector2 cropShift = Vector2.zero;
        public Vector2 pivot = Vector2.zero;

        public List<TDFactoryFrame> frames =
            new List<TDFactoryFrame>();
    }


    public class TDFactoryFrame
    {
        public string name = "";

        public TDFactoryRect frame =
            new TDFactoryRect();

        public TDFactoryRect spriteSourceSize =
            new TDFactoryRect();

        public Vector2Int sourceSize =
            Vector2Int.zero;

        public int duration = 0;

        public bool rotated = false;
        public bool trimmed = false;

        public Dictionary<string, TDFactorySocket>
            sockets =
                new Dictionary<string, TDFactorySocket>();
    }


    public class TDFactoryRect
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public TDFactoryRect()
        {
        }

        public TDFactoryRect(
            int x,
            int y,
            int w,
            int h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
    }


    public class TDFactorySocket
    {
        public float x;
        public float y;
        public float z;

        public float rx;
        public float ry;
        public float rz;

        public float sx = 1.0f;
        public float sy = 1.0f;
        public float sz = 1.0f;
    }
}