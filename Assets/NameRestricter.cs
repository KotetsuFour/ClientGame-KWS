using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameRestricter : MonoBehaviour
{
    private TMP_InputField input;
    [SerializeField] private int maxLength;
    private void Start()
    {
        input = GetComponent<TMP_InputField>();
    }
    public void adjustName()
    {
        for (int q = 0; q < input.text.Length; q++)
        {
            char c = input.text[q];
            if (!char.IsLetter(c) && c != ' ')
            {
                input.text.Replace(c + "", "");
            }
        }
        if (input.text.Length > maxLength)
        {
            input.text = input.text.Substring(0, maxLength);
        }
    }
}
