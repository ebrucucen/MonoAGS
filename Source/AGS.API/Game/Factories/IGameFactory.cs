﻿namespace AGS.API
{
    public interface IGameFactory
	{
		IGraphicsFactory Graphics { get; }
		IAudioFactory Sound { get; }
		IInventoryFactory Inventory { get; }
		IUIFactory UI { get; }
		IObjectFactory Object { get; }
		IRoomFactory Room { get; }
		IOutfitFactory Outfit { get; }
		IDialogFactory Dialog { get; }
	}
}
