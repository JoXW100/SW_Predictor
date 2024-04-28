using Microsoft.Xna.Framework;
using StardewValley;

namespace Predictor.Framework.Extentions
{
    internal static class Game1Extentions
    {
        public static void Predict_createMultipleItemDebris(PredictionContext _ctx, PredictionItem? item, Vector2 pixelOrigin, int direction, GameLocation? location = null, int groundLevel = -1, bool flopFish = false)
        {
            int stack = 0;
            if (item != null)
            {
                stack = item.Stack;
                item.Stack = 1;
            }

            Predict_createItemDebris(_ctx, item, pixelOrigin, direction == -1 ? _ctx.Random.Next(4) : direction, location, groundLevel, flopFish);
            for (int i = 1; i < stack; i++)
            {
                Predict_createItemDebris(_ctx, item?.GetOne(), pixelOrigin, direction == -1 ? _ctx.Random.Next(4) : direction, location, groundLevel, flopFish);
            }
        }

        public static Debris? Predict_createItemDebris(PredictionContext _ctx, PredictionItem? item, Vector2 pixelOrigin, int direction, GameLocation? location = null, int groundLevel = -1, bool flopFish = false)
        {
            _ctx.AddItemIfNotNull(item);
            return null;
            /*
            if (location == null)
            {
                location = currentLocation;
            }
            Vector2 targetLocation = new Vector2(pixelOrigin.X, pixelOrigin.Y);
            switch (direction)
            {
                case 0:
                    pixelOrigin.Y -= 16f + recentMultiplayerRandom.Next(32);
                    targetLocation.Y -= 35.2f;
                    break;
                case 1:
                    pixelOrigin.X += 16f;
                    pixelOrigin.Y -= 32 - recentMultiplayerRandom.Next(8);
                    targetLocation.X += 128f;
                    break;
                case 2:
                    pixelOrigin.Y += recentMultiplayerRandom.Next(16);
                    targetLocation.Y += 64f;
                    break;
                case 3:
                    pixelOrigin.X -= 16f;
                    pixelOrigin.Y -= 32 - recentMultiplayerRandom.Next(8);
                    targetLocation.X -= 128f;
                    break;
                case -1:
                    targetLocation = player.getStandingPosition();
                    break;
            }

            Debris debris = new Debris(item, pixelOrigin, targetLocation);
            if (flopFish && item.Category == -4)
            {
                debris.floppingFish.Value = true;
            }

            if (groundLevel != -1)
            {
                debris.chunkFinalYLevel = groundLevel;
            }

            // location.debris.Add(debris);
            _ctx.Items.Add(item);
            return debris;
            */
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile);
            }
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number, GameLocation location)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile, -1, 0, 1f, location);
            }
        }

        public static void Predict_createMultipleObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int number, long who, GameLocation location)
        {
            for (int i = 0; i < number; i++)
            {
                Predict_createObjectDebris(_ctx, id, xTile, yTile, who, location);
            }
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, GameLocation location)
        {
            Predict_createObjectDebris(_ctx, id, xTile, yTile, -1, 0, 1f, location);
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, long whichPlayer, GameLocation location)
        {
            // location.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), getFarmer(whichPlayer).getStandingPosition()));
            // var debris = new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), Game1.getFarmer(whichPlayer).getStandingPosition());
            _ctx.AddItemIfNotNull(id);
        }

        public static void Predict_createObjectDebris(PredictionContext _ctx, string id, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation? location = null)
        {
            /*
            if (location is null)
            {
                location = currentLocation;
            }
            Debris debris = new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), player.getStandingPosition())
            {
                itemQuality = itemQuality
            };

            foreach (Chunk chunk in debris.Chunks)
            {
                chunk.xVelocity.Value *= velocityMultiplyer;
                chunk.yVelocity.Value *= velocityMultiplyer;
            }

            if (groundLevel != -1)
            {
                debris.chunkFinalYLevel = groundLevel;
            }
            location.debris.Add(debris);
            */
            _ctx.AddItemIfNotNull(id, quality: itemQuality);
        }

        public static void Predict_createDebris(PredictionContext _ctx, int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
        {
            /*
            if (location == null)
            {
                location = currentLocation;
            }
            var debris = new Debris(debrisType, numberOfChunks, new Vector2(xTile * 64 + 32, yTile * 64 + 32), player.getStandingPosition());
            */
            var itemId = debrisType switch
            {
                0 or 378 => "(O)378",
                2 or 380 => "(O)380",
                4 or 382 => "(O)382",
                6 or 384 => "(O)384",
                10 or 386 => "(O)386",
                12 or 388 => "(O)388",
                14 or 390 => "(O)390",
                _ => "(O)" + debrisType
            };

            _ctx.AddItemIfNotNull(itemId, stack: numberOfChunks);
        }
    }
}
