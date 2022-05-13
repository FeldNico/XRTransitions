using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts;
using Scripts.Utils;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Dissolve : MonoBehaviour
{

    private bool _isInitialized = false;
    
    [SerializeField]
    private Renderer _planeRenderer;
    public Renderer PlaneRenderer => _planeRenderer;
    public Transform Origin => _origin;

    private DissolveCamera _leftPortalCamera;
    private DissolveCamera _rightPortalCamera;
    private TransitionManager _transitionManager;
    private Transform _origin;

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = transform.Find("RenderPlane").GetComponent<MeshRenderer>();
        }

        _transitionManager = FindObjectOfType<TransitionManager>();
        _origin = new GameObject("DissolveOrigin").transform;
        _origin.parent = FindObjectOfType<XROrigin>().transform;
        _origin.position = _transitionManager.MainCamera.transform.position;
        _origin.rotation = Quaternion.identity;
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
        Destroy(_origin.gameObject);
        Destroy(_leftPortalCamera.gameObject);
        Destroy(_rightPortalCamera.gameObject);
    }
}