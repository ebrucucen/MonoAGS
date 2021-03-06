﻿using AGS.API;

namespace AGS.Engine
{
    public class AGSTextComponent : AGSComponent, ITextComponent
    {
        private ILabelRenderer _labelRenderer;
        private IImageComponent _obj;

        public AGSTextComponent(ILabelRenderer labelRenderer)
        {
            _labelRenderer = labelRenderer;
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            _obj = entity.GetComponent<IImageComponent>();
            _obj.CustomRenderer = _labelRenderer;
        }

        public ITextConfig TextConfig
        {
            get { return _labelRenderer.Config; }
            set { _labelRenderer.Config = value; }
        }

        public string Text
        {
            get { return _labelRenderer.Text; }
            set { _labelRenderer.Text = value; }
        }

        public bool TextVisible
        {
            get { return _labelRenderer.TextVisible; }
            set { _labelRenderer.TextVisible = value; }
        }

        public SizeF LabelRenderSize
        {
            get { return _labelRenderer.BaseSize; }
            set
            {
                _labelRenderer.BaseSize = value;
                if (_obj != null && _obj.Image == null) _obj.Image = new EmptyImage(value.Width, value.Height);                
            }
        }

        public float TextHeight { get { return _labelRenderer.TextHeight; } }

        public float TextWidth { get { return _labelRenderer.TextWidth; } }
    }
}
