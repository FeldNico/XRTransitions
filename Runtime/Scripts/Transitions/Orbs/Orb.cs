using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Orb : MonoBehaviour
{
    [SerializeField] private Renderer _orbRenderer;
    public Renderer OrbRenderer => _orbRenderer;

    private bool _isInitialized = false;
    private bool _isInitiated = false;

    private XROrigin _xrOrigin;
    private Transform _xrOriginTransform;
    
    private OrbCamera _leftPortalCamera;
    private OrbCamera _rightPortalCamera;

    private OrbTransition _transition;
    private Transform _destination;

    private Transform _origin;
    public Transform Origin => _origin;

    public void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();
        _xrOriginTransform = _xrOrigin.transform;
    }

    public void Initialize(OrbTransition transition)
    {
        _transition = transition;

        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<OrbCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<OrbCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
        _isInitialized = true;
        
        DeInitiate();

        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1f);
            Initiate();
        }
    }

    private void Update()
    {
        if (_isInitialized && _isInitiated)
        {
            
            if (Vector3.Distance(transform.position, _transition.Camera.transform.position) <= 0.2f)
            {
                _xrOriginTransform.position = _destination.position;

                DeInitiate();
            }
        }
    }

    public void Initiate()
    {
        if (!_isInitialized)
        {
            return;
        }
        
        if (_origin == null)
        {
            _origin = new GameObject("OrbOriginDummy").transform;
            _origin.parent = FindObjectOfType<XROrigin>().transform;
        }

        foreach (Renderer child in GetComponentsInChildren<Renderer>(true))
        {
            child.enabled = true;
        }
        
        Transform cameraTransform = _transition.Camera.transform;
        _origin.position = cameraTransform.position;
        _origin.rotation = cameraTransform.rotation;
        _leftPortalCamera.StartRender();
        _rightPortalCamera.StartRender();
        _isInitiated = true;
    }

    public void DeInitiate()
    {
        if (!_isInitialized)
        {
            return;
        }

        _isInitiated = false;
        if (_origin != null)
        {
            Destroy(_origin.gameObject);
        }
        foreach (Renderer child in GetComponentsInChildren<Renderer>(true))
        {
            child.enabled = false;
        }
        _leftPortalCamera.StopRender();
        _rightPortalCamera.StopRender();
    }
}