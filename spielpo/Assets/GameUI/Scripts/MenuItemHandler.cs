using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameUI
{
    [RequireComponent(typeof(Outline))]
    public class MenuItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {

        Outline line;
        public UnityEvent onClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
            line.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            line.enabled = false;
        }

        private void OnEnable()
        {
            line = GetComponent<Outline>();
            if(line == null)
            {
                Debug.Log("HELP");
                return;
            }
            line.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }
    }
}