using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class Orb : MonoBehaviour
{
    [SerializeField] private Renderer _orbRenderer;
    public Renderer OrbRenderer => _orbRenderer;

    private TransitionManager _transitionManager;
    
    private XROrigin _xrOrigin;

    private OrbCamera _leftOrbCamera;
    private OrbCamera _rightOrbCamera;

    private OrbTransition _transition;
    private Transform _destination;

    private Transform _origin;

    public void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();
        _transitionManager = FindObjectOfType<TransitionManager>();
        
        _leftOrbCamera = new GameObject("LeftCamera").AddComponent<OrbCamera>();
        _rightOrbCamera = new GameObject("RightCamera").AddComponent<OrbCamera>();
        
        _origin = new GameObject("OrbOriginDummy").transform;
        _origin.parent = FindObjectOfType<XROrigin>().transform;
        
        Transform cameraTransform = _transitionManager.MainCamera.transform;
        _origin.position = cameraTransform.position;
        _origin.rotation = cameraTransform.rotation;
    }

    public void Initialize(OrbTransition transition)
    {
        _transition = transition;
        _destination = transition.Destination;
        
        _leftOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);
        _rightOrbCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _transitionManager.MainCamera.transform.position) <= 0.2f)
        {
            _transition.TriggerTransition(Traveller.GetPlayerTraveller(), _destination.position,
                Quaternion.AngleAxis(180f, Vector3.up));
        }
    }

    public void OnDestroy()
    {
        Destroy(_origin.gameObject);
        Destroy(_leftOrbCamera.gameObject);
        Destroy(_rightOrbCamera.gameObject);
    }
}