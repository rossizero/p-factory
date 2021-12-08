using GameUI;
using GameUI.Tile;
using Map;
using Map.Tile;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Events;
using UI.Ingame;
using System;

namespace PlayerInput
{

    public class SelectionHandler
        : MonoBehaviour,
        ISelectionHandler
    {

        public static SelectionHandler instance { get; private set; }
        public static UnityEvent OnSelectionChanged = new UnityEvent();

        [SerializeField]
        private World map;

        private HexTile _currentlySelectedTile;
        private HexTile lastSelectedTile;

        public HexTile currentlySelectedTile => _currentlySelectedTile;


        private HexTile firstClickSelected;

        public void Awake()
        {
            if (instance == null)
                instance = this;
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (context.interaction is MultiTapInteraction)
            {
                if (context.started)
                {
                    Click();
                    firstClickSelected = GameCursor.instance.hoveredTile;
                    return;
                }
                else if (context.performed)
                    Select();
                else if (context.canceled)
                    return;
            }
        }

        /// <summary>
        /// Make a collider interactable
        /// </summary>
        private void Click()
        {
            Ray ray = Camera.main.ScreenPointToRay(GameCursor.instance.cursorPosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (hit.collider.gameObject.GetComponent<DirectionChoser>() != null)
            {
                hit.collider.gameObject.GetComponent<DirectionChoser>().OnPointerClick();
            }
        }

        public void Select()
        {
            if (enabled)
            {
                if (firstClickSelected == GameCursor.instance.hoveredTile)
                {
                    lastSelectedTile = _currentlySelectedTile;
                    if (lastSelectedTile != null)
                        lastSelectedTile.highlighter.DisableSelection();

                    _currentlySelectedTile = GameCursor.instance.hoveredTile;
                    _currentlySelectedTile.highlighter.EnableSelection();
                    OnSelectionChanged.Invoke();
                }
            }
        }

    }
}
