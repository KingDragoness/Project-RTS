using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOWProcessor : MonoBehaviour
{

    public Shader blendShader;
    public RenderTexture rt_FOWActive;
    public RenderTexture rt_FOWExplored;
    public Camera cam_active;
    public Camera cam_explored;

    public RenderTexture rt_FOW_x4;

    private Material matPostFX;

    // Start is called before the first frame update
    void Start()
    {
        matPostFX = new Material(blendShader);
        matPostFX.SetTexture("_ExploredTex", rt_FOWExplored);
    }

    // Update is called once per frame
    void Update()
    {
        RenderTexture rt_temp_FOWActive1 = RenderTexture.GetTemporary(rt_FOWActive.width, rt_FOWActive.height, 0, rt_FOWActive.format);

        cam_active.Render();
        cam_explored.Render();

        Graphics.Blit(rt_FOWActive, rt_temp_FOWActive1);
        Graphics.Blit(rt_temp_FOWActive1, rt_FOWActive, matPostFX);
        Graphics.Blit(rt_FOWActive, rt_FOW_x4);

        RenderTexture.ReleaseTemporary(rt_temp_FOWActive1);
    }
}
