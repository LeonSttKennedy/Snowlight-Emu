﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

using Snowlight.Storage;
using Snowlight.Specialized;
using Snowlight.Game.Pets;

namespace Snowlight.Game.Items
{
    public class InventoryCache
    {
        private uint mCharacterId;
        private Dictionary<uint, Item> mInner;
        private Dictionary<uint, Pet> mInnerPets;

        public Dictionary<uint, Item> Items
        {
            get
            {
                lock (mInner)
                {
                    Dictionary<uint, Item> Copy = new Dictionary<uint, Item>();

                    foreach (KeyValuePair<uint, Item> Item in mInner)
                    {
                        Copy.Add(Item.Key, Item.Value);
                    }

                    return new Dictionary<uint, Item>(Copy);
                }
            }
        }

        public Dictionary<uint, Pet> Pets
        {
            get
            {
                lock (mInnerPets)
                {
                    Dictionary<uint, Pet> Copy = new Dictionary<uint, Pet>();

                    foreach (KeyValuePair<uint, Pet> Item in mInnerPets)
                    {
                        Copy.Add(Item.Key, Item.Value);
                    }

                    return new Dictionary<uint, Pet>(Copy);
                }
            }
        }

        public InventoryCache(SqlDatabaseClient MySqlClient, uint CharacterId)
        {
            mCharacterId = CharacterId;
            mInner = new Dictionary<uint, Item>();
            mInnerPets = new Dictionary<uint, Pet>();

            ReloadCache(MySqlClient);
        }

        public void ReloadCache(SqlDatabaseClient MySqlClient)
        {
            lock (mInner)
            {
                mInner.Clear();

                MySqlClient.SetParameter("userid", mCharacterId);
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM items WHERE user_id = @userid");

                foreach (DataRow Row in Table.Rows)
                {
                    Item Item = ItemFactory.CreateFromDatabaseRow(Row);

                    if (Item == null || Item.Definition == null || Item.InSoundManager)
                    {
                        continue;
                    }

                    if (Item.PendingExpiration && Item.ExpireTimeLeft <= 0)
                    {
                        Item.RemovePermanently(MySqlClient);
                        continue;
                    }

                    mInner.Add(Item.Id, Item);
                }            
            }

            lock (mInnerPets)
            {
                mInnerPets.Clear();

                MySqlClient.SetParameter("userid", mCharacterId);
                DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM user_pets WHERE user_id = @userid AND room_id = 0");

                foreach (DataRow Row in Table.Rows)
                {
                    Pet Pet = PetFactory.GetPetFromDatabaseRow(Row);
                    mInnerPets.Add(Pet.Id, Pet);
                }
            }
        }

        public void Add(Item Item)
        {
            lock (mInner)
            {
                if (!mInner.ContainsKey(Item.Id))
                {
                    mInner.Add(Item.Id, Item);
                }
            }
        }

        public bool RemoveItem(uint ItemId)
        {
            lock (mInner)
            {
                if (!mInner.ContainsKey(ItemId))
                {
                    return false;
                }

                return mInner.Remove(ItemId);
            }
        }
        public void Add(Pet Pet)
        {
            lock (mInnerPets)
            {
                if (!mInnerPets.ContainsKey(Pet.Id))
                {
                    mInnerPets.Add(Pet.Id, Pet);
                }
            }
        }

        public bool RemovePet(uint PetId)
        {
            lock (mInnerPets)
            {
                if (!mInnerPets.ContainsKey(PetId))
                {
                    return false;
                }

                return mInnerPets.Remove(PetId);
            }
        }

        public Pet GetPet(uint Id)
        {
            lock (mInnerPets)
            {
                if (mInnerPets.ContainsKey(Id))
                {
                    return mInnerPets[Id];
                }
            }

            return null;
        }

        public Dictionary<uint, Item> GetFloorItems()
        {
            lock (mInner)
            {
                Dictionary<uint, Item> Copy = new Dictionary<uint, Item>();

                foreach (KeyValuePair<uint, Item> Item in mInner)
                {
                    if (Item.Value.Definition.Type == ItemType.FloorItem)
                    {
                        Copy.Add(Item.Key, Item.Value);
                    }
                }

                return new Dictionary<uint, Item>(Copy);
            }
        }

        public Dictionary<uint, Item> GetWallItems()
        {
            lock (mInner)
            {
                Dictionary<uint, Item> Copy = new Dictionary<uint, Item>();

                foreach (KeyValuePair<uint, Item> Item in mInner)
                {
                    if (Item.Value.Definition.Type == ItemType.WallItem)
                    {
                        Copy.Add(Item.Key, Item.Value);
                    }
                }

                return new Dictionary<uint, Item>(Copy);
            }
        }

        public List<Item> GetSongDisks()
        {
            List<Item> Disks = new List<Item>();

            lock (mInner)
            {
                foreach (Item Item in mInner.Values)
                {
                    if (Item.Definition.Behavior == ItemBehavior.MusicDisk)
                    {
                        Disks.Add(Item);
                    }
                }
            }

            return Disks;
        }

        public Dictionary<uint, Item> GetAvatarEffects()
        {
            lock (mInner)
            {
                Dictionary<uint, Item> Copy = new Dictionary<uint, Item>();

                foreach (KeyValuePair<uint, Item> Item in mInner)
                {
                    if (Item.Value.Definition.Type == ItemType.AvatarEffect)
                    {
                        Copy.Add(Item.Key, Item.Value);
                    }
                }

                return new Dictionary<uint, Item>(Copy);
            }
        }

        public Dictionary<uint, Pet> GetPets()
        {
            lock (mInnerPets)
            {
                Dictionary<uint, Pet> Copy = new Dictionary<uint, Pet>();

                foreach (KeyValuePair<uint, Pet> Item in mInnerPets)
                {
                    Copy.Add(Item.Key, Item.Value);
                }

                return new Dictionary<uint, Pet>(Copy);
            }
        }

        public Item GetItem(uint ItemId)
        {
            lock (mInner)
            {
                if (mInner.ContainsKey(ItemId))
                {
                    return mInner[ItemId];
                }
            }

            return null;
        }

        public void ClearAndDeleteAllItems()
        {
            lock (mInner)
            {
                mInner.Clear();

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("userid", mCharacterId);
                    MySqlClient.ExecuteNonQuery("DELETE FROM wired_items WHERE item_id IN (SELECT id FROM items WHERE user_id = @userid)");

                    MySqlClient.SetParameter("userid", mCharacterId);
                    MySqlClient.ExecuteNonQuery("DELETE FROM user_gifts WHERE item_id IN (SELECT id FROM items WHERE user_id = @userid)");

                    MySqlClient.SetParameter("userid", mCharacterId);
                    MySqlClient.ExecuteNonQuery("DELETE FROM items WHERE user_id = @userid");
                }
            }
        }

        public void ClearAndDeleteAllPets()
        {
            lock (mInnerPets)
            {
                mInnerPets.Clear();

                using (SqlDatabaseClient MySqlClient = SqlDatabaseManager.GetClient())
                {
                    MySqlClient.SetParameter("userid", mCharacterId);
                    MySqlClient.ExecuteNonQuery("DELETE FROM user_pets WHERE user_id = @userid AND room_id = 0");
                }
            }
        }
    }
}
