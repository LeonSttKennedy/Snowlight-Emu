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
        FURNI_MOVE = 1,
        FURNI_ROTATE = 2,
        FURNI_PLACE = 3,
        FURNI_PICK = 4,
        FURNI_SWITCH = 5,
        FURNI_STACK = 6,
        FURNI_DECORATION_FLOOR = 7,
        FURNI_DECORATION_WALL = 8,
        SOCIAL_VISIT = 9,
        SOCIAL_CHAT = 10,
        SOCIAL_FRIEND = 11,
        SOCIAL_RESPECT = 12,
        SOCIAL_DANCE = 13,
        SOCIAL_WAVE = 14,
        PROFILE_CHANGE_LOOK = 15,
        PROFILE_CHANGE_MOTTO = 16,
        PROFILE_BADGE = 17,
        EXPLORE_FIND_SPECIFIC_ITEM = 18,
        EXPLORE_FIND_ITEM_BEHAVIOR = 19,
        GIVE_FOOD_TO_PET = 20,
        GIVE_WATER_TO_PET = 21,
        LEVEL_UP_A_PET = 22,
        PET_TO_OTHER_ROOM = 23,
        PETS_IN_ROOM = 24,
        SCRATCH_A_PET = 25,
        FIND_A_PET_TYPE = 26,
        FREEZE_JOIN = 27,
        FREEZE_PLAY = 28,
        FREEZE_THROW_BOMB = 29,
        FREEZE_WINNER = 30,
        FREEZE_SNOWBALL_HIT = 31,
        FREEZE_POWERUPPER = 32,
        FREEZE_EXTRA_LIFE = 33,
        FREEZE_SHIELD = 34,
        FREEZE_XBOMB = 35,
        FREEZE_MEGA_BOMB = 36,
        FREEZE_MORE_BOMBS = 37,
        FREEZE_BIGGER_BOMB = 38,
        BANZAI_JOIN = 39,
        BANZAI_PLAY = 40,
        BANZAI_WINNER = 41,
        BANZAI_PUSH_PUCK = 42,
        BANZAI_LOCK_TILE = 43,
        BANZAI_LOCK_TILES = 44
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

                case "explore_find_specific_item"
                :
                    return QuestType.EXPLORE_FIND_SPECIFIC_ITEM;

                case "explore_find_item_behavior":

                    return QuestType.EXPLORE_FIND_ITEM_BEHAVIOR;

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

                default:

                    return QuestType.NONE;
            }
        }
    }
}
