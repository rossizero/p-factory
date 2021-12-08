using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUI
{
    public class PanelGroup : MonoBehaviour
    {
        public GameObject[] panels;

        public int panelIndex;

        // Start is called before the first frame update
        void Awake()
        {
            ShowCurrentPanel();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void ShowCurrentPanel()
        {
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].gameObject.SetActive(i == panelIndex);
            }
        }

        public void SetPanelIndex(int i)
        {
            panelIndex = i;
            ShowCurrentPanel();
        }
    }
}