using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace PlayerInput
{
    /// <summary>
    /// Controls camera movement within the game. Camera cannot be manipulated through this class directly.
    /// Instead public properties are used to control camera movement indirectly.
    /// </summary>
    public class CameraController : MonoBehaviour
    {

        //
        // Fields and properties used for moving the camera around using keys on the keyboard.
        //
        [SerializeField]
        private float movementSpeed;
        private Vector2 _absoluteDirectionInput;

        /// <summary>
        /// This property describes the actual movement direction. This is needed since the camera is looking at the game at a angle, but moves on a plane parallel to the xz-plane.
        /// </summary>
        private Vector3 movementDirection =>
            Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * (new Vector3(_absoluteDirectionInput.normalized.x, 0, _absoluteDirectionInput.normalized.y));
        /// <summary>
        /// This property can be used to give absolute key directions for moving the camera.
        /// </summary>
        public Vector2 moveValue { set => _absoluteDirectionInput = value; }


        //
        // Fields and properties used for moving the camera around using a boundary around the screens edge. 
        //
        [SerializeField]
        [Range(0, 1)]
        private float boundarySizePercentage;
        [SerializeField]
        private float scrollSpeed;

        private Vector2 screenMiddle => new Vector2(Screen.width, Screen.height) / 2;
        private Vector2 cursorPositionFromMiddle => GameCursor.instance.cursorPosition - screenMiddle;
        private Vector2 boundarySize => new Vector2(Screen.width, Screen.height) * boundarySizePercentage;

        /// <summary>
        /// This property describes the actual direction the camera is moving on the xz-plane.
        /// </summary>
        private Vector3 scrollDirection =>
            Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * (new Vector3(cursorPositionFromMiddle.normalized.x, 0, cursorPositionFromMiddle.normalized.y));


        //
        // Fields and properties used for zooming the camera in and out.
        //
        [SerializeField]
        private int zoomSpeed;
        private int zoomAnimationFrames = 15;
        private int elapsedAnimationFrames = 0;
        private Vector3 zoomTarget;
        private bool zooming = false;

        //
        // Fields and properties used for rotating the camera around the object that is currently being looked at.
        //
        [SerializeField]
        [Range(0, Mathf.PI)]
        private float rotationSpeed;
        private Vector2 _rotationDirection;

        /// <summary>
        /// This property describes which point the camera will be rotated around perpendicular to the y-axis.
        /// </summary>
        private Vector3 rotationTargetPosition
        {
            get
            {
                RaycastHit hit;
                Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity);
                return hit.point;
            }
        }
        /// <summary>
        /// This property can be used to change rotation direction.
        /// </summary>
        public Vector2 rotationValue { set => _rotationDirection = value; }
        public void FixedUpdate()
        {
            Move();
            Scroll();
            Rotate();

        }

        public void Update()
        {
            if (zooming)
            {
                float interpolationRatio = (float)elapsedAnimationFrames / zoomAnimationFrames;
                Vector3 interpolatedPosition = Vector3.Lerp(new Vector3(0, 0, 0), zoomTarget, interpolationRatio);
                elapsedAnimationFrames = (elapsedAnimationFrames + 1) % (zoomAnimationFrames + 1);
                transform.Translate(interpolatedPosition);
                if (elapsedAnimationFrames == 0)
                    zooming = false;
            }
        }

        private void Move() => transform.Translate(movementDirection * movementSpeed, Space.World);

        private void Scroll()
        {
            if (
                GameCursor.instance.cursorPosition.x < boundarySize.x ||
                GameCursor.instance.cursorPosition.x > Screen.width - boundarySize.x ||
                GameCursor.instance.cursorPosition.y < boundarySize.y ||
                GameCursor.instance.cursorPosition.y > Screen.height - boundarySize.y
                )
                transform.Translate(scrollDirection * scrollSpeed, Space.World);
        }

        private void Zoom(int zoom)
        {
            if (zoom != 0 && !zooming)
            {
                zoomTarget = new Vector3(0, 0, (zoom > 0 ? zoomSpeed : -zoomSpeed));
                zooming = true;
            }
        }

        private void Rotate() => transform.RotateAround(rotationTargetPosition, Vector3.up, rotationSpeed * -_rotationDirection.x);

        public void OnMove(InputAction.CallbackContext context) => moveValue = context.action.ReadValue<Vector2>();

        public void OnZoom(InputAction.CallbackContext context)
        {
            if(context.performed && !BuildActionHandler.currentlyBuilding)
                Zoom((int)context.action.ReadValue<Vector2>().y);
        }

        public void OnRotate(InputAction.CallbackContext context) => rotationValue = context.action.ReadValue<Vector2>();

    }
}
