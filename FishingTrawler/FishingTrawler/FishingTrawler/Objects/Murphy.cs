using FishingTrawler.Objects.Rewards;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
            who.Halt();
            who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);

            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            if (!who.hasOrWillReceiveMail("FishingTrawler_IntroductionsMurphy"))
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Introduction", playerTerm), this));
                Game1.drawDialogue(this);

                who.modData[ModEntry.MURPHY_WAS_GREETED_TODAY_KEY] = "true";
                Game1.addMailForTomorrow("FishingTrawler_IntroductionsMurphy", true);
            }
            else if (who.modData[ModEntry.MURPHY_WAS_GREETED_TODAY_KEY].ToLower() == "false")
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

                who.modData[ModEntry.MURPHY_WAS_GREETED_TODAY_KEY] = "true";
            }
            else if (who.modData[ModEntry.MURPHY_WAS_GREETED_TODAY_KEY].ToLower() == "true" && who.modData[ModEntry.MURPHY_HAS_SEEN_FLAG_KEY].ToLower() == "false" && PlayerHasUnidentifiedFlagInInventory(who))
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Reward_Explanation_Flags", playerTerm), this));
                Game1.drawDialogue(this);
                Game1.afterDialogues = TakeAndIdentifyFlag;

                who.modData[ModEntry.MURPHY_HAS_SEEN_FLAG_KEY] = "true";
            }
            else if (who.modData[ModEntry.MURPHY_WAS_GREETED_TODAY_KEY].ToLower() == "true" && who.modData[ModEntry.MURPHY_SAILED_TODAY_KEY].ToLower() == "false")
            {
                // Show questions
                AskQuestionAfterGreeting();
            }
            else if (who.modData[ModEntry.MURPHY_SAILED_TODAY_KEY].ToLower() == "true" && who.modData[ModEntry.MURPHY_FINISHED_TALKING_KEY].ToLower() == "false")
            {
                string tripState = who.modData[ModEntry.MURPHY_WAS_TRIP_SUCCESSFUL_KEY].ToLower() == "true" ? "Successful" : "Failure";
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, String.Concat("After_Trip_", tripState), playerTerm), this));
                Game1.drawDialogue(this);

                who.modData[ModEntry.MURPHY_FINISHED_TALKING_KEY] = "true";
            }
            else if (who.modData[ModEntry.MURPHY_FINISHED_TALKING_KEY].ToLower() == "true")
            {
                this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Trip_Finished", playerTerm), this));
                Game1.drawDialogue(this);
            }
        }

        public override bool checkAction(Farmer who, GameLocation l)
        {
            if (who.CurrentItem != null && who.CurrentItem is AncientFlag ancientFlag)
            {
                tryToReceiveActiveObject(who);
                return true;
            }

            DisplayDialogue(who);
            return true;
        }

        public override void tryToReceiveActiveObject(Farmer who)
        {
            who.Halt();
            who.faceGeneralDirection(base.getStandingPosition(), 0, opposite: false, useTileCalculations: false);

            if (who.CurrentItem != null && who.CurrentItem is AncientFlag ancientFlag)
            {
                if (ancientFlag.flagType == FlagType.Unknown)
                {
                    return;
                }

                string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

                who.currentLocation.localSound("coin");
                who.reduceActiveItemByOne();

                if (ModEntry.GetHoistedFlag() == FlagType.Unknown)
                {
                    this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Given_Flag_To_Hoist", playerTerm), this));
                }
                else
                {
                    this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Given_Flag_To_Hoist_Return_Old", playerTerm), this));
                    Game1.player.addItemByMenuIfNecessary(new AncientFlag(ModEntry.GetHoistedFlag()));
                }

                Game1.drawDialogue(this);
                ModEntry.SetHoistedFlag(ancientFlag.flagType);
            }
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

            if (!who.hasOrWillReceiveMail("PeacefulEnd.FishingTrawler_minigameExplanation"))
            {
                Game1.addMailForTomorrow("PeacefulEnd.FishingTrawler_minigameExplanation", true);
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

            List<Response> answers = new List<Response>()
            {
                new Response("StartTrip", "I'm ready to set sail!")
            };

            if (PlayerHasUnidentifiedFlagInInventory(Game1.player))
            {
                answers.Add(new Response("IdentifyFlag", "I found another flag!"));
            }

            answers.Add(new Response("GotQuestion", "I've got some questions."));
            answers.Add(new Response("NoDeparture", "Maybe another time."));

            this.currentLocation.createQuestionDialogue(GetDialogue(ModResources.murphyDialoguePath, "Options", playerTerm), answers.ToArray(), OnPlayerResponse, this);
        }

        private void StartDepartureDialogue(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));
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

        private void HowToHoistDialogue(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "How_To_Hoist_Flag", playerTerm), this));
            Game1.drawDialogue(this);
        }

        private void WhatFlagIsHoisted(Farmer who)
        {
            string flagName = AncientFlag.GetFlagName(ModEntry.GetHoistedFlag()).Replace("The", "the");
            if (!flagName.Contains("the"))
            {
                flagName = String.Concat("the", " ", flagName);
            }
            this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "What_Flag_Is_Hoisted" + (ModEntry.GetHoistedFlag() == FlagType.Unknown ? "_None" : ""), flagName), this));
            Game1.drawDialogue(this);
        }

        private void RemoveHoistedFlag(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Remove_Current_Flag", playerTerm), this));
            Game1.drawDialogue(this);

            Game1.player.addItemByMenuIfNecessary(new AncientFlag(ModEntry.GetHoistedFlag()));
            ModEntry.SetHoistedFlag(FlagType.Unknown);
        }

        private void IdentifyFlag(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));

            this.CurrentDialogue.Push(new Dialogue(GetDialogue(ModResources.murphyDialoguePath, "Identify_Flag", playerTerm), this));
            Game1.drawDialogue(this);
            Game1.afterDialogues = TakeAndIdentifyFlag;
        }

        private void ShowMoreQuestions(Farmer who)
        {
            string playerTerm = GetDialogue(ModResources.murphyDialoguePath, "Player_" + (who.IsMale ? "Male" : "Female"));
            List<Response> answers = new List<Response>()
            {
                new Response("MinigameExplanation", "What does a deckhand do?")
            };

            if (PlayerHasIdentifiedFlagInInventory(Game1.player))
            {
                answers.Add(new Response("WantToHoist", "I'd like to hoist a flag."));
            }
            if (Game1.player.modData[ModEntry.MURPHY_HAS_SEEN_FLAG_KEY] == "true")
            {
                answers.Add(new Response("WhatFlag", "What flag is the trawler flying?"));
            }
            if (ModEntry.GetHoistedFlag() != FlagType.Unknown)
            {
                answers.Add(new Response("GetFlag", "Could I have my flag back?"));
            }

            answers.Add(new Response("NeverMind", "Actually never mind."));

            this.currentLocation.createQuestionDialogue(GetDialogue(ModResources.murphyDialoguePath, "More_Questions", playerTerm), answers.ToArray(), OnPlayerResponse, this);
        }

        private void OnPlayerResponse(Farmer who, string answer)
        {
            switch (answer)
            {
                case "StartTrip":
                    if (!who.hasOrWillReceiveMail("PeacefulEnd.FishingTrawler_minigameExplanation"))
                    {
                        Game1.afterDialogues = delegate () { this.ConfirmFirstTrip(who); };
                        Game1.addMailForTomorrow("PeacefulEnd.FishingTrawler_minigameExplanation", true);
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
                case "WantToHoist":
                    Game1.afterDialogues = delegate () { this.HowToHoistDialogue(who); };
                    break;
                case "WhatFlag":
                    Game1.afterDialogues = delegate () { this.WhatFlagIsHoisted(who); };
                    break;
                case "GetFlag":
                    Game1.afterDialogues = delegate () { this.RemoveHoistedFlag(who); };
                    break;
                case "IdentifyFlag":
                    Game1.afterDialogues = delegate () { this.IdentifyFlag(who); };
                    break;
                case "GotQuestion":
                    Game1.afterDialogues = delegate () { this.ShowMoreQuestions(who); };
                    break;
            }
        }

        private bool PlayerHasUnidentifiedFlagInInventory(Farmer who)
        {
            return who.items.Any(i => i != null && i.modData.ContainsKey(ModEntry.ANCIENT_FLAG_KEY) && i.modData[ModEntry.ANCIENT_FLAG_KEY] == FlagType.Unknown.ToString());
        }

        private bool PlayerHasIdentifiedFlagInInventory(Farmer who)
        {
            return who.items.Any(i => i != null && i.modData.ContainsKey(ModEntry.ANCIENT_FLAG_KEY) && i.modData[ModEntry.ANCIENT_FLAG_KEY] != FlagType.Unknown.ToString());
        }

        private void TakeAndIdentifyFlag()
        {
            if (!PlayerHasUnidentifiedFlagInInventory(Game1.player))
            {
                return;
            }

            // Get the count of possible flags
            int uniqueFlagTypes = Enum.GetNames(typeof(FlagType)).Length;

            // Remove the ancient flag, then add the randomly identified one
            AncientFlag ancientFlag = Game1.player.items.FirstOrDefault(i => i.modData.ContainsKey(ModEntry.ANCIENT_FLAG_KEY) && i.modData[ModEntry.ANCIENT_FLAG_KEY] == FlagType.Unknown.ToString()) as AncientFlag;

            Game1.player.removeItemFromInventory(ancientFlag);
            Game1.player.addItemByMenuIfNecessary(new AncientFlag((FlagType)Game1.random.Next(1, uniqueFlagTypes)));
        }
    }
}
