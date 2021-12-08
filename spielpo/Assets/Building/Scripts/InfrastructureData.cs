using Game;
using Map.Tile;
using System;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Building
{

    [Serializable]
    public class LevelDictionary : SerializableDictionary<INFRALEVEL, int> { }

    public enum INFRALEVEL
    {
        NONE, ONE, TWO, THREE, FOUR, FIVE, SIX, BASE
    }
    public class InfrastructureData : MonoBehaviour
    {
        private INFRALEVEL level;
        [SerializeField] private GameObject InfrastructureGameobject;
        [SerializeField] private ItemDictionary levelOneCost;
        [SerializeField] private ItemDictionary levelTwoCost;
        [SerializeField] private ItemDictionary levelThreeCost;
        [SerializeField] private ItemDictionary levelFourCost;
        [SerializeField] private ItemDictionary levelFiveCost;
        [SerializeField] private ItemDictionary levelSixCost;

        [SerializeField] public LevelDictionary levelDict;
        public int getMaximumCapacity => levelDict[level];
        public int getTransportCapacity => calculateTransportCapacity();
        public INFRALEVEL GetLevel => level;


        public static UnityEvent OnFirstInfrastructureBuilt = new UnityEvent();

        public void SetLevel(INFRALEVEL value)
        {
            while (level < value)
            {
                IncreaseLevel(false);
            }
        }

        private int calculateTransportCapacity()
        {
            if (level.Equals(INFRALEVEL.BASE))
                return 10;
            return Mathf.CeilToInt(levelDict[level] / 5);
        }

        /// <summary>
        /// Increases the level of infrastructure
        /// </summary>
        /// <param name="setWithoutPay">set false for setting</param>
        public void IncreaseLevel(bool setWithoutPay)
        {

            if (setWithoutPay)
            {
                if (level >= INFRALEVEL.SIX)
                    return;
                ItemDictionary dict = new ItemDictionary();
                switch (level)
                {
                    case INFRALEVEL.NONE:
                        dict = levelOneCost;
                        break;
                    case INFRALEVEL.ONE:
                        dict = levelTwoCost;
                        break;
                    case INFRALEVEL.TWO:
                        dict = levelThreeCost;
                        break;
                    case INFRALEVEL.THREE:
                        dict = levelFourCost;
                        break;
                    case INFRALEVEL.FOUR:
                        dict = levelFiveCost;
                        break;
                    case INFRALEVEL.FIVE:
                        dict = levelSixCost;
                        break;
                    case INFRALEVEL.SIX:
                        dict = levelSixCost;
                        break;
                }

                if (RessourceManager.checkInfraStructure(dict))
                {
                    RessourceManager.buildInfraStructure(dict);
                    if (level == INFRALEVEL.NONE)
                    {
                        HexTile tile = GetComponentInParent<HexTile>();
                        tile.IncreaseVisibility();
                        foreach (HexTile neighbor in tile.GetNeighbours())
                        {
                            if (neighbor.tileData.HasInfrastructure)
                            {
                                tile.tileData.PointsTo = neighbor;
                                break;
                            }
                        }
                    }
                }
                else
                    return;

            }
            level += 1;
            if (level == INFRALEVEL.ONE)
            {
                OnFirstInfrastructureBuilt.Invoke();
            }
            increaseSize();
        }
        public ItemDictionary costToNextLevel()
        {
            ItemDictionary dict = new ItemDictionary();
            switch (level)
            {
                case INFRALEVEL.NONE:
                    dict = levelOneCost;
                    break;
                case INFRALEVEL.ONE:
                    dict = levelTwoCost;
                    break;
                case INFRALEVEL.TWO:
                    dict = levelThreeCost;
                    break;
                case INFRALEVEL.THREE:
                    dict = levelFourCost;
                    break;
                case INFRALEVEL.FOUR:
                    dict = levelFiveCost;
                    break;
                case INFRALEVEL.FIVE:
                    dict = levelSixCost;
                    break;
                default:
                    break;
            }
            return dict;
        }

        private void increaseSize()
        {
            if (!InfrastructureGameobject.activeSelf)
            {
                InfrastructureGameobject.SetActive(true);
            }
            InfrastructureGameobject.transform.localScale *= 1.15f;
        }


    }
}