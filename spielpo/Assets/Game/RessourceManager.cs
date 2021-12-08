using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Building;
using GameTime;
using System;
using Map.Tile;
using Utility;

namespace Game
{

    [Serializable]
    public class ItemDictionary : SerializableDictionary<Item, int>
    {

        public ItemDictionary()
        {
            foreach (Item i in (Item[])Enum.GetValues(typeof(Item)))
            {
                this.Add(i, 0);
            }
        }

        public void AddRange(ItemDictionary other)
        {
            foreach (KeyValuePair<Item, int> pair in other)
            {
                if (this.ContainsKey(pair.Key))
                    this[pair.Key] += pair.Value;
                else
                    this.Add(pair);
            }
        }

        public int countItems()
        {
            int count = 0;
            foreach (int ct in this.Values)
            {
                count += ct;
            }
            return count;
        }
    }

    public enum Item
    {
        WOOD, PLANKS, STONES, COAL, FOOD, IRONORE, STEEL, ALIENSUBSTANCE, PLUMBUS
    }

    public class RessourceManager : MonoBehaviour, ITickable
    {
        public static ItemDictionary itemList = new ItemDictionary();

        TickPriority ITickable.priority => TickPriority.High;

        public void Tick()
        {
            return;
        }

        public string getResource(Item item)
        {
            return itemList[item].ToString();
        }

        /// <summary>
        /// Checks if a building can be built;
        /// Subtracts the Items necessary to build.
        /// </summary>
        /// <param name="building">The Building to be built</param>
        /// <returns>true if possible, else false</returns>
        public static bool checkBuildBuilding(BuildingData building)
        {
            if (building == null)
                return false;
            foreach (KeyValuePair<Item, int> pair in building.costs)
            {
                if (itemList[pair.Key] < pair.Value)
                    return false;
            }
            return true;
        }

        public static bool buildBuilding(BuildingData building)
        {
            if (building == null)
                return false;
            foreach (KeyValuePair<Item, int> pair in building.costs)
            {
                if (itemList[pair.Key] < pair.Value)
                    return false;
            }
            foreach (KeyValuePair<Item, int> pair in building.costs)
            {
                itemList[pair.Key] -= pair.Value;
            }
            return true;
        }

        public static void buildInfraStructure(ItemDictionary dict)
        {
            if (dict == null)
                return;
            foreach (KeyValuePair<Item, int> pair in dict)
            {
                if (itemList[pair.Key] < pair.Value)
                    return;
            }
            foreach (KeyValuePair<Item, int> pair in dict)
            {
                itemList[pair.Key] -= pair.Value;
            }
        }

        public static bool checkInfraStructure(ItemDictionary dict)
        {
            if (dict == null)
                return false;
            foreach (KeyValuePair<Item, int> pair in dict)
            {
                if (itemList[pair.Key] < pair.Value)
                    return false;
            }
            return true;
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (Item i in (Item[])Enum.GetValues(typeof(Item)))
            {
                itemList[i] = 50;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //checkTile() -> baseTile = lookingAtBaseTile()

        }


    }
}