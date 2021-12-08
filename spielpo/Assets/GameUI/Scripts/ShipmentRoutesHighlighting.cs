using Building;
using Map;
using Map.Tile;
using PlayerInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameUI
{
    public class ShipmentRoutesHighlighting : MonoBehaviour
    {
        [SerializeField] Material highlightMaterial;
        private List<LineRenderer> lineRenderers = new List<LineRenderer>();
        private List<GameObject> startAndEnd_Nodes = new List<GameObject>();

        private bool activated = false;

        private void Start()
        {
            InfrastructureDirectionSelector.OnDirectionChanged.AddListener(OnShipmentChange);
            InfrastructureData.OnFirstInfrastructureBuilt.AddListener(OnShipmentChange);
        }

        /// <summary>
        /// Call this function if a direction has changed or infrastructure has been built.
        /// </summary>
        private void OnShipmentChange()
        {
            lineRenderers.ForEach(delegate (LineRenderer line)
            {
                line.enabled = false;
            });

            startAndEnd_Nodes.ForEach(delegate (GameObject sphere)
            {
                Destroy(sphere);
            });

            lineRenderers.Clear();
            startAndEnd_Nodes.Clear();

            foreach (HexTile tile in FindObjectsOfType<HexTile>())
            {
                //these are the "starting" nodes.
                if (tile.tileData.HasInfrastructure && (tile.GetInputTiles.Count == 0 || tile.LoopToSelf))
                {
                    Color color = getColorForTile(tile);

                    LineRenderer line = GetUpdatedLineRenderer(tile, color);
                    lineRenderers.Add(line);

                    if (!tile.LoopToSelf)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.localScale *= 3;
                        sphere.transform.position = tile.transform.position;
                        sphere.GetComponent<MeshRenderer>().material = highlightMaterial;
                        sphere.GetComponent<MeshRenderer>().material.color = color;
                        sphere.GetComponent<SphereCollider>().enabled = false;
                        sphere.SetActive(activated);
                        startAndEnd_Nodes.Add(sphere);

                        //Add a Stop, only if the transport way is more than just one Tile
                        if (line.positionCount > 1)
                        {
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.localScale *= 3;
                            cube.transform.position = line.GetPosition(line.positionCount - 1);
                            cube.GetComponent<MeshRenderer>().material = highlightMaterial;
                            cube.GetComponent<MeshRenderer>().material.color = color;
                            cube.GetComponent<BoxCollider>().enabled = false;
                            cube.SetActive(activated);
                            startAndEnd_Nodes.Add(cube);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// always returns the same random color for a given tile
        /// </summary>
        /// <param name="tile">the tile</param>
        /// <returns>the color for this tile</returns>
        private Color getColorForTile(HexTile tile)
        {
            //save current random state (in case something depends on it's order)
            Random.State currentState = Random.state;
            //init random with unique seed for this cell (I know, pretty ugly to simply use this here)
            Random.InitState(WorldGenerator.convert2dto1dSeed(tile.Coordinate.X, tile.Coordinate.Z));
            //this will now always choose the same random color for a tile
            Color color = new Color(Random.Range(0F, 1F), Random.Range(0, 1F), Random.Range(0, 1F));
            //set current random state
            Random.state = currentState;
            return color;
        }

        public void ToggleLines(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                activated = !activated;
                Debug.Log($"LinerendererNUMBER {lineRenderers.Count} GO{activated}");
                foreach (LineRenderer line in lineRenderers)
                {
                    line.enabled = activated;
                }
                foreach (GameObject sphere in startAndEnd_Nodes)
                {
                    sphere.SetActive(activated);
                }
            }
        }

        private LineRenderer GetUpdatedLineRenderer(HexTile tile, Color color)
        {
            HexTile currentTile = tile;
            List<Vector3> positions = new List<Vector3>();
            positions.Add(currentTile.transform.position);

            LineRenderer line = tile.transform.gameObject.GetComponent<LineRenderer>();
            if (tile.GetComponent<LineRenderer>() == null)
            {
                line = tile.transform.gameObject.AddComponent<LineRenderer>();
            }

            line.material = highlightMaterial;
            line.material.color = color;
            line.enabled = activated;

            while (currentTile != null && currentTile.tileData.PointsTo != null && currentTile.tileData.HasInfrastructure && currentTile.PointsTo.tileData.HasInfrastructure)
            {
                positions.Add(currentTile.tileData.PointsTo.transform.position);
                currentTile = currentTile.tileData.PointsTo;
                //detected loop, adding in order to display the loop
                if (currentTile != null)
                {
                    if (currentTile.tileData.PointsTo != null && positions.Contains(currentTile.tileData.PointsTo.transform.position))
                        break;
                }
            }

            line.positionCount = positions.Count;


            for (int i = 0; i < positions.Count; i++)
            {
                line.SetPosition(i, positions[i]);
            }
            return line;
        }

    }
}