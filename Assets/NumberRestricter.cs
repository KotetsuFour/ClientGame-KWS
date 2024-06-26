using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumberRestricter : MonoBehaviour
{
    [SerializeField] private int min;
    [SerializeField] private int max;
    private TMP_InputField input;
    // Start is called before the first frame update
    void Start()
    {
        input = GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isValid()
    {
        int num;
        if (int.TryParse(input.text, out num)
            && num >= min && num <= max)
        {
            return true;
        }
        return false;
    }

    public void replaceInvalid()
    {
        for (int q = 0; q < input.text.Length; q++)
        {
            int num;
            if (!int.TryParse(input.text[q] + "", out num))
            {
                input.text = input.text.Replace(input.text[q], '0');
            }
        }
    }
}
