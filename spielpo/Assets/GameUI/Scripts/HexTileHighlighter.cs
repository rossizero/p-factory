using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Ingame
{

    public class HexTileHighlighter : MonoBehaviour
    {

        [SerializeField]
        private SpriteRenderer background;
        [SerializeField]
        private SpriteRenderer NE;
        [SerializeField]
        private SpriteRenderer E;
        [SerializeField]
        private SpriteRenderer SE;
        [SerializeField]
        private SpriteRenderer SW;
        [SerializeField]
        private SpriteRenderer W;
        [SerializeField]
        private SpriteRenderer NW;
        [SerializeField]
        private Map.Tile.HexTile self;

        private IDictionary<Map.Tile.HexDirection, SpriteRenderer> renderers = new Dictionary<Map.Tile.HexDirection, SpriteRenderer>();

        private bool _isSelected = false;

        public void Start()
        {
            renderers.Add(Map.Tile.HexDirection.NE, NE);
            renderers.Add(Map.Tile.HexDirection.E, E);
            renderers.Add(Map.Tile.HexDirection.SE, SE);
            renderers.Add(Map.Tile.HexDirection.SW, SW);
            renderers.Add(Map.Tile.HexDirection.W, W);
            renderers.Add(Map.Tile.HexDirection.NW, NW);
            background.enabled = false;
            foreach (SpriteRenderer renderer in renderers.Values)
            {
                renderer.enabled = false;
            }
        }

        public void EnableHover()
        {
            if (!_isSelected)
            {
                foreach (Map.Tile.HexDirection dir in Enum.GetValues(typeof(Map.Tile.HexDirection)))
                {
                    EnableEdgeHover(dir);
                }
                EnableBackgroundHover();
            }
        }

        public void DisableHover()
        {
            if (!_isSelected)
            {
                DisableBackgroundHover();
                foreach (Map.Tile.HexDirection dir in Enum.GetValues(typeof(Map.Tile.HexDirection)))
                {
                    DisableEdgeHover(dir);
                }
            }
        }

        public void EnableSelection()
        {
            _isSelected = true;
            foreach (Map.Tile.HexDirection dir in Enum.GetValues(typeof(Map.Tile.HexDirection)))
            {
                EnableEdgeSelection(dir);
            }
            EnableBackgroundSelection();

        }

        public void DisableSelection()
        {
            DisableBackgroundSelection();
            foreach (Map.Tile.HexDirection dir in Enum.GetValues(typeof(Map.Tile.HexDirection)))
            {
                DisableEdgeSelection(dir);
            }
            _isSelected = false;
        }

        private void EnableEdgeHover(Map.Tile.HexDirection edge)
        {
            EnableEdgeHover(renderers[edge]);
        }

        private void DisableEdgeHover(Map.Tile.HexDirection edge)
        {
            DisableEdgeHover(renderers[edge]);
        }

        private void EnableEdgeSelection(Map.Tile.HexDirection edge)
        {
            EnableEdgeSelection(renderers[edge]);
        }

        private void DisableEdgeSelection(Map.Tile.HexDirection edge)
        {
            DisableEdgeSelection(renderers[edge]);
        }

        private void DisableBackgroundSelection()
        {
            background.enabled = false;
        }

        private void EnableBackgroundSelection()
        {
            var color = Color.blue;
            color.a = 0.25f;
            background.color = color;
            background.enabled = true;
        }

        private void DisableBackgroundHover()
        {
            background.enabled = false;
        }

        private void EnableBackgroundHover()
        {
            var color = Color.white;
            color.a = 0.25f;
            background.color = color;
            background.enabled = true;
        }

        private void EnableEdgeHover(SpriteRenderer renderer)
        {
            var color = Color.white;
            color.a = 0.5f;
            renderer.color = color;
            renderer.enabled = true;
        }

        private void DisableEdgeHover(SpriteRenderer renderer)
        {
            renderer.enabled = false;
        }

        private void EnableEdgeSelection(SpriteRenderer renderer)
        {
            var color = Color.blue;
            color.a = 0.5f;
            renderer.color = color;
            renderer.enabled = true;

        }

        private void DisableEdgeSelection(SpriteRenderer renderer)
        {
            renderer.enabled = false;
        }

    }
}
