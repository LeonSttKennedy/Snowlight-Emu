using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Misc;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Items;
using Snowlight.Specialized;
using Snowlight.Game.Sessions;
using Snowlight.Game.Pathfinding;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Achievements;
using Snowlight.Game.Quests;

namespace Snowlight.Game.Bots.Behavior
{
    public enum PetBotAction
    {
        Idle = 0,
        Roaming = 1,
        Sleeping = 2,
        Drinking = 3,
        Eating = 4,
        PerformingTrick = 5,
        Playing = 6,
        Following = 7
    }

    public class PetBot : IBotBehavior
    {
        private Bot mSelfBot;
        private RoomActor mSelfActor;
        private RoomInstance mRoomInstance;
        private PetBotAction mCurrentAction;
        private double mActionStartedTimestamp;
        private int mActionData;
        private double mGstTimestamp;
        private List<PetTricks> mPossibleTricks;
        private PetTricks mTrick;
        private double mChatDelayer;
        private Vector2 mPositionToSet;
        private int mFollowingType;
        private bool mIsCommandedToDrink;
        private int mEatTimes;

        public override void Initialize(Bot Bot)
        {
            mSelfBot = Bot;
            mCurrentAction = PetBotAction.Idle;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mPossibleTricks = PetDataManager.GetTricksForType(mSelfBot.PetData.Type);
            mIsCommandedToDrink = false;
        }

        #region Interaction Items
        // Search for items on current room instance.
        // Find an item by current position or by item position on room

        private Item GetItemPosition(ItemBehavior ItemInteraction)
        {
            List<Item> FloorItems = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior.Equals(ItemInteraction) && I.TemporaryInteractionReferenceIds.Count == 0).ToList();

            Item Item = FloorItems.FirstOrDefault();

            return Item;
        }

        public Item GetItemByPosition(Vector2 Position, ItemBehavior Behavior)
        {
            uint FurniId = mRoomInstance.FurniMap[Position.X, Position.Y];

            if (FurniId > 0)
            {
                Item Item = mRoomInstance.GetItem(FurniId);

                if (Item != null && Item.Definition.Behavior.Equals(Behavior))
                {
                    return Item;
                }
            }

            return null;
        }
        #endregion

        #region Nest
        // Search for nest in current room instance by pet type :)

        private Item GetNestPosition(int PetType)
        {
            List<Item> Nests = mRoomInstance.GetFloorItems().Where(I => I.Definition.Behavior.Equals(ItemBehavior.PetNest)).OrderBy(O => O.Definition.BehaviorData).Reverse().ToList();
            
            Item Item = null;

            foreach (Item Items in Nests)
            {
                if (Items.Definition.BehaviorData == PetType && Items.TemporaryInteractionReferenceIds.Count > 0 ||
                    Items.Definition.BehaviorData == 2 && Items.TemporaryInteractionReferenceIds.Count > 0 ||
                    Items.Definition.BehaviorData == -1 && Items.TemporaryInteractionReferenceIds.Count > 0)
                {
                    continue;
                }

                if (Items.Definition.BehaviorData == PetType && Items.TemporaryInteractionReferenceIds.Count == 0 ||
                    Items.Definition.BehaviorData == 2 && Items.TemporaryInteractionReferenceIds.Count == 0 ||
                    Items.Definition.BehaviorData == -1 && Items.TemporaryInteractionReferenceIds.Count == 0)
                {
                    Item = Items;
                    break;
                }
            }

            return Item;
        }
        #endregion

        #region Pet Toys
        // This need to be proper implemented
        // Returns the Item to search in room instance by pet type
        // Diferent pet types needs diferents toys

        private ItemBehavior GetInteractionItemByType(int PetType)
        {
            switch(PetType)
            {
                default:

                    return ItemBehavior.PetBall;

                case 10:

                    return ItemBehavior.ChickenTrampoline;

                case 11:

                    return ItemBehavior.FrogPond;

                case 12:

                    return ItemBehavior.DragonTree;

                case 14:

                    return RandomGenerator.GetNext(0, 3) == 0 ? ItemBehavior.MonkeyPond : ItemBehavior.PetBall;

            }
        }
        #endregion

        private void RespondToEvent(string Event)
        {
            BotResponse Response = mSelfBot.GetResponseForMessage(Event);

            if (Response != null)
            {
                mSelfActor.Chat(Response.GetResponse());
            }

            mChatDelayer = UnixTimestamp.GetCurrent();
        }

        public override void OnSelfEnterRoom(RoomInstance Instance)
        {
            mSelfActor = Instance.GetActorByReferenceId(mSelfBot.Id, RoomActorType.AiBot);

            if (mSelfActor == null)
            {
                return;
            }

            mRoomInstance = Instance;
            mCurrentAction = PetBotAction.Idle;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mGstTimestamp = 1.5;

            RespondToEvent("SEE_OWNER");
        }

