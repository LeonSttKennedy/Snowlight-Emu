using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Quests
{
    public enum QuestType
    {
        NONE = 0,

        // Interact with items
        FURNI_MOVE = 1,
        FURNI_ROTATE = 2,
        FURNI_PLACE = 3,
        FURNI_PICK = 4,
        FURNI_SWITCH = 5,
        FURNI_STACK = 6,
        FURNI_DECORATION_FLOOR = 7,
        FURNI_DECORATION_WALL = 8,
        FURNI_SPECIFIC_SWITCH = 9,
        FIND_SPECIFIC_FURNI = 10,
        FIND_FURNI_BEHAVIOR = 11,

        // User walk on
        WALK_ON_SPECIFIC_FURNI = 12,

        // Find specific drink on dispenser machines
        FIND_SPECIFIC_DRINK = 13,

        // Interact with other users
        SOCIAL_VISIT = 14,
        SOCIAL_CHAT = 15,
        SOCIAL_FRIEND = 16,
        SOCIAL_RESPECT = 17,
        SOCIAL_DANCE = 18,
        SOCIAL_WAVE = 19,

        // Change user profile
        PROFILE_CHANGE_LOOK = 20,
        PROFILE_CHANGE_MOTTO = 21,
        PROFILE_BADGE = 22,

        // Avatar effect
        ACTIVATE_EFFECT = 23,

        // Pet goals
        GIVE_FOOD_TO_PET = 24,
        GIVE_WATER_TO_PET = 25,
        LEVEL_UP_A_PET = 26,
        PET_TO_OTHER_ROOM = 27,
        PETS_IN_ROOM = 28,
        SCRATCH_A_PET = 29,
        FIND_A_PET_TYPE = 30,

        // Beachball game
        BEACHBALL_GOAL = 31,

        // Freeze game
        FREEZE_JOIN = 32,
        FREEZE_PLAY = 33,
        FREEZE_THROW_BOMB = 34,
        FREEZE_WINNER = 35,
        FREEZE_SNOWBALL_HIT = 36,
        FREEZE_POWERUPPER = 37,
        FREEZE_EXTRA_LIFE = 38,
        FREEZE_SHIELD = 39,
        FREEZE_XBOMB = 40,
        FREEZE_MEGA_BOMB = 41,
        FREEZE_MORE_BOMBS = 42,
        FREEZE_BIGGER_BOMB = 43,
        
        // Banzai Game
        BANZAI_JOIN = 44,
        BANZAI_PLAY = 45,
        BANZAI_WINNER = 46,
        BANZAI_PUSH_PUCK = 47,
        BANZAI_LOCK_TILE = 48,
        BANZAI_LOCK_TILES = 49
    }

    public static class QuestTypeUtil
    {
        public static QuestType FromString(string TypeString)
        {
            switch (TypeString.ToLower())
            {
                case "move_item":

                    return QuestType.FURNI_MOVE;

                case "rotate_item":

                    return QuestType.FURNI_ROTATE;

                case "place_item":

                    return QuestType.FURNI_PLACE;

                case "pickup_item":

                    return QuestType.FURNI_PICK;

                case "switch_item":

                    return QuestType.FURNI_SWITCH;

                case "switch_specific_item":

                    return QuestType.FURNI_SWITCH;

                case "stack_item":

                    return QuestType.FURNI_STACK;

                case "decorate_floor":

                    return QuestType.FURNI_DECORATION_FLOOR;

                case "decorate_wall":

                    return QuestType.FURNI_DECORATION_WALL;

                case "social_visit":

                    return QuestType.SOCIAL_VISIT;

                case "social_chat":

                    return QuestType.SOCIAL_CHAT;

                case "social_friend":

                    return QuestType.SOCIAL_FRIEND;

                case "social_respect":

                    return QuestType.SOCIAL_RESPECT;

                case "social_dance":

                    return QuestType.SOCIAL_DANCE;

                case "social_wave":

                    return QuestType.SOCIAL_WAVE;

                case "profile_change_look":

                    return QuestType.PROFILE_CHANGE_LOOK;

                case "profile_change_motto":

                    return QuestType.PROFILE_CHANGE_MOTTO;

                case "profile_badge":

                    return QuestType.PROFILE_BADGE;

                case "find_specific_item"
                :
                    return QuestType.FIND_SPECIFIC_FURNI;

                case "find_item_behavior":

                    return QuestType.FIND_FURNI_BEHAVIOR;

                case "give_food_to_pet":

                    return QuestType.GIVE_FOOD_TO_PET;

                case "give_water_to_pet":

                    return QuestType.GIVE_WATER_TO_PET;

                case "pet_level_up":

                    return QuestType.LEVEL_UP_A_PET;

                case "pet_walk":

                    return QuestType.PET_TO_OTHER_ROOM;

                case "pets_in_room":

                    return QuestType.PETS_IN_ROOM;

                case "pet_scratch":

                    return QuestType.SCRATCH_A_PET;

                case "find_a_pet_type":

                    return QuestType.FIND_A_PET_TYPE;

                case "find_a_pet_race":

                    return QuestType.FIND_A_PET_TYPE;

                case "find_specific_drink":

                    return QuestType.FIND_SPECIFIC_DRINK;

                case "freeze_join":

                    return QuestType.FREEZE_JOIN;

                case "freeze_play":

                    return QuestType.FREEZE_PLAY;

                case "freeze_throw_bomb":

                    return QuestType.FREEZE_THROW_BOMB;

                case "freeze_winner":

                    return QuestType.FREEZE_WINNER;

                case "freeze_snowball_hit":

                    return QuestType.FREEZE_SNOWBALL_HIT;

                case "freeze_powerupper":

                    return QuestType.FREEZE_POWERUPPER;

                case "freeze_extra_life":

                    return QuestType.FREEZE_EXTRA_LIFE;

                case "freeze_shield":

                    return QuestType.FREEZE_SHIELD;

                case "freeze_xbomb":

                    return QuestType.FREEZE_XBOMB;

                case "freeze_mega_bomb":

                    return QuestType.FREEZE_MEGA_BOMB;

                case "freeze_more_bombs":

                    return QuestType.FREEZE_MORE_BOMBS;

                case "freeze_bigger_bomb":

                    return QuestType.FREEZE_BIGGER_BOMB;

                case "banzai_join":

                    return QuestType.BANZAI_JOIN;

                case "banzai_play":

                    return QuestType.BANZAI_PLAY;

                case "banzai_winner":

                    return QuestType.BANZAI_WINNER;

                case "banzai_push_puck":

                    return QuestType.BANZAI_PUSH_PUCK;

                case "banzai_lock_tile":

                    return QuestType.BANZAI_LOCK_TILE;

                case "banzai_lock_tiles":

                    return QuestType.BANZAI_LOCK_TILES;

                case "beachball_goal":

                    return QuestType.BEACHBALL_GOAL;

                case "activate_effect":

                    return QuestType.ACTIVATE_EFFECT;

                case "walk_on_specific_item":

                    return QuestType.WALK_ON_SPECIFIC_FURNI;

                default:

                    return QuestType.NONE;
            }
        }
    }
}
