using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingTrawler.Objects
{
    internal class Murphy : NPC
    {
        public Murphy() : base()
        {

        }

        public Murphy(AnimatedSprite sprite, Vector2 position, int facingDir, string name, Texture2D portrait, LocalizedContentManager content = null) : base(sprite, position, facingDir, name, content)
        {
            this.Portrait = portrait;
        }

        internal void DisplayDialogue(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            if (!who.hasOrWillReceiveMail("FishingTrawler_introductionsMurphy"))
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Introduction", playerTerm), this));

                Game1.addMailForTomorrow("FishingTrawler_introductionsMurphy", true);
                return;
            }

            Game1.drawDialogue(ModEntry.murphyNPC);
        }

        private string GetDialogue(string dialoguePath, string dialogueTitle, object title = null)
        {
            return Game1.content.LoadString(String.Concat(dialoguePath, ":", dialogueTitle), title);
        }
    }
}
