﻿extern alias IOS;

using System;
using System.Threading.Tasks;
using AGS.API;
using IOS::UIKit;

namespace AGS.Engine.IOS
{
    public class IOSInput : IInput
    {
        private IGameWindow _gameWindow;
        private IGameState _state;
        private int _virtualWidth, _virtualHeight;
        private IShouldBlockInput _shouldBlockInput;
        private DateTime _lastDrag;

        public IOSInput(IOSGestures gestures, AGS.API.Size virtualResolution,
                        IGameState state, IShouldBlockInput shouldBlockInput, IGameWindow gameWindow)
        {
            _shouldBlockInput = shouldBlockInput;
            _gameWindow = gameWindow;
            _state = state;
            this._virtualWidth = virtualResolution.Width;
            this._virtualHeight = virtualResolution.Height;
            MouseDown = new AGSEvent<AGS.API.MouseButtonEventArgs>();
            MouseUp = new AGSEvent<AGS.API.MouseButtonEventArgs>();
            MouseMove = new AGSEvent<MousePositionEventArgs>();
            KeyDown = new AGSEvent<KeyboardEventArgs>();
            KeyUp = new AGSEvent<KeyboardEventArgs>();

            IOSGameWindow.Instance.View.OnInsertText += onInsertText;
            IOSGameWindow.Instance.View.OnDeleteBackward += onDeleteBackwards;

            gestures.OnUserDrag += async (sender, e) =>
            {
                if (isInputBlocked()) return;
                DateTime now = DateTime.Now;
                _lastDrag = now;
                IsTouchDrag = true;
                setMousePosition(e);
                await MouseMove.InvokeAsync(sender, new MousePositionEventArgs(MouseX, MouseY));
                await Task.Delay(300);
                if (_lastDrag <= now) IsTouchDrag = false;
            };
            gestures.OnUserSingleTap += async (sender, e) =>
            {
                if (isInputBlocked()) return;
                setMousePosition(e);
                LeftMouseButtonDown = true;
                await MouseDown.InvokeAsync(sender, new MouseButtonEventArgs(MouseButton.Left, MouseX, MouseY));
                await Task.Delay(250);
                await MouseUp.InvokeAsync(sender, new MouseButtonEventArgs(MouseButton.Left, MouseX, MouseY));
                LeftMouseButtonDown = false;
            };
        }

        public IObject Cursor { get; set; }

        public IEvent<KeyboardEventArgs> KeyDown { get; private set; }

        public IEvent<KeyboardEventArgs> KeyUp { get; private set; }

        public bool LeftMouseButtonDown { get; private set; }

        public IEvent<MouseButtonEventArgs> MouseDown { get; private set; }

        public IEvent<MousePositionEventArgs> MouseMove { get; private set; }

        public IEvent<MouseButtonEventArgs> MouseUp { get; private set; }

        public PointF MousePosition { get; private set; }

        public float MouseX { get { return MousePosition.X; } }

        public float MouseY { get { return MousePosition.Y; } }

        public bool RightMouseButtonDown { get; private set; }

        public bool IsTouchDrag { get; private set; }

        public bool IsKeyDown(Key key)
        {
            return false;
        }

        private bool isInputBlocked()
        {
            return _shouldBlockInput.ShouldBlockInput();
        }

        private void setMousePosition(MousePositionEventArgs e)
        {
            float x = convertX(e.X);
            float y = convertY(e.Y);
            MousePosition = new PointF(x, y);
        }

        private float convertX(float x)
        {
            var viewport = getViewport();
            var virtualWidth = _virtualWidth / viewport.ScaleX;
            float density = (float)UIScreen.MainScreen.Scale;
            x = x - (GLUtils.ScreenViewport.X / density);
            float width = (_gameWindow.Width - (GLUtils.ScreenViewport.X * 2)) / density;
            x = MathUtils.Lerp(0f, 0f, width, virtualWidth, x);
            return x + viewport.X;
        }

