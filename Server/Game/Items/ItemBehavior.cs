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
        IcePuck = 28,
        Beachball = 29,
        Dice = 30,
        HoloDice = 31,
        SpinningBottle = 32,
        HabboWheel = 33,
        LoveShuffler = 34,
        StickyNote = 35,
        StickyPole = 36,
        Scoreboard = 37,
        Pet = 38,
        Dispenser = 39,
        Fireworks = 40,
        MusicDisk = 41,
        Rental = 42,
        Gift = 43,
        PuzzleBox = 44,
        PetNest = 45,
        PetToy = 46,
        PetBall = 47,
        PetFood = 48,
        PetWaterBowl = 49,
        GameCounter = 50,
        DuckHC = 51,
        DuckVIP = 52,
        Platform = 53,
        WelcomeGift = 54,
        RoomBackground = 55,
        BlackHole = 56,
        TotemLeg = 57,
        TotemHead = 58,
        TotemPlanet = 59,
        RollerRink = 60,
        SkateRail = 61,
        TagPole = 62,
        IceSkating = 63,
        EggTagPole = 64,
        BunnyRun = 65
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

                case "ice_puck":

                    return ItemBehavior.IcePuck;

                case "beach_ball":

                    return ItemBehavior.Beachball;

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

                case "pettoy":

                    return ItemBehavior.PetToy;

                case "petball":

                    return ItemBehavior.PetBall;

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
