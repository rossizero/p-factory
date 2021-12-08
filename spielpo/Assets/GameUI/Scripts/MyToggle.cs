using Game;
using GameTime;
using Map;
using Map.Tile;
using PlayerInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameUI.Toggle
{
    [RequireComponent(typeof(Image))]
    public class MyToggle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Color on;
        [SerializeField] Color off = Color.red;
        [SerializeField] public Item itemToDisplay;
        [SerializeField] private Image graphic;

        private void Update()
        {
            HexTile tile = SelectionHandler.instance.currentlySelectedTile;
            if (tile == null)
            {
                graphic.color = off;
                return;
            }
            if (tile.tileData.itemToTransport.Contains(itemToDisplay))
                graphic.color = on;
            else
                graphic.color = off;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HexTile tile = SelectionHandler.instance.currentlySelectedTile;
            if (tile == null)
                return;

            if (tile.tileData.itemToTransport.Contains(itemToDisplay))
            {
                tile.tileData.itemToTransport.Remove(itemToDisplay);
            }
            else
            {
                tile.tileData.itemToTransport.Add(itemToDisplay);
            }
        }

    }
}