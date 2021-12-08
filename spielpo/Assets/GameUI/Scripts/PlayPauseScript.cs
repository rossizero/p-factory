using GameTime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameUI {
    [RequireComponent(typeof(Image))]
    public class PlayPauseScript : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Sprite on;
        [SerializeField] Sprite off;
        private Image img;
        private TickManager manager;
        public UnityEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Clicked on Play/Pause");
            manager.OnPauseUnpause();
            onClick.Invoke();
        }

        // Start is called before the first frame update
        void Start()
        {
            img = GetComponent<Image>();
            manager = GetComponentInParent<TickManager>();
        }

        private void Update()
        {
            if (!manager.isRunning)
            {
                if(img.sprite == off)
                    img.sprite = on;
            } else
            {
                if (img.sprite == on)
                    img.sprite = off;
            }
        }
    }
}