using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.ID;
using ProceduralOres.Content.Items;

namespace ProceduralOres.Content.Tiles
{
    public class BlankIngot_Placed : ModTile
    {
        private static Dictionary<int, Texture2D> CustomTextures = new();

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
           

            Tile tile = Main.tile[i, j];
            int sourceItemType = tile.TileFrameX;
            if (!CustomTextures.ContainsKey(sourceItemType))
            {
                GenerateCustomTexture(sourceItemType);

                return false;
            }

            spriteBatch.Draw(CustomTextures[sourceItemType], (new Vector2(i + 12, j + 12) * 16) - Main.screenPosition, Lighting.GetColor(new(i, j)));

            return false;
        }
        

        private void GenerateCustomTexture(int sourceItemType)
        {
            try
            {
                GraphicsDevice device = Main.graphics.GraphicsDevice;
                Texture2D baseTexture = ModContent.Request<Texture2D>("ProceduralOres/Content/Tiles/BlankIngot_Placed").Value;
                Color[] baseColors = new Color[baseTexture.Width * baseTexture.Height];
                baseTexture.GetData(baseColors);
                int baseWidth = baseTexture.Width;
                int baseHeight = baseTexture.Height;

                Texture2D proceduralTex = ProceduralTextures.GetTexFromItem(new Item(sourceItemType));
                Color[] proceduralColors = new Color[proceduralTex.Width * proceduralTex.Height];
                proceduralTex.GetData(proceduralColors);
                int procWidth = proceduralTex.Width;
                int procHeight = proceduralTex.Height;

                Texture2D newTexture = new Texture2D(device, baseWidth, baseHeight);
                Color[] newColors = new Color[baseWidth * baseHeight];

                for (int y = 0; y < baseHeight; y += 2)
                {
                    for (int x = 0; x < baseWidth; x += 2)
                    {
                        int index = y * baseWidth + x;
                        float grayscale = (baseColors[index].R + baseColors[index].G + baseColors[index].B) / (3f * 255f);
                        int procX = (x * procWidth) / baseWidth;
                        int procY = (y * procHeight) / baseHeight;
                        Color proceduralColor = proceduralColors[procY * procWidth + procX];

                        float r = proceduralColor.R / 255f * grayscale;
                        float g = proceduralColor.G / 255f * grayscale;
                        float b = proceduralColor.B / 255f * grayscale;
                        newColors[index] = new Color(r, g, b, baseColors[index].A / 255f);
                        newColors[index + 1] = new Color(r, g, b, baseColors[index].A / 255f);
                        newColors[index + baseWidth] = new Color(r, g, b, baseColors[index].A / 255f);
                        newColors[index + baseWidth + 1] = new Color(r, g, b, baseColors[index].A / 255f);
                    }
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
            var bar= Main.LocalPlayer.HeldItem.ModItem as BlankIngot;
            int sourceItem = bar.SourceItemType;
            Tile tile = Main.tile[i, j];
            tile.TileFrameX = (short)sourceItem;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
            }
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            Tile t = Main.tile[i, j];
            var itemNew = new Item(ModContent.ItemType<BlankIngot>());
            var ingot = itemNew.ModItem as BlankIngot;
            ingot.SetSourceItem(new Item((int)t.TileFrameX));
            yield return itemNew;
        }

    }
}
