using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTime
{
    public interface ITickable
    {
        TickPriority priority { get; }

        void Tick();
    }

    public enum TickPriority
    {
        VeryLow = 1,
        Low = 2,
        Normal = 3,
        High = 4,
        VeryHigh = 5
    }

}
