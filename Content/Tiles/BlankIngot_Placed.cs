using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProceduralOres.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ProceduralOres.Content.Tiles;

public class BlankIngot_Placed : ModTile
{
    private static readonly Dictionary<int, Texture2D> CustomTextures = new();

    public override void SetStaticDefaults()
    {
        Main.tileShine[Type] = 1100;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.MetalBar"));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Main.tile[i, j];
        int sourceItemType = tile.TileFrameX;
        if (!CustomTextures.TryGetValue(sourceItemType, out var texture))
        {
            GenerateCustomTexture(sourceItemType);

            return false;
        }

        spriteBatch.Draw(texture, new Vector2(i + 12, j + 12) * 16 - Main.screenPosition,
            Lighting.GetColor(new Point(i, j)));

        return false;
    }


    private static void GenerateCustomTexture(int sourceItemType)
    {
        try
        {
            var device = Main.graphics.GraphicsDevice;
            var baseTexture = ModContent.Request<Texture2D>("ProceduralOres/Content/Tiles/BlankIngot_Placed").Value;
            var baseColors = new Color[baseTexture.Width * baseTexture.Height];
            baseTexture.GetData(baseColors);
            var baseWidth = baseTexture.Width;
            var baseHeight = baseTexture.Height;

            var proceduralTex = ProceduralTextures.GetTexFromItem(new Item(sourceItemType));
            var proceduralColors = new Color[proceduralTex.Width * proceduralTex.Height];
            proceduralTex.GetData(proceduralColors);
            var procWidth = proceduralTex.Width;
            var procHeight = proceduralTex.Height;

            var newTexture = new Texture2D(device, baseWidth, baseHeight);
            var newColors = new Color[baseWidth * baseHeight];

            for (var y = 0; y < baseHeight; y += 2)
            for (var x = 0; x < baseWidth; x += 2)
            {
                var index = y * baseWidth + x;
                var grayscale = (baseColors[index].R + baseColors[index].G + baseColors[index].B) / (3f * 255f);
                var procX = x * procWidth / baseWidth;
                var procY = y * procHeight / baseHeight;
                var proceduralColor = proceduralColors[procY * procWidth + procX];

                var r = proceduralColor.R / 255f * grayscale;
                var g = proceduralColor.G / 255f * grayscale;
                var b = proceduralColor.B / 255f * grayscale;
                newColors[index] = new Color(r, g, b, baseColors[index].A / 255f);
                newColors[index + 1] = new Color(r, g, b, baseColors[index].A / 255f);
                newColors[index + baseWidth] = new Color(r, g, b, baseColors[index].A / 255f);
                newColors[index + baseWidth + 1] = new Color(r, g, b, baseColors[index].A / 255f);
            }

            newTexture.SetData(newColors);
            CustomTextures[sourceItemType] = newTexture;
        }
        catch (Exception e)
        {
            Main.NewText($"Error generating texture: {e.Message}", Color.Red);
        }
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        var bar = Main.LocalPlayer.HeldItem.ModItem as BlankIngot;
        var sourceItem = bar!.SourceItemType;
        var tile = Main.tile[i, j];
        tile.TileFrameX = (short)sourceItem;
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        var t = Main.tile[i, j];
        var itemNew = new Item(ModContent.ItemType<BlankIngot>());
        var ingot = itemNew.ModItem as BlankIngot;
        ingot!.SetSourceItem(new Item(t.TileFrameX));
        yield return itemNew;
    }
}