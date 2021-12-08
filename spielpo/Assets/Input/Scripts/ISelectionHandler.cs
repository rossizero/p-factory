using Map.Tile;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PlayerInput
{
    public interface ISelectionHandler
    {
        HexTile currentlySelectedTile { get; }

        void OnSelect(InputAction.CallbackContext context);
        void Select();

    }
}
