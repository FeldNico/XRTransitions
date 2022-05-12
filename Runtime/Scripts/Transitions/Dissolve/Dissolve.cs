using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts;
using Scripts.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Dissolve : MonoBehaviour
{

    private bool _isInitialized = false;
    
    [SerializeField]
    private Renderer _planeRenderer;
    public Renderer PlaneRenderer => _planeRenderer;

    private DissolveCamera _leftPortalCamera;
    private DissolveCamera _rightPortalCamera;

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = transform.Find("RenderPlane").GetComponent<MeshRenderer>();
        }
        
    }

    public void Initialize(DissolveTransition transition)
    {
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<DissolveCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<DissolveCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);
        _isInitialized = true;
    }

    private void OnDestroy()
    {
        Destroy(_leftPortalCamera);
        Destroy(_rightPortalCamera);
    }
}