﻿using System;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using AGS.API;

namespace AGS.Engine
{
	public class GLText
	{
		private string _text;
		private int _maxWidth;
		private ITextConfig _config;
		private Bitmap _bitmap;
		private int _texture;
		private bool _renderChanged;
		private BitmapPool _bitmapPool;
		private SizeF _baseSize;

		public GLText (BitmapPool pool, string text = "", int maxWidth = int.MaxValue)
		{
			this._maxWidth = maxWidth;
			this._text = text;
			this._bitmapPool = pool;
			_texture = createTexture ();
			_config = new AGSTextConfig ();

			drawToBitmap();
		}

		public int Texture { get { return _texture; } }

		public int BitmapWidth { get { return _bitmap.Width; } }
		public int BitmapHeight { get { return _bitmap.Height; } }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public void SetProperties(SizeF baseSize, string text = null, ITextConfig config = null, int? maxWidth = null)
		{
			bool changeNeeded = 
				(text != null && text != _text)
				|| (config != null && config != _config)
				|| (maxWidth != null && maxWidth.Value != _maxWidth)
				|| !baseSize.Equals(_baseSize);
			if (!changeNeeded) return;

			_text = text;
			if (config != null) _config = config;
			if (maxWidth != null) _maxWidth = maxWidth.Value;
			_baseSize = baseSize;

			drawToBitmap();
		}

		public void Destroy()
		{
			Bitmap bitmap = _bitmap;
			if (bitmap != null) _bitmapPool.Release(bitmap);
			//todo: uncomment and remove from our textures map
			//GL.DeleteTexture (texture);
		}

		public void Refresh()
		{
			if (_renderChanged)
			{
				_renderChanged = false;
				uploadBitmapToOpenGl ();
			}
		}

		private int createTexture()
		{
			int text_texture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, text_texture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1024, 1024, 0,
			//	OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero); // just allocate memory, so we can update efficiently using TexSubImage2D
			return text_texture;
		}

		private void drawToBitmap ()
		{
			SizeF textSize = _text.Measure(_config.Font, _maxWidth);
			float widthOffset = Math.Max(_config.OutlineWidth, Math.Abs(_config.ShadowOffsetX));
			float heightOffset = Math.Max(_config.OutlineWidth, Math.Abs(_config.ShadowOffsetY));
			float widthF = textSize.Width + widthOffset;
			float heightF = textSize.Height + heightOffset;
			Width = Math.Max((int)widthF + 2, (int)_baseSize.Width + 1);
			Height = Math.Max((int)heightF + 2, (int)_baseSize.Height + 1);
			int width = MathUtils.GetNextPowerOf2(Width);
			int height = MathUtils.GetNextPowerOf2(Height);
			Bitmap bitmap = _bitmap;
			if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
			{
				if (bitmap != null) _bitmapPool.Release(bitmap);
				_bitmap = _bitmapPool.Acquire(width, height);
			}

			Font font = _config.Font;
			Brush outlineBrush = _config.OutlineBrush;

			using (Graphics gfx = Graphics.FromImage (_bitmap)) 
			{
				gfx.SmoothingMode = SmoothingMode.AntiAlias;
				gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
				gfx.TextRenderingHint = TextRenderingHint.AntiAlias;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
				gfx.CompositingQuality = CompositingQuality.HighQuality;

				float left = alignX(textSize.Width);
				float top = alignY(textSize.Height);
				float centerX = left + _config.OutlineWidth / 2f;
				float centerY = top + _config.OutlineWidth / 2f;
				float right = left + _config.OutlineWidth;
				float bottom = top + _config.OutlineWidth;

				gfx.Clear(Color.Transparent);

				if (_config.OutlineWidth > 0f)
				{
					gfx.DrawString(_text, font, outlineBrush, left, top);
					gfx.DrawString(_text, font, outlineBrush, centerX, top);
					gfx.DrawString(_text, font, outlineBrush, right, top);

					gfx.DrawString(_text, font, outlineBrush, left, centerY);
					gfx.DrawString(_text, font, outlineBrush, right, centerY);

					gfx.DrawString(_text, font, outlineBrush, left, bottom);
					gfx.DrawString(_text, font, outlineBrush, centerX, bottom);
					gfx.DrawString(_text, font, outlineBrush, right, bottom);
				}
				if (_config.ShadowBrush != null)
				{
					gfx.DrawString(_text, font, _config.ShadowBrush, centerX + _config.ShadowOffsetX, 
						centerY + _config.ShadowOffsetY);
				}
				gfx.DrawString(_text, font, _config.Brush, centerX, centerY);

				//This should be a better way to render the outline (DrawPath renders the outline, and FillPath renders the text)
				//but for some reason some lines are missing when we render like that, at least on the mac
				/*if (_outlineWidth > 0f)
				{
					GraphicsPath path = new GraphicsPath ();
					Pen outlinePen = new Pen (_outlineBrush, _outlineWidth) { LineJoin = LineJoin.Round };
					path.AddString(_text, _font.FontFamily, (int)_font.Style, _font.Size, new Point (), new StringFormat ());
					//gfx.ScaleTransform(1.3f, 1.35f);
					gfx.DrawPath(outlinePen, path);
					gfx.FillPath(_brush, path);
				}
				else 
					gfx.DrawString (_text, _font, _brush, 0f, 0f);*/
			}

			_renderChanged = true;
		}

		private float alignX(float width)
		{
			switch (_config.Alignment)
			{
				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					return 0f;
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					return _baseSize.Width / 2 - width / 2;
				default:
					return _baseSize.Width - width;
			}
		}

		private float alignY(float height)
		{
			const int reducePadding = 2;
			switch (_config.Alignment)
			{
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					return _bitmap.Height - _baseSize.Height - reducePadding;
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					return _bitmap.Height - _baseSize.Height/2f - height/2f - reducePadding/2f;
				default:
					return _bitmap.Height - height - reducePadding;
			}
		}

		private void uploadBitmapToOpenGl()
		{
			try
			{
				// Upload the Bitmap to OpenGL.
				// Do this only when text changes.
				BitmapData data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.BindTexture(TextureTarget.Texture2D, _texture);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0); 
				_bitmap.UnlockBits(data);
			}
			catch (InvalidOperationException e) 
			{
				Debug.WriteLine (e.ToString ());
				throw;
			}
		}
	}
}

