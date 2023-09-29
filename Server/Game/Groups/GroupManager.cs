using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Snowlight.Storage;

namespace Snowlight.Game.Groups
{
	public class GroupManager
	{
		private static Dictionary<int, Group> mGroupList;

		public static void Initialize(SqlDatabaseClient MySqlClient)
		{
			int CountLoaded = 0;
			mGroupList = new Dictionary<int, Group>();

			DataTable dataTable = MySqlClient.ExecuteQueryTable("SELECT * FROM groups");
			foreach (DataRow Row in dataTable.Rows)
			{
				mGroupList.Add((int)Row["Id"], new Group((int)Row["Id"], (string)Row["name"],
                    (string)Row["desc"], (uint)Row["owner_id"], (string)Row["badge"], (uint)Row["room_id"],
                    Group.StateFromString(Row["privacy"].ToString()), MySqlClient));

				CountLoaded++;
			}

			Output.WriteLine("Loaded " + CountLoaded + " group(s).", OutputLevel.DebugInformation);
		}

		public static void ClearGroups()
		{
			mGroupList.Clear();
		}

		public static Group GetGroup(int id)
		{
			if (mGroupList.ContainsKey(id))
			{
				return mGroupList[id];
			}

			return null;
		}

		public static ReadOnlyCollection<int> GetUserGroups(uint UserId)
        {
			List<int> GroupIds = new List<int>();
			foreach(Group Groups in mGroupList.Values)
            {
				if(Groups.MembershipList.Contains(UserId))
                {
					GroupIds.Add(Groups.Id);
                }
            }

			return GroupIds.AsReadOnly();
        }

		public static void UpdateGroup(SqlDatabaseClient MySqlClient, int GroupId)
		{
			Group group = GetGroup(GroupId);
			if (group != null)
			{
				MySqlClient.SetParameter("gid", GroupId);
				DataRow Row = MySqlClient.ExecuteQueryRow("SELECT * FROM groups WHERE id = @gid LIMIT 1");
				
				mGroupList.Remove(GroupId);
                mGroupList.Add(GroupId, new Group((int)Row["Id"], (string)Row["name"],
                    (string)Row["desc"], (uint)Row["owner_id"], (string)Row["badge"], (uint)Row["room_id"],
                    Group.StateFromString(Row["privacy"].ToString()), MySqlClient));
            }
		}
	}
}