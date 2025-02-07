using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Terraria.UI;
using System.Linq;
using ProceduralOres.Content.Tiles;
using Terraria.GameContent.Creative;

namespace ProceduralOres.Content.Items
{ 
    public class BlankIngot : ModItem
    {
        public int SourceItemType { get; private set; }
        public string SourceItemName { get; private set; }
        private static Dictionary<int, Texture2D> CustomTextures = new Dictionary<int, Texture2D>();

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
                GraphicsDevice device = Main.graphics.GraphicsDevice;

                Texture2D baseTexture = TextureAssets.Item[Item.type].Value;
                Color[] baseColors = new Color[baseTexture.Width * baseTexture.Height];
                baseTexture.GetData(baseColors);
                int baseWidth = baseTexture.Width;
                int baseHeight = baseTexture.Height;

                Texture2D proceduralTex = ProceduralTextures.GetTexFromItem(new Item(SourceItemType));
                Color[] proceduralColors = new Color[proceduralTex.Width * proceduralTex.Height];
                proceduralTex.GetData(proceduralColors);
                int procWidth = proceduralTex.Width;
                int procHeight = proceduralTex.Height;

                newTexture = new Texture2D(device, baseWidth, baseHeight);
                Color[] newColors = new Color[baseWidth * baseHeight];

                for (int y = 0; y < baseHeight; y += 2)
                {
                    for (int x = 0; x < baseWidth; x += 2)
                    {
                        int index = y * baseWidth + x;
                        if (baseColors[index].A == 0)
                        {
                            newColors[index] = Color.Transparent;
                            newColors[index + 1] = Color.Transparent;
                            newColors[index + baseWidth] = Color.Transparent;
                            newColors[index + baseWidth + 1] = Color.Transparent;
                            continue;
                        }

                        float grayscale = (baseColors[index].R + baseColors[index].G + baseColors[index].B) / (3f * 255f);
                        int procX = (x * procWidth) / baseWidth;
                        int procY = (y * procHeight) / baseHeight;
                        Color proceduralColor = proceduralColors[procY * procWidth + procX];

                        float r = proceduralColor.R / 255f * grayscale;
                        float g = proceduralColor.G / 255f * grayscale;
                        float b = proceduralColor.B / 255f * grayscale;
                        Color finalColor = new Color(r, g, b, baseColors[index].A / 255f);

                        newColors[index] = finalColor;
                        newColors[index + 1] = finalColor;
                        newColors[index + baseWidth] = finalColor;
                        newColors[index + baseWidth + 1] = finalColor;
                    }
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




        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (SourceItemType == 0)
            {
                return true;
            }

            if (!CustomTextures.ContainsKey(SourceItemType))
            {
                GenerateCustomTexture();
                return true;
            }

            Texture2D texture = CustomTextures[SourceItemType];

            Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame);
            Vector2 drawOrigin = itemFrame.Size() / 2f;
            Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);

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


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (SourceItemType == 0)
            {
                return true;
            }

            if (!CustomTextures.ContainsKey(SourceItemType))
            {
                GenerateCustomTexture();
                return true;
            }

            spriteBatch.Draw(
                CustomTextures[SourceItemType],
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
            if(context is RecipeItemCreationContext con)
            {
                SetSourceItem(con.ConsumedItems[0]);
            }
        }

        public override bool CanStack(Item source)
        {
            BlankIngot that = source.ModItem as BlankIngot;
            return that.SourceItemType == this.SourceItemType;
        }

        public override bool CanStackInWorld(Item source)
        {
            BlankIngot that = source.ModItem as BlankIngot;
            return that.SourceItemType == this.SourceItemType;
        }
    }

    public class BlankIngotRecipeSystem : ModSystem
    {
        public override void AddRecipes()
        {
            for (int i = 1; i < ItemLoader.ItemCount; i++)
            {
                Item item = new Item(i);
                if (item.IsAir ||
                    item.ModItem is BlankIngot)
                    continue;

                var sourceItem = ContentSamples.ItemsByType[i];
                int craftAmount = CalculateCraftingAmount(sourceItem.value, sourceItem.rare);

                var recipe = Recipe.Create(ModContent.ItemType<Items.BlankIngot>());
                recipe.AddIngredient(i, craftAmount);

                if (i != ItemID.Furnace)
                {
                    recipe.AddTile<Infinifurnace>();
                }
                else
                {
                    recipe.AddTile(TileID.Furnaces);
                }

                recipe.Register();
                var ingot = recipe.createItem.ModItem as BlankIngot;
                ingot.SetSourceItem(ContentSamples.ItemsByType[i]);
            }
        }


        private int CalculateCraftingAmount(int value, int rare)
        {
            float rarityFactor = Math.Max(1, 5 - rare); 
            float valueFactor = Math.Max(1, (float)Math.Ceiling(Math.Sqrt(value / 1000f)));
            int baseAmount = (int)Math.Ceiling(20f / (valueFactor * rarityFactor));

            return Math.Min(Math.Max(baseAmount, 1), 99);
        }
    }

}