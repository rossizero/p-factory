using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Events;

using Map.Tile;


namespace PlayerInput
{
    public class InfrastructureDirectionSelector :
        MonoBehaviour,
        IInfrastructureDirectionSelector
    {

        public static UnityEvent OnDirectionChanged = new UnityEvent();

        public bool recording { get; private set; } = false;

        public void Start() => GameCursor.OnHoveredTileChanged.AddListener(UpdateDirection);

        public void OnSelectDirection(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                if (context.started)
                    return;
                if (context.performed)
                    OnSelectDirection_performed();
                if (context.canceled)
                    OnSelectDirection_canceled();
            }
        }

        private void OnSelectDirection_performed()
        {
            if (enabled)
            {
                recording = true;
                SelectionHandler.instance.Select();
            }
        }

        private void OnSelectDirection_canceled()
        {
            if (enabled)
                recording = false;
        }

        private void UpdateDirection()
        {
            if (recording && enabled && SelectionHandler.instance.currentlySelectedTile != null)
            {
                HexTile selectedTile = SelectionHandler.instance.currentlySelectedTile;
                HexDirection direction = selectedTile.GetDirectionTowards(GameCursor.instance.hoveredTile);
                Debug.Log("Changed direction to: " + direction);
                HexTile neighborInDirection = selectedTile.GetNeighbour(direction);
                selectedTile.tileData.PointsTo = neighborInDirection;
                OnDirectionChanged.Invoke();
            }
        }
    }
}
