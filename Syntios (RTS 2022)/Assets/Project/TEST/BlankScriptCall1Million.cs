using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankScriptCall1Million : MonoBehaviour
{

    public int count = 100000;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int x = 0; x < count; x++)
        {
            CountingAddingTest(x);
        }
    }

    private float TEST = 0;

    private void CountingAddingTest(int lx)
    {
        TEST = 0;
        TEST += lx * 0.1f;
    }
}
