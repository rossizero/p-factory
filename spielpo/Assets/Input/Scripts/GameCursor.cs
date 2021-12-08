using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

using Map;
using Map.Tile;
using UI.Ingame;
using System;

namespace PlayerInput
{
    public class GameCursor
        : MonoBehaviour,
        IGameCursor
    {

        public static GameCursor instance { get; private set; }
        public static UnityEvent OnHoveredTileChanged = new UnityEvent();

        [SerializeField]
        private World map;
        [SerializeField]
        private Texture2D cursorTexture;
        private HexTile lastHoveredTile;

        public bool UIEnabled { get; private set; } = true;
        public Vector2 cursorPosition { get; private set; }

        public RaycastHit raycastHitUnderCursor
        {
            get
            {
                Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                return hit;
            }
        }

        public HexTile hoveredTile => map.GetTile(raycastHitUnderCursor.point);

        public void Awake()
        {
            if (instance == null)
                instance = this;
            Cursor.SetCursor(cursorTexture, new Vector2(0, 0), CursorMode.Auto);
        }

        public void Update()
        {
            if (enabled)
                UpdateTileHover();
            else
            {
                hoveredTile.highlighter.DisableHover();
                lastHoveredTile.highlighter.DisableHover();
            }

        }

        private void UpdateTileHover()
        {
            if (lastHoveredTile == null)
            {
                lastHoveredTile = hoveredTile;
                HoverRoutine(hoveredTile);
                return;
            }
            else if (lastHoveredTile.Equals(hoveredTile))
            {
                return;
            }
            else
            {
                HoverRoutine(hoveredTile);
                OnHoveredTileChanged.Invoke();
            }

        }

        public void OnCursorChanged(InputAction.CallbackContext context)
        {
            cursorPosition = context.action.ReadValue<Vector2>();
        }

        private void HoverRoutine(HexTile hoveredTile)
        {
            try
            {
                lastHoveredTile.highlighter.DisableHover();
                hoveredTile.highlighter.EnableHover();
                hoveredTile.IncreaseVisibility();
                foreach (HexTile t in hoveredTile.GetNeighbours(3))
                {
                    if (t != null)
                        t.IncreaseVisibility();
                }

                foreach (HexTile t in lastHoveredTile.GetNeighbours(3))
                {
                    if (t != null)
                        t.DecreaseVisibility();
                }
                lastHoveredTile.DecreaseVisibility();
                lastHoveredTile = hoveredTile;
            } catch (Exception e)
            {
                //Do Sth.
            }
        }

    }
}
