﻿using System;

namespace AGS.API
{
    /// <summary>
    /// Represents a color.
    /// </summary>
	public struct Color
	{
		private const int SHIFT_A = 24;
		private const int SHIFT_R = 16;
		private const int SHIFT_G = 8;
		private const int SHIFT_B = 0;

		private readonly uint _value;

		private Color(uint argb)
		{
			_value = argb;
		}

        /// <summary>
        /// A single value which encodes the color within it, 8 bits per color element, 
        /// where the order is A (alpha)-R (red)-G (green)-B (blue).
        /// </summary>
        /// <value>The color value.</value>
		public uint Value { get { return _value; } }

        /// <summary>
        /// Gets the red element of the color. 0 is no red and 255 is fully red.
        /// </summary>
        /// <value>The red element of the color.</value>
		public byte R { get { return(byte)((Value >> SHIFT_R)); } }

        /// <summary>
        /// Gets the green element of the color. 0 is no red and 255 is fully green.
        /// </summary>
        /// <value>The green element of the color.</value>
		public byte G { get { return(byte)((Value >> SHIFT_G)); } }

        /// <summary>
        /// Gets the blue element of the color. 0 is no red and 255 is fully blue.
        /// </summary>
        /// <value>The blue element of the color.</value>
		public byte B { get { return(byte)((Value >> SHIFT_B)); } }

        /// <summary>
        /// Gets the alpha element of the color. 0 is fully transparent and 255 is fully opaque.
        /// </summary>
        /// <value>The alpha element of the color.</value>
		public byte A { get { return(byte)((Value >> SHIFT_A)); } }

		public override string ToString()
		{
			return string.Format("[Color: Value={0}, R={1}, G={2}, B={3}, A={4}]", Value, R, G, B, A);
		}

        /// <summary>
        /// Gets a color from ARGB elements (alpha, red, green, blue).
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
		public static Color FromArgb(byte a, byte r, byte g, byte b)
		{
			return new Color ((uint)(r << SHIFT_R |
									 g << SHIFT_G |
									 b << SHIFT_B |
									 a << SHIFT_A));
		}

        /// <summary>
        /// Gets a color from RGBA elements (red, green, blue, alpha).
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
		public static Color FromRgba(byte r, byte g, byte b, byte a)
		{
			return FromArgb(a, r, g, b);
		}

        /// <summary>
        /// Gets a color from a hexa value (in ARGB format).
        /// </summary>
        /// <returns>The hexa.</returns>
        /// <param name="argb">ARGB.</param>
		public static Color FromHexa(uint argb)
		{
			return new Color (argb);
		}

        /// <summary>
        /// Gets a color from HSLA format (https://en.wikipedia.org/wiki/HSL_and_HSV)
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="hue">Hue.</param>
        /// <param name="saturation">Saturation.</param>
        /// <param name="lightness">Lightness.</param>
        /// <param name="a">The alpha component.</param>
		public static Color FromHsla(int hue, float saturation, float lightness, byte a)
		{
			float h = hue / 60f;
			byte r, g, b;
			if (saturation == 0)
			{
				r = (byte)Math.Round(lightness * 255d);
				g = (byte)Math.Round(lightness * 255d);
				b = (byte)Math.Round(lightness * 255d);
			}
			else
			{
				double t1, t2;
				double th = h / 6.0d;

				if (lightness < 0.5d)
				{
					t2 = lightness * (1d + saturation);
				}
				else
				{
					t2 = (lightness + saturation) - (lightness * saturation);
				}
				t1 = 2d * lightness - t2;

				double tr, tg, tb;
				tr = th + (1.0d / 3.0d);
				tg = th;
				tb = th - (1.0d / 3.0d);

				tr = colorCalc(tr, t1, t2);
				tg = colorCalc(tg, t1, t2);
				tb = colorCalc(tb, t1, t2);
				r = (byte)Math.Round(tr * 255d);
				g = (byte)Math.Round(tg * 255d);
				b = (byte)Math.Round(tb * 255d);
			}
			return FromArgb(a, r, g, b);
		}

		private static double colorCalc(double c, double t1, double t2)
		{

			if (c < 0) c += 1d;
			if (c > 1) c -= 1d;
			if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
			if (2.0d * c < 1.0d) return t2;
			if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
			return t1;
		}

        /// <summary>
        /// Gets the hue of the color (based on HSL format: https://en.wikipedia.org/wiki/HSL_and_HSV).
        /// </summary>
        /// <returns>The hue.</returns>
		public int GetHue()
		{
			decimal r = (decimal)R / 255m;
			decimal g = (decimal)G / 255m;
			decimal b = (decimal)B / 255m;
			decimal cmax = Math.Max(r, Math.Max(g, b));
			decimal cmin = Math.Min(r, Math.Min(g, b));
			decimal delta = cmax - cmin;
			if (delta == 0) return 0;

			decimal hue = 0;
			if (cmax == r) hue = 60m * (((g - b) / delta));
			else if (cmax == g) hue = 60m * (((b - r) / delta) + 2m);
			else hue = 60m * (((r - g) / delta) + 4m);
			int h = (int)Math.Round(hue);
			if (h < 0) h += 360;
			return h;
		}

        /// <summary>
        /// Gets the saturation of the color (based on HSL format: https://en.wikipedia.org/wiki/HSL_and_HSV).
        /// </summary>
        /// <returns>The saturation.</returns>
		public float GetSaturation()
		{
			decimal r = (decimal)R / 255m;
			decimal g = (decimal)G / 255m;
			decimal b = (decimal)B / 255m;
			decimal cmax = Math.Max(r, Math.Max(g, b));
			decimal cmin = Math.Min(r, Math.Min(g, b));
			decimal delta = cmax - cmin;
			if (delta == 0) return 0f;
			return (float)delta / (1 - Math.Abs(2 * GetLightness() - 1));
		}

        /// <summary>
        /// Gets the lightness of the color (based on HSL format: https://en.wikipedia.org/wiki/HSL_and_HSV).
        /// </summary>
        /// <returns>The saturation.</returns>
		public float GetLightness()
		{
			decimal r = (decimal)R / 255m;
			decimal g = (decimal)G / 255m;
			decimal b = (decimal)B / 255m;
			decimal cmax = Math.Max(r, Math.Max(g, b));
			decimal cmin = Math.Min(r, Math.Min(g, b));
			return (float)(cmax + cmin) / 2f;
		}
	}
}

