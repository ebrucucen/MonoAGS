﻿using AGS.API;

namespace AGS.Engine
{
    public class AGSRotateComponent : AGSComponent, IRotateComponent
    {
        private readonly IRotate _rotate;

        public AGSRotateComponent(IRotate rotate)
        {
            _rotate = rotate;
        }

        public float Angle {  get { return _rotate.Angle; } set { _rotate.Angle = value; } }

        public IEvent<AGSEventArgs> OnAngleChanged { get { return _rotate.OnAngleChanged; } }
    }
}
