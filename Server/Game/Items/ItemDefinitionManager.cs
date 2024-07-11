using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Snowlight.Storage;
using Snowlight.Util;
using Snowlight.Utils;

namespace Snowlight.Game.Items
{
    public static class ItemDefinitionManager
    {
        private static Dictionary<uint, ItemDefinition> mDefinitions;
        private static List<ItemDefinition> mOldGiftDefinitionList;

        public static void Initialize(SqlDatabaseClient MySqlClient)
        {
            mDefinitions = new Dictionary<uint, ItemDefinition>();
            mOldGiftDefinitionList = new List<ItemDefinition>();

            int Count = 0;
            int Failed = 0;

            DataTable Table = MySqlClient.ExecuteQueryTable("SELECT * FROM item_definitions");

            foreach (DataRow Row in Table.Rows)
            {
                uint DefinitionId = (uint)Row["id"];

                ItemStackingBehavior StackingBehavior = ItemStackingBehavior.Normal;

                switch (Row["stacking_behavior"].ToString().ToLower())
                {
                    case "terminator":

                        StackingBehavior = ItemStackingBehavior.Terminator;
                        break;

                    case "initiator":

                        StackingBehavior = ItemStackingBehavior.Initiator;
                        break;

                    case "ignore":

                        StackingBehavior = ItemStackingBehavior.Ignore;
                        break;

                    case "disable":

                        StackingBehavior = ItemStackingBehavior.InitiateAndTerminate;
                        break;
                }

                ItemWalkableMode WMode = ItemWalkableMode.Never;

                switch (Row["walkable"].ToString())
                {
                    case "1":

                        WMode = ItemWalkableMode.Limited;
                        break;

                    case "2":

                        WMode = ItemWalkableMode.Always;
                        break;
                }

                string Behavior = Row["behavior"].ToString();

                ItemDefinition Definition = new ItemDefinition(DefinitionId, (uint)Row["sprite_id"],
                    (string)Row["name"], (string)Row["public_name"], GetTypeFromString(Row["type"].ToString()),
                    ItemBehaviorUtil.FromString(Behavior), (int)Row["behavior_data"], StackingBehavior,
                    WMode, (int)Row["room_limit"], (int)Row["size_x"], (int)Row["size_y"], (float)Row["height"],
                    (string)Row["height_adjustable"], (Row["allow_recycling"].ToString() == "1"),
                    (Row["allow_trading"].ToString() == "1"), (Row["allow_selling"].ToString() == "1"),
                    (Row["allow_gifting"].ToString() == "1"), (Row["allow_inventory_stacking"].ToString() == "1"));

                mDefinitions.Add(DefinitionId, Definition);

                MySqlClient.SetParameter("itembehavior", Behavior);
                DataRow PetStatusRow = MySqlClient.ExecuteQueryRow("SELECT * FROM pet_interactions WHERE behavior = @itembehavior LIMIT 1");

                if(PetStatusRow != null)
                {
                    Definition.SetPetStatussesInteraction(PetStatusRow["trick"].ToString());
                }

                MySqlClient.SetParameter("definitionid", DefinitionId);
                DataRow PetTypeInteractorRow = MySqlClient.ExecuteQueryRow("SELECT * FROM pet_item_configs WHERE definition_id = @definitionid LIMIT 1");

                if (PetTypeInteractorRow != null)
                {
                    List<int> PetTypes = new List<int>();

                    string[] splitedTypes = PetTypeInteractorRow["pet_type_list"].ToString().Split('|');

                    for (int i = 0; i < splitedTypes.Length; i++)
                    {
                        PetTypes.Add(int.Parse(splitedTypes[i]));
                    }

                    Definition.SetPetTypeInteractors(PetTypes);
                }

                Count++;             
            }

            foreach (ItemDefinition Definition in mDefinitions.Values)
            {
                if (Definition.Behavior == ItemBehavior.Gift && Definition.BehaviorData == 1)
                {
                    mOldGiftDefinitionList.Add(Definition);
                }
            }

            Output.WriteLine("Loaded " + Count + " item definition(s)" + (Failed > 0 ? " (" + Failed + " skipped due to errors)" : "") + ".", OutputLevel.DebugInformation);
        }

        private static ItemType GetTypeFromString(string Type)
        {
            switch (Type.ToLower())
            {
                default:
                case "s":

                    return ItemType.FloorItem;
                
                case "i":

                    return ItemType.WallItem;

                case "h":

                    return ItemType.ClubSubscription;

                case "p":

                    return ItemType.Pet;

                case "e":

                    return ItemType.AvatarEffect;
            }
        }

        public static ItemDefinition OldGiftDefinitions()
        {
            int Random = RandomGenerator.GetNext(0, 6);
            return mOldGiftDefinitionList[Random];
        }

        public static ItemDefinition GetDefinition(uint Id)
        {
            lock (mDefinitions)
            {
                if (mDefinitions.ContainsKey(Id))
                {
                    return mDefinitions[Id];
                }
            }

            return null;
        }

        public static ItemDefinition GetDefinitionBySpriteId(uint SpriteId)
        {
            lock (mDefinitions)
            {
                foreach (ItemDefinition ItemDefs in mDefinitions.Values)
                {
                    if (ItemDefs.SpriteId == SpriteId)
                    {
                        return ItemDefs;
                    }
                }
            }

            return null;
        }

        public static ItemDefinition GetDefinitionByName(string Name)
        {
            return mDefinitions.Values.Where(Def => Def.Name.Equals(Name)).ToList().FirstOrDefault();
        }
    }
}