        private float convertY(float y)
        {
            var viewport = getViewport();
            var virtualHeight = _virtualHeight / viewport.ScaleY;
            float density = (float)UIScreen.MainScreen.Scale;
            y = y - (GLUtils.ScreenViewport.Y / density);
            float height = (_gameWindow.Height - (GLUtils.ScreenViewport.Y * 2)) / density;
            y = MathUtils.Lerp(0f, virtualHeight, height, 0f, y);
            return y + viewport.Y;
        }

        private IViewport getViewport()
        {
            return _state.Room.Viewport;
        }

        private async void onInsertText(object sender, string text)
        {
            foreach (char c in text)
            {
                bool isShift;
                Key key = mapKey(c, out isShift);
                await fireKeyPress(key, isShift);
            }
        }

        private async void onDeleteBackwards(object sender, EventArgs args)
        {
            await fireKeyPress(Key.BackSpace, false);
        }

        private async Task fireKeyPress(Key key, bool isShift)
        {
            var keyDown = KeyDown;
            var keyUp = KeyUp;
            if (keyDown != null)
            {
                if (isShift) await keyDown.InvokeAsync(this, new KeyboardEventArgs(Key.ShiftLeft));
                await keyDown.InvokeAsync(this, new KeyboardEventArgs(key));
            }
            await Task.Delay(5);
            if (keyUp != null)
            {
                await keyUp.InvokeAsync(this, new KeyboardEventArgs(key));
                if (isShift) await keyUp.InvokeAsync(this, new KeyboardEventArgs(Key.ShiftLeft));
            }
        }

        private Key mapKey(char c, out bool isShift)
        {
            isShift = false;
            if (c >= 'a' && c <= 'z')
            {
                return c - 'a' + Key.A;
            }
            if (c >= 'A' && c <= 'Z')
            {
                isShift = true;
                return c - 'A' + Key.A;
            }
            if (c >= '0' && c <= '9')
            {
                return c - '0' + Key.Number0;
            }
            switch (c)
            {
                case '!': return withShift(Key.Number1, out isShift);
                case '@': return withShift(Key.Number2, out isShift);
                case '#': return withShift(Key.Number3, out isShift);
                case '$': return withShift(Key.Number4, out isShift);
                case '%': return withShift(Key.Number5, out isShift);
                case '^': return withShift(Key.Number6, out isShift);
                case '&': return withShift(Key.Number7, out isShift);
                case '*': return withShift(Key.Number8, out isShift);
                case '(': return withShift(Key.Number9, out isShift);
                case ')': return withShift(Key.Number0, out isShift);
                case '-': return Key.Minus;
                case '_': return withShift(Key.Minus, out isShift);
                case '=': return Key.Plus;
                case '+': return withShift(Key.Plus, out isShift);
                case '`': return Key.Tilde;
                case '~': return withShift(Key.Tilde, out isShift);
                case '[': return Key.BracketLeft;
                case '{': return withShift(Key.BracketLeft, out isShift);
                case ']': return Key.BracketRight;
                case '}': return withShift(Key.BracketRight, out isShift);
                case '\\': return Key.BackSlash;
                case '|': return withShift(Key.BackSlash, out isShift);
                case ';': return Key.Semicolon;
                case ':': return withShift(Key.Semicolon, out isShift);
                case '\'': return Key.Quote;
                case '"': return withShift(Key.Quote, out isShift);
                case ',': return Key.Comma;
                case '<': return withShift(Key.Comma, out isShift);
                case '.': return Key.Period;
                case '>': return withShift(Key.Period, out isShift);
                case '/': return Key.Slash;
                case '?': return withShift(Key.Slash, out isShift);
                case ' ': return Key.Space;
                default: return withShift(Key.Slash, out isShift);
            }
        }

        private Key withShift(Key key, out bool isShift)
        {
            isShift = true;
            return key;
        }
    }
}
