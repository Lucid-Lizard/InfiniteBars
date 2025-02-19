using ProceduralOres.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ProceduralOres.Content.Items;

public class Infinifurnace_Item : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 10;
        Item.useAnimation = 15;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Infinifurnace>();
    }

    public override void AddRecipes()
    {
        var recipe = Recipe.Create(ModContent.ItemType<Infinifurnace_Item>());
        recipe.AddIngredient(ModContent.ItemType<BlankIngot>(), 5);
        recipe.Register();
        recipe.AddTile(TileID.Anvils);
        var thing = recipe.requiredItem[0].ModItem as BlankIngot;
        thing!.SetSourceItem(new Item(ItemID.Furnace));
    }
}