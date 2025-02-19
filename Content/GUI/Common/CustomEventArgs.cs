using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace InfiniTerraria.Content.GUI.Common;

public delegate void ItemChangedEventHandler(CustomItemSlot slot, ItemChangedEventArgs e);

public delegate void ItemVisiblityChangedEventHandler(CustomItemSlot slot, ItemVisibilityChangedEventArgs e);

public delegate void PanelDragEventHandler(UIPanel sender, PanelDragEventArgs e);

public class PanelDragEventArgs(StyleDimension x, StyleDimension y) : EventArgs
{
    public readonly StyleDimension X = x;
    public readonly StyleDimension Y = y;
}

public class ItemChangedEventArgs(Item oldItem, Item newItem) : EventArgs
{
    public readonly Item NewItem = newItem;
    public readonly Item OldItem = oldItem;
}

public class ItemVisibilityChangedEventArgs(bool visibility) : EventArgs
{
    public readonly bool Visibility = visibility;
}