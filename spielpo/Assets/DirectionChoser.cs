using Map.Tile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.Ingame
{
    public class DirectionChoser : MonoBehaviour
    {
        [SerializeField] public HexDirection dir;
        public SpriteRenderer GetRenderer => GetComponent<SpriteRenderer>();
        public Collider GetCollider => GetComponent<Collider>();

        private void OnEnable()
        {
            GetRenderer.enabled = true;
            GetCollider.enabled = true;
        }

        private void OnDisable()
        {
            GetRenderer.enabled = false;
            GetCollider.enabled = false;
        }

        public void OnPointerClick()
        {
            GetComponentInParent<HexTileInfrastructureHighlighter>().setPointsTo(dir);
        }
    }
}