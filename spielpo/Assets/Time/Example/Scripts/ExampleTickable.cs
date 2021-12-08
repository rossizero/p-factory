using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTime
{
    namespace Examples
    {
        public class ExampleTickable : MonoBehaviour, ITickable
        {

            private int ticked = 0;

            public TickPriority priority => TickPriority.Normal;

            public void Tick()
            {
                ticked++;
                Debug.Log("Example Tickable was ticked " + ticked + " times.");
            }

        }
    }
}
