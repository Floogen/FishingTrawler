using FishingTrawler.Framework.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.NetworkInformation;
using FishingTrawler.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FishingTrawler.Framework.Objects.Items.Tools
{
    public enum AnimationState
    {
        Idle,
        Windup,
        Throw,
        Kneel,
        WaitAfterKneel,
        StartPullup,
        FinishPullup,
        ShowFish
    }

    public class Trident
    {
        internal static int caughtFishId;
        internal static float fishSize;
        internal static int fishQuality;
        internal static int fishCount;
        internal static bool isRecordSizeFish;

        internal static double cooldownTimer;
        internal static double animationTimer;
        internal static double displayTimer;

        internal static AnimationState animationState;
        internal static Vector2 targetPosition;
        internal static int oldFacingDirection;

        public static GenericTool CreateInstance()
        {
            var trident = new GenericTool(string.Empty, string.Empty, -1, 6, 6);
            trident.modData[ModDataKeys.TRIDENT_TOOL_KEY] = true.ToString();

            return trident;
        }

        public static bool IsValid(Tool tool)
        {
            if (tool is not null && tool.modData.ContainsKey(ModDataKeys.TRIDENT_TOOL_KEY))
            {
                return true;
            }

            return false;
        }

        private static void Reset(Farmer who)
        {
            caughtFishId = -1;
            animationState = AnimationState.Idle;

            who.forceCanMove();
        }

        public static bool Use(GameLocation location, int x, int y, Farmer who)
        {
            if (animationState is not AnimationState.Idle)
            {
                return false;
            }
            Reset(who);

            var standingPosition = who.getTileLocation();
            switch (who.FacingDirection)
            {
                case Game1.up:
                    standingPosition.Y += -1f;
                    break;
                case Game1.right:
                    standingPosition.X += 1f;
                    break;
                case Game1.down:
                    standingPosition.Y += 1f;
                    break;
                case Game1.left:
                    standingPosition.X += -1f;
                    break;
            }

            if (location.canFishHere() is false || string.IsNullOrEmpty(location.doesTileHaveProperty((int)standingPosition.X, (int)standingPosition.Y, "Water", "Back")) is true)
            {
                Game1.addHUDMessage(new HUDMessage("You need to be facing water to use the trident.", 3) { timeLeft = 1000f });
                return false;
            }
            targetPosition = standingPosition;

           // Check to see if there are fish in this location
           caughtFishId = GetRandomFishForLocation(location);
            if (caughtFishId == -1)
            {
                Game1.addHUDMessage(new HUDMessage("There are no fish here that can be caught with the trident.", 3) { timeLeft = 1000f });
                return false;
            }

            // Handle exhaustion
            if (who.Stamina <= 1f)
            {
                if (!who.isEmoting)
                {
                    who.doEmote(36);
                }

                return false;
            }

            float oldStamina = who.Stamina;
            who.Stamina -= 12f - (float)who.FishingLevel * 0.1f;
            who.checkForExhaustion(oldStamina);

            // Set required flags
            who.Halt();
            who.canReleaseTool = false;
            who.UsingTool = true;
            who.CanMove = false;

            // Set animation
            animationState = AnimationState.Idle;
            oldFacingDirection = who.FacingDirection;

            // Get the fish
            fishSize = GetFishSize(who, caughtFishId);
            fishQuality = GetFishQuality(who, fishSize);
            fishCount = Game1.random.NextDouble() < who.FishingLevel / 100f ? 2 : 1;
            isRecordSizeFish = who.caughtFish(caughtFishId, (int)fishSize, false, 1);

            // Give the experience
            who.gainExperience(1, Math.Max(1, (fishQuality + 1) * 3));

            //base.Update(who.FacingDirection, 0, who);
            return false;
        }

        private static int GetFishQuality(Farmer who, float fishSize)
        {
            int initialSize = ((!((double)fishSize < 0.33)) ? (((double)fishSize < 0.66) ? 1 : 2) : 0);

            return Math.Max(0, initialSize - 1);
        }

        private static float GetFishSize(Farmer who, int whichFish)
        {
            Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");

            int minFishSize = 0;
            int maxFishSize = 0;
            if (data.ContainsKey(whichFish))
            {
                string[] rawData = data[whichFish].Split('/');
                minFishSize = Convert.ToInt32(rawData[3]);
                maxFishSize = Convert.ToInt32(rawData[4]);
            }

            return minFishSize;
        }

        private static int GetRandomFishForLocation(GameLocation location)
        {
            List<int> eligibleFishIds = new List<int>();

            // Iterate through any valid locations to find the fish eligible for rewarding (fish need to be in season and player must have minimum level for it)
            Dictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            if (!locationData.ContainsKey(location.Name))
            {
                return -1;
            }

            string[] rawFishData = locationData[location.Name].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
            Dictionary<int, string> rawFishDataWithLocation = new Dictionary<int, string>();
            if (rawFishData.Length > 1)
            {
                for (int j = 0; j < rawFishData.Length; j += 2)
                {
                    rawFishDataWithLocation[Convert.ToInt32(rawFishData[j])] = rawFishData[j + 1];
                }
            }
            eligibleFishIds.AddRange(rawFishDataWithLocation.Keys.Where(i => !TrawlerRewards.forbiddenFish.Contains(i)).Distinct());

            if (eligibleFishIds.Count == 0)
            {
                return -1;
            }

            return eligibleFishIds[Game1.random.Next(eligibleFishIds.Count)];
        }

        public static void Update(Tool tool, GameTime gameTime, Farmer who)
        {
            bool isAnimationOver = true;
            if (animationTimer >= 0f)
            {
                isAnimationOver = false;
                animationTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                // Move animation to next state
                switch (animationState)
                {
                    case AnimationState.Idle:
                        animationState = AnimationState.Windup;
                        break;
                    case AnimationState.Windup:
                        animationState = AnimationState.Throw;
                        break;
                    case AnimationState.Throw:
                        animationState = AnimationState.Kneel;
                        break;
                    case AnimationState.Kneel:
                        animationState = AnimationState.WaitAfterKneel;
                        break;
                    case AnimationState.WaitAfterKneel:
                        animationState = AnimationState.StartPullup;
                        break;
                    case AnimationState.StartPullup:
                        animationState = AnimationState.FinishPullup;
                        break;
                    case AnimationState.FinishPullup:
                        animationState = AnimationState.ShowFish;
                        break;
                }
            }

            // Handle current AnimationState
            if (animationState is AnimationState.Windup && isAnimationOver is true)
            {
                animationTimer = 250f;
                who.FarmerSprite.setCurrentFrame(84);
            }
            else if (animationState is AnimationState.Throw && isAnimationOver is true)
            {
                animationTimer = 125f;
                who.FarmerSprite.setCurrentFrame(2);
            }
            else if (animationState is AnimationState.Kneel && isAnimationOver is true)
            {
                animationTimer = 125f;
                who.FarmerSprite.setCurrentFrame(5);

                who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, targetPosition * 64f, flicker: false, flipped: false));
            }
            else if (animationState is AnimationState.WaitAfterKneel && isAnimationOver is true)
            {
                animationTimer = 500f;
                who.FarmerSprite.setCurrentFrame(4);
            }
            else if (animationState is AnimationState.StartPullup && isAnimationOver is true)
            {
                animationTimer = 300f;
                who.FarmerSprite.setCurrentFrame(5);
            }
            else if (animationState is AnimationState.FinishPullup && isAnimationOver is true)
            {
                animationTimer = 300f;
                who.FarmerSprite.setCurrentFrame(25);
                //who.FarmerSprite.StopAnimation();
            }
            else if (animationState is AnimationState.ShowFish)
            {
                who.faceDirection(2);
                who.FarmerSprite.setCurrentFrame(84);

                displayTimer -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (displayTimer <= 0f && Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.didPlayerJustClickAtAll() || Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton))
                {
                    Reset(who);
                    tool.endUsing(who.currentLocation, who);

                    who.faceDirection(oldFacingDirection);
                }
            }
        }

        public static void Draw(SpriteBatch b, Farmer who)
        {
            if (animationState is AnimationState.ShowFish && caughtFishId > 0)
            {
                ReplicateVanillaFishDisplay(b, who);
            }
            else
            {
                switch (animationState)
                {
                    case AnimationState.Windup:
                        b.Draw(FishingTrawler.assetManager.tridentTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(52f, -48f)), new Rectangle(0, 0, 16, 16), Color.White * 0.8f, 2.35f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);
                        break;
                    case AnimationState.Throw:
                        b.Draw(FishingTrawler.assetManager.tridentTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(56f, 0f)), new Rectangle(0, 0, 16, 16), Color.White * 0.8f, 2.35f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);
                        break;
                    case AnimationState.Kneel:
                        b.Draw(FishingTrawler.assetManager.tridentTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(56f, 32f)), new Rectangle(16, 0, 16, 16), Color.White * 0.8f, 2.35f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);
                        break;
                    case AnimationState.WaitAfterKneel:
                        b.Draw(FishingTrawler.assetManager.tridentTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(56f, 32f)), new Rectangle(16, 0, 16, 16), Color.White * 0.8f, 2.35f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);
                        break;
                    case AnimationState.StartPullup:
                        b.Draw(FishingTrawler.assetManager.tridentTexture, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(56f, 32f)), new Rectangle(16, 0, 16, 16), Color.White * 0.8f, 2.35f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);
                        break;
                }
            }
        }

        private static void ReplicateVanillaFishDisplay(SpriteBatch b, Farmer who)
        {
            bool caughtDoubleFish = fishCount == 2;

            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -288f + yOffset)), new Rectangle(31, 1870, 73, 49), Color.White * 0.8f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.06f);

            b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-124f, -284f + yOffset) + new Vector2(44f, 68f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, Trident.caughtFishId, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.0001f + 0.06f);

            b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(0f, -56f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, Trident.caughtFishId, 16, 16), Color.White, ((float)Math.PI * 3f / 4f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.002f + 0.06f);
            if (caughtDoubleFish)
            {
                b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-8f, -56f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, Trident.caughtFishId, 16, 16), Color.White, ((float)Math.PI * 4f / 5f), new Vector2(8f, 8f), 3f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.002f + 0.058f);
            }

            string name = Game1.objectInformation[caughtFishId].Split('/')[4];
            b.DrawString(Game1.smallFont, name, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(26f - Game1.smallFont.MeasureString(name).X / 2f, -278f + yOffset)), Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.002f + 0.06f);
            if (fishSize != -1)
            {
                b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14082"), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(20f, -214f + yOffset)), Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.002f + 0.06f);
                b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize)), Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(85f - Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fishSize * 2.54) : ((double)fishSize))).X / 2f, -179f + yOffset)), isRecordSizeFish ? (Color.Blue * Math.Min(1f, yOffset / 8f + 1.5f)) : Game1.textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)who.getStandingY() / 10000f + 0.002f + 0.06f);
            }

            if (caughtDoubleFish)
            {
                Utility.drawTinyDigits(2, b, Game1.GlobalToLocal(Game1.viewport, who.Position + new Vector2(-120f, -284f + yOffset) + new Vector2(23f, 29f) * 4f), 3f, (float)who.getStandingY() / 10000f + 0.0001f + 0.061f, Color.White);
            }
        }
    }
}
