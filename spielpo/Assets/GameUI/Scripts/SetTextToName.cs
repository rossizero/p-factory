using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetTextToName : MonoBehaviour
{
    private TMP_Text text;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        text.text = gameObject.name;
    }
}
