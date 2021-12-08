using Map.Tile;
using PlayerInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Ingame
{
    public class HexTileInfrastructureHighlighter : MonoBehaviour
    {

        [SerializeField] private HexTile self;
        [SerializeField] List<DirectionChoser> directionChosers;

        public void Start()
        {
            foreach (DirectionChoser renderer in directionChosers)
            {
                renderer.enabled = false;
            }
        }

        private void Update()
        {
            if (SelectionHandler.instance.currentlySelectedTile != null)
            {
                if (SelectionHandler.instance.currentlySelectedTile.Equals(self) && self.tileData.HasInfrastructure)
                {
                    foreach (DirectionChoser renderer in directionChosers)
                    {
                        if (self.GetNeighbour(renderer.dir) != null && self.GetNeighbour(renderer.dir).tileData.HasInfrastructure)
                            renderer.enabled = true;
                        if (self.PointsTo != null && self.GetDirectionTowards(self.PointsTo) == renderer.dir)
                        {
                            renderer.GetRenderer.material.color = new Color(1, 0, 0, 0.8f);
                        }
                        else
                        {
                            renderer.GetRenderer.material.color = new Color(1, 1, 1, 0.3f);
                        }
                    }
                }
                else
                {
                    foreach (DirectionChoser renderer in directionChosers)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// sets self's pointsTo in given direction if neighbour is not null
        /// </summary>
        /// <param name="direction">direction to make the tile point to</param>
        public void setPointsTo(HexDirection direction)
        {
            if (self.GetNeighbour(direction) != null)
            {
                self.tileData.PointsTo = self.GetNeighbour(direction);
                InfrastructureDirectionSelector.OnDirectionChanged.Invoke();
            }
        }
    }
}
