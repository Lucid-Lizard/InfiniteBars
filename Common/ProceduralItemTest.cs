using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;

public class ProceduralItemTest : GlobalItem
{
    private bool _proceduralDraw;
    public override bool InstancePerEntity => true;

    public override void UpdateInventory(Item item, Player player)
    {
        _proceduralDraw = item == player.HeldItem && Main.keyState.IsKeyDown(Keys.LeftShift);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (!_proceduralDraw)
            return true;

        try
        {
            var tex = ProceduralTextures.GetTexFromItem(item);

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