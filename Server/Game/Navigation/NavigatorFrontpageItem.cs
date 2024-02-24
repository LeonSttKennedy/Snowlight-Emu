using System;
using System.Collections.Generic;
using System.Text;

using Snowlight.Game.Rooms;
using Snowlight.Communication;

namespace Snowlight.Game.Navigation
{
    public enum NavigatorOfficialItemDisplayType
    {
        Banner = 0,
        Detailed = 1,
    }

    public enum NavigatorOfficialItemImageType
    {
        Internal = 0,
        External = 1
    }

    public class NavigatorOfficialItem
    {
        private uint mId;
        private uint mParentId;
        private uint mRoomId;
        private bool mIsCategory;
        private bool mIsAdvertsement;
        private NavigatorOfficialItemDisplayType mDisplayType;
        private string mName;
        private string mDescr;
        private NavigatorOfficialItemImageType mImageType;
        private string mImage;
        private string mBannerLabel;
        private string mSearchTag;
        private bool mCategoryAutoExpand;

        private List<uint> mConnectedRooms;

        public uint Id
        {
            get
            {
                return mId;
            }
        }

        public uint ParentId
        {
            get
            {
                return mParentId;
            }
        }

        public uint RoomId
        {
            get
            {
                return mRoomId;
            }
        }

        public bool IsCategory
        {
            get
            {
                return mIsCategory;
            }
        }

        public bool IsAdvertsement
        {
            get
            {
                return mIsAdvertsement;
            }
        }

        public NavigatorOfficialItemDisplayType DisplayType
        {
            get
            {
                return mDisplayType;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string Descr
        {
            get
            {
                return mDescr;
            }
        }

        public NavigatorOfficialItemImageType ImageType
        {
            get
            {
                return mImageType;
            }
        }

        public string Image
        {
            get
            {
                return mImage;
            }
        }

        public string BannerLabel
        {
            get
            {
                return mBannerLabel;
            }
        }

        public string SearchTag
        {
            get
            {
                return mSearchTag;
            }
        }

        public bool CategoryAutoExpand
        {
            get
            {
                return mCategoryAutoExpand;
            }
        }

        public List<uint> ConnectedRoomsList
        {
            get
            {
                return mConnectedRooms;
            }
        }

        public NavigatorOfficialItem(uint Id, uint ParentId, uint RoomId, bool IsCategory, bool IsAdvertsement,
            NavigatorOfficialItemDisplayType DisplayType, string Name, string Descr, NavigatorOfficialItemImageType ImageType,
            string Image, string BannerLabel, string SearchTag, bool CategoryAutoExpand, List<uint> ConnectedRoomsList)
        {
            mId = Id;
            mParentId = ParentId;
            mRoomId = RoomId;
            mIsCategory = IsCategory;
            mIsAdvertsement = IsAdvertsement;
            mDisplayType = DisplayType;
            mName = Name;
            mDescr = Descr;
            mImageType = ImageType;
            mImage = Image;
            mBannerLabel = BannerLabel;
            mSearchTag = SearchTag;
            mCategoryAutoExpand = CategoryAutoExpand;
            mConnectedRooms = ConnectedRoomsList;
        }

        public RoomInstance TryGetRoomInstance()
        {
            return RoomManager.GetInstanceByRoomId(mRoomId);
        }

        public RoomInfo GetRoomInfo()
        {
            return RoomInfoLoader.GetRoomInfo(mRoomId);
        }

        public int GetTotalUsersInPublicRoom()
        {
            int TotalUsersInThisItem = 0;

            if (GetRoomInfo() != null)
            {
                TotalUsersInThisItem += GetRoomInfo().CurrentUsers;
            }

            if (mConnectedRooms.Count > 0)
            {
                foreach (uint RoomId in mConnectedRooms)
                {
                    RoomInfo ConnectedRoom = RoomInfoLoader.GetRoomInfo(RoomId);
                    if (ConnectedRoom != null)
                    {
                        TotalUsersInThisItem += ConnectedRoom.CurrentUsers;
                    }
                }
            }

            return TotalUsersInThisItem;
        }
    }
}
