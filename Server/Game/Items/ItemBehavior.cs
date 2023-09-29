using System;

namespace Snowlight.Game.Items
{
    public enum ItemBehavior
    {
        None = 0,
        StaticItem = 1,
        Wallpaper = 2,
        Floor = 3,
        Landscape = 4,
        Switchable = 5,
        Seat = 6,
        Bed = 7,
        Gate = 8,
        Teleporter = 9,
        Poster = 10,
        OneWayGate = 11,
        TraxPlayer = 12,
        ExchangeItem = 13,
        Moodlight = 14,
        Roller = 15,
        AvatarEffectGenerator = 16,
        PrizeTrophy = 17,
        WiredTrigger = 18,
        WiredCondition = 19,
        WiredEffect = 20,
        TimedAlert = 21,
        StepSwitch = 22,
        Football = 23,
        Dice = 24,
        HoloDice = 25,
        SpinningBottle = 26,
        HabboWheel = 27,
        LoveShuffler = 28,
        StickyNote = 29,
        StickyPole = 30,
        Scoreboard = 31,
        Pet = 32,
        Dispenser = 33,
        Fireworks = 34,
        MusicDisk = 35,
        Rental = 36,
        Gift = 37,
        PuzzleBox = 38,
        PetNest = 39,
        PetBall = 40,
        PetFood = 41,
        PetWaterBowl = 42,
        GameCounter = 43,
        DuckHC = 44,
        DuckVIP = 45,
        Platform = 46,
        WelcomeGift = 47,
        RoomBackground = 48,
        DragonTree = 49,
        ChickenTrampoline = 50,
        FrogPond = 51,
        MonkeyPond = 52,
        BlackHole = 53
    }

    public static class ItemBehaviorUtil
    {
        public static ItemBehavior FromString(string BehaviorString)
        {
            switch (BehaviorString.ToLower())
            {
                case "rental":

                    return ItemBehavior.Rental;

                case "musicdisk":

                    return ItemBehavior.MusicDisk;

                case "fireworks":

                    return ItemBehavior.Fireworks;

                case "dispenser":

                    return ItemBehavior.Dispenser;

                case "pet":

                    return ItemBehavior.Pet;

                case "scoreboard":

                    return ItemBehavior.Scoreboard;

                case "prizetrophy":

                    return ItemBehavior.PrizeTrophy;

                case "stickypole":

                    return ItemBehavior.StickyPole;

                case "stickynote":

                    return ItemBehavior.StickyNote;

                case "loveshuffler":

                    return ItemBehavior.LoveShuffler;

                case "spinningbottle":

                    return ItemBehavior.SpinningBottle;

                case "habbowheel":

                    return ItemBehavior.HabboWheel;

                case "dice":

                    return ItemBehavior.Dice;

                case "holodice":

                    return ItemBehavior.HoloDice;

                case "football":

                    return ItemBehavior.Football;

                case "autoswitch":

                    return ItemBehavior.StepSwitch;

                case "alert":

                    return ItemBehavior.TimedAlert;

                case "wiredeffect":

                    return ItemBehavior.WiredEffect;

                case "wiredcondition":

                    return ItemBehavior.WiredCondition;

                case "wiredtrigger":

                    return ItemBehavior.WiredTrigger;

                case "effectgenerator":

                    return ItemBehavior.AvatarEffectGenerator;

                case "roller":

                    return ItemBehavior.Roller;

                case "moodlight":

                    return ItemBehavior.Moodlight;

                case "traxplayer":

                    return ItemBehavior.TraxPlayer;

                case "onewaygate":

                    return ItemBehavior.OneWayGate;

                case "teleporter":

                    return ItemBehavior.Teleporter;

                case "gate":

                    return ItemBehavior.Gate;

                case "exchange":

                    return ItemBehavior.ExchangeItem;

                case "bed":

                    return ItemBehavior.Bed;

                case "seat":

                    return ItemBehavior.Seat;

                case "platform":

                    return ItemBehavior.Platform;

                case "switch":

                    return ItemBehavior.Switchable;

                case "landscape":

                    return ItemBehavior.Landscape;

                case "floor":

                    return ItemBehavior.Floor;

                case "wallpaper":

                    return ItemBehavior.Wallpaper;

                case "gift":

                    return ItemBehavior.Gift;

                case "gamecounter":

                    return ItemBehavior.GameCounter;

                case "puzzlebox":

                    return ItemBehavior.PuzzleBox;

                case "petnest":

                    return ItemBehavior.PetNest;

                case "petball":

                    return ItemBehavior.PetBall;

                case "chicken_trampoline":

                    return ItemBehavior.ChickenTrampoline;

                case "frog_pond":

                    return ItemBehavior.FrogPond;

                case "monkey_pond":

                    return ItemBehavior.MonkeyPond;

                case "petfood":

                    return ItemBehavior.PetFood;

                case "petwaterbowl":

                    return ItemBehavior.PetWaterBowl;

                case "duckhc":

                    return ItemBehavior.DuckHC;

                case "duckvip":

                    return ItemBehavior.DuckVIP;

                case "welcome_gift":

                    return ItemBehavior.WelcomeGift;

                case "room_background":

                    return ItemBehavior.RoomBackground;

                case "dragon_tree":

                    return ItemBehavior.DragonTree;

                case "hole":

                    return ItemBehavior.BlackHole;

                case "poster":

                    return ItemBehavior.Poster;

                default:

                    return ItemBehavior.StaticItem;
            }
        }
    }
}
