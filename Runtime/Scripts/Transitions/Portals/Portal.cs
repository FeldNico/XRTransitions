using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts;
using Scripts.Utils;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Renderer PlaneRenderer => _planeRenderer;
    
    [SerializeField]
    private Renderer _planeRenderer;

    private bool _isInitialized = false;
    
    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;

    private PortalTransition _transition;
    private TransitionManager _transitionManager;
    private Transform _destination;
    private Transform _mainCameraTransform;
    private Vector3 _lastPosition;
    private List<(Transform, Transform)> _dummyList = new();
    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = transform.Find("RenderPlane").GetComponent<MeshRenderer>();
        }
        _transitionManager = FindObjectOfType<TransitionManager>();
        _mainCameraTransform = _transitionManager.MainCamera.transform;
    }

    public void Initialize(PortalTransition transition)
    {
        _transition = transition;
        
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
        _lastPosition = _mainCameraTransform.position;
        _isInitialized = true;
    }

    private bool FrontOfPortal(Vector3 pos)
    {
        Transform t = transform;
        return Math.Sign(Vector3.Dot(pos - t.position, t.forward)) > 0;
    }

    private async void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (FrontOfPortal(_lastPosition) && !FrontOfPortal(_mainCameraTransform.position))
        {
            await _transition.TriggerTransition();
            foreach (var (_, dummyTransform) in _dummyList)
            {
                Destroy(dummyTransform.gameObject);
            }
            _dummyList.Clear();
        }
        else
        {
            foreach (var (originalTransform, dummyTransform) in _dummyList)
            {
                var localToWorldMatrix = _destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix * originalTransform.localToWorldMatrix;
                dummyTransform.SetPositionAndRotation(localToWorldMatrix.GetColumn(3),localToWorldMatrix.rotation);
            }
        }
    }

    private void LateUpdate()
    {
        _lastPosition = _mainCameraTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == _mainCameraTransform)
        {
            MeshFilter[] filters = _mainCameraTransform.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null).ToArray();
            foreach (MeshFilter filter in filters)
            {
                GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
                MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
                MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
                dummyFilter.GetCopyOf(filter);
                dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
                dummy.transform.localScale = filter.transform.lossyScale;
                dummy.transform.parent = transform;
                _dummyList.Add((filter.transform,dummy.transform));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == _mainCameraTransform)
        {
            foreach (var (_, dummyTransform) in _dummyList)
            {
                Destroy(dummyTransform.gameObject);
            }
            _dummyList.Clear();
        }
    }

    private void OnDestroy()
    {
        Destroy(_leftPortalCamera.gameObject);
        Destroy(_rightPortalCamera.gameObject);
        foreach (var (_, dummyTransform) in _dummyList)
        {
            Destroy(dummyTransform.gameObject);
        }
        _dummyList.Clear();
    }
}