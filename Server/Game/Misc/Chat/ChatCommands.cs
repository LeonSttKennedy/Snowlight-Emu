using System;
using System.Collections.Generic;
using System.Linq;

using Snowlight.Game.Sessions;
using Snowlight.Communication.Outgoing;
using Snowlight.Game.Infobus;
using Snowlight.Game.Rooms;
using Snowlight.Game.Bots;
using Snowlight.Storage;
using Snowlight.Game.Achievements;
using Snowlight.Game.Pets;
using Snowlight.Util;
using System.Text;
using Snowlight.Game.Moderation;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Runtime.InteropServices;
using Snowlight.Communication;
using System.Data;

namespace Snowlight.Game.Misc
{
    public static class ChatCommands
    {
        [DllImport("kernel32.dll")] 
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo); 
        private const int PROCESSOR_ARCHITECTURE_AMD64 = 9; 
        private const int PROCESSOR_ARCHITECTURE_IA64 = 6; 
        private const int PROCESSOR_ARCHITECTURE_INTEL = 0;
        [StructLayout(LayoutKind.Sequential)] 
        private struct SYSTEM_INFO 
        { 
            public short wProcessorArchitecture; 
            public short wReserved; 
            public int dwPageSize; 
            public IntPtr lpMinimumApplicationAddress; 
            public IntPtr lpMaximumApplicationAddress; 
            public IntPtr dwActiveProcessorMask; 
            public int dwNumberOfProcessors; 
            public int dwProcessorType; 
            public int dwAllocationGranularity; 
            public short wProcessorLevel; 
            public short wProcessorRevision; 
        }
        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            SYSTEM_INFO si = new SYSTEM_INFO(); 
            GetNativeSystemInfo(ref si); 
            switch (si.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_AMD64: return ProcessorArchitecture.Amd64;
                case PROCESSOR_ARCHITECTURE_IA64: return ProcessorArchitecture.IA64;
                case PROCESSOR_ARCHITECTURE_INTEL: return ProcessorArchitecture.X86;
                default:
                    return ProcessorArchitecture.None; // that's weird :-)
            } 
        }
        static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }
        public static bool HandleCommand(Session Session, string Input)
        {
            //Input = Input.Substring(1, Input.Length - 1);
            string[] Bits = Input.Split(' ');

            RoomInstance Instance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
            RoomActor Actor = (Instance == null ? null : Instance.GetActorByReferenceId(Session.CharacterId));

            switch (Bits[0].ToLower())
            {
                #region Staff Commands
                #region :ha <msg>
                case "ha":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        string Msg = Input.Substring(3).Replace("*", "\n");
                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose("Message from Hotel Management:\r\n" + Msg + "\r\n- " + Session.CharacterInfo.Username));
                        return true;
                    }
                #endregion
                #region :aha <msg>
                case "aha":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        string Msg = Input.Substring(4).Replace("*", "\n");
                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose("Message from Hotel Management:\r\n" + Msg));
                        return true;
                    }
                #endregion
                #region :hal <link> <msg>
                case "hal":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        string Url = Bits[1];
                        Input = Input.Substring(4).Replace(Url, "");
                        string Message = Input.Substring(1).Replace("*", "\n");

                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose("Message from Hotel Management:\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username, Url));
                        return true;
                    }
                #endregion
                #region :msgstaff <msg>
                case "msgstaff":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        string Msg = Input.Substring(9).Replace("*", "\n");
                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose("Message from Staff User: " + Session.CharacterInfo.Username + "\r\n" + Msg), "moderation_tool");
                        return true;
                    }
                #endregion
                #region :roomalert <msg>
                case "roomalert":
                    {
                        RoomInstance RoomInstance = RoomManager.GetInstanceByRoomId(Session.CurrentRoomId);
                        if (!Session.HasRight("moderation_tool") || RoomInstance == null)
                        {
                            return false;
                        }

                        string Msg = Input.Substring(10).Replace("*", "\n");
                        foreach (RoomActor RoomActor in RoomInstance.Actors)
                        {
                            if(!RoomActor.IsBot)
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                                TargetSession.SendData(NotificationMessageComposer.Compose("Message from Hotel Management:\r\n" + Msg + "\r\n- " + Session.CharacterInfo.Username));
                            }
                        }

                        return true;
                    }
                #endregion
                #region :kick <username>
                case "kick":
                    {
                        if (!Session.HasRight("moderation_tool"))
                        {
                            return false;
                        }

                        if (Bits.Length < 2)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Invalid syntax - :kick <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("moderation_tool") || !TargetSession.InRoom)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' is offline, not in a room, or cannot be kicked.", 0, ChatType.Whisper));
                            return true;
                        }

                        RoomManager.RemoveUserFromRoom(TargetSession, true);
                        TargetSession.SendData(NotificationMessageComposer.Compose("You have been kicked from the room by a community staff member."));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Kicked user from room (chat command)",
                                "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ").");
                        }

                        return true;
                    }
                #endregion
                #region :roomunmute
                case "roomunmute":
                    {
                        if (!Session.HasRight("mute"))
                        {
                            return false;
                        }

                        if (Instance.RoomMuted)
                        {
                            Instance.RoomMuted = false;
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "The current room has been unmuted successfully.", 0, ChatType.Whisper));
                        }
                        else
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "This room is not muted.", 0, ChatType.Whisper));
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Unmuted room", "Room '"
                                + Instance.Info.Name + "' (ID " + Instance.RoomId + ")");
                        }

                        return true;
                    }
                #endregion
                #region :roommute
                case "roommute":
                    {
                        if (!Session.HasRight("mute"))
                        {
                            return false;
                        }

                        if (!Instance.RoomMuted)
                        {
                            Instance.RoomMuted = true;
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "The current room has been muted successfully.", 0, ChatType.Whisper));
                        }
                        else
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "This room is already muted.", 0, ChatType.Whisper));
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted room", "Room '"
                                + Instance.Info.Name + "' (ID " + Instance.RoomId + ")");
                        }

                        return true;
                    }
                #endregion
                #region :unmute <username>
                case "unmute":
                    {
                        if (!Session.HasRight("mute"))
                        {
                            return false;
                        }

                        if (Bits.Length < 2)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Invalid syntax - :unmute <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());

                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' does not exist or is not online.", 0, ChatType.Whisper));
                            return true;
                        }

                        if (!TargetSession.CharacterInfo.IsMuted)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' is not muted.", 0, ChatType.Whisper));
                            return true;
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            TargetSession.CharacterInfo.Unmute(MySqlClient);
                        }

                        TargetSession.SendData(NotificationMessageComposer.Compose("You have been unmuted. Please reload the room."));
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' was successfully unmuted.", 0, ChatType.Whisper));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Unmuted user",
                                "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ").");
                        }

                        return true;
                    }
                #endregion
                #region :mute <username>
                case "mute":
                    {
                        if (!Session.HasRight("mute"))
                        {
                            return false;
                        }

                        if (Bits.Length < 2)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Invalid syntax - :mute <username> [length in seconds]", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        int TimeToMute = 0;

                        if (Bits.Length >= 3)
                        {
                            int.TryParse(Bits[2], out TimeToMute);
                        }

                        if (TimeToMute <= 0)
                        {
                            TimeToMute = 300;
                        }

                        if (TimeToMute > 3600)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "The maximum mute time is one hour.", 0, ChatType.Whisper));
                            return true;
                        }

                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("mute"))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' does not exist, is not online, or cannot be muted.", 0, ChatType.Whisper));
                            return true;
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            TargetSession.CharacterInfo.Mute(MySqlClient, TimeToMute);
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted user",
                                "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ") for " + TimeToMute + " seconds.");
                        }

                        TargetSession.SendData(RoomMutedComposer.Compose(TimeToMute));
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, "User '" + Username + "' has been muted successfully for " + TimeToMute + " seconds.", 0, ChatType.Whisper));
                        return true;
                    }
                #endregion
                #region :t
                case "t":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        Session.SendData(NotificationMessageComposer.Compose("Position: " + Actor.Position.ToString() + ", Rotation: " + Actor.BodyRotation));
                        return true;
                    }
                #endregion
                #region :clipping
                case "clipping":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        Actor.OverrideClipping = !Actor.OverrideClipping;
                        Actor.ApplyEffect(Actor.ClippingEnabled ? 0 : 23);
                        Session.CurrentEffect = 0;
                        return true;
                    }
                #endregion
                #region :update_catalog
                case "update_catalog":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Snowlight.Game.Catalog.CatalogManager.RefreshCatalogData(MySqlClient);
                        }
                        Session.SendData(NotificationMessageComposer.Compose("Catalog Reloaded"));
                        return true;
                    }
                #endregion
                #region :update_items
                case "update_items":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            Snowlight.Game.Items.ItemDefinitionManager.Initialize(MySqlClient);
                        }
                        Session.SendData(NotificationMessageComposer.Compose("Items reloaded"));
                        return true;
                    }
                #endregion
                #region :superkick <username>
                case "superkick":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        if (Bits.Length < 2)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Invalid syntax - :kick <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("moderation_tool") || !TargetSession.InRoom)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, "Target user '" + Username + "' is offline or cannot be kicked.", 0, ChatType.Whisper));
                            return true;
                        }

                        SessionManager.StopSession(TargetSession.Id);

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Superkicked user from server (chat command)",
                               "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ").");
                        }

                        return true;
                    }
                #endregion
                #endregion

                #region Vip
                #region :emptyinv <yes>
                case "emptyinv":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            return false;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose("Are you sure you want to clear your inventory ? You will lose all the furniture!\n" +
                            "To confirm, type \":emptyinv yes\". \n\nOnce you do this, there is no going back!\n(If you do not want to empty it, just ignore this message!)"));
                        }
                        else
                        {
                            if (Bits.Length == 2 && Bits[1].ToString() == "yes")
                            {
                                Session.InventoryCache.ClearAndDeleteAll();
                                Session.SendData(InventoryRefreshComposer.Compose());
                                Session.SendData(NotificationMessageComposer.Compose("Your inventory has been emptied."));
                            }
                            else if (Bits.Length == 2 && Bits[1].ToString() != "yes")
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, "To confirm, you must type in :emptyinv yes", 0, ChatType.Whisper));
                            }
                        }
                        return true;
                    }
                #endregion
                #region :emptypets <yes>
                case "emptypets":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            return false;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose("Are you sure you want to clear your pet inventory ? You will lose all your cute pets!\n" +
                            "To confirm, type \":emptypets yes\". \n\nOnce you do this, there is no going back!\n(If you do not want to empty it, just ignore this message!)"));
                        }
                        else
                        {
                            if (Bits.Length == 2 && Bits[1].ToString() == "yes")
                            {
                                foreach (Pet Pet in Session.PetInventoryCache.Pets.Values)
                                {
                                    Session.SendData(InventoryPetRemovedComposer.Compose(Pet.Id));
                                    using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                    {
                                        Session.PetInventoryCache.ReloadCache(MySqlClient);
                                    }
                                }
                                Session.PetInventoryCache.ClearAndDeleteAll();
                                Session.SendData(NotificationMessageComposer.Compose("Your pets inventory has been emptied."));
                            }
                            else if (Bits.Length == 2 && Bits[1].ToString() != "yes")
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, "To confirm, you must type in :emptypets yes", 0, ChatType.Whisper));
                            }
                        }
                        return true;
                    }
                #endregion
                #region :status
                case "status":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            return false;
                        }

                        List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();
                        DateTime Now = DateTime.Now;
                        TimeSpan Uptime = (Now - Program.ServerStarted);
                        Session.SendData(NotificationMessageComposer.Compose(string.Concat(new object[]
                        {
                            "Server uptime is " + Uptime.Days + " day(s), " + Uptime.Hours + " hour(s), " + Uptime.Minutes + " minute(s) and " + Uptime.Seconds + " second(s)",
                            "\nThere are " + OnlineUsers.Count  + " " + (OnlineUsers.Count == 1? "user online" : "users online"),
                            "\nThere are " + RoomManager.RoomInstances.Count + " " + (RoomManager.RoomInstances.Count == 1? "room loaded" : "rooms loaded"),
                            "\n\nSystem",
                            "\nCPU architecture: " + GetProcessorArchitecture().ToString().ToLower(),
                            "\nCPU cores: "+ Environment.ProcessorCount,
                            "\nMemory usage: " + (((GetTotalMemoryInBytes() / 1024d) / 1024d) / 1024d).ToString().Substring(0, 4) + " MB"
                        })));
                        return true;
                    }
                #endregion
                #region :online
                case "online":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            return false;
                        }

                        List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

                        Session.SendData(NotificationMessageComposer.Compose(String.Concat(new object[] {
                            "There are currently " + OnlineUsers.Count + " " + (OnlineUsers.Count == 1? "user" : "users") + " online.\n",
                            "List of users online:\n\n",
                            string.Join(", ", OnlineUsers)
                        })));
                        return true;
                    }
                #endregion
                #endregion

                #region Regular
                #region :commands
                case "commands":
                    {
                        StringBuilder CommandsBuilder = new StringBuilder("");
                        if(Session.HasRight("hotel_admin") || Session.HasRight("moderation_tool") || Session.HasRight("mute"))
                        {
                            CommandsBuilder.Append("The following commands are available to staff users:\n");
                            if (Session.HasRight("hotel_admin"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:ha <message> - Sends a global alert.",
                                        "\n:aha <message> - Sends anonimate a global alert.",
                                        "\n:hal <link> <message> - Sends a global alert with link.",
                                        "\n:msgstaff <message> - Sends a staff users alert.",
                                        "\n:superkick <message> - Kicks a user from hotel.",
                                        "\n:clipping - Walk wherever you want.",
                                        "\n:t - Shows the coords to you.",
                                        "\n:update_catalog - Update catalog.",
                                        "\n:update_items - Update items definitions."
                                    }));
                            }
                            if (Session.HasRight("moderation_tool"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:roomalert <message> - Sends an alert to all users in current room.",
                                        "\n:kick <username> - Kicks an user from current room."
                                    }));
                            }
                            if (Session.HasRight("mute"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:mute <username> <seconds> - Makes a user mute for time determined.",
                                        "\n:unmute <username> - Unmute the user.",
                                        "\n:roommute - Mutes the current room.",
                                        "\n:roomunmute - Unmutes the current room."
                                    }));
                            }
                            CommandsBuilder.Append("\n\n");
                        }

                        if(Session.HasRight("club_vip"))
                        {
                            CommandsBuilder.Append(string.Concat(new object[]
                                { 
                                    "The following commands are available to vip users:\n",
                                    "\n:emptypets <yes> - Empty your pets inventory.",
                                    "\n:emptyinv <yes> - Empty your items inventory.",
                                    "\n:online - Shows who are online.",
                                    "\n:status - Shows to you the stats from the server.",
                                }));
                            CommandsBuilder.Append("\n\n");
                        }

                        if (Session.HasRight("club_regular"))
                        {
                            CommandsBuilder.Append(string.Concat(new object[]
                                {
                                    "The following commands are available to club users:\n",
                                    "\n:chooser - Show all users on current room.",
                                    "\n:furni - Show all furnitures on current room.",
                                }));
                            CommandsBuilder.Append("\n\n");
                        }

                        CommandsBuilder.Append(string.Concat(new object[] 
                        { 
                            "The following commands are available to regular users:\n",
                            "\n:commands - Shows which commands are available to you.",
                            "\n:about - Shows you which server is behind this retro.",
                            "\n:pickall - Picks up all furniture from your room."
                        }));

                        if (Session.HasRight("hotel_admin") || Session.HasRight("moderation_tool") || Session.HasRight("mute") || Session.HasRight("club_vip"))
                        {
                            Session.SendData(MessageOfTheDayComposer.Compose(CommandsBuilder.ToString()));
                        }
                        else
                        {
                            Session.SendData(NotificationMessageComposer.Compose(CommandsBuilder.ToString()));    
                        }

                        return true;
                    }
                #endregion
                #region :about
                case "about":
                    {
                        Session.SendData(UserAlertModernComposer.Compose("Powered by Snowlight", "This hotel is proudly powered by Snowlight, the premium open-source Habbo Hotel emulator."));
                        return true;
                    }
                #endregion
                #region :pickall
                case "pickall":
                    {
                        if (!Instance.CheckUserRights(Session, true))
                        {
                            Session.SendData(NotificationMessageComposer.Compose("You do not have rights to pickall in this room."));
                            return true;
                        }

                        Instance.PickAllToUserInventory(Session);
                        return true;
                    }
                    #endregion
                #endregion
            }

            return false;
        }
    }
}
