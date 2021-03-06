﻿using System;
using AGS.API;

namespace AGS.Engine
{
	public static class Tweens
	{
		public static Tween TweenX(this ISprite sprite, float toX, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.X, toX, x => sprite.X = x, timeInSeconds, easing);
		}

		public static Tween TweenY(this ISprite sprite, float toY, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Y, toY, y => sprite.Y = y, timeInSeconds, easing);
		}

		public static Tween TweenZ(this ISprite sprite, float toZ, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Z, toZ, z => sprite.Z = z, timeInSeconds, easing);
		}

		public static Tween TweenAngle(this ISprite sprite, float toAngle, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Angle, toAngle, a => sprite.Angle = a, timeInSeconds, easing);
		}

		public static Tween TweenOpacity(this ISprite sprite, byte toOpacity, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Opacity, (float)toOpacity, o => sprite.Opacity = (byte)o, timeInSeconds, easing);
		}

		public static Tween TweenRed(this ISprite sprite, byte toRed, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Tint.R, (float)toRed, r => sprite.Tint = Color.FromArgb(sprite.Tint.A, (byte)r,
				sprite.Tint.G, sprite.Tint.B), timeInSeconds, easing);
		}

		public static Tween TweenGreen(this ISprite sprite, byte toGreen, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Tint.G, (float)toGreen, g => sprite.Tint = Color.FromArgb(sprite.Tint.A, sprite.Tint.R,
				(byte)g, sprite.Tint.B), timeInSeconds, easing);
		}

		public static Tween TweenBlue(this ISprite sprite, byte toBlue, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Tint.B, (float)toBlue, b => sprite.Tint = Color.FromArgb(sprite.Tint.A, sprite.Tint.R,
				sprite.Tint.G, (byte)b), timeInSeconds, easing);
		}

		public static Tween TweenHue(this ISprite sprite, int toHue, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run((float)sprite.Tint.GetHue(), (float)toHue, h => sprite.Tint = Color.FromHsla((int)h, sprite.Tint.GetSaturation(),
				sprite.Tint.GetLightness(), sprite.Tint.A), timeInSeconds, easing);
		}

		public static Tween TweenSaturation(this ISprite sprite, float toSaturation, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Tint.GetSaturation(), toSaturation, s => sprite.Tint = Color.FromHsla(sprite.Tint.GetHue(), s,
				sprite.Tint.GetLightness(), sprite.Tint.A), timeInSeconds, easing);
		}

		public static Tween TweenLightness(this ISprite sprite, float toLightness, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Tint.GetLightness(), toLightness, l => sprite.Tint = Color.FromHsla(sprite.Tint.GetHue(), 
				sprite.Tint.GetSaturation(), l, sprite.Tint.A), timeInSeconds, easing);
		}

		public static Tween TweenAnchorX(this ISprite sprite, float toAnchorX, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Anchor.X, toAnchorX, x => sprite.Anchor = new PointF (x, sprite.Anchor.Y), timeInSeconds, easing);
		}

		public static Tween TweenAnchorY(this ISprite sprite, float toAnchorY, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Anchor.Y, toAnchorY, y => sprite.Anchor = new PointF (sprite.Anchor.X, y), timeInSeconds, easing);
		}

		public static Tween TweenScaleX(this ISprite sprite, float toScaleX, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.ScaleX, toScaleX, x => sprite.ScaleBy(x, sprite.ScaleY), timeInSeconds, easing);
		}

		public static Tween TweenScaleY(this ISprite sprite, float toScaleY, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.ScaleY, toScaleY, y => sprite.ScaleBy(sprite.ScaleX, y), timeInSeconds, easing);
		}

		public static Tween TweenWidth(this ISprite sprite, float toWidth, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Width, toWidth, w => sprite.ScaleTo(w, sprite.Height), timeInSeconds, easing);
		}

		public static Tween TweenHeight(this ISprite sprite, float toHeight, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sprite.Height, toHeight, h => sprite.ScaleTo(sprite.Width, h), timeInSeconds, easing);
		}

		public static Tween TweenX(this IViewport viewport, float toX, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(viewport.X, toX, x => viewport.X = x, timeInSeconds, easing);
		}

		public static Tween TweenY(this IViewport viewport, float toY, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(viewport.Y, toY, y => viewport.Y = y, timeInSeconds, easing);
		}

		public static Tween TweenScaleX(this IViewport viewport, float toScaleX, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(viewport.ScaleX, toScaleX, x => viewport.ScaleX = x, timeInSeconds, easing);
		}

		public static Tween TweenScaleY(this IViewport viewport, float toScaleY, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(viewport.ScaleY, toScaleY, y => viewport.ScaleY = y, timeInSeconds, easing);
		}

		public static Tween TweenAngle(this IViewport viewport, float toAngle, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(viewport.Angle, toAngle, a => viewport.Angle = a, timeInSeconds, easing);
		}

		public static Tween TweenVolume(this ISound sound, float toVolume, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sound.Volume, toVolume, v => sound.Volume = v, timeInSeconds, easing);
		}

		public static Tween TweenPitch(this ISound sound, float toPitch, float timeInSeconds, Func<float, float> easing = null)
		{
			return Tween.Run(sound.Pitch, toPitch, p => sound.Pitch = p, timeInSeconds, easing);
		}
	}
}

