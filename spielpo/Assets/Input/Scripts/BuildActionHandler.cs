using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Building;
using Game;
using System;
using System.Linq;

namespace PlayerInput
{

    public class BuildActionHandler : MonoBehaviour
    {

        public static bool currentlyBuilding { get; private set; } = false;

        [SerializeField]
        private Map.World world;
        [SerializeField]
        private List<BuildingData> prefabs;

        private BuildingData buildingInstance = null;

        public void Update()
        {
            if (currentlyBuilding && buildingInstance != null)
            {
                UpdateBuildActionVisualization();
            }
        }

        public void OnMouseWheeleUpInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                int vect = (int)context.action.ReadValue<Vector2>().y;

                if (currentlyBuilding && buildingInstance != null)
                {
                    int toBuild = (int)buildingInstance.buildingType;

                    if (vect > 0)
                        toBuild++;
                    else if (vect < 0)
                        toBuild--;

                    toBuild %= Enum.GetValues(typeof(BuildingType)).Cast<int>().Max() + 1;
                    toBuild = Mathf.Max(toBuild, (int)BuildingType.None + 1);

                    StartBuildAction(toBuild);
                }
            }
        }

        /// <summary>
        /// Start building a Building, instantiates it
        /// </summary>
        /// <param name="type">The Buildingtype as Integer, Editor doesn´t support ENUMS atm</param>
        public void StartBuildAction(int type)
        {
            AbortBuildAction();
            foreach (BuildingData building in prefabs)
            {
                if (building.buildingType.Equals((BuildingType)type))
                {
                    buildingInstance = Instantiate(building);
                    break;
                }
            }
            if (buildingInstance != null)
            {
                currentlyBuilding = true;
            }
        }

        public void UpdateBuildActionVisualization()
        {
            Map.Tile.HexTile hoveredTile = GameCursor.instance.hoveredTile;
            if (hoveredTile != null && buildingInstance != null)
            {
                if (RessourceManager.checkBuildBuilding(buildingInstance) && (hoveredTile.resource == buildingInstance.requiredRessource
                    || buildingInstance.requiredRessource == Map.Biome.Resources.ResourceType.NONE) && hoveredTile.tileData.building == null
                    && !hoveredTile.IsUnderwater &&
                    (buildingInstance.buildingType == BuildingType.Base || hoveredTile.tileData.HasInfrastructure))
                {
                    buildingInstance.transform.SetPositionAndRotation(hoveredTile.transform.position, hoveredTile.transform.rotation);
                    foreach(MeshRenderer m in buildingInstance.GetComponentsInChildren<MeshRenderer>())
                        m.material.color = Color.white;
                }
                else
                {
                    buildingInstance.transform.SetPositionAndRotation(hoveredTile.transform.position, hoveredTile.transform.rotation);
                    foreach (MeshRenderer m in buildingInstance.GetComponentsInChildren<MeshRenderer>())
                        m.material.color = Color.red;
                }
            }
        }

        public void AbortBuildAction()
        {
            if (currentlyBuilding && buildingInstance != null)
            {
                currentlyBuilding = false;
                Destroy(buildingInstance.gameObject);
                buildingInstance = null;
            }
        }

        public void FinalizeBuildAction()
        {
            UpdateBuildActionVisualization();
            Map.Tile.HexTile hoveredTile = GameCursor.instance.hoveredTile;
            if (hoveredTile != null && buildingInstance != null)
            {
                if (RessourceManager.checkBuildBuilding(buildingInstance) && (hoveredTile.resource == buildingInstance.requiredRessource
                    || buildingInstance.requiredRessource == Map.Biome.Resources.ResourceType.NONE) && hoveredTile.tileData.building == null
                    && !hoveredTile.IsUnderwater)
                {
                    if (buildingInstance.buildingType == BuildingType.Base)
                    {

                        hoveredTile.IncreaseVisibility();

                        foreach (Map.Tile.HexTile a in hoveredTile.GetNeighbours(5))
                        {
                            a.IncreaseVisibility();
                        }
                        foreach (Map.Tile.HexTile a in hoveredTile.GetNeighbours(1))
                        {
                            a.tileData.infrastructure.SetLevel(INFRALEVEL.ONE);
                        }
                        hoveredTile.tileData.infrastructure.SetLevel(INFRALEVEL.BASE);

                    }
                    else if (hoveredTile.tileData.infrastructure.GetLevel > INFRALEVEL.NONE)
                    {
                        hoveredTile.IncreaseVisibility();
                        foreach (Map.Tile.HexTile a in hoveredTile.GetNeighbours(2))
                        {
                            a.IncreaseVisibility();
                        }
                    }
                    else
                    {
                        return;
                    }

                    RessourceManager.buildBuilding(buildingInstance);
                    currentlyBuilding = false;
                    hoveredTile.tileData.building = buildingInstance;
                    buildingInstance = null;
                }
            }
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (context.started)
                // This doesnt actually do anything
                return;
            else if (context.performed)
                FinalizeBuildAction();
            else if (context.canceled)
                // This doesnt actually do anything
                return;
        }

        public void OnAbort(InputAction.CallbackContext context)
        {
            AbortBuildAction();
        }

        public void OnBuildBase(InputAction.CallbackContext context)
        {
            if (context.started)
                StartBuildAction((int)BuildingType.Base);
        }

    }
}