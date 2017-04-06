﻿using System;
using AGS.API;
using System.Collections.Generic;
using Autofac;

namespace AGS.Engine
{
	public class AGSRendererLoop : IRendererLoop
	{
		private readonly IGameState _gameState;
        private readonly IGame _game;
		private readonly IImageRenderer _renderer;
		private readonly IComparer<IObject> _comparer;
		private readonly AGSWalkBehindsMap _walkBehinds;
		private readonly IInput _input;
		private readonly Resolver _resolver;
		private readonly IAGSRoomTransitions _roomTransitions;
        private IGLUtils _glUtils;
        private IShader _lastShaderUsed;
		private IObject _mouseCursorContainer;
        private IFrameBuffer _fromTransitionBuffer, _toTransitionBuffer;        

		public AGSRendererLoop (Resolver resolver, IGame game, IImageRenderer renderer, IInput input, AGSWalkBehindsMap walkBehinds,
			IAGSRoomTransitions roomTransitions, IGLUtils glUtils)
		{
            this._glUtils = glUtils;
			this._resolver = resolver;
			this._walkBehinds = walkBehinds;
            this._game = game;
			this._gameState = game.State;
			this._renderer = renderer;
			this._input = input;
			this._comparer = new RenderOrderSelector ();
			this._roomTransitions = roomTransitions;
			_roomTransitions.Transition = new RoomTransitionInstant ();
		}

		#region IRendererLoop implementation

		public bool Tick ()
		{
            if (_gameState.Room == null) return false;
			IRoom room = _gameState.Room;

			switch (_roomTransitions.State)
			{
				case RoomTransitionState.NotInTransition:
					activateShader();
					renderRoom(room);
					break;
				case RoomTransitionState.BeforeLeavingRoom:
                    if (_roomTransitions.Transition == null)
                    {
                        _roomTransitions.State = RoomTransitionState.NotInTransition;
                        return false;
                    }
                    else if (_gameState.Cutscene.IsSkipping)
                    {
                        _roomTransitions.State = RoomTransitionState.PreparingTransition;
                        return false;
                    }
					else if (!_roomTransitions.Transition.RenderBeforeLeavingRoom(getDisplayList(room), obj => renderObject(room, obj)))
					{
						if (_fromTransitionBuffer == null) _fromTransitionBuffer = renderToBuffer(room);
						_roomTransitions.State = RoomTransitionState.PreparingTransition;
						return false;
					}
					break;
				case RoomTransitionState.PreparingTransition:
					return false;
				case RoomTransitionState.InTransition:
                    if (_gameState.Cutscene.IsSkipping)
                    { 
                        _fromTransitionBuffer = null;
                        _toTransitionBuffer = null;
                        _roomTransitions.State = RoomTransitionState.AfterEnteringRoom;
                        return false;
                    }
					if (_toTransitionBuffer == null) _toTransitionBuffer = renderToBuffer(room);
					if (!_roomTransitions.Transition.RenderTransition(_fromTransitionBuffer, _toTransitionBuffer))
					{
						_fromTransitionBuffer = null;
						_toTransitionBuffer = null;
						_roomTransitions.State = RoomTransitionState.AfterEnteringRoom;
						return false;
					}
					break;
				case RoomTransitionState.AfterEnteringRoom:
                    if (_gameState.Cutscene.IsSkipping || !_roomTransitions.Transition.RenderAfterEnteringRoom(getDisplayList(room), obj => renderObject(room, obj)))
					{
						_roomTransitions.SetOneTimeNextTransition(null);
						_roomTransitions.State = RoomTransitionState.NotInTransition;
						return false;
					}
					break;
				default:
					throw new NotSupportedException (_roomTransitions.State.ToString());
			}
			return true;
		}

		#endregion

        private IFrameBuffer renderToBuffer(IRoom room)
		{
            TypedParameter sizeParam = new TypedParameter(typeof(Size), _game.Settings.WindowSize);
            IFrameBuffer frameBuffer = _resolver.Container.Resolve<IFrameBuffer>(sizeParam);
			frameBuffer.Begin();
			renderRoom(room);
			frameBuffer.End();
			return frameBuffer;
		}

		private void renderRoom(IRoom room)
		{
			List<IObject> displayList = getDisplayList(room);
            SortDebugger.DebugIfNeeded(displayList);

			foreach (IObject obj in displayList) 
			{
				renderObject(room, obj);
			}
		}

