using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TDFactorySocketData
{
    public string name;

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

[Serializable]
public class TDFactoryFrameSocketData
{
    public string frameName;

    public List<TDFactorySocketData> sockets =
        new List<TDFactorySocketData>();
}

[Serializable]
public class TDFactoryAnimationSocketData
{
    public string animationName;

    public string jsonPath;

    public Vector2 pivot;
    public Vector2 cropShift;

    public string direction;
    public bool loop;

    public List<TDFactoryFrameSocketData> frames =
        new List<TDFactoryFrameSocketData>();
}

public class TDFactorySocketController : MonoBehaviour
{
    [SerializeField]
    private List<TDFactoryAnimationSocketData> animations =
        new List<TDFactoryAnimationSocketData>();
		
	public void SetSocketData(
		List<TDFactoryAnimationSocketData> data)
	{
		animations = data;
	}
	
/* 	public void AppendSocketData(
		List<TDFactoryAnimationSocketData> data)
	{
		if (data == null)
		{
			return;
		}

		animations.AddRange(
			data
		);
	} */
	
	public void UpdateSocketData(
		List<TDFactoryAnimationSocketData> data)
	{
		if (data == null)
		{
			return;
		}

		foreach (
			TDFactoryAnimationSocketData incomingAnimation
			in data
		)
		{
			TDFactoryAnimationSocketData existingAnimation =
				animations.Find(
					animation =>
						animation.animationName ==
						incomingAnimation.animationName
				);

			if (existingAnimation == null)
			{
				animations.Add(
					incomingAnimation
				);

				continue;
			}

			existingAnimation.jsonPath =
				incomingAnimation.jsonPath;

			existingAnimation.pivot =
				incomingAnimation.pivot;

			existingAnimation.cropShift =
				incomingAnimation.cropShift;

			existingAnimation.direction =
				incomingAnimation.direction;

			existingAnimation.loop =
				incomingAnimation.loop;

			foreach (
				TDFactoryFrameSocketData incomingFrame
				in incomingAnimation.frames
			)
			{
				TDFactoryFrameSocketData existingFrame =
					existingAnimation.frames.Find(
						frame =>
							frame.frameName ==
							incomingFrame.frameName
					);

				if (existingFrame == null)
				{
					existingAnimation.frames.Add(
						incomingFrame
					);

					continue;
				}

				foreach (
					TDFactorySocketData incomingSocket
					in incomingFrame.sockets
				)
				{
					TDFactorySocketData existingSocket =
						existingFrame.sockets.Find(
							socket =>
								socket.name ==
								incomingSocket.name
						);

					if (existingSocket == null)
					{
						existingFrame.sockets.Add(
							incomingSocket
						);

						continue;
					}

					existingSocket.x =
						incomingSocket.x;

					existingSocket.y =
						incomingSocket.y;

					existingSocket.z =
						incomingSocket.z;

					existingSocket.rx =
						incomingSocket.rx;

					existingSocket.ry =
						incomingSocket.ry;

					existingSocket.rz =
						incomingSocket.rz;

					existingSocket.sx =
						incomingSocket.sx;

					existingSocket.sy =
						incomingSocket.sy;

					existingSocket.sz =
						incomingSocket.sz;
				}
			}
		}
	}
	
	public void UpdateSocketsToCurrentSprite()
	{
		if (mSpriteRenderer == null)
		{
			mSpriteRenderer =
				GetComponent<SpriteRenderer>();
		}

		if (mSpriteRenderer == null)
		{
			return;
		}

		Sprite currentSprite =
			mSpriteRenderer.sprite;

		if (currentSprite == null)
		{
			return;
		}

		ApplySocketData(
			currentSprite
		);
	}

    private SpriteRenderer mSpriteRenderer;

    private Sprite mLastSprite;

    private void Awake()
    {
        mSpriteRenderer =
            GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (mSpriteRenderer == null)
        {
            return;
        }

        Sprite currentSprite =
            mSpriteRenderer.sprite;

        if (currentSprite == null)
        {
            return;
        }

        if (currentSprite == mLastSprite)
        {
            return;
        }

        mLastSprite =
            currentSprite;

        ApplySocketData(
			currentSprite
        );
    }

	private void ApplySocketData(
		Sprite currentSprite)
	{
		string frameName = currentSprite.name;
		foreach (
			TDFactoryAnimationSocketData animationData
			in animations
		)
		{
			foreach (
				TDFactoryFrameSocketData frameData
				in animationData.frames
			)
			{
				if (
					frameData.frameName != frameName
				)
				{
					continue;
				}

				foreach (
					TDFactorySocketData socketData
					in frameData.sockets
				)
				{
					float pixelsPerUnit =
						currentSprite.pixelsPerUnit;
						
					float localX =
						(
							socketData.x
							- (
								animationData.pivot.x
								- animationData.cropShift.x
							)
						) / pixelsPerUnit;

					float localY =
						(
							socketData.y
							- (
								animationData.pivot.y
								- animationData.cropShift.y
							)
						) / pixelsPerUnit;
						
					float localZ = socketData.z/pixelsPerUnit;

					Transform socketTransform =
						transform.Find(
							socketData.name
						);

					if (socketTransform == null)
					{
						continue;
					}

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

				return;
			}
		}
	}
}