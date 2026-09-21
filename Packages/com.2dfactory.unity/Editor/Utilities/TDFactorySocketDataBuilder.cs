using System;
using System.Collections.Generic;

public static class TDFactorySocketDataBuilder
{
    public static List<TDFactoryAnimationSocketData>
        BuildSocketData(
            string jsonFilePath,
            TDFactoryJsonParser.TDFactoryJsonMeta jsonMeta)
	{
		List<TDFactoryAnimationSocketData> animationSocketData =
			new List<TDFactoryAnimationSocketData>();

		foreach (var tagPair in jsonMeta.frameTags)
		{
			TDFactoryJsonParser.TDFactoryFrameTag sourceTag =
				tagPair.Value;

			TDFactoryAnimationSocketData animationData =
				new TDFactoryAnimationSocketData();

			animationData.animationName =
				tagPair.Key;

			animationData.jsonPath =
				jsonFilePath;

			animationData.pivot =
				sourceTag.pivot;

			animationData.cropShift =
				sourceTag.cropShift;

			animationData.direction =
				sourceTag.direction;

			animationData.loop =
				string.Equals(
					sourceTag.loop,
					"true",
					StringComparison.OrdinalIgnoreCase
				);

			foreach (
				TDFactoryJsonParser.TDFactoryFrame sourceFrame
				in sourceTag.frames
			)
			{
				TDFactoryFrameSocketData frameData =
					new TDFactoryFrameSocketData();

				frameData.frameName =
					sourceFrame.name;

				foreach (var socketPair in sourceFrame.sockets)
				{
					TDFactoryJsonParser.TDFactorySocket sourceSocket =
						socketPair.Value;

					TDFactorySocketData socketData =
						new TDFactorySocketData();

					socketData.name =
						socketPair.Key;

					socketData.x =
						sourceSocket.x;

					socketData.y =
						sourceSocket.y;

					socketData.z =
						sourceSocket.z;

					socketData.rx =
						sourceSocket.rx;

					socketData.ry =
						sourceSocket.ry;

					socketData.rz =
						sourceSocket.rz;

					socketData.sx =
						sourceSocket.sx;

					socketData.sy =
						sourceSocket.sy;

					socketData.sz =
						sourceSocket.sz;

					frameData.sockets.Add(
						socketData
					);
				}

				animationData.frames.Add(
					frameData
				);
			}

			animationSocketData.Add(
				animationData
			);
		}

		return animationSocketData;
	}
}