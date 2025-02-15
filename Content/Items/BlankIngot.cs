using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProceduralOres.Content.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ProceduralOres.Content.Items;

public class BlankIngot : ModItem
{
    private static readonly Dictionary<int, Texture2D> CustomTextures = new();
    public int SourceItemType { get; private set; }
    private string SourceItemName { get; set; }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BlankIngot_Placed>(), SourceItemType);
        Item.width = 30;
        Item.height = 24;
        Item.maxStack = 999;
        Item.value = 0;
        Item.rare = ItemRarityID.White;
    }


    public void SetSourceItem(Item sourceItem)
    {
        SourceItemType = sourceItem.type;
        SourceItemName = sourceItem.Name;
        Item.SetNameOverride($"{SourceItemName} Bar");
        Item.rare = sourceItem.rare;
        Item.value = sourceItem.value * 5;

        GenerateCustomTexture();
    }

    private void GenerateCustomTexture()
    {
        Texture2D newTexture = null;
        try
        {
            var device = Main.graphics.GraphicsDevice;

            var baseTexture = TextureAssets.Item[Item.type].Value;
            var baseColors = new Color[baseTexture.Width * baseTexture.Height];
            baseTexture.GetData(baseColors);
            var baseWidth = baseTexture.Width;
            var baseHeight = baseTexture.Height;

            var proceduralTex = ProceduralTextures.GetTexFromItem(new Item(SourceItemType));
            var proceduralColors = new Color[proceduralTex.Width * proceduralTex.Height];
            proceduralTex.GetData(proceduralColors);
            var procWidth = proceduralTex.Width;
            var procHeight = proceduralTex.Height;

            newTexture = new Texture2D(device, baseWidth, baseHeight);
            var newColors = new Color[baseWidth * baseHeight];

            for (var y = 0; y < baseHeight; y += 2)
            for (var x = 0; x < baseWidth; x += 2)
            {
                var index = y * baseWidth + x;
                if (baseColors[index].A == 0)
                {
                    newColors[index] = Color.Transparent;
                    newColors[index + 1] = Color.Transparent;
                    newColors[index + baseWidth] = Color.Transparent;
                    newColors[index + baseWidth + 1] = Color.Transparent;
                    continue;
                }

                var grayscale = (baseColors[index].R + baseColors[index].G + baseColors[index].B) / (3f * 255f);
                var procX = x * procWidth / baseWidth;
                var procY = y * procHeight / baseHeight;
                var proceduralColor = proceduralColors[procY * procWidth + procX];

                var r = proceduralColor.R / 255f * grayscale;
                var g = proceduralColor.G / 255f * grayscale;
                var b = proceduralColor.B / 255f * grayscale;
                var finalColor = new Color(r, g, b, baseColors[index].A / 255f);

                newColors[index] = finalColor;
                newColors[index + 1] = finalColor;
                newColors[index + baseWidth] = finalColor;
                newColors[index + baseWidth + 1] = finalColor;
            }

            newTexture.SetData(newColors);

            CustomTextures[SourceItemType] = newTexture;
            newTexture = null;
        }
        catch (Exception e)
        {
            Main.NewText($"Error generating texture: {e.Message}", Color.Red);
        }
        finally
        {
            newTexture?.Dispose();
        }
    }


    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation,
        ref float scale, int whoAmI)
    {
        if (SourceItemType == 0) return true;

        if (!CustomTextures.TryGetValue(SourceItemType, out var texture))
        {
            GenerateCustomTexture();
            return true;
        }

        Main.GetItemDrawFrame(Item.type, out _, out var itemFrame);
        var drawOrigin = itemFrame.Size() / 2f;
        var drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

        spriteBatch.Draw(
            texture,
            drawPosition,
            itemFrame,
            lightColor,
            rotation,
            drawOrigin,
            scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }


    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor, Vector2 origin, float scale)
    {
        if (SourceItemType == 0) return true;

        if (!CustomTextures.TryGetValue(SourceItemType, out var texture))
        {
            GenerateCustomTexture();
            return true;
        }

        spriteBatch.Draw(
            texture,
            position,
            frame,
            drawColor,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }

    public override void SaveData(TagCompound tag)
    {
        if (SourceItemType != 0)
        {
            tag["SourceItemType"] = SourceItemType;
            tag["SourceItemName"] = SourceItemName;
        }
    }

    public override void LoadData(TagCompound tag)
    {
        SourceItemType = tag.GetInt("SourceItemType");
        SourceItemName = tag.GetString("SourceItemName");
        if (SourceItemType != 0)
        {
            Item.SetNameOverride($"{SourceItemName} Bar");
            GenerateCustomTexture();
        }
    }

    public override void Unload()
    {
        CustomTextures.Clear();
    }

    public override void OnCreated(ItemCreationContext context)
    {
        if (context is RecipeItemCreationContext con) SetSourceItem(con.ConsumedItems[0]);
    }

    public override bool CanStack(Item source)
    {
        var that = source.ModItem as BlankIngot;
        return that!.SourceItemType == SourceItemType;
    }

    public override bool CanStackInWorld(Item source)
    {
        var that = source.ModItem as BlankIngot;
        return that!.SourceItemType == SourceItemType;
    }
}

public class BlankIngotRecipeSystem : ModSystem
{
    public override void AddRecipes()
    {
        for (var i = 1; i < ItemLoader.ItemCount; i++)
        {
            var item = new Item(i);
            if (item.IsAir ||
                item.ModItem is BlankIngot)
                continue;

            var sourceItem = ContentSamples.ItemsByType[i];
            var craftAmount = CalculateCraftingAmount(sourceItem.value, sourceItem.rare);

            var recipe = Recipe.Create(ModContent.ItemType<BlankIngot>());
            recipe.AddIngredient(i, craftAmount);

            if (i != ItemID.Furnace)
                recipe.AddTile<Infinifurnace>();
            else
                recipe.AddTile(TileID.Furnaces);

            recipe.Register();
            var ingot = recipe.createItem.ModItem as BlankIngot;
            ingot!.SetSourceItem(ContentSamples.ItemsByType[i]);
        }
    }


    private static int CalculateCraftingAmount(int value, int rare)
    {
        float rarityFactor = Math.Max(1, 5 - rare);
        var valueFactor = Math.Max(1, (float)Math.Ceiling(Math.Sqrt(value / 1000f)));
        var baseAmount = (int)Math.Ceiling(20f / (valueFactor * rarityFactor));

        return Math.Min(Math.Max(baseAmount, 1), 99);
    }
}