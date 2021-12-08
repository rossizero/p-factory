using Game;
using Map.Biome.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Building
{
    public enum BuildingType
    {
        None, Base, Lumberjack, Stonemason, Smeltery, Kohler, Farm, Fishery, Mine, Sawmill, Extractor, Factory
    }

    public class BuildingData : MonoBehaviour
    {
        public BuildingType buildingType = BuildingType.None;


        [SerializeField] public ResourceType requiredRessource = ResourceType.NONE;
        [SerializeField] public ItemDictionary costs = new ItemDictionary();
        [SerializeField] public ItemDictionary produces = new ItemDictionary();
        [SerializeField] public ItemDictionary needs = new ItemDictionary();
    }
}