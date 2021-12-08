using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameUI
{
    public class DeactivateUserInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent onEnter;
        public UnityEvent onExit;
        public bool DisableCompletely = false;

        private void OnEnable()
        {
            if(DisableCompletely)
                onEnter.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onExit.Invoke();
        }

        private void OnDisable()
        {
            onExit.Invoke();
        }
    }
}