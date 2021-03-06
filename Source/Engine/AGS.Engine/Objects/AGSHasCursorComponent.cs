﻿using AGS.API;

namespace AGS.Engine
{
    public class AGSHasCursorComponent : AGSComponent, IHasCursorComponent
    {
        private bool _showingObjectSpecificCursor;
        private IGame _game;
        private IObject _lastCursor;

        public AGSHasCursorComponent(IGame game)
        {
            _game = game;
            _game.Events.OnRepeatedlyExecute.Subscribe(onRepeatedlyExecute);
        }

        public IObject SpecialCursor { get; set; }

        private void onRepeatedlyExecute(object sender, AGSEventArgs e)
        {
            var state = _game.State;
            IObject hotspot = state.Room.GetObjectAt(_game.Input.MouseX, _game.Input.MouseY);
            if (hotspot == null)
            {
                turnOffObjectSpecificCursor();
                return;
            }
            IHasCursorComponent specialCursor = hotspot.GetComponent<IHasCursorComponent>();
            if (specialCursor == null)
            {
                turnOffObjectSpecificCursor();
                return;
            }
            if (_game.Input.Cursor != specialCursor.SpecialCursor)
            {
                _lastCursor = _game.Input.Cursor;
                _game.Input.Cursor = specialCursor.SpecialCursor;
            }
            _showingObjectSpecificCursor = true;
        }

        private void turnOffObjectSpecificCursor()
        {
            if (!_showingObjectSpecificCursor) return;
            _showingObjectSpecificCursor = false;
            var lastCursor = _lastCursor;
            if (lastCursor != null)
            {
                _game.Input.Cursor = _lastCursor;
            }
        }
    }
}

