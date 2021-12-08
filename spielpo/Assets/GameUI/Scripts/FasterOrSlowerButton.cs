using GameTime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameUI
{
    [RequireComponent(typeof(Image))]
    public class FasterOrSlowerButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] bool fasterTrueSlowerFalse = true;
        [SerializeField] Color disabledColor = Color.gray;
        TickManager manager;
        Image img;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (fasterTrueSlowerFalse)
            {
                manager.increaseGamespeed();
            } else
            {
                manager.decreaseGamespeed();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            img = GetComponent<Image>();
            manager = GetComponentInParent<TickManager>();
        }

        // Update is called once per frame
        void Update()
        {
            if ((fasterTrueSlowerFalse && manager.gameSpeed == manager.maxGameSpeed) || (!fasterTrueSlowerFalse && manager.gameSpeed == manager.minGameSpeed))
            {
                img.color = disabledColor;
            } else
            {
                if(img.color != Color.white)
                    img.color = Color.white;
            }
        }
    }
}