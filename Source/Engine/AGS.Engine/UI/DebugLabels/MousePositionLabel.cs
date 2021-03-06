﻿using System;
using AGS.API;

namespace AGS.Engine
{
	public class MousePositionLabel
	{
		private ILabel _label;
		private IGame _game;

		public MousePositionLabel(IGame game, ILabel label)
		{
			_label = label;
			_game = game;
		}

		public void Start()
		{
			_game.Input.MouseMove.Subscribe(onMouseMove);
			_game.Events.OnSavedGameLoad.Subscribe((sender, e) => onSaveGameLoaded());
		}

		private void onSaveGameLoaded()
		{
			_label = _game.Find<ILabel>(_label.ID);
		}

		private void onMouseMove(object sender, MousePositionEventArgs args)
		{
			_label.Text = new PointF (args.X, args.Y).ToString();
		}
	}
}

