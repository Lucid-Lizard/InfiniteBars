using System.Collections.Generic;
using InfiniTerraria.Content.GUI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProceduralOres.Content.Items;
using ProceduralOres.Core.Loaders.UILoading;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ProceduralOres.Content.GUI;

public class InfinifurnaceUI : SmartUIState
{
    private static readonly Asset<Texture2D> BackgroundTexture;
    private static readonly Rectangle FurnaceFrameDeactivated = new(0, 0, 96, 68);
    private static readonly Rectangle FurnaceFrameActivated = new(0, 68, 96, 68);

    private static readonly LocalizedText SmeltingHintText =
        Language.GetText("Mods.ProceduralOres.GUI.Infinifurnace.SmeltingHint");

    private static readonly LocalizedText RequiredHintText =
        Language.GetText("Mods.ProceduralOres.GUI.Infinifurnace.RequiredHint");

    private static readonly Item EmptyItem;
    private static DraggableUIPanel _panel;
    private static UIText _hintText;
    private static UIImageFramed _background;
    private static CustomItemSlot _outputSlot;
    private static CustomItemSlot _inputSlot;

    static InfinifurnaceUI()
    {
        EmptyItem = new Item();
        EmptyItem.SetDefaults();

        if (!Main.dedServ)
            BackgroundTexture = ModContent.Request<Texture2D>("ProceduralOres/Assets/Textures/InfinifurnaceUI");
    }

    private static bool IsActive { get; set; }

    public override bool Visible => IsActive;

    public override int InsertionIndex(List<GameInterfaceLayer> layers)
    {
        return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Radial Hotbars"));
    }

    public static void ToggleActive()
    {
        SetActive(!IsActive);
    }

    private static void SetActive(bool active)
    {
        if (active)
            UILoader.GetUIState<InfinifurnaceUI>().Update(null);
        else
            ReturnInputItem();

        IsActive = active;
    }

    public override void OnInitialize()
    {
        _panel = new DraggableUIPanel
        {
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent
        };
        _panel.Width.Set(96, 0);
        _panel.Height.Set(68, 0);
        _panel.SetPadding(0);

        _background = new UIImageFramed(BackgroundTexture, FurnaceFrameDeactivated);
        _panel.Append(_background);

        _inputSlot = new InfinifurnaceItemSlot();
        _inputSlot.Left.Set(10, 0);
        _inputSlot.Top.Set(28, 0);
        _inputSlot.ItemChanged += InputSlotOnItemChanged;
        _panel.Append(_inputSlot);

        _outputSlot = new InfinifurnaceItemSlot();
        _outputSlot.Left.Set(58, 0);
        _outputSlot.Top.Set(28, 0);
        _outputSlot.TakeOnly = true;
        _outputSlot.ItemChanged += OutputSlotOnItemChanged;
        _panel.Append(_outputSlot);

        _hintText = new UIText(SmeltingHintText)
        {
            HAlign = 0.5f
        };
        _hintText.Top.Set(12, 1f);
        _panel.Append(_hintText);

        Append(_panel);
    }

    private static void InputSlotOnItemChanged(CustomItemSlot slot, ItemChangedEventArgs e)
    {
        var newInputItem = e.NewItem;
        if (!newInputItem.IsAir)
        {
            var curStack = newInputItem.stack;
            var craftingAmount = BlankIngotRecipeSystem.CalculateCraftingAmount(newInputItem.value, newInputItem.rare);
            _hintText.SetText(RequiredHintText.WithFormatArgs(curStack, craftingAmount));
            if (curStack >= craftingAmount)
            {
                _hintText.TextColor = new Color(253, 221, 3);
                _background.SetFrame(FurnaceFrameActivated);
                SetResultItem(newInputItem, curStack / craftingAmount);
            }
            else
            {
                _hintText.TextColor = Color.White;
                _background.SetFrame(FurnaceFrameDeactivated);
                ClearResultItem();
            }
        }
        else
        {
            Reset();
        }
    }

    private static void OutputSlotOnItemChanged(CustomItemSlot slot, ItemChangedEventArgs e)
    {
        var newStack = e.NewItem.stack;
        var oldStack = e.OldItem.stack;
        var stackDiff = oldStack - newStack;

        if (stackDiff == 0)
            return;

        var inputItem = _inputSlot.Item;
        var craftingAmount = BlankIngotRecipeSystem.CalculateCraftingAmount(inputItem.value, inputItem.rare);
        var newInputStack = inputItem.stack - craftingAmount * stackDiff;
        if (newInputStack <= 0)
        {
            _inputSlot.SetItem(EmptyItem);
        }
        else
        {
            inputItem.stack = newInputStack;
            _inputSlot.SetItem(inputItem);
        }
    }

    private static void SetResultItem(Item sourceItem, int resAmount)
    {
        var resultItem = new Item(ModContent.ItemType<BlankIngot>(), resAmount);
        (resultItem.ModItem as BlankIngot)!.SetSourceItem(sourceItem);
        _outputSlot.SetItem(resultItem, false);
    }

    private static void ClearResultItem()
    {
        _outputSlot.SetItem(EmptyItem, false);
    }

    private static void Reset()
    {
        _hintText.SetText(SmeltingHintText);
        _hintText.TextColor = Color.White;
        _background.SetFrame(FurnaceFrameDeactivated);
        ClearResultItem();
    }

    public static void ReturnInputItem()
    {
        if (_inputSlot.Item.IsAir)
            return;
        
        var player = Main.LocalPlayer;
        _inputSlot.Item.noGrabDelay = 0;
        player.GetItem(player.whoAmI, _inputSlot.Item, GetItemSettings.GetItemInDropItemCheck);
        _inputSlot.SetItem(EmptyItem, false);
        Reset();
    }

    public override void SafeUpdate(GameTime gameTime)
    {
        // Determines the panel's position if it hasn't been dragged
        if (_panel.Offset == Vector2.Zero)
        {
            _panel.Left.Set(Main.screenWidth / 2f - 48, 0);
            _panel.Top.Set(Main.screenHeight / 2f + 62, 0);
        }

        Recalculate();
    }
}

internal class InfinifurnaceItemSlot : CustomItemSlot
{
    public InfinifurnaceItemSlot()
    {
        ShouldDrawBackground = false;
        Scale = 28f / 52f;
    }
}