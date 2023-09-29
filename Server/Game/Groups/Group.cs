using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Snowlight.Storage;

namespace Snowlight.Game.Groups
{
	public enum GroupState
    {
		Private = 0,
		Exclusive = 1,
        Regular = 2
    }

	public class Group
	{
		private int mId;
		private string mName;
		private string mDesc;
		private uint mOwnerId;
		private List<uint> mMemberList;
		private List<uint> mPendingRequestsList;
		private string mBadge;
		private uint mRoomId;
		private GroupState mState;

		public int Id
        {
			get
            {
				return mId;
            }
        }

		public string Name
        {
			get
            {
				return mName;
            }
        }

		public string Desc
        {
			get 
			{
				return mDesc;
			}
        }

		public ReadOnlyCollection<uint> MembershipList
        {
			get
            {
				List<uint> Copy = new List<uint>();
				Copy.AddRange(mMemberList);
				return Copy.AsReadOnly();
            }
        }
		public ReadOnlyCollection<uint> PendingRequestList
		{
			get
			{
				List<uint> Copy = new List<uint>();
				Copy.AddRange(mPendingRequestsList);
				return Copy.AsReadOnly();
			}
		}
		public string Badge
		{
			get
			{
				return mBadge;
			}
		}

		public uint RoomId
		{
			get
			{
				return mRoomId;
			}
		}

		public GroupState State
        {
			get
            {
				return mState;
            }
        }
		public Group(int Id, string Name, string Desc, uint OwnerId, string Badge, uint RoomId,
			GroupState State, SqlDatabaseClient MySqlClient)
		{
			mId = Id;
			mName = Name;
			mDesc = Desc;
			mOwnerId = OwnerId;
			mBadge = Badge;
			mRoomId = RoomId;
			mState = State;

			mMemberList = new List<uint>();
			mPendingRequestsList = new List<uint>();

			ReloadGroupMembership(MySqlClient);
		}

		public void ReloadGroupMembership(SqlDatabaseClient MySqlClient)
        {
			mMemberList.Clear();
			mPendingRequestsList.Clear();

			MySqlClient.SetParameter("gid", mId);
			MySqlClient.SetParameter("confirmed", "1");
			DataTable TableC = MySqlClient.ExecuteQueryTable("SELECT user_id FROM group_memberships WHERE group_id = @gid AND confirmed = @confirmed");
			foreach (DataRow Row in TableC.Rows)
			{
				AddMember((uint)Row[0]);
			}

			MySqlClient.SetParameter("gid", mId);
			MySqlClient.SetParameter("confirmed", "0");
			DataTable TableP = MySqlClient.ExecuteQueryTable("SELECT user_id FROM group_memberships WHERE group_id = @gid AND confirmed = @confirmed");
			foreach (DataRow Row in TableP.Rows)
			{
				AddPendingRequest((uint)Row[0]);
			}
		}

		public int ResquestStatus(uint UserId)
        {
			int status = mState > GroupState.Private ? 0 : 1;
			
			if(mPendingRequestsList.Contains(UserId))
            {
				status = 2;
            }

			if(mMemberList.Contains(UserId))
            {
				status = 1;
            }

			return status;
        }

		public bool HasPendingRequestByUserId(uint UserId)
        {
			return mPendingRequestsList.Contains(UserId);
        }

		public void AddMember(uint MemberId)
		{
			if (!mMemberList.Contains(MemberId))
			{
				mMemberList.Add(MemberId);
			}
		}

		public void RemoveMember(uint MemberId)
		{
			if (mMemberList.Contains(MemberId))
			{
				mMemberList.Remove(MemberId);
			}
		}

		public void AddPendingRequest(uint MemberId)
		{
			if (!mPendingRequestsList.Contains(MemberId))
			{
				mPendingRequestsList.Add(MemberId);
			}
		}

		public void RemovePendingRequest(uint MemberId)
		{
			if (mPendingRequestsList.Contains(MemberId))
			{
				mPendingRequestsList.Remove(MemberId);
			}
		}

		public void AddMemberInDatabase(uint UserId)
        {
			if(mState > GroupState.Private)
            {
				using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
				{
					MySqlClient.SetParameter("gid", mId);
					MySqlClient.SetParameter("uid", UserId);
					MySqlClient.SetParameter("confirmed", mState == GroupState.Exclusive ? 0 : 1);
					MySqlClient.ExecuteNonQuery("INSERT INTO group_memberships (group_id, user_id, confirmed) VALUES (@gid, @uid, @confirmed)");

					ReloadGroupMembership(MySqlClient);
				}
			}
		}

		public static GroupState StateFromString(string State)
        {
			switch (State.ToLower())
			{
				default:
				case "regular":

					return GroupState.Regular;

				case "exclusive":

					return GroupState.Exclusive;

				case "private":

					return GroupState.Private;
			}
		}
	}
}