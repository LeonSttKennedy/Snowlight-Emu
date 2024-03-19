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
        Dice = 26,
        HoloDice = 27,
        SpinningBottle = 28,
        HabboWheel = 29,
        LoveShuffler = 30,
        StickyNote = 31,
        StickyPole = 32,
        Scoreboard = 33,
        Pet = 34,
        Dispenser = 35,
        Fireworks = 36,
        MusicDisk = 37,
        Rental = 38,
        Gift = 39,
        PuzzleBox = 40,
        PetNest = 41,
        PetBall = 42,
        PetFood = 43,
        PetWaterBowl = 44,
        GameCounter = 45,
        DuckHC = 46,
        DuckVIP = 47,
        Platform = 48,
        WelcomeGift = 49,
        RoomBackground = 50,
        DragonTree = 51,
        ChickenTrampoline = 52,
        FrogPond = 53,
        MonkeyPond = 54,
        BlackHole = 55,
        TotemLeg = 56,
        TotemHead = 57,
        TotemPlanet = 58
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
