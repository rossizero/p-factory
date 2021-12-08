using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTime;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TickCounterUI : MonoBehaviour, ITickable
{
    TMP_Text text;
    int count = 0;

    public TickPriority priority => TickPriority.Normal;

    public void Tick()
    {
        count++;
        text.text = count.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }
}
