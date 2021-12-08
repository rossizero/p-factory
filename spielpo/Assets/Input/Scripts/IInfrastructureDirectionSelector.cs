using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerInput
{
    public interface IInfrastructureDirectionSelector
    {
        void OnSelectDirection(InputAction.CallbackContext context);
    }
}