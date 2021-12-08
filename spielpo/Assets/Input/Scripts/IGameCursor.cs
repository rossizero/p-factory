using UnityEngine;
using UnityEngine.InputSystem;
using Map.Tile;

public interface IGameCursor { 

    RaycastHit raycastHitUnderCursor { get; }
    Vector2 cursorPosition { get; }
    HexTile hoveredTile { get; }

    void OnCursorChanged(InputAction.CallbackContext context);

}
