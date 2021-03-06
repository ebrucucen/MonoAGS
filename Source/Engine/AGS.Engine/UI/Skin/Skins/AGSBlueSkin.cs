﻿using AGS.API;

namespace AGS.Engine
{
    public class AGSBlueSkin
    {
        private AGSColoredSkin _skin;

        public AGSBlueSkin(IGraphicsFactory factory, IGLUtils glUtils)
        {
            _skin = new AGSColoredSkin(factory)
            {
                ButtonIdleBackColor = Colors.CornflowerBlue,
                ButtonHoverBackColor = Colors.Blue,
                ButtonPushedBackColor = Colors.DarkSlateBlue,
                ButtonBorderStyle = AGSBorders.SolidColor(glUtils, Colors.DarkBlue, 1f),
                TextBoxBackColor = Colors.CornflowerBlue,                
                TextBoxBorderStyle = AGSBorders.SolidColor(glUtils, Colors.DarkBlue, 1f),
                CheckboxCheckedColor = Colors.DarkSlateBlue,
                CheckboxNotCheckedColor = Colors.CornflowerBlue,
                CheckboxHoverCheckedColor = Colors.Blue,
                CheckboxHoverNotCheckedColor = Colors.Blue,
                CheckboxBorderStyle = AGSBorders.SolidColor(glUtils, Colors.DarkBlue, 1f),
                DialogBoxColor = Colors.DarkSlateBlue,
                DialogBoxBorder = AGSBorders.SolidColor(glUtils, Colors.DarkBlue, 2f)
            };
        }

        public ISkin CreateSkin()
        {
            return _skin.CreateSkin();
        }
    }
}
