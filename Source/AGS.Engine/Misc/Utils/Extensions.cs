﻿using System;
using System.Collections.Generic;
using AGS.API;
using System.Reflection;
using OpenTK;

namespace AGS.Engine
{
	public static class Extensions
	{
		public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> map, TKey key, Func<TValue> getValue)
		{
			TValue value;
			if (!map.TryGetValue (key, out value)) 
			{
				value = getValue ();
				map[key] = value;
			}
			return value;
		}

		public static TEntity Remember<TEntity>(this TEntity entity, IGame game, 
			Action<TEntity> resetEntity) where TEntity : class, IEntity
		{
			resetEntity(entity);
			game.Events.OnSavedGameLoad.Subscribe((sender, e) =>
			{
				entity = game.Find<TEntity>(entity.ID);
				resetEntity(entity);
			});
			return entity;
		}

		public static IEnumerable<Type> GetInterfaces(this Type someType)
		{
			var t = someType;
			while (t != null)
			{
				var ti = t.GetTypeInfo();
				foreach (var m in ti.ImplementedInterfaces)
					yield return m;
				t = ti.BaseType;
			}
		}

		public static float AlignX(this ITextConfig config, float width, AGS.API.SizeF baseSize)
		{
			const float reducePadding = 2f;
			switch (config.Alignment)
			{
				case Alignment.TopLeft:
				case Alignment.MiddleLeft:
				case Alignment.BottomLeft:
					return -reducePadding + config.PaddingLeft;
				case Alignment.TopCenter:
				case Alignment.MiddleCenter:
				case Alignment.BottomCenter:
					return baseSize.Width / 2 - width / 2 - reducePadding / 2;
				default:
					return baseSize.Width - width - reducePadding - config.PaddingRight;
			}
		}

		public static float AlignY(this ITextConfig config, float bitmapHeight, float height, AGS.API.SizeF baseSize)
		{
			const float reducePadding = 2f;
			switch (config.Alignment)
			{
				case Alignment.TopLeft:
				case Alignment.TopCenter:
				case Alignment.TopRight:
					return bitmapHeight - baseSize.Height - reducePadding + config.PaddingTop;
				case Alignment.MiddleLeft:
				case Alignment.MiddleCenter:
				case Alignment.MiddleRight:
					return bitmapHeight - baseSize.Height/2f - height/2f - reducePadding/2f;
				default:
					return bitmapHeight - height - reducePadding - config.PaddingBottom;
			}
		}

		public static Vector3 ToVector3(this PointF point)
		{
			return new Vector3 (point.X, point.Y, 0f);
		}

		public static Vector2 ToVector2(this PointF point)
		{
			return new Vector2 (point.X, point.Y);
		}

		public static IGLColor ToGLColor(this Color color)
		{
			float r = color.R / 255f;
			float g = color.G / 255f;
			float b = color.B / 255f;
			float a = color.A / 255f;
			return new GLColor (r, g, b, a);
		}

		public static SizeF GetTextSize(this ITextConfig config, string text, SizeF labelSize)
		{
			switch (config.AutoFit)
			{
				case AutoFit.TextShouldFitLabel:
                    var textSize = config.Font.MeasureString(text);
                    return new SizeF(Math.Min(textSize.Width, labelSize.Width), Math.Min(textSize.Height, labelSize.Height));
				case AutoFit.TextShouldWrapWithoutHeightFitting:
				case AutoFit.TextShouldWrapAndLabelShouldFitHeight:
					return config.Font.MeasureString(text, (int)labelSize.Width);
				default:
					return config.Font.MeasureString(text);
			}
		}
	}
}
