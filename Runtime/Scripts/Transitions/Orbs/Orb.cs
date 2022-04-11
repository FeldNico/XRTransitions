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

    private OrbCamera _leftPortalCamera;
    private OrbCamera _rightPortalCamera;

    private OrbTransition _transition;
    private Transform _destination;

    private Transform _origin;
    public Transform Origin => _origin;

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
            var pos = _destination.position;
            pos.y = FindObjectOfType<XROrigin>().CameraYOffset;
            _destination.position = pos;

            if (Vector3.Distance(transform.position, _transition.Camera.transform.position) <= 0.2f)
            {
                var xrOrigin = FindObjectOfType<XROrigin>();
                var cameraTransform = _transition.Camera.transform;

                var diff = xrOrigin.transform.position - cameraTransform.transform.position;
                xrOrigin.transform.position = _destination.position + diff;
                DeInitiate();
                _orbRenderer.enabled = false;
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
        _leftPortalCamera.StopRender();
        _rightPortalCamera.StopRender();
    }
}