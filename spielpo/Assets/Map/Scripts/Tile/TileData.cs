using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Building;
using GameTime;
using Utility;
using Map.Tile;
using System;

namespace Game
{
    [RequireComponent(typeof(HexTile))]
    [RequireComponent(typeof(InfrastructureData))]
    public class TileData : MonoBehaviour, ITickable
    {
        [SerializeField] public BuildingData building = null;
        //TODO infrastructure level (Itemcontainer size)
        [SerializeField] public InfrastructureData infrastructure;
        [NonSerialized] public ItemDictionary itemList = new ItemDictionary();
        [NonSerialized] public List<Item> itemToTransport = new List<Item>();
        public HexTile HexTile => GetComponent<HexTile>();
        public HexTile PointsTo { get; set; }

        public bool HasInfrastructure => infrastructure.GetLevel > INFRALEVEL.NONE;

        private void Start()
        {
            //Init the List to transport
            foreach (Item i in (Item[])Enum.GetValues(typeof(Item)))
            {
                itemToTransport.Add(i);
            }
        }

        public TickPriority priority => TickPriority.Normal;


        /// <summary>
        /// TODO handle transport
        /// </summary>
        public void Tick()
        {
            if (building != null)
            {
                if (building.buildingType == BuildingType.Base)
                {
                    foreach (KeyValuePair<Item, int> pair in itemList)
                    {
                        RessourceManager.itemList[pair.Key] += pair.Value;
                    }
                    itemList = new ItemDictionary();
                }
                else
                {
                    bool producing = true;
                    //Check if the needs of a building are fulfilled
                    foreach (KeyValuePair<Item, int> pair in building.needs)
                    {
                        producing = producing && (pair.Value <= itemList[pair.Key]);
                    }
                    if (!producing)
                    {
                        Debug.Log($"{building.buildingType} on Tile {GetComponentInParent<HexTile>().Coordinate} has not enough Supply!!");
                    }


                    //Check if the new produced Items have space in the infrastructure
                    producing = producing && (infrastructure.getMaximumCapacity >= itemList.countItems() + building.produces.countItems() - building.needs.countItems());
                    if (!producing)
                    {
                        Debug.Log($"{building.buildingType} on Tile {GetComponentInParent<HexTile>().Coordinate} Cannot produce, not enough space");
                    }
                    //Only produce if all requirements are met
                    if (producing)
                    {
                        foreach (KeyValuePair<Item, int> pair in building.needs)
                        {
                            itemList[pair.Key] -= pair.Value;
                        }
                        foreach (KeyValuePair<Item, int> pair in building.produces)
                        {
                            itemList[pair.Key] += pair.Value;
                        }
                    }
                }
            }

            //Shipping
            if (PointsTo != null && infrastructure.GetLevel > INFRALEVEL.NONE)
            {
                if (building != null && building.buildingType == BuildingType.Base)
                {
                    int maxToTransport = Mathf.Min(PointsTo.tileData.GetCurrentCapacity(), infrastructure.getTransportCapacity);
                    ItemDictionary countingTransport = new ItemDictionary();

                    foreach (Item item in itemToTransport)
                    {
                        while (RessourceManager.itemList[item] > 0 && countingTransport.countItems() < maxToTransport)
                        {
                            countingTransport[item]++;
                            RessourceManager.itemList[item]--;
                        }
                    }
                    PointsTo.tileData.itemList.AddRange(countingTransport);
                }
                else
                {
                    int maxToTransport = Mathf.Min(PointsTo.tileData.GetCurrentCapacity(), infrastructure.getTransportCapacity);

                    ItemDictionary countingTransport = new ItemDictionary();
                    
                    foreach (Item item in itemToTransport)
                    {
                        if (itemList.ContainsKey(item))
                        {
                            while (itemList[item] > 0 && countingTransport.countItems() < maxToTransport)
                            {
                                countingTransport[item]++;
                                itemList[item]--;
                            }
                        }
                    }
                    PointsTo.tileData.itemList.AddRange(countingTransport);
                }
            }
        }

        public bool CanBuildInfrastructure()
        {
            bool b = false;
            foreach (HexTile t in this.HexTile.GetNeighbours())
            {
                b = b || t.tileData.infrastructure.GetLevel > Building.INFRALEVEL.NONE;
            }
            return b;
        }

        public int GetCurrentCapacity()
        {
            return infrastructure.getMaximumCapacity - itemList.countItems();
        }
    }
}