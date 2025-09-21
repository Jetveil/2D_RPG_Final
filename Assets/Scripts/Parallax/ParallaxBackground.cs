using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField]
    private ParallaxLayer[] backgroundLayers;

    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        CalculateImageLength();
    }


    private void FixedUpdate()
    {
        float currentCameraPositionX = mainCamera.transform.position.x;
        float distanceToMoveCamera = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMoveCamera);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }

    private void CalculateImageLength()
    {
        foreach (ParallaxLayer layer in backgroundLayers)
            layer.CalculateImageWidth();
    }
    
    
}