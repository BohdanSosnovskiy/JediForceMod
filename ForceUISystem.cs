using JediForceMod.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace JediForceMod
{
    public class ForceUISystem : ModSystem
    {
        internal UserInterface ForceUserInterface;
        internal ForceUI MyForceUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                MyForceUI = new ForceUI();
                ForceUserInterface = new UserInterface();
                ForceUserInterface.SetState(MyForceUI);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (ForceUI.Visible)
            {
                ForceUserInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "JediForceMod: Force UI",
                    delegate {
                        if (ForceUI.Visible)
                        {
                            ForceUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
