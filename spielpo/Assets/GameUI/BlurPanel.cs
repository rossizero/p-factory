using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GameUI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    public class BlurPanel : MonoBehaviour
    {
        [SerializeField]
        private float time = 0.5f;
        [SerializeField]
        private float delay = 0f;
        [SerializeField]
        private GameObject BlurVolume;

        CanvasGroup canvasG;
        Image img;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (BlurVolume != null)
                BlurVolume.SetActive(true);
            if (Application.isPlaying)
            {
                canvasG.alpha = 0;
                StartCoroutine("tween");
            }
        }

        private void OnDisable()
        {
            if (BlurVolume != null)
                BlurVolume.SetActive(false);
        }

        private void Awake()
        {
            canvasG = GetComponent<CanvasGroup>();
            img = GetComponent<Image>();
        }


        void UpdateBlur(float value)
        {
            canvasG.alpha = value;
            if (value > 0.3f)
            {
                Color c = img.color;
                c.a = value - 0.3f;
                img.color = c;
            }
        }

        /// <summary>
        /// Tweens between 0 and 0.7 to make the panel blacky
        /// </summary>
        /// <returns></returns>
        IEnumerator tween()
        {
            float start = 0, stop = 1;
            yield return new WaitForSeconds(delay);

            float startTime = Time.time;

            for (int i = 5; i > 0; i--)
            {
                start += stop / 10;
                UpdateBlur(start);
                yield return new WaitForSeconds(time / 10);
            }
            UpdateBlur(stop);
            yield break;
        }

    }
}