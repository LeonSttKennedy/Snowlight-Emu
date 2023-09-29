using System;

using Snowlight.Game.Rooms;
using Snowlight.Specialized;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Snowlight.Game.Pathfinding;
using Snowlight.Game.Misc;
using Snowlight.Util;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Communication;

namespace Snowlight.Game.Bots.Behavior
{
    public class GuideBot : IBotBehavior
    {
        private Bot mSelfBot;
        private RoomActor mSelfActor;
        private bool mNeedsRotation;

        private int mNextSpeechAttempt;
        private int mNextMovementAttempt;

        private int mServingItemId;
        private uint mServingActorId;
        private bool mMovingToServePos;
        private Vector2 mActorServePos;

        public bool InRoom
        {
            get
            {
                return (mSelfActor != null);
            }
        }

        public override void Initialize(Bot Bot)
        {
            mSelfBot = Bot;
        }

        public override void OnSelfEnterRoom(RoomInstance Instance)
        {
            mSelfActor = Instance.GetActorByReferenceId(mSelfBot.Id, RoomActorType.AiBot);

            if (mSelfActor == null) return;

            mNeedsRotation = false;
            mNextSpeechAttempt = RandomGenerator.GetNext(20, 255);
            mNextMovementAttempt = RandomGenerator.GetNext(20, 255);
            mServingItemId = 0;
            mServingActorId = 0;
            mMovingToServePos = false;
            mActorServePos = null;

            mSelfActor.BodyRotation = Instance.Model.DoorRotation;
            mSelfActor.HeadRotation = Instance.Model.DoorRotation;
            mSelfActor.UpdateNeeded = true;

            mSelfActor.Chat("Hi and welcome to uberHotel! I am a bot Guide and I'm here to help you.", false, true);
            mSelfActor.Chat("I can give you tips and hints on what to do here, just ask me a question :)", false, true);
        }

        public override void OnSelfLeaveRoom(RoomInstance Instance)
        {
            if (mSelfActor == null)
            {
                return;
            }

            mSelfActor = null;
        }

        public override void OnUserChat(RoomInstance Instance, RoomActor Actor, string MessageText, bool Shout)
        {
            if (mSelfActor == null || Actor.Type == RoomActorType.AiBot || mServingItemId > 0 ||
                (Distance.Calculate(Actor.Position.GetVector2(), mSelfActor.Position.GetVector2()) > mSelfBot.ResponseDistance))
            {
                return;
            }

            BotResponse Response = mSelfBot.GetResponseForMessage(MessageText);

            if (Response != null)
            {
                switch (Response.ResponseMode)
                {
                    default:
                    case ChatType.Say:
                    case ChatType.Shout:

                        bool mActorBotShout = Response.ResponseMode == ChatType.Shout;
                        mSelfActor.Chat(Response.GetResponse(), mActorBotShout);
                        break;

                    case ChatType.Whisper:

                        mSelfActor.Whisper(Response.GetResponse(), Actor.ReferenceId);
                        break;

                }

                if (Response.ResponseServeId > 0)
                {
                    mMovingToServePos = true;
                    mServingItemId = Response.ResponseServeId;
                    mServingActorId = Actor.Id;
                    mActorServePos = new Vector2(mSelfActor.Position.X, mSelfActor.Position.Y);

                    mSelfActor.MoveTo(mSelfBot.ServePosition);

                    if (mNextMovementAttempt < 50)
                    {
                        mNextMovementAttempt = 50;
                    }
                }

                if (mNextSpeechAttempt < 50)
                {
                    mNextSpeechAttempt += 10;
                }
            }
        }
        
        public override void OnUserEnter(RoomInstance Instance, RoomActor Actor)
        {
            if (mSelfActor == null || Actor.Type == RoomActorType.AiBot)
            {
                return;
            }

            if (mSelfBot.Effect > 0)
            {
                mSelfActor.ApplyEffect(mSelfBot.Effect);
            }
        }

        public override void OnUserLeave(RoomInstance Instance, RoomActor Actor)
        {
            if (mSelfActor == null || Actor.Type == RoomActorType.AiBot)
            {
                return;
            }
        }

        public override void PerformUpdate(RoomInstance Instance)
        {
            if (mNextSpeechAttempt <= 0)
            {
                BotRandomSpeech RandomSpeech = BotManager.GetRandomSpeechForBotDefinition(mSelfBot.DefinitionId);

                if (RandomSpeech != null && RandomSpeech.Message.Length > 0)
                {
                    bool Shout = RandomSpeech.MessageMode == ChatType.Shout;
                    mSelfActor.Chat(RandomSpeech.Message, Shout);
                }

                mNextSpeechAttempt = RandomGenerator.GetNext(0, 255);
            }
            else
            {
                mNextSpeechAttempt--;
            }

            if (!mSelfActor.IsMoving && mNextMovementAttempt <= 0)
            {
                switch (mSelfBot.WalkMode)
                {
                    default:
                    case BotWalkMode.STAND:

                        break;

                    case BotWalkMode.FREEROAM:
                        
                        Vector2 Target = new Vector2(RandomGenerator.GetNext(0, Instance.Model.Heightmap.SizeX),
                            RandomGenerator.GetNext(0, Instance.Model.Heightmap.SizeY));
                        
                        if (Target.X != Instance.Model.DoorPosition.X || Target.Y != Instance.Model.DoorPosition.Y)
                        {
                            mSelfActor.MoveTo(Target);
                        }

                        break;

                    case BotWalkMode.SPECIFIED_RANGE:

                        ReadOnlyCollection<Vector2> Possibilites = mSelfBot.PredefinedPositions;
                        mSelfActor.MoveTo(Possibilites[RandomGenerator.GetNext(0, (Possibilites.Count - 1))]);
                        break;
                }

                mNextMovementAttempt = RandomGenerator.GetNext(0, /*180*/90);
            }
            else
            {
                mNextMovementAttempt--;

                if (!mSelfActor.IsMoving)
                {
                    if (mMovingToServePos)
                    {
                        mMovingToServePos = false;
                        mSelfActor.CarryItem(mServingItemId);
                        mSelfActor.MoveTo(mActorServePos);
                    }
                    else if (mServingItemId > 0)
                    {
                        mSelfActor.CarryItem(0);

                        RoomActor TargetActor = Instance.GetActor(mServingActorId);

                        if (TargetActor != null)
                        {
                            TargetActor.CarryItem(mServingItemId);

                            int NewRot = Rotation.Calculate(mActorServePos, TargetActor.Position.GetVector2());

                            mSelfActor.HeadRotation = NewRot;
                            mSelfActor.BodyRotation = NewRot;
                            mNeedsRotation = true;

                            mSelfActor.UpdateNeeded = true;
                        }

                        mServingItemId = 0;
                    }
                    else if (mNeedsRotation && mSelfBot.Rotation >= 0)
                    {
                        mSelfActor.BodyRotation = mSelfBot.Rotation;
                        mSelfActor.HeadRotation = mSelfBot.Rotation;
                        mNeedsRotation = false;
                    }
                }
            }
        }
    }
}
