﻿using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Catalog;
using Snowlight.Game.Rights;
using Snowlight.Game.Sessions;
using Snowlight.Communication;
using Snowlight.Game.Advertisements;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Items;

namespace Snowlight.Game.Misc
{
    class TestCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "hotel_admin"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Test"; }
        }

        public void Execute(Session Session, RoomInstance Instance, RoomActor Actor, string[] Params)
        {
            /*ServerMessage IDK = new ServerMessage(254);
            Session.SendData(IDK);*/

            /*ServerMessage IDK = new ServerMessage(653);
            IDK.AppendInt32(42);
            Session.SendData(IDK);*/

            /*ServerMessage IDK = new ServerMessage(826);
            IDK.AppendInt32(42);
            IDK.AppendStringWithBreak("Room 42");
            IDK.AppendInt32(8);
            IDK.AppendInt32(2);
            Session.SendData(IDK);*/

            /*int.TryParse(Params[1], out int ID);
            int.TryParse(Params[2], out int ID2);
            int.TryParse(Params[3], out int ID3);

            ServerMessage IDK = new ServerMessage(825);
            IDK.AppendInt32(ID);                    // Means ITEM ID
            IDK.AppendInt32(ID2);                   // Pet Type
            IDK.AppendInt32(ID3);                   // Pet Race
            IDK.AppendStringWithBreak(Params[4]);   // Pet Color Code
            Session.SendData(IDK);*/

            /*int.TryParse(Params[1], out int ID);

            ServerMessage IDK = new ServerMessage(912);
            IDK.AppendInt32(62);
            IDK.AppendStringWithBreak("Room 62");
            IDK.AppendInt32(ID);
            Session.SendData(IDK);*/

            /* GROUP JOIN ERROR
             * 0 = group user limit reached
             * 1 = limit of groups per user reached
             * 2 = group closed
             * 3 = group cannot accept membership request

            int.TryParse(Params[1], out int Id);
            ServerMessage N = new ServerMessage(916);
            N.AppendInt32(Id);
            Session.SendData(N);*/
        }
    }
}
