﻿using System;
using AGS.API;

namespace AGS.Engine
{
    public class GLUtils : IGLUtils
	{
		private static readonly Vector2 _bottomLeft = new Vector2 (0.0f, 1.0f);
		private static readonly Vector2 _bottomRight = new Vector2 (1.0f, 1.0f);
		private static readonly Vector2 _topRight = new Vector2 (1.0f, 0.0f);
		private static readonly Vector2 _topLeft = new Vector2 (0.0f, 0.0f);

        private static readonly short[] _quadIndices = {3,0,1,  // first triangle (top left - bottom left - bottom right)
                                                       3,1,2}; // second triangle (top left - bottom right - top right)

        private int _vbo, _ebo;

        private int _lastResolutionWidth, _lastResolutionHeight;

        private readonly IGraphicsBackend _graphics;

        public GLUtils(IGraphicsBackend graphics)
        {
            _graphics = graphics;
        }

        public static Rectangle ScreenViewport { get; private set; }

        public void AdjustResolution(int width, int height)
        {
            if (_lastResolutionWidth == width && _lastResolutionHeight == height) return;
            _lastResolutionWidth = width;
            _lastResolutionHeight = height;

            _graphics.MatrixMode(MatrixType.Projection);
            _graphics.LoadIdentity();

            _graphics.Ortho(0, width, 0, height, -1, 1);
            
            _graphics.MatrixMode(MatrixType.ModelView);
            _graphics.LoadIdentity();
        }

        public void RefreshViewport(IGameSettings settings, IGameWindow gameWindow)
        { 
            if (settings.PreserveAspectRatio) //http://www.david-amador.com/2013/04/opengl-2d-independent-resolution-rendering/
            {
                float targetAspectRatio = (float)settings.VirtualResolution.Width / settings.VirtualResolution.Height;
                Size screen = new Size(gameWindow.Width, gameWindow.Height);
                int width = screen.Width;
                int height = (int)(width / targetAspectRatio + 0.5f);
                if (height > screen.Height)
                {
                    //It doesn't fit our height, we must switch to pillarbox then
                    height = screen.Height;
                    width = (int)(height * targetAspectRatio + 0.5f);
                }

                // set up the new viewport centered in the backbuffer
                int viewX = (screen.Width / 2) - (width / 2);
                int viewY = (screen.Height / 2) - (height / 2);

                ScreenViewport = new Rectangle(viewX, viewY, width, height);
                _graphics.Viewport(viewX, viewY, width, height);
            }
            else
            {
                ScreenViewport = new Rectangle(0, 0, gameWindow.Width, gameWindow.Height);
                _graphics.Viewport(0, 0, gameWindow.Width, gameWindow.Height);
            }
        }

		public void GenBuffers()
		{
			_vbo = _graphics.GenBuffer();
            _graphics.BindBuffer(_vbo, BufferType.ArrayBuffer);
            _graphics.InitPointers(GLVertex.Size);
		
            _ebo = _graphics.GenBuffer();
            _graphics.BindBuffer(_ebo, BufferType.ElementArrayBuffer);
		}

		public void DrawQuad(int texture, Vector3 bottomLeft, Vector3 bottomRight, 
			Vector3 topLeft, Vector3 topRight, float r, float g, float b, float a)
		{
			GLVertex[] vertices = new GLVertex[]{ new GLVertex(bottomLeft.Xy, _bottomLeft, r,g,b,a), 
				new GLVertex(bottomRight.Xy, _bottomRight, r,g,b,a), new GLVertex(topRight.Xy, _topRight, r,g,b,a),
				new GLVertex(topLeft.Xy, _topLeft, r,g,b,a)};

            DrawQuad(texture, vertices);
		}

		public void DrawQuad(int texture, Vector3 bottomLeft, Vector3 bottomRight, 
			Vector3 topLeft, Vector3 topRight, IGLColor bottomLeftColor, IGLColor bottomRightColor,
			IGLColor topLeftColor, IGLColor topRightColor)
		{
			GLVertex[] vertices = new GLVertex[]{ new GLVertex(bottomLeft.Xy, _bottomLeft, bottomLeftColor), 
				new GLVertex(bottomRight.Xy, _bottomRight, bottomRightColor), new GLVertex(topRight.Xy, _topRight, topRightColor),
				new GLVertex(topLeft.Xy, _topLeft, topLeftColor)};

            DrawQuad(texture, vertices);
		}

        public void DrawQuad(int texture, Vector3 bottomLeft, Vector3 bottomRight, 
			Vector3 topLeft, Vector3 topRight, IGLColor color, FourCorners<Vector2> texturePos)
		{
			GLVertex[] vertices = new GLVertex[]{ new GLVertex(bottomLeft.Xy, texturePos.BottomLeft, color), 
				new GLVertex(bottomRight.Xy, texturePos.BottomRight, color), new GLVertex(topRight.Xy, texturePos.TopRight, color),
				new GLVertex(topLeft.Xy, texturePos.TopLeft, color)};

            DrawQuad(texture, vertices);
		}

