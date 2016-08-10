﻿using System;
using OpenTK;
using AGS.API;

namespace AGS.Engine
{
	public class GLMatrixBuilder : IGLMatrixBuilder, IGLMatrices
	{
		public GLMatrixBuilder()
		{
		}

		public static readonly PointF NoScaling = new PointF(1f,1f);

		#region IGLMatrices implementation

		public Matrix4 ModelMatrix { get; private set; }

		public Matrix4 ViewportMatrix { get; private set; }

		#endregion

		//http://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/
		//http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-17-quaternions/
		public IGLMatrices Build(ISprite obj, ISprite sprite, IObject parent, Matrix4 viewport, PointF areaScaling)
		{
			Matrix4 spriteMatrix = getModelMatrix (sprite, NoScaling);
			Matrix4 objMatrix = getModelMatrix (obj, areaScaling);

			ModelMatrix = spriteMatrix * objMatrix;
			while (parent != null)
			{
				Matrix4 parentMatrix = getModelMatrix(parent, NoScaling);
				ModelMatrix = ModelMatrix * parentMatrix;
				parent = parent.TreeNode.Parent;
			}
			ViewportMatrix = viewport;

			return this;		
		}

		private Matrix4 getModelMatrix(ISprite sprite, PointF areaScaling)
		{
			PointF anchorOffsets = getAnchorOffsets (sprite.Anchor, sprite.Width, sprite.Height);
			Matrix4 anchor = Matrix4.CreateTranslation (new Vector3(-anchorOffsets.X, -anchorOffsets.Y, 0f));
			Matrix4 scale = Matrix4.CreateScale (new Vector3 (sprite.ScaleX * areaScaling.X, 
				sprite.ScaleY * areaScaling.Y, 1f));
			Matrix4 rotation = Matrix4.CreateRotationZ(sprite.Angle);
			Matrix4 transform = Matrix4.CreateTranslation (new Vector3(sprite.X, sprite.Y, 0f));
			return anchor * scale * rotation * transform;
		}

		private PointF getAnchorOffsets(PointF anchor, float width, float height)
		{
			float x = MathUtils.Lerp (0f, 0f, 1f, width, anchor.X);
			float y = MathUtils.Lerp (0f, 0f, 1f, height, anchor.Y);
			return new PointF (x, y);
		}
	}
}
