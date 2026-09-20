using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Snowlight.Game.Items;

namespace Snowlight.Game.Rooms
{
    public partial class GameManager
    {
        private Dictionary<Item, TeamColors> mFootballGateTeam;
        private Dictionary<uint, string> mUserClothesHolder;
        private Dictionary<uint, int> mUserBallDistance;

        public Dictionary<Item, TeamColors> FootballGateTeam
        {
            get
            {
                return mFootballGateTeam;
            }

            set
            {
                mFootballGateTeam = value;
            }
        }

        public Dictionary<uint, string> UserClothesHolder
        {

            get
            {
                return mUserClothesHolder;
            }

            set
            {
                mUserClothesHolder = value; 
            }
        }

        public Dictionary<uint, int> UserFootBallDistance
        {
            get
            {
                return mUserBallDistance;
            }

            set
            {
                mUserBallDistance = value;
            }
        }

        public void RegisterFootballGate(Item Item)
        {
            if (!mFootballGateTeam.ContainsKey(Item))
            {
                List<string> ListTeams = Enum.GetNames(typeof(TeamColors)).ToList();

                foreach (string Colors in ListTeams)
                {
                    if (Colors == "None")
                    {
                        continue;
                    }

                    Enum.TryParse(Colors, true, out TeamColors TeamColor);

                    if (!mFootballGateTeam.ContainsValue(TeamColor))
                    {
                        mFootballGateTeam.Add(Item, TeamColor);

                        List<string> ItemExtraData = Item.Flags.Split(',').ToList();

                        if (ItemExtraData.Count < 3)
                        {
                            ItemExtraData.Add(((int)TeamColor).ToString());
                        }
                        else
                        {
                            ItemExtraData[2] = ((int)TeamColor).ToString();
                        }

                        Item.Flags = string.Join(",", ItemExtraData);
                        Item.DisplayFlags = Item.Flags;
                        RoomManager.MarkWriteback(Item, true);

                        break;
                    }
                }
            }
        }

        public void UnRegisterFootballGate(Item Item)
        {
            if (mFootballGateTeam.ContainsKey(Item))
            {
                mFootballGateTeam.Remove(Item);
            }
        }
    }
}
