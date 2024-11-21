using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedFunctionScript : MonoBehaviour
{

    public float time = 1f;
    public TimedFunction.OnFunction function;

    private void Update()
    {
        time -= Time.deltaTime;

        if (time <= 0f)
        {
            function?.Invoke();
            Destroy(gameObject);
        }
    }


}
