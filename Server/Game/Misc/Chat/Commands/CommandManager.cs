using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Snowlight.Util;
using Snowlight.Game.Rooms;
using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;

namespace Snowlight.Game.Misc
{
    public static class CommandManager
    {
        private static string mPrefix = ":";

        private static Dictionary<string, IChatCommand> mUsefulCommands;

        public static void Initialize()
        {
            mPrefix = ":";
            mUsefulCommands = new Dictionary<string, IChatCommand>();

            RegisterNormal();
            RegisterClub();
            RegisterVip();
            RegisterModerationTool();
            RegisterMute();
            RegisterHotelAdmin();
        }

        public static bool HandleCommand(Session Session, string Message)
        {
            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            RoomActor Actor = (Instance == null ? null : Instance.GetActorByReferenceId(Session.CharacterId));

            if (!Message.StartsWith(mPrefix))
                return false;

            if (Message == mPrefix + "commands")
            {
                StringBuilder List = new StringBuilder();
                if (Session.HasRight("hotel_admin") || Session.HasRight("moderation_tool") || Session.HasRight("mute"))
                {
                    List.Append(ExternalTexts.GetValue("command_staff_user_list") + "\n");
                    if (Session.HasRight("hotel_admin"))
                    {
                        foreach (var StaffList in mUsefulCommands.Values.Where(o => o.PermissionRequired == "hotel_admin").ToList())
                        {
                            List.Append("\n:" + GetCommandString(StaffList) + " " + StaffList.Parameters + " - " + StaffList.Description);
                        }
                    }

                    if (Session.HasRight("moderation_tool"))
                    {
                        foreach (var StaffList in mUsefulCommands.Values.Where(o => o.PermissionRequired == "moderation_tool").ToList())
                        {
                            List.Append("\n:" + GetCommandString(StaffList) + " " + StaffList.Parameters + " - " + StaffList.Description);
                        }
                    }

                    if (Session.HasRight("mute"))
                    {
                        foreach (var StaffList in mUsefulCommands.Values.Where(o => o.PermissionRequired == "mute").ToList())
                        {
                            List.Append("\n:" + GetCommandString(StaffList) + " " + StaffList.Parameters + " - " + StaffList.Description);
                        }
                    }
                    List.Append("\n\n");
                }

                if (Session.HasRight("club_vip"))
                {
                    List.Append(ExternalTexts.GetValue("command_vip_user_list") + "\n");
                    foreach (var VipList in mUsefulCommands.Values.Where(o => o.PermissionRequired == "club_vip").ToList())
                    {
                        List.Append("\n:" + GetCommandString(VipList) + " " + VipList.Parameters + " - " + VipList.Description);
                    }

                    List.Append("\n\n");
                }

                if (Session.HasRight("club_regular"))
                {
                    List.Append(ExternalTexts.GetValue("command_club_user_list") + "\n");
                    List.Append(string.Concat(new object[] { 
                        "\n:furni - Show all furnitures on current room.",
                        "\n:chooser - Show all users on current room."
                    }));

                    foreach (var HcList in mUsefulCommands.Values.Where(o => o.PermissionRequired == "club_regular").ToList())
                    {
                        List.Append("\n:" + GetCommandString(HcList) + " " + HcList.Parameters + " - " + HcList.Description);
                    }

                    List.Append("\n\n");
                }

                List.Append(string.Concat(new object[] { 
                    ExternalTexts.GetValue("command_regular_user_list") + "\n",
                    "\n:commands - Shows which commands are available to you."
                }));

                foreach (var RegularList in mUsefulCommands.Values.Where(o => o.PermissionRequired == string.Empty).ToList())
                {
                    List.Append("\n:" + GetCommandString(RegularList) + " " + RegularList.Parameters + " - " + RegularList.Description);
                }

                if (Session.HasRight("hotel_admin") || Session.HasRight("moderation_tool") || Session.HasRight("mute") || Session.HasRight("club_vip"))
                {
                    Session.SendData(MessageOfTheDayComposer.Compose(List.ToString()));
                }
                else
                {
                    Session.SendData(NotificationMessageComposer.Compose(List.ToString()));
                }
                return true;
            }

            Message = Message.Substring(1);
            string[] Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            IChatCommand Cmd = null;
            if (mUsefulCommands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                if (!string.IsNullOrEmpty(Cmd.PermissionRequired))
                {
                    if (!Session.HasRight(Cmd.PermissionRequired))
                    {
                        switch(Cmd.PermissionRequired.ToLower())
                        {
                            case "club_vip":
                                {
                                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                                    return true;
                                }

                            case "club_regular":
                                {
                                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_club_error"), 4, ChatType.Whisper));
                                    return true;
                                }

                            default:
                                {
                                    return false;
                                }
                        }
                    }
                }

                Cmd.Execute(Session, Instance, Actor, Split);
                return true;
            }
            return false;
        }
        private static void RegisterNormal()
        {
            Register("about", new AboutCommand());
            Register("disablemimic", new DisableMimicCommand());
            Register("disablegifts", new DisableGiftsCommand());
            Register("disablediagonal", new DisableDiagonalCommand());
            Register("follow", new FollowCommand());
            Register("pickall", new PickallCommand());
        }

