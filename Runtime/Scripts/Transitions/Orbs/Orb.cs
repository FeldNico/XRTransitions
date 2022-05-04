using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Orb : MonoBehaviour
{
    [SerializeField] private Renderer _orbRenderer;
    public Renderer OrbRenderer => _orbRenderer;

    private bool _isInitialized = false;
    private bool _isInitiated = false;

    private XROrigin _xrOrigin;
    private Transform _xrOriginTransform;

    private OrbCamera _leftOrbCamera;
    private OrbCamera _rightOrbCamera;

    private OrbTransition _transition;
    private Transform _destination;

    private Transform _origin;
    public Transform Origin => _origin;

    private Vector3 _originalPosition;

    public void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();
        _xrOriginTransform = _xrOrigin.transform;
        _originalPosition = transform.position;
        
        foreach (var child in GetComponentsInChildren<Renderer>(true))
        {
            child.enabled = false;
        }

    }

    public void Initialize(OrbTransition transition)
    {
        _transition = transition;

        _leftOrbCamera = new GameObject("LeftCamera").AddComponent<OrbCamera>();
        _leftOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightOrbCamera = new GameObject("RightCamera").AddComponent<OrbCamera>();
        _rightOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
        _isInitialized = true;

        foreach (var child in GetComponentsInChildren<Renderer>(true))
        {
            child.enabled = true;
        }

        DeInitiate();
        //Initiate();
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

        Transform cameraTransform = _transition.Camera.transform;
        _origin.position = cameraTransform.position;
        _origin.rotation = cameraTransform.rotation;
        _leftOrbCamera.StartRender();
        _rightOrbCamera.StartRender();
        _isInitiated = true;
    }

    public void DeInitiate()
    {
        if (!_isInitialized)
        {
            return;
        }

        var grab = GetComponent<XRGrabInteractable>();
        if (grab.isSelected)
        {
            grab.interactionManager.CancelInteractableSelection(grab);
        }

        transform.position = _originalPosition;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        if (_origin != null)
        {
            Destroy(_origin.gameObject);
        }

        _leftOrbCamera.StopRender();
        _rightOrbCamera.StopRender();
        _isInitiated = false;
    }
}