using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class SceneDepthInitalizer : MonoBehaviour
{
    public static SceneDepthInitalizer Instance { get; private set; }

    [SerializeField] Transform _controllerDepth;
    [SerializeField] Transform _binDepth;


    public float PartDisFromCam { get; private set; }
    public float DepthOfParts { get; private set; }
    public float DepthOfBins { get; private set; }

    Camera _mainCamera;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

         _mainCamera = Camera.main;
    }

    void Start()
    {
        RegisterDepthObjectController(_controllerDepth);
        RegisterDepthPartBin(_binDepth);
    }


    private void RegisterDepthObjectController(Transform controller)
    {
        FigureOutzDepthDifference(controller);
         DepthOfParts = _mainCamera.transform.position.z - PartDisFromCam;
    }
    private void FigureOutzDepthDifference(Transform controller)
    {
        PartDisFromCam = _mainCamera.transform.position.z - controller.position.z;
    }

    private void RegisterDepthPartBin(Transform bin)
    {
        float disFromCam = _mainCamera.transform.position.z - bin.position.z;
        DepthOfBins = _mainCamera.transform.position.z - disFromCam;
    }
}