        private static void RegisterClub()
        {
            Register("afk", new AfkCommand());
            Register("sit", new SitCommand());
            Register("status", new StatusCommand());
            Register("online", new UsersOnlineCommand());
        }
        
        private static void RegisterVip()
        {
            Register("drink", new DrinkCommand());
            Register("empty", new EmptyCommand());
            Register("enable", new EnableCommand());
            Register("eventalert", new EventAlertCommand());
            Register("flagme", new FlagmeCommand());
            Register("give", new GiveCommand());
            Register("mimic", new MimicCommand());
            Register("moonwalk", new MoonWalkCommand());
            Register("redeemcoins", new RedeemCoinsCommand());
        }

        private static void RegisterModerationTool()
        {
            Register("ra", new RoomAlertCommand());
            Register("ara", new AnonimateRoomAlertCommand());
            Register("ral", new RoomAlertLinkCommand());
            Register("ras", new RoomAlertStaffCommand());
            Register("kick", new KickCommand());
            Register("infobus", new InfobusCommand());
            Register("userinfo", new UserInfoCommand());
            Register("makesay", new MakeSayCommand());
        }

        private static void RegisterMute()
        {
            Register("mute", new MuteCommand());
            Register("unmute", new UnmuteCommand());
            Register("roommute", new RoomMuteCommand());
            Register("roomunmute", new RoomUnmuteCommand());
        }

        private static void RegisterHotelAdmin()
        {
            Register("ha", new HotelAlertCommand());
            Register("aha", new AnonimateHotelAlertCommand());
            Register("hal", new HotelAlertLinkCommand());
            Register("has", new HotelAlertStaffCommand());
            Register("superkick", new SuperKickCommand());
            Register("clipping", new ClippingCommand());
            Register("teleport", new TeleportCommand());
            Register("coords", new CoordsCommand());
            Register("update", new UpdateCommand());
            Register("directbadge", new DirectBadgeCommand());
            Register("massbadge", new MassBadgeCommand());
            Register("directgive", new DirectGiveCommand());
            Register("massgive", new MassGiveCommand());
            Register("shutdown", new ShutdownCommand());
            Register("test", new TestCommand());
        }

        public static void Register(string CommandText, IChatCommand Command)
        {
            mUsefulCommands.Add(CommandText, Command);
        }

        public static string GetCommandString(IChatCommand Command)
        {
            return mUsefulCommands.ContainsValue(Command) ? mUsefulCommands.FirstOrDefault(K => K.Value == Command).Key : string.Empty;
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }
    }
}
