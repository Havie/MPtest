using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public static FPSCounter Instance;

    [SerializeField] Text guiText = default;

    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        if (!guiText)
        {
            Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
            enabled = false;
            return;
        }
        timeleft = updateInterval;


    }


    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            guiText.text = format;

            if (fps < 30)
                guiText.material.color = Color.yellow;
            else
                if (fps < 10)
                guiText.material.color = Color.red;
            else
                guiText.material.color = Color.green;
            //	DebugConsole.Log(format,level);
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }

        if (Input.GetKeyDown(KeyCode.P))
            ProfileAllTextures();
    }

    void ProfileAllTextures()
    {
        Debug.Log("heard");
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
        foreach (Texture t in textures)
        {
            Debug.Log("Texture object " + t.name + " using: " + Profiler.GetRuntimeMemorySizeLong(t) + "Bytes");
        }
    }

    public void ProfileAnObject(GameObject go)
    {
        var mat = go.GetComponent<MeshRenderer>().material;
        var t = mat.mainTexture;
        UIManager.Instance.DebugLog($"Tex:<color=green>{t.name}</color> using <color=orange>{Profiler.GetRuntimeMemorySizeLong(t)}</color> bytes");

    }
}
