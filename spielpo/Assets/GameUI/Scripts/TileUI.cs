using Game;
using GameUI.Toggle;
using Map;
using Map.Tile;
using PlayerInput;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using GameTime;

namespace GameUI.Tile
{
    public class TileUI : MonoBehaviour, ITickable
    {
        TileData tile;
        [SerializeField] TMP_Text buildingText;
        [SerializeField] TMP_Text ressourceText;
        [SerializeField] List<TMP_Text> infrastructureText;
        List<ItemDisplayer> itemDisplayers = new List<ItemDisplayer>();
        private List<LineRenderer> lines = new List<LineRenderer>();
        [SerializeField] Material lineMaterial;

        public TickPriority priority => TickPriority.Normal;

        private void Start()
        {
            itemDisplayers.AddRange(GetComponentsInChildren<ItemDisplayer>());
            InfrastructureDirectionSelector.OnDirectionChanged.AddListener(Visualize);
            SelectionHandler.OnSelectionChanged.AddListener(Visualize);
        }

        public void OnSelected(InputAction.CallbackContext context)
        {
            if (context.interaction is MultiTapInteraction)
            {
                if (context.started)
                    // This doesnt actually do anything
                    return;
                else if (context.performed)
                    Visualize();
                else if (context.canceled)
                    // This doesnt actually do anything
                    return;
            }
        }

        public void Tick()
        {
            if(gameObject.activeSelf)
                Visualize();
        }

        /// <summary>
        /// Visualize information about a tile onSelect of a Tile or on Tick.
        /// </summary>
        public void Visualize()
        {
            gameObject.SetActive(true);
            HexTile hexTile = SelectionHandler.instance.currentlySelectedTile;
            if(hexTile != null)
                this.tile = hexTile.tileData;
            if (tile != null)
            {
                updateLine(tile.HexTile);
                //Set ItemsUI of specified tile
                foreach (ItemDisplayer id in itemDisplayers)
                {
                    //If a base is on it we display main resources.
                    ItemDictionary itemsToDisplay = new ItemDictionary();
                    if(tile.building != null && tile.building.buildingType == Building.BuildingType.Base)
                    {
                        itemsToDisplay = RessourceManager.itemList;
                    } else
                    {
                        itemsToDisplay = tile.itemList;
                    }

                    if (itemsToDisplay.ContainsKey(id.GetItemType()))
                    {
                        id.number = itemsToDisplay[id.GetItemType()];
                    }
                }
                // set Building text of tile
                if (tile.building != null)
                    buildingText.text = tile.building.buildingType.ToString();
                else
                    buildingText.text = "None";
                //Set Resource text (Forest, IronOre, etc..)
                ressourceText.text = tile.HexTile.resource.ToString() + " - " + tile.HexTile.resourceQuality.ToString();
                //Set level UI of infrastructure of tile
                foreach (TMP_Text text in infrastructureText)
                    text.text = "holds " + tile.infrastructure.getMaximumCapacity + " - sends " + tile.infrastructure.getTransportCapacity + "\nevery Tick";
            }
        }

        private void OnDisable()
        {
            tile = null;
            //The Events from the panel are already disabled, so we need to Enable our InputSystem again 
            if(SelectionHandler.instance != null)
                SelectionHandler.instance.enabled = true;
            if(GameCursor.instance != null)
            GameCursor.instance.enabled = true;
        }

        private void updateLine(HexTile tile)
        {
            //updateLines(new List<HexTile>(new HexTile[] { tile }));
        }

        private void updateLines(List<HexTile> highlightedTiles)
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line);
            }
            lines.Clear();
            foreach (HexTile tile in highlightedTiles)
            {
                HexTile currentTile = tile;
                List<Vector3> positions = new List<Vector3>();
                positions.Add(currentTile.transform.position);

                LineRenderer line = tile.transform.gameObject.GetComponent<LineRenderer>();
                if (tile.GetComponent<LineRenderer>() == null)
                {
                    line = tile.transform.gameObject.AddComponent<LineRenderer>();
                }

                line.material = lineMaterial;

                while (currentTile != null && currentTile.tileData.PointsTo != null && !positions.Contains(currentTile.tileData.PointsTo.transform.position))
                {
                    positions.Add(currentTile.tileData.PointsTo.transform.position);
                    currentTile = currentTile.tileData.PointsTo;
                }

                line.positionCount = positions.Count;

                for (int i = 0; i < positions.Count; i++)
                {
                    line.SetPosition(i, positions[i]);
                }
                lines.Add(line);
            }
        }
    }
}