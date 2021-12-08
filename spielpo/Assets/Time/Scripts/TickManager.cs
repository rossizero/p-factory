using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using System.Linq;
using UnityEngine.Events;

namespace GameTime
{
    public class TickManager : MonoBehaviour
    {

        [SerializeField]
        public int gameSpeed { get; private set; } = 2;
        [SerializeField]
        public int minGameSpeed { get; private set; } = 1;
        [SerializeField]
        public int maxGameSpeed { get; private set; } = 5;
        [SerializeField]
        private float initialWait = 0.1f;

        private bool _running = false;
        public bool isRunning => _running;
        public bool isNotRunning => !_running;

        private IEnumerable<ITickable> tickables => FindObjectsOfType<MonoBehaviour>().OfType<ITickable>();

        private void ExecuteTick()
        {
            var sortedTickables = tickables.OrderByDescending(tickable => (int)(tickable.priority)).ToList();
            foreach (ITickable tickable in sortedTickables)
            {
                tickable.Tick();
            }
        }

        public int increaseGamespeed()
        {
            if (gameSpeed < maxGameSpeed)
            {
                gameSpeed++;
                if (isRunning)
                {
                    CancelInvoke();
                    InvokeRepeating("ExecuteTick", initialWait, maxGameSpeed + 1 - gameSpeed);
                }
            }
            return gameSpeed;
        }

        public int decreaseGamespeed()
        {
            if (gameSpeed > minGameSpeed)
            {
                gameSpeed--;
                if (isRunning)
                {
                    CancelInvoke();
                    InvokeRepeating("ExecuteTick", initialWait, maxGameSpeed + 1 - gameSpeed);
                }
            }
            return gameSpeed;
        }

        public void Step()
        {
            if (!_running)
            {
                ExecuteTick();
            }
        }

        public void Pause()
        {
            if (_running)
            {
                _running = false;
                CancelInvoke();
            }
        }

        public void Unpause()
        {
            if (!_running)
            {
                _running = true;
                InvokeRepeating("ExecuteTick", initialWait, maxGameSpeed + 1 - gameSpeed);
            }
        }

        public void OnPauseUnpause()
        {
            if (isRunning)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }

        public void OnPlayPauseInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
                OnPauseUnpause();
        }

        public void OnStepInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
                OnStep();
        }

        public void OnFasterInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
                increaseGamespeed();
        }

        public void OnSlowerInput(InputAction.CallbackContext context)
        {
            if (context.canceled)
                decreaseGamespeed();
        }

        public void OnStep() => Step();

    }
}