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

            if (!who.modData.ContainsKey(ModEntry.MURPHY_GREETED_TODAY_KEY))
            {
                who.modData.Add(ModEntry.MURPHY_GREETED_TODAY_KEY, "false");
            }

            // TODO: Need to create letter from Willy and add it as condition for Murphy to appear.
            if (!who.hasOrWillReceiveMail("FishingTrawler_introductionsMurphy"))
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Introduction", playerTerm), this));
                Game1.drawDialogue(this);

                who.modData[ModEntry.MURPHY_GREETED_TODAY_KEY] = "true";
                Game1.addMailForTomorrow("FishingTrawler_introductionsMurphy", true);
            }
            else if (who.modData[ModEntry.MURPHY_GREETED_TODAY_KEY].ToLower() == "false")
            {
                if (Game1.isRaining || Game1.isSnowing)
                {
                    this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Greeting_Rainy", playerTerm), this));
                    Game1.drawDialogue(this);
                }
                else
                {
                    this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Greeting", playerTerm), this));
                    Game1.drawDialogue(this);
                    Game1.afterDialogues = AskQuestionAfterGreeting;
                }

                who.modData[ModEntry.MURPHY_GREETED_TODAY_KEY] = "true";
            }
            else if (who.modData[ModEntry.MURPHY_GREETED_TODAY_KEY].ToLower() == "true")
            {
                // Show questions
                AskQuestionAfterGreeting();
            }

            // TODO: Implement dialogue for flag identification / rewards
        }

        private void ConfirmFirstTrip(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));
            Response[] answers = new Response[2]
            {
                new Response("YesExplain", "Yes"),
                new Response("NoExplain", "No")
            };

            this.currentLocation.createQuestionDialogue(GetDialogue(ModResources.murphyDialoguePath, "Confirm_First_Trip", playerTerm), answers, OnPlayerResponse, this);
        }

        private void ExplainMinigame(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            this.CurrentDialogue.Push(new Dialogue(GetMinigameExplanation(playerTerm), this));
            Game1.drawDialogue(this);

            if (!who.hasOrWillReceiveMail("FishingTrawler_minigameExplanation"))
            {
                Game1.addMailForTomorrow("FishingTrawler_minigameExplanation", true);
            }
        }

        private string GetDialogue(string dialoguePath, string dialogueTitle, object title = null)
        {
            return Game1.content.LoadString(String.Concat(dialoguePath, ":", dialogueTitle), title);
        }

        private string GetMinigameExplanation(object title = null)
        {
            return String.Concat(GetDialogue(ModResources.murphyDialoguePath, "Minigame_Explanation_Hull", title), GetDialogue(ModResources.murphyDialoguePath, "Minigame_Explanation_Bailing", title), GetDialogue(ModResources.murphyDialoguePath, "Minigame_Explanation_Nets", title), GetDialogue(ModResources.murphyDialoguePath, "Minigame_Explanation_Engine", title), GetDialogue(ModResources.murphyDialoguePath, "Minigame_Explanation_Finish", title));
        }

        private void AskQuestionAfterGreeting()
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (Game1.player.IsMale ? "Male" : "Female"));

            Response[] answers = new Response[3]
            {
                new Response("StartTrip", "I'm ready to set sail!"),
                new Response("MinigameExplanation", "What does a deckhand do?"),
                new Response("NoDeparture", "Maybe another time.")
            };

            this.currentLocation.createQuestionDialogue(GetDialogue(ModResources.murphyDialoguePath, "Options", playerTerm), answers, OnPlayerResponse, this);
        }

        private void StartDepartureDialogue(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (Game1.player.IsMale ? "Male" : "Female"));
            if (who.freeSpotsInInventory() == 0)
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Full_Inventory", playerTerm), this));
                Game1.drawDialogue(this);
                return;
            }

            this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Start_Departure", playerTerm), this));
            Game1.afterDialogues = delegate () { ModEntry.trawlerObject.StartDeparture(); };
            Game1.drawDialogue(this);
        }

        private void OnPlayerResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "StartTrip":
                    if (!who.hasOrWillReceiveMail("FishingTrawler_minigameExplanation"))
                    {
                        Game1.afterDialogues = delegate () { this.ConfirmFirstTrip(who); };
                        Game1.addMailForTomorrow("FishingTrawler_minigameExplanation", true);
                    }
                    else
                    {
                        // Start trip
                        this.StartDepartureDialogue(who);
                    }
                    break;
                case "MinigameExplanation":
                case "YesExplain":
                    this.ExplainMinigame(who);
                    break;
                case "NoExplain":
                    // Start trip
                    this.StartDepartureDialogue(who);
                    break;
            }
        }
    }
}
