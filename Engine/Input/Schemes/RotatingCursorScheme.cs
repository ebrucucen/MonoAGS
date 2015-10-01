﻿using System;
using AGS.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace AGS.Engine
{
	public class RotatingCursorScheme
	{
		private List<Cursor> _cursors = new List<Cursor>(10);
		private IGame _game;
		private bool _handlingClick;

		public const string WALK_MODE = "Walk";
		public const string LOOK_MODE = "Look";
		public const string INTERACT_MODE = "Interact";
		public const string WAIT_MODE = "Wait";

		private int _currentMode;

		public RotatingCursorScheme(IGame game, IAnimationContainer lookCursor, IAnimationContainer walkCursor,
			IAnimationContainer interactCursor, IAnimationContainer waitCursor)
		{
			RotatingEnabled = true;
			_game = game;

			if (walkCursor != null) AddCursor(WALK_MODE, walkCursor, true);
			if (lookCursor != null) AddCursor(LOOK_MODE, lookCursor, true);
			if (interactCursor != null) AddCursor(INTERACT_MODE, interactCursor, true);
			if (waitCursor != null) AddCursor(WAIT_MODE, waitCursor, false);
		}

		public bool RotatingEnabled { get; set; }

		public string CurrentMode 
		{ 
			get { return _cursors[_currentMode].Mode; }
			set 
			{
				int index = _cursors.FindIndex(c => c.Mode == value);
				if (index < 0)
				{
					Debug.WriteLine("Did not find cursor mode {0}", value);
					return;
				}
				_currentMode = index;
				setCursor();
			}
		}
			
		public void AddCursor(string mode, IAnimationContainer animation, bool rotating)
		{
			_cursors.Add(new Cursor (mode, animation, rotating));
		}

		public void Start()
		{
			setCursor();
			_game.Input.MouseDown.SubscribeToAsync(onMouseDown);
		}

		private void setCursor()
		{
			_game.Input.Cursor = _cursors[_currentMode].Animation;
		}

		private async Task onMouseDown(object sender, MouseButtonEventArgs e)
		{
			var state = _game.State;
			if (_handlingClick || !state.Player.Character.Enabled)
				return;

			if (e.Button == MouseButton.Left)
			{
				await onLeftMouseDown(e, state);
			}
			else if (e.Button == MouseButton.Right)
			{
				onRightMouseDown(e, state);
			}
		}

		private async Task onLeftMouseDown(MouseButtonEventArgs e, IGameState state)
		{
			string mode = CurrentMode;
			IObject hotspot = state.Player.Character.Room.GetObjectAt(e.X, e.Y);

			if (state.Player.Character.Inventory == null || 
				state.Player.Character.Inventory.ActiveItem == null)
			{
				if (hotspot != null && hotspot.Room == null) 
				{
					IInventoryItem inventoryItem = state.Player.Character.Inventory.Items.FirstOrDefault(
						i => i.Graphics == hotspot);
					if (inventoryItem != null)
					{
						if (mode != LOOK_MODE)
						{
							if (inventoryItem.ShouldInteract) mode = INTERACT_MODE;
							else
							{
								state.Player.Character.Inventory.ActiveItem = inventoryItem;
								_game.Input.Cursor = inventoryItem.CursorGraphics;
								return;
							}
						}
					}
					else return; //Blocking clicks when hovering UI objects
				}

				if (mode == WALK_MODE)
				{
					AGSLocation location = new AGSLocation (e.X, e.Y, state.Player.Character.Z);
					await state.Player.Character.WalkAsync(location).ConfigureAwait(true);
				}
				else if (mode != WAIT_MODE)
				{
					_handlingClick = true;
					try
					{
						if (hotspot == null) return;

						if (mode == LOOK_MODE)
						{
							await hotspot.Interactions.OnLook.InvokeAsync(this, new ObjectEventArgs (hotspot));
						}
						else if (mode == INTERACT_MODE)
						{
							await hotspot.Interactions.OnInteract.InvokeAsync(this, new ObjectEventArgs (hotspot));
						}
						else
						{
							await hotspot.Interactions.OnCustomInteract.InvokeAsync(this, new CustomInteractionEventArgs (hotspot, mode));
						}
					}
					finally
					{
						_handlingClick = false;
					}
				}
			}
			else if (hotspot != null)
			{
				if (hotspot.Room == null)
				{
					IInventoryItem inventoryItem = state.Player.Character.Inventory.Items.FirstOrDefault(
						                              i => i.Graphics == hotspot);
					if (inventoryItem != null)
					{

						await state.Player.Character.Inventory.OnCombination(state.Player.Character.Inventory.ActiveItem,
							inventoryItem).InvokeAsync(this, new InventoryCombinationEventArgs (
							state.Player.Character.Inventory.ActiveItem, inventoryItem));
					}
					return;
				}

				await hotspot.Interactions.OnInventoryInteract.InvokeAsync(this, new InventoryInteractEventArgs(hotspot,
					state.Player.Character.Inventory.ActiveItem));
			}
		}

		private void onRightMouseDown(MouseButtonEventArgs e, IGameState state)
		{
			if (!RotatingEnabled) return;

			IInventory inventory = state.Player.Character.Inventory;

			int startMode = _currentMode;
			Cursor cursor = _cursors[_currentMode];

			if (inventory != null && inventory.ActiveItem != null)
			{
				inventory.ActiveItem = null;
			}
			else
			{
				if (!cursor.Rotating) return;
			}

			do
			{
				_currentMode = (_currentMode + 1) % _cursors.Count;
				cursor = _cursors[_currentMode];
			} while (!cursor.Rotating && _currentMode != startMode);

			setCursor();
		}

		private class Cursor
		{
			public Cursor(string mode, IAnimationContainer animation, bool rotating)
			{
				Mode = mode;
				Animation = animation;
				Rotating = rotating;
			}

			public string Mode { get; private set; }
			public IAnimationContainer Animation { get; private set; }
			public bool Rotating { get; private set;}
		}
	}
}

