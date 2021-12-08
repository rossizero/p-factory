using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using GameTime;

namespace GameUI
{
    [RequireComponent(typeof(TMP_Text))]
    public class GameSpeedDisplayer : MonoBehaviour
    {
        TickManager manager;
        TMP_Text text;

        // Start is called before the first frame update
        void Start()
        {
            manager = GetComponentInParent<TickManager>();
            text = GetComponent<TMP_Text>();
        }

        // Update is called once per frame
        void Update()
        {
            text.text = manager.gameSpeed.ToString();
        }
    }
}