        public override void OnSelfLeaveRoom(RoomInstance Instance)
        {
            if (mSelfActor == null)
            {
                return;
            }

            //mSelfActor.Chat("*leaves*", false);

            mSelfActor = null;
        }

        public override void OnUserChat(RoomInstance Instance, RoomActor Actor, string MessageText, bool Shout)
        {
            string Message = MessageText.ToLower().Trim();
            string PetName = mSelfBot.PetData.Name.ToLower();

            if (Message.ToLower().Equals(PetName))
            {
                int NewRotation = Rotation.Calculate(mSelfActor.Position.GetVector2(), Actor.Position.GetVector2());
                mSelfActor.BodyRotation = NewRotation;
                mSelfActor.HeadRotation = NewRotation;
                mSelfActor.UpdateNeeded = true;
            }

            if (mSelfActor == null || Actor.Type != RoomActorType.UserCharacter ||
                !Message.StartsWith(PetName + " ") || Message.Length <= PetName.Length)
            {
                return;
            }

            /*if (mSelfBot.PetData.Energy < 20)
            {
                mSelfActor.SetStatus("gst", "hng");
                RespondToEvent("PET_TIRED");
                return;
            }*/

            if (mSelfBot.PetData.Happiness < 20)
            {
                mSelfActor.SetStatus("gst", "sad");
                RespondToEvent("UNHAPPY");
                return;
            }

            int SkipLength = PetName.Length + 1;
            string Command = MessageText.Substring(SkipLength, MessageText.Length - SkipLength).ToLower().Trim();

            // Needs to update this pet response for commands by another system that consider pet attributes
            int Random = RandomGenerator.GetNext(1, 8);

            if (mSelfBot.PetData.Energy > 10 && Random < 6 || mSelfBot.PetData.Energy > 10 && mSelfBot.PetData.Level > 15)
            {
                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    mSelfActor.ClearStatusses();
                    mSelfActor.UpdateNeeded = true;

                    bool IsOwner = mSelfBot.PetData.OwnerId.Equals(Actor.ReferenceId);

                    int ExpAdd = 0;
                    PetTricks Trick = null;

                    PetCommand PetCommand = PetDataManager.TryInvokeCommand(mSelfBot.PetData, Command, IsOwner);
                    switch (PetCommand)
                    {
                        /*
                         * Pet Commands Count: 44
                         * Available Commands Count: 41
                         * Missing Commands Count: 3
                         *
                         * Missing Commands List:
                         * Play football    (General)
                         * Switch TV        (Spider)
                         * Torch            (Dragon)
                        */

                        #region Default Commands (Count: 23)

                        #region free
                        case PetCommand.Free:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            ChangeActionToRoaming();
                            break;
                        #endregion

                        #region sit
                        case PetCommand.Sit:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("sit|" + Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'), false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region down
                        case PetCommand.Down:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("lay|" + Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'), false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region here
                        case PetCommand.Here:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            mSelfActor.UpdateNeeded = true;

                            ChangeActionToRoaming(Actor.SquareInFront);
                            break;
                        #endregion

                        #region come here
                        case PetCommand.ComeHere:

                            goto case PetCommand.Here;
                        #endregion

                        #region beg
                        case PetCommand.Beg:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            int ActorRotation = Rotation.Calculate(mSelfActor.Position.GetVector2(), Actor.Position.GetVector2());
                            mSelfActor.BodyRotation = ActorRotation;
                            mSelfActor.HeadRotation = ActorRotation;
                            mSelfActor.UpdateNeeded = true;

                            Trick = new PetTricks("beg", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region play dead
                        case PetCommand.PlayDead:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("ded", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region stand
                        case PetCommand.Stand:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("std", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region jump
                        case PetCommand.Jump:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("jmp", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region speak
                        case PetCommand.Speak:

                            mSelfActor.UpdateNeeded = true;

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            BotRandomSpeech RandomSpeech = BotManager.GetRandomSpeechForBotDefinition(mSelfBot.DefinitionId);

                            if (RandomSpeech != null && RandomSpeech.Message.Length > 0)
                            {
                                bool CommandSpeakShout = RandomSpeech.MessageMode == ChatType.Shout;
                                mSelfActor.Chat(RandomSpeech.Message, CommandSpeakShout);
                            }
                            break;
                        #endregion

                        #region silent
                        case PetCommand.Silent:

                            mSelfActor.UpdateNeeded = true;
                            RespondToEvent("PET_COMMAND_SILENT");

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            mChatDelayer = UnixTimestamp.GetCurrent() + RandomGenerator.GetNext(40, 80);
                            break;
                        #endregion

                        #region nest
                        case PetCommand.Nest:

                            mSelfActor.UpdateNeeded = true;

                            Item Nest = GetNestPosition(mSelfBot.PetData.Type);

                            if (Nest != null)
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                mSelfActor.MoveTo(Nest.RoomPosition.GetVector2());
                            }

                            break;
                        #endregion

                        #region drink
                        case PetCommand.Drink:

                            mSelfActor.UpdateNeeded = true;

                            Item WaterBowl = GetItemPosition(ItemBehavior.PetWaterBowl);

                            if (WaterBowl != null && WaterBowl.TemporaryInteractionReferenceIds.Count.Equals(0))
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeActionToDrink(true);
                            }

                            break;
                        #endregion

                        #region eat
                        case PetCommand.Eat:

                            mSelfActor.UpdateNeeded = true;

                            Item Food = GetItemPosition(ItemBehavior.PetFood);

                            if (Food != null)
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeActionToEat();
                            }

                            break;
                        #endregion

                        #region play
                        case PetCommand.Play:

                            mSelfActor.UpdateNeeded = true;

                            Item Toy = GetItemPosition(GetInteractionItemByType(mSelfBot.PetData.Type));

                            if (Toy != null && Toy.TemporaryInteractionReferenceIds.Count.Equals(0))
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeAction(PetBotAction.Playing);
                            }

                            break;
                        #endregion

                        #region stay
                        case PetCommand.Stay:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            ChangeActionToIdle(RandomGenerator.GetNext(15, 45));
                            break;
                        #endregion

                        #region wag tail
                        case PetCommand.WagTail:

                            goto case PetCommand.Wave;
                        #endregion

                        #region move forward
                        case PetCommand.MoveForward:

                            if(mRoomInstance.CanInitiateMoveToPosition(mSelfActor.SquareInFront))
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeActionToRoaming(mSelfActor.SquareInFront);
                            }

                            break;
                        #endregion

                        #region turn left
                        case PetCommand.TurnLeft:

                            if (mRoomInstance.CanInitiateMoveToPosition(mSelfActor.SquareLeft))
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeActionToRoaming(mSelfActor.SquareLeft);
                            }

                            break;
                        #endregion

                        #region turn right
                        case PetCommand.TurnRight:

                            if (mRoomInstance.CanInitiateMoveToPosition(mSelfActor.SquareRight))
                            {
                                if (IsOwner)
                                {
                                    ExpAdd = RandomGenerator.GetNext(1, 10);
                                }

                                ChangeActionToRoaming(mSelfActor.SquareRight);
                            }

                            break;
                        #endregion

                        #region follow
                        case PetCommand.Follow:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            ChangeActionToFollowing(0);

                            break;
                        #endregion

                        #region follow left
                        case PetCommand.FollowLeft:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            ChangeActionToFollowing(1);

                            break;
                        #endregion

                        #region follow right
                        case PetCommand.FollowRight:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            mSelfBot.PetData.AddExperience(MySqlClient, Instance, ExpAdd);

                            ChangeActionToFollowing(2);

                            break;
                        #endregion

                        #endregion

                        #region Chicken Commands (Count: 3)

                        #region triple jump
                        case PetCommand.TripleJump:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("trj", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region chicken dance
                        case PetCommand.ChickenDance:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("dnc", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region high jump
                        case PetCommand.HighJump:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("hij", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #endregion

                        #region Spider Commands (Count: 4)

                        #region bounce
                        case PetCommand.Bounce:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("bnc", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region spin
                        case PetCommand.Spin:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("spn", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region dance
                        case PetCommand.Dance:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("dan", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region flat
                        case PetCommand.Flat:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("flt", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #endregion

                        #region Frog Commands (Count: 5)

                        #region relax
                        case PetCommand.Relax:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("rlx", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region croak
                        case PetCommand.Croak:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("crk", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region wave
                        case PetCommand.Wave:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("wav", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region dip
                        case PetCommand.Dip:
                            
                            goto case PetCommand.Play;
                        #endregion

                        #region mambo
                        case PetCommand.Mambo:

                            goto case PetCommand.Dance;
                        #endregion

                        #endregion

                        #region Dragon Commands (Count: 6)

                        #region roll
                        case PetCommand.Roll:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("rll", true, ItemBehavior.DragonTree);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region hang
                        case PetCommand.Hang:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("hg", true, ItemBehavior.DragonTree);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region breathe fire
                        case PetCommand.BreatheFire:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("flm", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region ring of fire
                        case PetCommand.RingofFire:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("rng", true, ItemBehavior.DragonTree);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region spread wings
                        case PetCommand.SpreadWings:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("wng", false, ItemBehavior.None);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #region swing
                        case PetCommand.Swing:

                            if (IsOwner)
                            {
                                ExpAdd = RandomGenerator.GetNext(1, 10);
                            }

                            Trick = new PetTricks("swg", true, ItemBehavior.DragonTree);

                            ChangeActionToPerformingTrick(Trick);
                            break;
                        #endregion

                        #endregion

                        default:

                            mSelfActor.SetStatus("gst", "que");
                            mSelfActor.UpdateNeeded = true;

                            RespondToEvent("PET_UNKNOWNCOMMAND");
                            break;
                    }

                    if(ExpAdd > 0)
                    {
                        mSelfBot.PetData.AddExperience(MySqlClient, Instance, ExpAdd);
                    }

                    mSelfBot.PetData.PetEnergy(false);
                }
            }
            else
            {
                mSelfActor.ClearStatusses();
                mSelfActor.SetStatus("gst", "sad");
                mSelfActor.UpdateNeeded = true;

                if (mSelfBot.PetData.Energy < 10)
                {
                    ChangeAction(PetBotAction.Sleeping);
                }
                else
                {
                    RespondToEvent("PET_LAZY");
                    mSelfBot.PetData.PetEnergy(false);
                }
            }
        }

        public override void OnUserEnter(RoomInstance Instance, RoomActor Actor)
        {
            if (mSelfActor == null || Actor.ReferenceId != mSelfBot.PetData.OwnerId || Actor.Type != RoomActorType.UserCharacter
                || (mCurrentAction != PetBotAction.Idle && mCurrentAction != PetBotAction.Roaming))
            {
                return;
            }

            if (RandomGenerator.GetNext(0, 1) == 1)
            {
                int NewRotation = Rotation.Calculate(mSelfActor.Position.GetVector2(), Actor.Position.GetVector2());
                mSelfActor.BodyRotation = NewRotation;
                mSelfActor.HeadRotation = NewRotation;
                mSelfActor.UpdateNeeded = true;

                RespondToEvent("SEE_OWNER");
            }
        }

        public override void OnUserLeave(RoomInstance Instance, RoomActor Actor)
        {
            mSelfBot.PetData.MoveToUserInventory();
            RoomManager.MarkWriteback(mSelfBot.PetData);

            Session OwnerSession = SessionManager.GetSessionByCharacterId(Actor.ReferenceId);
            if (OwnerSession != null)
            {
                OwnerSession.InventoryCache.Add(mSelfBot.PetData);
                OwnerSession.SendData(InventoryPetAddedComposer.Compose(mSelfBot.PetData));
            }
        }

        #region Change Pet Action

        private void ChangeAction(PetBotAction Action)
        {
            mCurrentAction = Action;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mActionData = 0;
            mGstTimestamp = 1.5;
        }

        #region Idle
        private void ChangeActionToIdle(double GestureTimestamp = 0)
        {
            mCurrentAction = PetBotAction.Idle;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mGstTimestamp = GestureTimestamp > 0 ? GestureTimestamp : 1.5;
            mIsCommandedToDrink = false;
            mEatTimes = 0;
        }
        #endregion

        #region Drink
        private void ChangeActionToDrink(bool IsForced)
        {
            mCurrentAction = PetBotAction.Drinking;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mGstTimestamp = 1.5;
            mIsCommandedToDrink = IsForced;
        }
        #endregion

        #region Eat
        private void ChangeActionToEat()
        {
            mCurrentAction = PetBotAction.Eating;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mGstTimestamp = 1.5;
            mActionData = 0;
            mEatTimes = RandomGenerator.GetNext(1, 6);
        }
        #endregion

        #region PerformingTrick
        private void ChangeActionToPerformingTrick(PetTricks Trick = null)
        {
            mCurrentAction = PetBotAction.PerformingTrick;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mActionData = 0;
            mTrick = Trick;
            mGstTimestamp = 1.5;
        }
        #endregion

        #region Roaming
        private void ChangeActionToRoaming(Vector2 PositionToSet = null)
        {
            mCurrentAction = PetBotAction.Roaming;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mActionData = 0;
            mPositionToSet = PositionToSet;
            mGstTimestamp = 1.5;
        }
        #endregion

        #region Following
        private void ChangeActionToFollowing(int FollowingType)
        {
            mCurrentAction = PetBotAction.Following;
            mActionStartedTimestamp = UnixTimestamp.GetCurrent();
            mActionData = 0;
            mGstTimestamp = 1.5;
            mFollowingType = FollowingType;
        }
        #endregion

        #endregion

        public override void PerformUpdate(RoomInstance Instance)
        {
            if(mSelfActor == null) return;

            Bot Bot = (Bot)mSelfActor.ReferenceObject;
            if(Bot == null) return;

            Pet Pet = Bot.PetData;
            if(Pet == null) return;

            double TimeSinceLastMessage = (UnixTimestamp.GetCurrent() - mChatDelayer);

            switch (mCurrentAction)
            {
                #region Brain
                default:
                case PetBotAction.Idle:

                    double TimeSinceIdle = UnixTimestamp.GetCurrent() - mActionStartedTimestamp;
                    
                    mSelfActor.ClearStatusses();
                    mSelfActor.UpdateNeeded = true;
                    
                    if (RandomGenerator.GetNext(0, 6) == 0)
                    {
                        if (TimeSinceIdle >= mGstTimestamp)
                        {
                            switch (RandomGenerator.GetNext(0, 8))
                            {
                                case 0:
                                case 1:

                                    if (RandomGenerator.GetNext(0, 4) == 0)
                                    {
                                        if (mSelfBot.PetData.Type == 12 ||
                                            GetInteractionItemByType(mSelfBot.PetData.Type) == ItemBehavior.PetBall &&
                                            RandomGenerator.GetNext(0, 4) == 3)
                                        {
                                            goto case 7;
                                        }
                                        else
                                        {
                                            ChangeAction(PetBotAction.Playing);
                                        }
                                    }
                                    else
                                    {
                                        ChangeActionToIdle();
                                    }

                                    break;

                                case 2:
                                case 3:
                                case 4:

                                    if (RandomGenerator.GetNext(0, 4) == 0)
                                    {
                                        ChangeAction(PetBotAction.Sleeping);
                                    }
                                    else
                                    {
                                        ChangeAction(PetBotAction.Roaming);
                                    }

                                    break;
                                
                                case 5:
                                case 6:

                                    if (RandomGenerator.GetNext(0, 4) == 0)
                                    {
                                        if (mSelfBot.PetData.Type < 9 && RandomGenerator.GetNext(0, 4) == 3)
                                        {
                                            ChangeActionToDrink(false);
                                        }
                                        else
                                        {
                                            ChangeAction(PetBotAction.Roaming);
                                        }
                                    }
                                    else
                                    {
                                        goto case 7;
                                    }

                                    break;

                                
                                case 7:
                                    
                                    PetBotAction Action = mSelfBot.PetData.Energy > 50 && mSelfBot.PetData.Happiness > 65 ?
                                        PetBotAction.PerformingTrick : PetBotAction.Idle;

                                    PetTricks Trick = Action == PetBotAction.PerformingTrick && mPossibleTricks.Count > 0 ?
                                        mPossibleTricks[RandomGenerator.GetNext(0, mPossibleTricks.Count - 1)] : null;

                                    if (Action == PetBotAction.Idle)
                                    {
                                        ChangeActionToIdle();
                                    }
                                    else if(Action == PetBotAction.PerformingTrick)
                                    {
                                        ChangeActionToPerformingTrick(Trick);
                                    }

                                    break;

                                case 8:

                                    mSelfActor.BodyRotation = RandomGenerator.GetNext(0, 8);
                                    mSelfActor.HeadRotation = mSelfActor.BodyRotation;
                                    mSelfActor.UpdateNeeded = true;
                                    break;
                            }
                        }

                        if (RandomGenerator.GetNext(0, 10) == 0)
                        {
                            if (TimeSinceLastMessage >= 60)
                            {
                                BotRandomSpeech RandomSpeech = BotManager.GetRandomSpeechForBotDefinition(mSelfBot.DefinitionId);

                                if (RandomSpeech != null && RandomSpeech.Message.Length > 0)
                                {
                                    bool Shout = RandomSpeech.MessageMode == ChatType.Shout;
                                    mSelfActor.Chat(RandomSpeech.Message, Shout);
                                }
                            }

                            mChatDelayer = UnixTimestamp.GetCurrent();
                        }
                    }

                    break;
                #endregion

                #region Roaming
                case PetBotAction.Roaming:

                    if (mActionData == 0)
                    {
                        Vector2 Target = mPositionToSet != null ? mPositionToSet :
                            new Vector2(RandomGenerator.GetNext(0, Instance.Model.Heightmap.SizeX),
                                RandomGenerator.GetNext(0, Instance.Model.Heightmap.SizeY));

                        if (!Instance.CanInitiateMoveToPosition(Target) ||
                            Target == Instance.Model.DoorPosition.GetVector2())
                        {
                            break;
                        }

                        mSelfActor.MoveTo(Target);
                        
                        mActionData = Distance.Calculate(mSelfActor.Position.GetVector2(), Target);

                        Vector3 ToDb = new Vector3(Target.X, Target.Y, mRoomInstance.GetUserStepHeight(Target));
                        Pet.MoveToRoom(Pet.RoomId, ToDb, mSelfActor.BodyRotation);
                        RoomManager.MarkWriteback(Pet);
                    }
                    else
                    {
                        mActionData--;

                        if (mActionData == 0)
                        {
                            ChangeActionToIdle();
                        }
                    }

                    break;
                #endregion

                #region Performing a Trick
                case PetBotAction.PerformingTrick:
                    
                    ItemBehavior TrickBehavior = mTrick != null && mTrick.NeedsToy ? mTrick.ToyBehavior :
                        GetInteractionItemByType(mSelfBot.PetData.Type);

                    Item TrickInteractionItem = GetItemPosition(TrickBehavior);

                    if(TrickInteractionItem != null && TrickInteractionItem.TemporaryInteractionReferenceIds.Count > 0
                        && TrickInteractionItem.TemporaryInteractionReferenceIds[0] != mSelfActor.Id)
                    {
                        ChangeActionToIdle();
                    }

                    if (mActionData == 0)
                    {
                        if (mTrick != null && mTrick.NeedsToy)
                        {
                            if (TrickInteractionItem != null &&
                            (mSelfActor.Position.GetVector2() != mPositionToSet ||
                            mSelfActor.Position.GetVector2().Equals(mPositionToSet)))
                            {
                                mSelfActor.MoveTo(TrickInteractionItem.RoomPosition.GetVector2(), true);

                                mActionData = RandomGenerator.GetNext(10, 40);
                            }
                        }
                        else
                        {
                            mActionData = RandomGenerator.GetNext(4, 20);
                        }
                    }
                    else
                    {
                        if (mTrick != null && mTrick.NeedsToy)
                        {
                            mSelfActor.SetStatus("gst", "plf");
                            mSelfActor.UpdateNeeded = true;

                            mSelfActor.HeadRotation = 2;
                            mSelfActor.BodyRotation = mSelfActor.HeadRotation;
                            mSelfActor.UpdateNeeded = true;

                            Item InteractionToy = GetItemByPosition(mSelfActor.Position.GetVector2(), TrickBehavior);
                            if (InteractionToy != null && mSelfActor.Position.GetVector2().Equals(InteractionToy.RoomPosition.GetVector2()))
                            {
                                if (!InteractionToy.TemporaryInteractionReferenceIds.ContainsKey(0))
                                {
                                    InteractionToy.TemporaryInteractionReferenceIds.Add(0, mSelfActor.Id);
                                }

                                mSelfActor.ClearStatusses();
                                mSelfActor.SetStatus(mTrick.Trick, Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'));
                                mSelfActor.UpdateNeeded = true;
                            }
                        }
                        else
                        {
                            string[] Trick = mTrick.Trick.Split('|');

                            if (Trick.Length > 1)
                            {
                                mSelfActor.SetStatus(Trick[0], Trick[1]);
                            }
                            else
                            {
                                mSelfActor.SetStatus(Trick[0]);
                            }

                            mSelfActor.UpdateNeeded = true;
                        }

                        mActionData--;

                        if (mActionData == 0)
                        {
                            if(TrickInteractionItem != null && TrickInteractionItem.TemporaryInteractionReferenceIds.ContainsKey(0))
                            {
                                TrickInteractionItem.TemporaryInteractionReferenceIds.Remove(0);
                            }

                            mSelfActor.ClearStatusses();
                            ChangeActionToIdle();
                        }
                    }

                    break;
                #endregion

                #region Sleeping
                case PetBotAction.Sleeping:

                    Item Nest = GetNestPosition(mSelfBot.PetData.Type);

                    if (mActionData == 0)
                    {
                        if (mSelfBot.PetData.Energy < 30 && RandomGenerator.GetNext(0, 2) == 0)
                        {
                            if (Nest != null && !Nest.TemporaryInteractionReferenceIds.ContainsKey(0) 
                                && mSelfActor.Position.GetVector2() != Nest.RoomPosition.GetVector2())
                            {
                                mSelfActor.MoveTo(Nest.RoomPosition.GetVector2());
                            }
                        }

                        mActionData = RandomGenerator.GetNext(40, 45);
                    }
                    else
                    {
                        mSelfActor.ClearStatusses();
                        mSelfActor.SetStatus("lay", Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'));
                        mSelfActor.SetStatus("slp");
                        mSelfActor.SetStatus("gst trd");
                        mSelfActor.UpdateNeeded = true;

                        Item InteractionNest = GetItemByPosition(mSelfActor.Position.GetVector2(), ItemBehavior.PetNest);
                        if (InteractionNest != null)
                        {
                            if (!InteractionNest.TemporaryInteractionReferenceIds.ContainsKey(0))
                            {
                                InteractionNest.TemporaryInteractionReferenceIds.Add(0, mSelfActor.Id);
                            }

                            if ((mSelfActor.UserStatusses.ContainsKey("lay") ||
                                mSelfActor.UserStatusses.ContainsKey("slp")) 
                                && RandomGenerator.GetNext(0, 8) == 0)
                            {
                                mSelfBot.PetData.PetEnergy(true, false, mRoomInstance);
                            }
                        }

                        if((mSelfActor.UserStatusses.ContainsKey("lay") ||
                           mSelfActor.UserStatusses.ContainsKey("slp")) &&
                           RandomGenerator.GetNext(0, 14) == 0)
                        {
                            mSelfBot.PetData.PetEnergy(true, false, mRoomInstance);
                        }

                        if ((mSelfActor.UserStatusses.ContainsKey("lay") ||
                            mSelfActor.UserStatusses.ContainsKey("slp"))
                            && TimeSinceLastMessage >= 60)
                        {
                            RespondToEvent("PET_TIRED");
                        }

                        mActionData--;

                        if (mActionData == 0)
                        {
                            if (InteractionNest != null && InteractionNest.TemporaryInteractionReferenceIds.ContainsKey(0))
                            {
                                InteractionNest.TemporaryInteractionReferenceIds.Remove(0);
                            }

                            mSelfActor.ClearStatusses();
                            ChangeActionToIdle();
                        }
                    }
                    break;
                #endregion

                #region Drinking
                case PetBotAction.Drinking:

                    Item WaterBowl = GetItemPosition(ItemBehavior.PetWaterBowl);

                    if (WaterBowl != null && WaterBowl.TemporaryInteractionReferenceIds.Count > 0
                            && WaterBowl.TemporaryInteractionReferenceIds[0] != mSelfActor.Id)
                    {
                        ChangeActionToIdle();
                    }

                    if (mActionData == 0)
                    {
                        if (WaterBowl != null && mSelfActor.Position.GetVector2() != WaterBowl.RoomPosition.GetVector2())
                        {
                            if (mRoomInstance.CanInitiateMoveToPosition(WaterBowl.RoomPosition.GetVector2()))
                            {
                                mSelfActor.MoveTo(WaterBowl.RoomPosition.GetVector2());

                                mActionData = RandomGenerator.GetNext(4, 20);
                            }
                        }
                    }
                    else
                    {
                        if (mSelfActor.Position.GetVector2().Equals(WaterBowl.RoomPosition.GetVector2()))
                        {
                            string Action = mSelfBot.PetData.Type == 8 ? "shk" : "snf";
                            mSelfActor.HeadRotation = WaterBowl.RoomRotation;
                            mSelfActor.BodyRotation = WaterBowl.RoomRotation;

                            mSelfActor.ClearStatusses();
                            mSelfActor.SetStatus(Action, Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'));
                            mSelfActor.UpdateNeeded = true;

                            WaterBowl.RequestUpdate(mActionData);
                            WaterBowl.Update(Instance);
                            WaterBowl.BroadcastStateUpdate(Instance);

                            // ?? LUL
                            if (mRoomInstance.Info.OwnerId.Equals(mSelfBot.PetData.OwnerId))
                            {
                                Session ObjectSession = SessionManager.GetSessionByCharacterId(mSelfBot.PetData.OwnerId);
                                if (ObjectSession != null && !mIsCommandedToDrink)
                                {
                                    QuestManager.ProgressUserQuest(ObjectSession, QuestType.GIVE_WATER_TO_PET);
                                }
                            }

                            mActionData--;
                        }

                        if (mActionData == 0)
                        {
                            mSelfActor.ClearStatusses();
                            ChangeActionToIdle();
                        }
                    }
                    break;
                #endregion

                #region Eating
                case PetBotAction.Eating:

                    if(Instance.Info.OwnerId != mSelfBot.PetData.OwnerId && !Instance.Info.AllowPetEating)
                    {
                        ChangeActionToIdle();
                    }

                    Item Food = GetItemPosition(ItemBehavior.PetFood);

                    if (Food != null && Food.TemporaryInteractionReferenceIds.Count > 0
                            && Food.TemporaryInteractionReferenceIds[0] != mSelfActor.Id)
                    {
                        ChangeActionToIdle();
                    }

                    if (mActionData == 0)
                    {
                        if (Food != null && mSelfActor.Position.GetVector2() != Food.RoomPosition.GetVector2())
                        {
                            if (mRoomInstance.CanInitiateMoveToPosition(Food.RoomPosition.GetVector2()))
                            {
                                mSelfActor.MoveTo(Food.RoomPosition.GetVector2());
                            }
                        }

                        mActionData = RandomGenerator.GetNext(4, 20);
                        int Flags = Food.DisplayFlags.Equals(string.Empty) ? 0 : int.Parse(Food.DisplayFlags);
                        mEatTimes = mEatTimes > Food.Definition.BehaviorData ? mEatTimes - (Food.Definition.BehaviorData - Flags)
                            : mEatTimes;
                    }
                    else
                    {
                        Item InteractionFood = GetItemByPosition(mSelfActor.Position.GetVector2(), ItemBehavior.PetFood);
                        if (InteractionFood != null && mSelfActor.Position.GetVector2().Equals(InteractionFood.RoomPosition.GetVector2()))
                        {
                            if (!InteractionFood.TemporaryInteractionReferenceIds.ContainsKey(0))
                            {
                                InteractionFood.TemporaryInteractionReferenceIds.Add(0, mSelfActor.Id);
                            }

                            string Action = mSelfBot.PetData.Type == 8 ? "shk" : "eat";
                            mSelfActor.HeadRotation = InteractionFood.RoomRotation;
                            mSelfActor.BodyRotation = InteractionFood.RoomRotation;

                            mSelfActor.ClearStatusses();

                            int InteractionFoodFlags = InteractionFood != null ?
                                InteractionFood.DisplayFlags.Equals(string.Empty) ? 0 :
                                int.Parse(InteractionFood.DisplayFlags) : 6;

                            int TimesReamaing = InteractionFood.Definition.BehaviorData - InteractionFoodFlags;

                            if (mEatTimes >= 1)
                            {
                                if (TimesReamaing > 0 && mEatTimes > TimesReamaing)
                                {
                                    mEatTimes = TimesReamaing;

                                    if (RandomGenerator.GetNext(0, 1) == 0)
                                    {
                                        mSelfBot.PetData.PetEnergy(true, true, mRoomInstance);
                                    }
                                }

                                mSelfActor.SetStatus(Action, Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'));

                                InteractionFood.RequestUpdate(1);
                                InteractionFood.Update(Instance);
                                InteractionFood.BroadcastStateUpdate(Instance);

                                mEatTimes--;
                            }

                            mSelfActor.UpdateNeeded = true;

                            // ?? LUL
                            if (mRoomInstance.Info.OwnerId.Equals(mSelfBot.PetData.OwnerId))
                            {
                                Session ObjectSession = SessionManager.GetSessionByCharacterId(mSelfBot.PetData.OwnerId);
                                if (ObjectSession != null)
                                {
                                    QuestManager.ProgressUserQuest(ObjectSession, QuestType.GIVE_FOOD_TO_PET);
                                }
                            }

                            mActionData--;
                        }

                        if (mActionData == 0)
                        {
                            InteractionFood.TemporaryInteractionReferenceIds.Remove(0);
                            mSelfActor.ClearStatusses();
                            mSelfActor.UpdateNeeded = true;
                            ChangeActionToIdle();
                        }
                    }
                    break;
                #endregion

                #region Playing
                case PetBotAction.Playing:

                    ItemBehavior Behavior = GetInteractionItemByType(mSelfBot.PetData.Type);
                    Item InteractionItem = GetItemPosition(Behavior);

                    if (InteractionItem != null && InteractionItem.TemporaryInteractionReferenceIds.Count > 0
                            && InteractionItem.TemporaryInteractionReferenceIds[0] != mSelfActor.Id)
                    {
                        ChangeActionToIdle();
                    }

                    string status = Behavior.Equals(ItemBehavior.FrogPond) ? "dip" : 
                        (Behavior.Equals(ItemBehavior.MonkeyPond) ? "swm" : "pla");

                    if (mActionData == 0)
                    {
                        if (InteractionItem != null && 
                            (mSelfActor.Position.GetVector2() != InteractionItem.RoomPosition.GetVector2() ||
                            mSelfActor.Position.GetVector2().Equals(InteractionItem.RoomPosition.GetVector2())))
                        {
                            mSelfActor.MoveTo(InteractionItem.RoomPosition.GetVector2(), true);

                            mActionData = RandomGenerator.GetNext(10, 40);
                        }
                    }
                    else
                    {
                        mSelfActor.SetStatus("gst", "plf");
                        mSelfActor.UpdateNeeded = true;

                        Item Interaction = GetItemByPosition(mSelfActor.Position.GetVector2(), Behavior);
                        if (Interaction != null && mSelfActor.Position.GetVector2().Equals(Interaction.RoomPosition.GetVector2()))
                        {
                            bool NeedsRotation = mSelfActor.HeadRotation != Interaction.RoomRotation + 2 &&
                                mSelfActor.BodyRotation != Interaction.RoomRotation + 2;

                            if (NeedsRotation)
                            {
                                mSelfActor.HeadRotation = Interaction.RoomRotation + 2;
                                mSelfActor.BodyRotation = mSelfActor.HeadRotation;
                                mSelfActor.UpdateNeeded = true;
                            }

                            mSelfActor.ClearStatusses();
                            mSelfActor.SetStatus(status, Math.Round(mSelfActor.Position.Z, 1).ToString().Replace(',', '.'));
                            mSelfActor.UpdateNeeded = true;

                            Interaction.RequestUpdate(1);

                            mActionData--;
                        }

                        if (mActionData == 0)
                        {
                            Interaction.RequestUpdate(1);

                            mSelfActor.ClearStatusses();
                            ChangeActionToIdle();
                        }
                    }

                    break;
                #endregion

                #region Following
                case PetBotAction.Following:

                    if (mActionData == 0)
                    {
                        mActionData = RandomGenerator.GetNext(25, 45);
                    }
                    else
                    {
                        RoomActor Owner = Instance.GetActorByReferenceId(mSelfBot.PetData.OwnerId);
                        if (Owner != null)
                        {
                            mPositionToSet = mFollowingType == 0 ? Owner.SquareBehind :
                                (mFollowingType == 1 ? Owner.SquareLeft : 
                                (mFollowingType == 2 ? Owner.SquareRight : null));
                        }

                        if(mPositionToSet == null)
                        {
                            mActionData = 1;
                        }
                        else
                        {
                            if (!mSelfActor.Position.GetVector2().Equals(mPositionToSet) &&
                                Instance.CanInitiateMoveToPosition(mPositionToSet))
                            {
                                mSelfActor.MoveTo(mPositionToSet);
                            }
                        }

                        mActionData--;

                        if (mActionData == 0)
                        {
                            mSelfActor.ClearStatusses();
                            ChangeActionToIdle();
                        }
                    }

                    break;
                #endregion
            }
        }
    }
}
