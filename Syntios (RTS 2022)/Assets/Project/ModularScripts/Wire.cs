using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Wire : MonoBehaviour
{

    public LineRenderer linerenderer;
    public Transform origin;
    public Transform target;
    public Vector3 posOrigin;
    public Vector3 posTarget;
    public Vector3 offset;

    void Update()
    {

        InstantUpdate();

    }

    public void InstantUpdate()
    {
        if (linerenderer == null) return;

        if (origin != null) linerenderer.SetPosition(0, origin.transform.position); else linerenderer.SetPosition(0, posOrigin);
        if (target != null) linerenderer.SetPosition(1, target.transform.position + offset); else linerenderer.SetPosition(1, posTarget + offset);
    }
}
