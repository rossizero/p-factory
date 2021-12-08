using Game;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameUI
{
    [RequireComponent(typeof(TMP_Text))]
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] Item type;
        private TMP_Text text;
        public bool useMainResources = true;
        public int number { private get; set; } = 0;

        private void Start()
        {
            text = GetComponent<TMP_Text>();
            if (text == null)
            {
                text = GetComponentInChildren<TMP_Text>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (useMainResources)
                text.text = RessourceManager.itemList[type].ToString();
            else
                text.text = number.ToString();
        }

        public Item GetItemType()
        {
            return type;
        }
    }
}