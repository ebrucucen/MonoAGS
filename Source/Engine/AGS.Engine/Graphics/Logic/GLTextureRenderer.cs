﻿using AGS.API;

namespace AGS.Engine
{
	public class GLTextureRenderer : IGLTextureRenderer
	{
        private readonly IGLUtils _glUtils;

        public GLTextureRenderer(IGLUtils glUtils)
		{
            _glUtils = glUtils;
		}

		#region IGLTextureRenderer implementation

		public void Render(int texture, IGLBoundingBox boundingBox, IGLColor color)
		{
			Vector3 bottomLeft = boundingBox.BottomLeft;
			Vector3 topLeft = boundingBox.TopLeft;
			Vector3 bottomRight = boundingBox.BottomRight;
			Vector3 topRight = boundingBox.TopRight;

			_glUtils.DrawQuad (texture, bottomLeft, bottomRight, topLeft, topRight, color.R,
				color.G, color.B, color.A);
		}

		#endregion
	}
}

