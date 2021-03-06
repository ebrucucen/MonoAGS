﻿using System;
using AGS.API;
using System.Threading.Tasks;


namespace AGS.Engine
{
	public class AGSGameLoop : IGameLoop
	{
		private IGameState _gameState;
		private AGS.API.Size _virtualResolution;
		private IRoom _lastRoom;
		private IAGSRoomTransitions _roomTransitions;

		public AGSGameLoop (IGameState gameState, AGS.API.Size virtualResolution, IAGSRoomTransitions roomTransitions)
		{
			this._gameState = gameState;
			this._virtualResolution = virtualResolution;
			this._roomTransitions = roomTransitions;
		}

		#region IGameLoop implementation

		public virtual async Task UpdateAsync()
		{
            if (_gameState.Room == null) return;
			IRoom room = _gameState.Room;
            bool changedRoom = _lastRoom != room;
			if (_roomTransitions.State != RoomTransitionState.NotInTransition)
			{
				if (_roomTransitions.State == RoomTransitionState.PreparingTransition)
				{
					if (changedRoom)
					{
						if (_lastRoom != null) _lastRoom.Events.OnAfterFadeOut.Invoke(this, new AGSEventArgs ());
						room.Events.OnBeforeFadeIn.Invoke(this, new AGSEventArgs ());
						updateViewport(room, changedRoom);
						if (_lastRoom == null) _roomTransitions.State = RoomTransitionState.NotInTransition;
						else _roomTransitions.State = RoomTransitionState.InTransition;
					}
				}
				return;
			}
			if (room.Background != null) runAnimation (room.Background.Animation);
			foreach (var obj in room.Objects) 
			{
				if (!obj.Visible)
					continue;
				if (!room.ShowPlayer && obj == _gameState.Player)
					continue;
				runAnimation (obj.Animation);
			}

			updateViewport (room, changedRoom);
			await updateRoom(room);
		}

		#endregion

		private void updateViewport (IRoom room, bool playerChangedRoom)
		{
			ICamera camera = room.Viewport.Camera;
			if (camera != null) 
			{
                camera.Tick(room.Viewport, room.Limits, _virtualResolution, playerChangedRoom);
			}
		}

        private async Task updateRoom(IRoom room)
		{
			if (_lastRoom == room) return;
            _lastRoom = room;
            await room.Events.OnAfterFadeIn.InvokeAsync(this, new AGSEventArgs ());
		}

		private void runAnimation(IAnimation animation)
		{
			if (animation == null || animation.State.IsPaused)
				return;
			if (animation.State.TimeToNextFrame < 0)
				return;
			if (_gameState.Cutscene.IsSkipping && animation.Configuration.Loops > 0)
			{
				animation.State.TimeToNextFrame = 0;
				while (animation.NextFrame()) ;
			}
			else
			{
				animation.State.TimeToNextFrame--;
				if (animation.State.TimeToNextFrame < 0)
					animation.NextFrame();
			}
		}
	}
}

