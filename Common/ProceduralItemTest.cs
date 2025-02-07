using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

public class ProceduralItemTest : GlobalItem
{
    public override bool InstancePerEntity => true;
    private bool ProceduralDraw;

    public override void UpdateInventory(Item item, Player player)
    {
        ProceduralDraw = false;
        if (item == player.HeldItem && Main.keyState.IsKeyDown(Keys.LeftShift))
        {
            ProceduralDraw = true;
        }
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {

        if (!ProceduralDraw)
            return true;

        try
        {
            Texture2D tex = ProceduralTextures.GetTexFromItem(item);


            spriteBatch.Draw(
                tex,
                position,
                null,
                Color.White,
                0f,
                Vector2.Zero,  
                1f,
                SpriteEffects.None,
                0f
            );

            return false;
        }
        catch (Exception e)
        {
            Main.NewText($"Draw error: {e.Message}");
            return true;
        }
    }
}