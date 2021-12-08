using Building;
using GameUI;
using PlayerInput;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameUI
{
    public class HouseUIManager : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] GameObject itemListCost;
        [SerializeField] GameObject itemListNeeds;
        [SerializeField] GameObject itemListProduces;
        [SerializeField] BuildingData prefab;
        [SerializeField] TMP_Text buildingName;
        [SerializeField] TMP_Text buildingDescripton;

        /// <summary>
        /// We want to build the building specified here (prefab). 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            FindObjectOfType<BuildActionHandler>().StartBuildAction((int)prefab.buildingType);
        }

        private void OnEnable()
        {
            buildingName.text = prefab.buildingType.ToString();
            buildingDescripton.text = $"This Building needs {prefab.requiredRessource} to be built";
            foreach (ItemDisplayer d in itemListCost.GetComponentsInChildren<ItemDisplayer>())
            {
                if (prefab.costs.ContainsKey(d.GetItemType()))
                {
                    d.number = prefab.costs[d.GetItemType()];
                }
                else
                {
                    d.number = 0;
                }
            }

            foreach (ItemDisplayer d in itemListNeeds.GetComponentsInChildren<ItemDisplayer>())
            {
                if (prefab.needs.ContainsKey(d.GetItemType()))
                {
                    d.number = prefab.needs[d.GetItemType()];
                }
                else
                {
                    d.number = 0;
                }
            }

            foreach (ItemDisplayer d in itemListProduces.GetComponentsInChildren<ItemDisplayer>())
            {
                if (prefab.produces.ContainsKey(d.GetItemType()))
                {
                    d.number = prefab.produces[d.GetItemType()];
                }
                else
                {
                    d.number = 0;
                }
            }
        }
    }
}