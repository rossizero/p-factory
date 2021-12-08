using Game;
using Map.Tile;
using PlayerInput;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace GameUI.Tile
{

    public class BuildInfrastructure : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Color onClickColor = Color.green;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] float fadeDuration = 0.2f;
        [SerializeField] Image graphic;
        HexTile tile;
        private Color _normalColor;
        private Color _onClickColor;

        List<ItemDisplayer> itemDisplayers = new List<ItemDisplayer>();

        private void Start()
        {
            _normalColor = normalColor;
            _onClickColor = onClickColor;
            graphic.color = _normalColor;
            itemDisplayers.AddRange(GetComponentsInChildren<ItemDisplayer>());
        }

        public void onBuildInfrastructureKey(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                tile = SelectionHandler.instance.currentlySelectedTile;
                if (tile != null)
                {
                    if (tile.tileData.CanBuildInfrastructure())
                        tile.tileData.infrastructure.IncreaseLevel(true);
                }
            }

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(fadeColor());

            if (tile != null)
            {
                if (tile.tileData.CanBuildInfrastructure())
                    tile.tileData.infrastructure.IncreaseLevel(true);
            }
        }

        IEnumerator fadeColor()
        {
            float counter = 0f;
            while (counter < fadeDuration)
            {
                counter += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, counter / fadeDuration);
                graphic.color = Color.Lerp(_onClickColor, _normalColor, alpha);
                yield return null;
            }
            graphic.color = _normalColor;
            yield return null;
        }

        private void Update()
        {
            tile = SelectionHandler.instance.currentlySelectedTile;

            if (tile != null)
            {
                foreach (ItemDisplayer id in itemDisplayers)
                {
                    if (tile.tileData.infrastructure.costToNextLevel().ContainsKey(id.GetItemType()))
                    {
                        id.number = tile.tileData.infrastructure.costToNextLevel()[id.GetItemType()];
                    } else
                    {
                        id.number = 0;
                    }
                }

                if (RessourceManager.checkInfraStructure(tile.tileData.infrastructure.costToNextLevel()) 
                    && tile.tileData.CanBuildInfrastructure())
                {
                    _normalColor = normalColor;
                    _onClickColor = onClickColor;
                } else
                {
                    _onClickColor = normalColor;
                    _normalColor = onClickColor;
                }
                graphic.color = _normalColor;
            }
        }
    }
}