        public void DrawQuad(int texture, GLVertex[] vertices)
        {
            texture = getTexture(texture);
            _graphics.BindTexture2D(texture);

            _graphics.BufferData(vertices, GLVertex.Size, BufferType.ArrayBuffer);
            _graphics.InitPointers(GLVertex.Size);

            _graphics.BufferData(_quadIndices, sizeof(short), BufferType.ElementArrayBuffer);
            _graphics.SetShaderAppVars();

            _graphics.DrawElements(PrimitiveMode.Triangles, 6, _quadIndices);
        }

        public bool DrawQuad(IFrameBuffer frameBuffer, ISquare square, GLVertex[] vertices)
        {
            if (frameBuffer == null) return false;
            vertices[0] = new GLVertex(square.BottomLeft.ToVector2(), _bottomLeft, Colors.White);
            vertices[1] = new GLVertex(square.BottomRight.ToVector2(), _bottomRight, Colors.White);
            vertices[2] = new GLVertex(square.TopRight.ToVector2(), _topRight, Colors.White);
            vertices[3] = new GLVertex(square.TopLeft.ToVector2(), _topLeft, Colors.White);
            DrawQuad(frameBuffer.Texture.ID, vertices);
            return true;
        }

        public IFrameBuffer BeginFrameBuffer(ISquare square, IRuntimeSettings settings)
        {
            float width = square.MaxX - square.MinX;
            float height = square.MaxY - square.MinY;
            var aspectRatio = new SizeF(settings.WindowSize.Width / (float)settings.VirtualResolution.Width,
                                        settings.WindowSize.Height / (float)settings.VirtualResolution.Height);

            var frameBuffer = new GLFrameBuffer(new Size((int)Math.Ceiling(width * aspectRatio.Width),
                                                         (int)Math.Ceiling(height * aspectRatio.Height)), _graphics);
            frameBuffer.Begin();
            return frameBuffer;
        }

        public void DrawTriangleFan(int texture, GLVertex[] vertices)
		{
            texture = getTexture(texture);
            _graphics.BindTexture2D(texture);

            drawArrays(PrimitiveMode.TriangleFan, vertices);
		}

        public void DrawTriangle(int texture, GLVertex[] vertices)
        { 
            texture = getTexture(texture);
            _graphics.BindTexture2D(texture);

            drawArrays(PrimitiveMode.Triangles, vertices);
        }
			
		public void DrawCross(float x, float y, float width, float height,
			float r, float g, float b, float a)
		{
            GLVertex[] vertices = new GLVertex[]{
                new GLVertex(new Vector2(x - width, y - height/10), _bottomLeft, r,g,b,a),
                new GLVertex(new Vector2(x + width, y - height/10), _bottomRight, r,g,b,a),
                new GLVertex(new Vector2(x + width, y + height/10), _topRight, r,g,b,a),
                new GLVertex(new Vector2(x - width, y + height/10), _topLeft, r,g,b,a)
            };
            DrawQuad(0, vertices);

            vertices = new GLVertex[]{
				new GLVertex(new Vector2(x - width/10, y - height), _bottomLeft, r,g,b,a), 
				new GLVertex(new Vector2(x + width/10, y - height), _bottomRight, r,g,b,a), 
				new GLVertex(new Vector2(x + width/10, y + height), _topRight, r,g,b,a),
				new GLVertex(new Vector2(x - width/10, y + height), _topLeft, r,g,b,a)
			};

            DrawQuad(0, vertices);
		}

		public void DrawLine(float x1, float y1, float x2, float y2, 
			float width, float r, float g, float b, float a)
		{
            int texture = getTexture(0);
            _graphics.BindTexture2D(texture);
			_graphics.LineWidth (width);
			GLVertex[] vertices = new GLVertex[]{ new GLVertex(new Vector2(x1,y1), _bottomLeft, r,g,b,a), 
				new GLVertex(new Vector2(x2,y2), _bottomRight, r,g,b,a)};

            drawArrays(PrimitiveMode.Lines, vertices);
		}

        private void drawArrays(PrimitiveMode primitive, GLVertex[] vertices)
        {
            _graphics.BufferData(vertices, GLVertex.Size, BufferType.ArrayBuffer);
            _graphics.DrawArrays(primitive, 0, vertices.Length);
        }

        private static int getTexture(int texture)
        {
            return texture == 0 ? GLImageRenderer.EmptyTexture.ID : texture;
        }
	}
}

