using Snowlight.Game.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowlight.Game.Rooms
{
    public partial class GameManager
    {
        public bool BanzaiStarted
        {
            get
            {
                return mGameStarted.ContainsKey(GameType.BattleBanzai) ? mGameStarted[GameType.BattleBanzai] : false;
            }
        }
    }
}
