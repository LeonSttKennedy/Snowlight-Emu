using System;
using System.Data;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Microsoft.VisualBasic;

using Snowlight.Util;
using Snowlight.Storage;
using Snowlight.Game.Bots;
using Snowlight.Game.Pets;
using Snowlight.Game.Rooms;
using Snowlight.Game.Infobus;
using Snowlight.Game.Sessions;
using Snowlight.Game.Moderation;
using Snowlight.Game.Achievements;
using Snowlight.Communication.Outgoing;


using Snowlight.Communication;
using Snowlight.Game.Rights;
using Snowlight.Game.Items;
using Snowlight.Game.Navigation;

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

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");

                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ha_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a global alert",
                               "Message: '" + Message + "'");
                        }

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

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");

                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ha_fixed_text") + "\r\n" + Message));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a anonimate global alert",
                               "Message: '" + Message + "'");
                        }

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
                        string Message = MergeParams(Bits, 2).Replace("\\n", "\n");

                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ha_fixed_text")  + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username, Url));

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a global alert with link",
                               "Message: '" + Message + "' \nLink: " + Url);
                        }

                        return true;
                    }
                #endregion
                #region :has <msg>
                case "has":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");
                        SessionManager.BroadcastPacket(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_has_fixed_text") + " " + Session.CharacterInfo.Username + "\r\n" + Message), "moderation_tool");

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent a staff alert",
                               "Message: '" + Message + "'");
                        }

                        return true;
                    }
                #endregion
                #region :ra <msg>
                case "ra":
                    {
                        if (!Session.HasRight("moderation_tool"))
                        {
                            return false;
                        }

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");
                        foreach (RoomActor RoomActor in Instance.Actors)
                        {
                            if (!RoomActor.IsBot)
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ra_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username));
                            }
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an alert to a room",
                               "Message: '" + Message + "'");
                        }

                        return true;
                    }
                #endregion
                #region :ara <msg>
                case "ara":
                    {
                        if (!Session.HasRight("moderation_tool"))
                        {
                            return false;
                        }

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");
                        foreach (RoomActor RoomActor in Instance.Actors)
                        {
                            if (!RoomActor.IsBot)
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ra_fixed_text") + "\r\n" + Message));
                            }
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an anonimate alert to a room",
                               "Message: '" + Message + "'");
                        }

                        return true;
                    }
                #endregion
                #region :ral <link> <msg>
                case "ral":
                    {
                        if (!Session.HasRight("moderation_tool"))
                        {
                            return false;
                        }
                        string Url = Bits[1];
                        string Message = MergeParams(Bits, 2).Replace("\\n", "\n");

                        foreach (RoomActor RoomActor in Instance.Actors)
                        {
                            if (!RoomActor.IsBot)
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                                TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ra_fixed_text") + "\r\n" + Message + "\r\n- " + Session.CharacterInfo.Username, Url));
                            }
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an alert to a room with link",
                               "Message: '" + Message + "' \nLink: " + Url);
                        }
                        return true;
                    }
                #endregion
                #region :ras <msg>
                case "ras":
                    {
                        if (!Session.HasRight("moderation_tool"))
                        {
                            return false;
                        }

                        string Message = MergeParams(Bits, 1).Replace("\\n", "\n");
                        foreach (RoomActor RoomActor in Instance.Actors)
                        {
                            if (!RoomActor.IsBot)
                            {
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(RoomActor.Name));
                                if (TargetSession.HasRight("moderation_tool"))
                                {
                                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_ras_fixed_text") + " " + Session.CharacterInfo.Username + "\r\n" + Message));
                                }
                            }
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Sent an alert to the staff's in a room",
                               "Message: '" + Message + "'");
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :kick <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("moderation_tool") || !TargetSession.InRoom)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_kick_error", Username), 0, ChatType.Whisper));
                            return true;
                        }

                        RoomManager.RemoveUserFromRoom(TargetSession, true);
                        TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_kick_targetuser_message")));

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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :unmute <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());

                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_unmute_error", Username), 0, ChatType.Whisper));
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :mute <username> [length in seconds]", 0, ChatType.Whisper));
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_maxtime"), 0, ChatType.Whisper));
                            return true;
                        }

                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("mute"))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_error", Username), 0, ChatType.Whisper));
                            return true;
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            TargetSession.CharacterInfo.Mute(MySqlClient, TimeToMute);
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Muted user",
                                "User '" + TargetSession.CharacterInfo.Username + "' (ID " + TargetSession.CharacterId + ") for " + TimeToMute + " seconds.");
                        }

                        TargetSession.SendData(RoomMutedComposer.Compose(TimeToMute));
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mute_success", new string[] { Username, TimeToMute.ToString() }), 0, ChatType.Whisper));
                        return true;
                    }
                #endregion
                #region :coords
                case "coords":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_coords_text", new string[] { Actor.Position.ToString(), Actor.BodyRotation.ToString() })));
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
                #region :update <variable>
                case "update":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :update <a thing to update>", 0, ChatType.Whisper));
                            return true;
                        }

                        string ToSend = string.Empty;
                        string ToUpdate = Bits[1];
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            switch (ToUpdate.ToLower())
                            {
                                #region Development update commands
                                case "catalog_order":
                                    {
                                        // parent_id = 2 ( Pixels furniture catalog tab )
                                        DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM catalog ORDER BY title ASC");
                                        int i = 1;
                                        foreach (DataRow Row in Table.Rows)
                                        {
                                            if ((int)Row["parent_id"] == 2)
                                            {
                                                MySqlClient.SetParameter("order", i);
                                                MySqlClient.SetParameter("id", (int)Row["id"]);
                                                MySqlClient.ExecuteNonQuery("UPDATE catalog SET order_num = @order WHERE id = @id");
                                                i += 1;
                                            }
                                        }
                                        Session.SendData(NotificationMessageComposer.Compose("Catalog order updated!"));
                                        return true;
                                    }
                                #endregion

                                case "achievements":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_achievements");
                                        AchievementManager.ReloadAchievements(MySqlClient);
                                        goto End;
                                    }

                                case "catalog":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_catalog");
                                        Snowlight.Game.Catalog.CatalogManager.RefreshCatalogData(MySqlClient);
                                        goto End;
                                    }

                                case "filter":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_filter");
                                        ChatWordFilter.Initialize(MySqlClient);
                                        goto End;
                                    }

                                case "items":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_items");
                                        Snowlight.Game.Items.ItemDefinitionManager.Initialize(MySqlClient);
                                        goto End;
                                    }

                                case "navigator":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_navigator");
                                        Navigator.ReloadFlatCategories(MySqlClient);
                                        Navigator.ReloadOfficialItems(MySqlClient);
                                        goto End;
                                    }

                                case "serversettings":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_serversettings");
                                        ServerSettings.Initialize(MySqlClient);
                                        goto End;
                                    }

                                case "texts":
                                    {
                                        ToSend = ExternalTexts.GetValue("command_update_texts");
                                        ExternalTexts.Initialize(MySqlClient);
                                        goto End;
                                    }

                                default:
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_update_error", ToUpdate), 0, ChatType.Whisper));
                                        return true;
                                    }
                            }
                        }

                        End:
                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_update_success", ToSend), 0, ChatType.Whisper));
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :superkick <username>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));

                        if (TargetSession == null || TargetSession.HasRight("moderation_tool") || !TargetSession.InRoom)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_superkick_error", Username), 0, ChatType.Whisper));
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
                #region :directbadge <username> <code>
                case "directbadge":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        // Verify if username or badge code is empty
                        if (Bits.Length < 3)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :directbadge <username> <badge code>", 0, ChatType.Whisper));
                            return true;
                        }

                        string Username = UserInputFilter.FilterString(Bits[1].Trim());
                        string BadgeCode = UserInputFilter.FilterString(Bits[2].Trim());

                        // Verify if badge code is in database
                        Badge BadgeToGive = RightsManager.GetBadgeByCode(BadgeCode);
                        if (BadgeToGive == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_badge_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        // Verify the user session is connected
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
                        if (TargetSession == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_targetuser_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        // Getting the current room instance the user is in
                        RoomInstance TargetInstance = RoomManager.GetInstanceByRoomId(TargetSession.CurrentRoomId);
                        if (TargetInstance == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_instance_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        // Just to confirm if user is in a room
                        RoomActor TargetActor = TargetInstance.GetActorByReferenceId(TargetSession.CharacterId);
                        if (TargetActor == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_actor_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        // Also verify if badge has rights set, after this verification, executes the addition of badge
                        if (RightsManager.GetRightsForBadge(BadgeToGive).Count == 0)
                        {
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                if (!TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                {
                                    TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, "static");
                                    TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, BadgeToGive.Id);
                                    TargetSession.SendData(InventoryNewItemsComposer.Compose(4, BadgeToGive.Id));

                                    TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);

                                    TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                                    TargetSession.SendData(RoomChatComposer.Compose(TargetActor.Id, ExternalTexts.GetValue("command_dmbadge_targetuser_success"), 1, ChatType.Whisper));

                                    Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_directbadge_success")));
                                    ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given a badge",
                                        Session.CharacterInfo.Username + " had give a badge ( " + BadgeToGive.Code + " ) to " + TargetSession.CharacterInfo.Username);
                                }
                                else if (TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                {
                                    Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_directbadge_error"), 0, ChatType.Whisper));
                                }
                            }
                        }
                        else
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_rights_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        return true;
                    }
                #endregion
                #region :massbadge <method> <badge code>
                case "massbadge":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        if (Bits.Length < 3)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :massbadge <method: room or all> <badge code>", 0, ChatType.Whisper));
                            return true;
                        }

                        string BadgeCode = UserInputFilter.FilterString(Bits[2].Trim());

                        // Verify if badge code is in database
                        Badge BadgeToGive = RightsManager.GetBadgeByCode(BadgeCode);
                        if (BadgeToGive == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_badge_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        string Method = UserInputFilter.FilterString(Bits[1].Trim());
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            if (RightsManager.GetRightsForBadge(BadgeToGive).Count == 0)
                            {
                                switch (Method.ToLower())
                                {
                                    case "room":
                                        {
                                            foreach (RoomActor RoomActors in Instance.Actors)
                                            {
                                                if (!RoomActors.IsBot)
                                                {
                                                    Session TargetSession = SessionManager.GetSessionByCharacterId(RoomActors.ReferenceId);
                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        if (!TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                                        {
                                                            TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, "static");
                                                            TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, BadgeToGive.Id);
                                                            TargetSession.SendData(InventoryNewItemsComposer.Compose(4, BadgeToGive.Id));

                                                            TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);

                                                            TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                                                            TargetSession.SendData(RoomChatComposer.Compose(RoomActors.Id, ExternalTexts.GetValue("command_dmbadge_targetuser_success"), 1, ChatType.Whisper));
                                                        }
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    case "all":
                                        {
                                            Dictionary<uint, Session> Sessions = SessionManager.Sessions;
                                            foreach (Session TargetSession in Sessions.Values)
                                            {
                                                if (TargetSession.Authenticated && !TargetSession.Stopped)
                                                {
                                                    if (TargetSession.CharacterId != Session.CharacterId)
                                                    {
                                                        if (!TargetSession.BadgeCache.Badges.Contains(BadgeToGive))
                                                        {
                                                            TargetSession.BadgeCache.UpdateAchievementBadge(MySqlClient, BadgeToGive.Code, BadgeToGive, "static");
                                                            TargetSession.NewItemsCache.MarkNewItem(MySqlClient, 4, BadgeToGive.Id);
                                                            TargetSession.SendData(InventoryNewItemsComposer.Compose(4, BadgeToGive.Id));

                                                            TargetSession.BadgeCache.ReloadCache(MySqlClient, TargetSession.AchievementCache);

                                                            TargetSession.SendData(UserBadgeInventoryComposer.Compose(TargetSession.BadgeCache.Badges, TargetSession.BadgeCache.EquippedBadges));
                                                            TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_dmbadge_targetuser_success") + "\r\n- " + Session.CharacterInfo.Username));
                                                        }
                                                    }
                                                }
                                            }
                                            goto End;
                                        }

                                    default:
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_massbadge_method_error"), 0, ChatType.Whisper));
                                            return true;
                                        }
                                }
                            }
                            else
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_dmbadge_rights_error"), 0, ChatType.Whisper));
                                return true;
                            }

                            End:
                            string ToSend = (Method.Equals("room") ? "command_massbadge_room_success" : "command_massbadge_all_success");
                            string ToMethod = (Method.Equals("room") ? "to room: " + Instance.Info.Name + " (ID: " + Instance.Info.Id + ")." : "to all online users.");
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue(ToSend)));
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given a badge",
                                Session.CharacterInfo.Username + " had give a badge ( " + BadgeToGive.Code + " ) " + ToMethod);
                        }

                        return true;
                    }
                #endregion
                #region :directgive <username> <type> <amount>
                case "directgive":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        if (Bits.Length < 4)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :directgive <username> <type: coins/credits or pixels> <quantity>", 0, ChatType.Whisper));
                            return true;
                        }

                        int Amount;
                        string Username = UserInputFilter.FilterString(Bits[1].Trim());

                        // Verify the user session is connected
                        Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
                        if (TargetSession == null)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_targetuser_error"), 0, ChatType.Whisper));
                            return true;
                        }

                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            string Type = UserInputFilter.FilterString(Bits[2].Trim());
                            switch (Type.ToLower())
                            {
                                case "coins":
                                case "credits":
                                    {
                                        if (int.TryParse(Bits[3], out Amount))
                                        {
                                            TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                            TargetSession.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                            TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                            goto End;
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                            return true;
                                        }
                                    }

                                case "pixels":
                                    {
                                        if (int.TryParse(Bits[3], out Amount))
                                        {
                                            TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Amount);
                                            TargetSession.SendData(ActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPointsBalance, Amount));
                                            TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Type }) + "\r\n- " + Session.CharacterInfo.Username));
                                            goto End;
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                            return true;
                                        }
                                    }

                                default:
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Type), 0, ChatType.Whisper));
                                        return true;
                                    }
                            }

                            End:
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_success", new string[] { Amount.ToString(), Type, TargetSession.CharacterInfo.Username }), 0, ChatType.Whisper));
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given " + Type,
                                Session.CharacterInfo.Username + " had give " + Amount + " " + Type + " to " + TargetSession.CharacterInfo.Username);

                        }

                        return true;
                    }
                #endregion
                #region :massgive <method> <type> <quantity>
                case "massgive":
                    {
                        if (!Session.HasRight("hotel_admin"))
                        {
                            return false;
                        }

                        if (Bits.Length < 4)
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_invalid_syntax") + " - :massgive <method: room or all> <type: coins/credits or pixels> <quantity>", 0, ChatType.Whisper));
                            return true;
                        }

                        int Amount;
                        string Type = UserInputFilter.FilterString(Bits[1].Trim());
                        string Currency = UserInputFilter.FilterString(Bits[2].Trim());


                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            switch (Currency.ToLower())
                            {
                                case "credits":
                                case "coins":
                                    {
                                        if (int.TryParse(Bits[3], out Amount))
                                        {
                                            switch (Type.ToLower())
                                            {
                                                case "room":
                                                    {
                                                        foreach (RoomActor RoomActors in Instance.Actors)
                                                        {
                                                            if (!RoomActors.IsBot)
                                                            {
                                                                Session TargetSession = SessionManager.GetSessionByCharacterId(RoomActors.ReferenceId);

                                                                if (TargetSession.CharacterId != Session.CharacterId)
                                                                {
                                                                    TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                                                    TargetSession.SendData(CreditsBalanceComposer.Compose(TargetSession.CharacterInfo.CreditsBalance));
                                                                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                                }
                                                            }
                                                        }
                                                        goto End;
                                                    }

                                                case "all":
                                                    {
                                                        Dictionary<uint, Session> Sessions = SessionManager.Sessions;
                                                        foreach (Session TargetSession in Sessions.Values)
                                                        {
                                                            if (TargetSession.Authenticated && !TargetSession.Stopped)
                                                            {
                                                                if (TargetSession.CharacterId != Session.CharacterId)
                                                                {
                                                                    TargetSession.CharacterInfo.UpdateCreditsBalance(MySqlClient, Amount);
                                                                    TargetSession.SendData(CreditsBalanceComposer.Compose(TargetSession.CharacterInfo.CreditsBalance));
                                                                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                                }
                                                            }
                                                        }
                                                        goto End;
                                                    }

                                                default:
                                                    {
                                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Type), 0, ChatType.Whisper));
                                                        return true;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                            return true;
                                        }
                                    }

                                case "pixels":
                                    {
                                        if (int.TryParse(Bits[3], out Amount))
                                        {
                                            switch (Type.ToLower())
                                            {
                                                case "room":
                                                    {
                                                        foreach (RoomActor RoomActors in Instance.Actors)
                                                        {
                                                            if (!RoomActors.IsBot)
                                                            {
                                                                Session TargetSession = SessionManager.GetSessionByCharacterId(RoomActors.ReferenceId);

                                                                if (TargetSession.CharacterId != Session.CharacterId)
                                                                {
                                                                    TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Amount);
                                                                    TargetSession.SendData(ActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPointsBalance, Amount));
                                                                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                                }
                                                            }
                                                        }
                                                        goto End;
                                                    }

                                                case "all":
                                                    {
                                                        Dictionary<uint, Session> Sessions = SessionManager.Sessions;
                                                        foreach (Session TargetSession in Sessions.Values)
                                                        {
                                                            if (TargetSession.Authenticated && !TargetSession.Stopped)
                                                            {
                                                                if (TargetSession.CharacterId != Session.CharacterId)
                                                                {
                                                                    TargetSession.CharacterInfo.UpdateActivityPointsBalance(MySqlClient, Amount);
                                                                    TargetSession.SendData(ActivityPointsBalanceComposer.Compose(TargetSession.CharacterInfo.ActivityPointsBalance, Amount));
                                                                    TargetSession.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_give_targetuser_success", new string[] { Amount.ToString(), Currency }) + "\r\n- " + Session.CharacterInfo.Username));
                                                                }
                                                            }
                                                        }
                                                        goto End;
                                                    }

                                                default:
                                                    {
                                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_method_error", Type), 0, ChatType.Whisper));
                                                        return true;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_quantity_error"), 0, ChatType.Whisper));
                                            return true;
                                        }
                                    }

                                default:
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_give_currency_error", Currency), 0, ChatType.Whisper));
                                        return true;
                                    }
                            }
                            End:
                            string ToSend = (Type.Equals("room") ? "command_massgive_room_success" : "command_massgive_all_success");
                            string ToMethod = (Type.Equals("room") ? "to room: " + Instance.Info.Name + " (ID: " + Instance.Info.Id + ")." : "to all online users.");
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue(ToSend), 0, ChatType.Whisper));
                            ModerationLogs.LogModerationAction(MySqlClient, Session, "Had given " + Type,
                                Session.CharacterInfo.Username + " had give " + Amount + " " + Currency + " " + ToMethod);
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptyinv_info")));
                        }
                        else
                        {
                            if (Bits.Length == 2 && Bits[1].ToString() == "yes")
                            {
                                Session.InventoryCache.ClearAndDeleteAll();
                                Session.SendData(InventoryRefreshComposer.Compose());
                                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptyinv_success")));
                            }
                            else if (Bits.Length == 2 && Bits[1].ToString() != "yes")
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_emptyinv_confirm"), 0, ChatType.Whisper));
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptypets_info")));
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
                                Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_emptypets_success")));
                            }
                            else if (Bits.Length == 2 && Bits[1].ToString() != "yes")
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_emptypets_confirm"), 0, ChatType.Whisper));
                            }
                        }
                        return true;
                    }
                #endregion
                #region :redeemcoins <yes>
                case "redeemcoins":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        if (Instance.TradeManager.UserHasActiveTrade(Session.CharacterId))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_trade_disable"), 4, ChatType.Whisper));
                            return true;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_redeemcoins_info")));
                        }
                        else
                        {
                            if (Bits.Length == 2 && Bits[1].ToString() == "yes")
                            {
                                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                                {
                                    int CoinsAmount = 0;

                                    foreach (Item Items in Session.InventoryCache.Items.Values)
                                    {
                                        if (Items.Definition.Behavior == ItemBehavior.ExchangeItem && Items.RoomId == 0)
                                        {
                                            CoinsAmount += Items.Definition.BehaviorData;
                                            Session.InventoryCache.RemoveItem(Items.Id);
                                            Items.RemovePermanently(MySqlClient);
                                        }
                                    }

                                    Session.InventoryCache.ReloadCache(MySqlClient);
                                    Session.SendData(InventoryRefreshComposer.Compose());

                                    if (CoinsAmount > 0)
                                    {
                                        Session.CharacterInfo.UpdateCreditsBalance(MySqlClient, CoinsAmount);
                                        Session.SendData(CreditsBalanceComposer.Compose(Session.CharacterInfo.CreditsBalance));
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_success", CoinsAmount.ToString()), 0, ChatType.Whisper));
                                    }
                                    else
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_error"), 4, ChatType.Whisper));
                                    }
                                }
                            }
                            else if (Bits.Length == 2 && Bits[1].ToString() != "yes")
                            {
                                Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_redeemcoins_confirm"), 0, ChatType.Whisper));
                            }
                        }
                        return true;
                    }
                #endregion
                #region :mimic <username>
                case "mimic":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        if (Bits.Length == 1)
                        {
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_mimic_info")));
                            return true;
                        }
                        else
                        {
                            using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                            {
                                string Username = Bits[1];
                                DataRow Data = null;
                                Session TargetSession = SessionManager.GetSessionByCharacterId(CharacterResolverCache.GetUidFromName(Username));
                                if (TargetSession == null)
                                {
                                    MySqlClient.SetParameter("username", Username);
                                    Data = MySqlClient.ExecuteQueryRow("SELECT * FROM characters WHERE username LIKE @username LIMIT 1");
                                    if (Data == null)
                                    {
                                        Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_mimic_error", Username), 4, ChatType.Whisper));
                                        return true;
                                    }
                                }

                                Session.CharacterInfo.UpdateFigure(MySqlClient,
                                    (Data != null ? (string)Data["gender"] : TargetSession.CharacterInfo.Gender.ToString()),
                                    (Data != null ? UserInputFilter.FilterString((string)Data["figure"]) : TargetSession.CharacterInfo.Figure));

                                Session.SendInfoUpdate();
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
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        // Total online players peak
                        int alltime = 0;
                        int daily = 0;
                        using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                        {
                            string alltimeplayerpeak = MySqlClient.ExecuteScalar("SELECT all_time_player_peak FROM server_statistics LIMIT 1").ToString();
                            int.TryParse(alltimeplayerpeak, out alltime);

                            string dailyplayerpeak = MySqlClient.ExecuteScalar("SELECT daily_player_peak FROM server_statistics LIMIT 1").ToString();
                            int.TryParse(dailyplayerpeak, out daily);
                        }

                        // Total users online
                        List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

                        // Total server online time
                        DateTime Now = DateTime.Now;
                        TimeSpan Uptime = Now - Program.ServerStarted;

                        string[] Args = { Uptime.Days.ToString(), Uptime.Hours.ToString(), Uptime.Minutes.ToString(), Uptime.Seconds.ToString(),
                            OnlineUsers.Count.ToString(), RoomManager.RoomInstances.Count.ToString(), daily.ToString(), alltime.ToString(),
                            GetProcessorArchitecture().ToString().ToLower(), Environment.ProcessorCount.ToString(),
                            Math.Round(GetTotalMemoryInBytes() / 1024d / 1024d / 1024d, 2).ToString()
                        };

                        Session.SendData(NotificationMessageComposer.Compose(string.Concat(new object[]
                        {
                            "[SERVER]",
                            "\nUptime: " + Args[0] + " day(s), " + Args[1] + " hour(s), " + Args[2] + " minute(s) and " + Args[3] + " second(s)",
                            "\nThere are " + Args[4]  + " user(s) online",
                            "\nThere are " + Args[5] + " room(s) loaded",
                            "\nDaily player peak: " + Args[6],
                            "\nAll time player peak: " + Args[7],
                            "\n\n",
                            "[SYSTEM]",
                            "\nCPU architecture: " + Args[8],
                            "\nCPU cores: "+ Args[9],
                            "\nMemory usage: " + Args[10] + " MB"
                        })));
                        return true;
                    }
                #endregion
                #region :online
                case "online":
                    {
                        if (!Session.HasRight("club_vip"))
                        {
                            Session.SendData(RoomChatComposer.Compose(Actor.Id, ExternalTexts.GetValue("command_vip_error"), 4, ChatType.Whisper));
                            return true;
                        }

                        List<string> OnlineUsers = SessionManager.ConnectedUserData.Values.ToList();

                        Session.SendData(NotificationMessageComposer.Compose(String.Concat(new object[] {
                            "There are currently " + OnlineUsers.Count + " user(s) online.\n",
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
                        if (Session.HasRight("hotel_admin") || Session.HasRight("moderation_tool") || Session.HasRight("mute"))
                        {
                            CommandsBuilder.Append(ExternalTexts.GetValue("command_staff_user_list") + "\n");
                            if (Session.HasRight("hotel_admin"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:ha <message> - Sends a global alert.",
                                        "\n:aha <message> - Sends an anonimate global alert.",
                                        "\n:hal <link> <message> - Sends a global alert with link.",
                                        "\n:has <message> - Sends a staff users alert.",
                                        "\n:superkick <message> - Kicks a user from hotel.",
                                        "\n:clipping - Walk wherever you want.",
                                        "\n:coords - Shows the coords to you.",
                                        "\n:update <variable> - Reloads a specific part of the hotel.",
                                        "\n:directbadge <username> <code> - Gives to a single user an badge.",
                                        "\n:massbadge <method> <code> - Gives to all users an badge.",
                                        "\n:directgive <username> <type> <amount> - Gives to a user an amount of coin.",
                                        "\n:massgive <method> <type> <amount> - Give forall users an amount of coin.",
                                    }));
                            }
                            if (Session.HasRight("moderation_tool"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:ra <message> - Sends an alert to all users in current room.",
                                        "\n:ara <message> - Sends an anonimate alert to all users in current room.",
                                        "\n:ral <link> <message> - Sends an alert to all users in current room with link.",
                                        "\n:ras <message> - Sends an alert to all staff users in current room.",
                                        "\n:kick <username> - Kicks an user from current room.",
                                    }));
                            }
                            if (Session.HasRight("mute"))
                            {
                                CommandsBuilder.Append(string.Concat(new object[]
                                    {
                                        "\n:mute <username> <seconds> - Makes a user mute for time determined.",
                                        "\n:unmute <username> - Unmute the user.",
                                        "\n:roommute - Mutes the current room.",
                                        "\n:roomunmute - Unmutes the current room.",
                                    }));
                            }
                            CommandsBuilder.Append("\n\n");
                        }

                        if (Session.HasRight("club_vip"))
                        {
                            CommandsBuilder.Append(string.Concat(new object[]
                                {
                                    ExternalTexts.GetValue("command_vip_user_list") + "\n",
                                    "\n:emptypets <yes> - Empty your pets inventory.",
                                    "\n:emptyinv <yes> - Empty your items inventory.",
                                    "\n:redeemcoins <yes> - Turns all exchange items in your hand back into coins.",
                                    "\n:mimic <username> - Allows you to copy the look from another user.",
                                    "\n:online - Shows who are online.",
                                    "\n:status - Shows to you the stats from the server.",
                                }));
                            CommandsBuilder.Append("\n\n");
                        }

                        if (Session.HasRight("club_regular"))
                        {
                            CommandsBuilder.Append(string.Concat(new object[]
                                {
                                    ExternalTexts.GetValue("command_club_user_list") + "\n",
                                    "\n:chooser - Show all users on current room.",
                                    "\n:furni - Show all furnitures on current room.",
                                }));
                            CommandsBuilder.Append("\n\n");
                        }

                        CommandsBuilder.Append(string.Concat(new object[]
                        {
                            ExternalTexts.GetValue("command_regular_user_list") + "\n",
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
                            Session.SendData(NotificationMessageComposer.Compose(ExternalTexts.GetValue("command_pickall_error")));
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
