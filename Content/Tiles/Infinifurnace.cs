using Microsoft.Xna.Framework;
using ProceduralOres.Content.GUI;
using ProceduralOres.Content.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ProceduralOres.Content.Tiles;

public class Infinifurnace : ModTile
{
    public override string HighlightTexture => "ProceduralOres/Content/Tiles/Infinifurnace_Highlight";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(255, 100, 0), Language.GetText("ItemName.Furnace"));
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Infinifurnace_Item>();
    }

    public override bool RightClick(int i, int j)
    {
        InfinifurnaceUI.ToggleActive();

        return true;
    }

    public override void NumDust(int x, int y, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}