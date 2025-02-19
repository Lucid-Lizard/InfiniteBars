using ProceduralOres.Content.GUI;
using Terraria.ModLoader;

namespace ProceduralOres.Core;

public class ProceduralSystem : ModSystem
{
    public override void PreSaveAndQuit()
    {
        InfinifurnaceUI.ReturnInputItem();
    }
}