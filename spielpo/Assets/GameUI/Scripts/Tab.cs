using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace GameUI
{
    public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public TabGroup TabGroup;

        public List<Graphic> Graphics;
        public List<Color> ActiveColors;
        public List<Color> HoverColors;
        public List<Color> InactiveColors;

        public UnityEvent onClick;
        public UnityEvent onHover;
        public UnityEvent onExit;

        public void Select()
        {
            if (onClick != null)
            {
                onClick.Invoke();
            }
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            TabGroup.onTabSelected(this);
            Select();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TabGroup.onTabEnter(this);
            onHover.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TabGroup.onTabExit(this);
            onExit.Invoke();
        }

        // Start is called before the first frame update
        void Start()
        {
            TabGroup.Subscribe(this);
            if (Graphics == null)
                Graphics = new List<Graphic>();
        }
    }
}