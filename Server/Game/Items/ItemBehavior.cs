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
        WiredAddon = 21,
        TimedAlert = 22,
        StepSwitch = 23,
        Football = 24,
        FootballGate = 25,
        FootballGoal = 26,
        FootballScore = 27,
        Dice = 28,
        HoloDice = 29,
        SpinningBottle = 30,
        HabboWheel = 31,
        LoveShuffler = 32,
        StickyNote = 33,
        StickyPole = 34,
        Scoreboard = 35,
        Pet = 36,
        Dispenser = 37,
        Fireworks = 38,
        MusicDisk = 39,
        Rental = 40,
        Gift = 41,
        PuzzleBox = 42,
        PetNest = 43,
        BallToy = 44,
        PetFood = 45,
        PetWaterBowl = 46,
        GameCounter = 47,
        DuckHC = 48,
        DuckVIP = 49,
        Platform = 50,
        WelcomeGift = 51,
        RoomBackground = 52,
        DragonToy = 53,
        ChickenToy = 54,
        FrogToy = 55,
        MonkeyToy = 56,
        BlackHole = 57,
        TotemLeg = 58,
        TotemHead = 59,
        TotemPlanet = 60,
        SpiderToy = 61,
        RollerRink = 62,
        SkateRail = 63,
        TagPole = 64,
        IceSkating = 65,
        EggTagPole = 66,
        BunnyRun = 67
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

                case "football_gate":

                    return ItemBehavior.FootballGate;

                case "football_goal":

                    return ItemBehavior.FootballGoal;

                case "football_score":

                    return ItemBehavior.FootballScore;

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

                case "wiredaddon":

                    return ItemBehavior.WiredAddon;

                case "effectgenerator":

                    return ItemBehavior.AvatarEffectGenerator;

                case "rollerrink":

                    return ItemBehavior.RollerRink;

                case "skaterail":

                    return ItemBehavior.SkateRail;

                case "tagpole":

                    return ItemBehavior.TagPole;

                case "iceskating":

                    return ItemBehavior.IceSkating;

                case "eggtagpole":

                    return ItemBehavior.EggTagPole;

                case "bunnyrun":

                    return ItemBehavior.BunnyRun;

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

                case "ball_toy":

                    return ItemBehavior.BallToy;

                case "chicken_toy":

                    return ItemBehavior.ChickenToy;

                case "frog_toy":

                    return ItemBehavior.FrogToy;

                case "monkey_toy":

                    return ItemBehavior.MonkeyToy;

                case "dragon_toy":

                    return ItemBehavior.DragonToy;

                case "spider_toy":

                    return ItemBehavior.SpiderToy;

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

                case "hole":

                    return ItemBehavior.BlackHole;

                case "poster":

                    return ItemBehavior.Poster;

                case "totemleg":

                    return ItemBehavior.TotemLeg;

                case "totemhead":

                    return ItemBehavior.TotemHead;

                case "totemplanet":

                    return ItemBehavior.TotemPlanet;

                case "none":

                    return ItemBehavior.None;

                default:

                    return ItemBehavior.StaticItem;
            }
        }
    }
}
