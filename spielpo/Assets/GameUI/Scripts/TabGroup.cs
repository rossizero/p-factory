using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class TabGroup : MonoBehaviour
    {

        public List<Tab> tabs;
        private Tab selected;

        public PanelGroup group;

        private void OnEnable()
        {
            if (tabs != null)
                onTabSelected(tabs[0]);

        }

        public void Subscribe(Tab button)
        {
            if (tabs == null)
            {
                tabs = new List<Tab>();
            }
            tabs.Add(button);
        }

        public void onTabEnter(Tab button)
        {
            ResetTabs();
            if (selected == null || button != selected)
            {
                foreach (Graphic g in button.Graphics)
                {
                    int ind = button.Graphics.IndexOf(g);
                    g.color = button.HoverColors[ind];
                }
            }
        }

        public void onTabExit(Tab button)
        {
            ResetTabs();
        }

        public void onTabSelected(Tab button)
        {
            selected = button;
            selected.Select();

            ResetTabs();


            foreach (Graphic g in button.Graphics)
            {
                int ind = button.Graphics.IndexOf(g);
                g.color = button.ActiveColors[ind];
            }


            int index = button.transform.GetSiblingIndex();
            if (group != null)
            {
                group.SetPanelIndex(index);
            }
        }

        public void ResetTabs()
        {
            foreach (Tab b in tabs)
            {
                if (b != null && b == selected) continue;
                foreach (Graphic g in b.Graphics)
                {
                    int ind = b.Graphics.IndexOf(g);
                    g.color = b.InactiveColors[ind];
                }
            }
        }
    }
}