		private void renderObject(IRoom room, IObject obj)
		{
            Size resolution = obj.RenderLayer == null || obj.RenderLayer.IndependentResolution == null ? 
                _game.Settings.VirtualResolution :
                obj.RenderLayer.IndependentResolution.Value;
            _glUtils.AdjustResolution(resolution.Width, resolution.Height);

            IImageRenderer imageRenderer = getImageRenderer(obj);

			imageRenderer.Prepare(obj, obj, room.Viewport);

			var shader = applyObjectShader(obj);

			imageRenderer.Render (obj, room.Viewport);

			removeObjectShader(shader);
		}

		private static IShader applyObjectShader(IObject obj)
		{
			var shader = obj.Shader;
			if (shader != null) shader = shader.Compile();
			if (shader != null) shader.Bind();
			return shader;
		}

		private void removeObjectShader(IShader shader)
		{
			if (shader == null) return;

			if (_lastShaderUsed != null) _lastShaderUsed.Bind();
			else shader.Unbind();
		}

		private void activateShader()
		{
			var shader = AGSGame.Shader;
			if (shader != null) shader = shader.Compile();
			if (shader == null)
			{
				if (_lastShaderUsed != null) _lastShaderUsed.Unbind();
				return;
			}
			_lastShaderUsed = shader;
			shader.Bind();
		}

		private IImageRenderer getImageRenderer(IObject obj)
		{
			return obj.CustomRenderer ?? getAnimationRenderer(obj) ?? _renderer;
		}

		private IImageRenderer getAnimationRenderer(IObject obj)
		{
			if (obj.Animation == null) return null;
			return obj.Animation.Sprite.CustomRenderer;
		}

		private void addCursor(List<IObject> displayList, IRoom room)
		{
			IObject cursor = _input.Cursor;
			if (cursor == null) return;
			if (_mouseCursorContainer == null || _mouseCursorContainer.Animation != cursor.Animation)
			{
                _mouseCursorContainer = cursor;
			}
			_mouseCursorContainer.X = (_input.MouseX - room.Viewport.X) * room.Viewport.ScaleX;
			_mouseCursorContainer.Y = (_input.MouseY - room.Viewport.Y) * room.Viewport.ScaleY;
			addToDisplayList(displayList, _mouseCursorContainer, room);
		}

		private List<IObject> getDisplayList(IRoom room)
		{
			int count = 1 + room.Objects.Count + _gameState.UI.Count;

			List<IObject> displayList = new List<IObject> (count);

			if (room.Background != null)
				addToDisplayList(displayList, room.Background);

			foreach (IObject obj in room.Objects) 
			{
				if (!room.ShowPlayer && obj == _gameState.Player) 
					continue;
				addToDisplayList(displayList, obj, room);
			}

			foreach (var area in room.Areas) addDebugDrawArea(displayList, area, room);
            foreach (var area in room.Areas)
			{
                if (!area.Enabled || room.Background == null || room.Background.Image == null) continue;
                IObject drawable = _walkBehinds.GetDrawable(area, room.Background.Image.OriginalBitmap);
                if (drawable == null) continue;
                addToDisplayList(displayList, drawable, room);
			}

			foreach (IObject ui in _gameState.UI)
			{
				addToDisplayList(displayList, ui, room);
			}

			displayList.Sort(_comparer);
			addCursor(displayList, room);
			return displayList;
		}

		private void addDebugDrawArea(List<IObject> displayList, IArea area, IRoom room)
		{
			if (area.Mask.DebugDraw == null) return;
			addToDisplayList(displayList, area.Mask.DebugDraw, room);
		}

		private void addToDisplayList(List<IObject> displayList, IObject obj, IRoom room)
		{
			if (!obj.Visible)
			{
				IImageRenderer imageRenderer = getImageRenderer(obj);

				imageRenderer.Prepare(obj, obj, room.Viewport);
				return;
			}

            addToDisplayList(displayList, obj);
		}

        private void addToDisplayList(List<IObject> displayList, IObject obj)
        {
            obj.Properties.Ints.SetValue(RenderOrderSelector.SortDefaultIndex, displayList.Count);
            displayList.Add(obj);
        }
	}
}

