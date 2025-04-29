using System;

namespace Snowlight.Communication.Outgoing
{
    public static class MusicPlayingComposer
    {
        public static ServerMessage Compose(uint SongId, int PlaylistItemNumber, int SyncTimestampMs)
        {
            ServerMessage Message = new ServerMessage(OpcodesOut.MUSIC_PLAYING);

            if (SongId == 0)
            {
                Message.AppendInt32(-1);                    // Current song id
                Message.AppendInt32(-1);                    // Current song position
                Message.AppendInt32(-1);                    // Next song id
                Message.AppendInt32(-1);                    // Next song position
                Message.AppendInt32(0);                     // Sync count
            }
            else
            {
                Message.AppendUInt32(SongId);               // Current song id
                Message.AppendInt32(PlaylistItemNumber);    // Current song position
                Message.AppendUInt32(SongId);               // Next song id
                Message.AppendInt32(0);                     // Next song position
                Message.AppendInt32(SyncTimestampMs);       // Sync count
            }

            return Message;
        }
    }
}
