using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    private bool _isPlayerInBounds = false;
    private bool _isAnimating;
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
        Context.OnEnter += context =>
        {
            if (context == _transition.GetStartContext())
            {
                _lastPosition = _mainCameraTransform.position;
            }
        };
        _isInitialized = true;
    }

    public async void AnimPortal()
    {
        if (_isAnimating)
        {
            return;
        }

        _isAnimating = true;
            
        if (gameObject.activeSelf)
        {
            transform.localScale = Vector3.one;

            var startTime = Time.time;
            while (Time.time < startTime + 0.15f)
            {
                transform.localScale =
                    Vector3.Lerp(Vector3.one, Vector3.zero, (Time.time - startTime) / 0.15f);
                await Task.Delay(1);
            }

            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
            transform.localScale = Vector3.one;
                
            _isAnimating = false;
        }
        else
        {
            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;

            var startTime = Time.time;
            while (Time.time < startTime + 0.15f)
            {
                transform.localScale =
                    Vector3.Lerp(Vector3.zero, Vector3.one, (Time.time - startTime) / 0.15f);
                await Task.Delay(1);
            }

            transform.localScale = Vector3.one;
                
            _isAnimating = false;
        }
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

        if (_isPlayerInBounds)
        {
            if (FrontOfPortal(_lastPosition) ^ FrontOfPortal(_mainCameraTransform.position))
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
    }

    private void LateUpdate()
    {
        _lastPosition = _mainCameraTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == _mainCameraTransform)
        {
            _isPlayerInBounds = true;
            _lastPosition = _mainCameraTransform.position;
            MeshFilter[] filters = _transitionManager.XROrigin.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null).ToArray();
            foreach (MeshFilter filter in filters)
            {
                GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
                MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
                MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
                dummyFilter.GetCopyOf(filter);
                dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
                dummyRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
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
            _isPlayerInBounds = false;
            foreach (var (_, dummyTransform) in _dummyList)
            {
                Destroy(dummyTransform.gameObject);
            }
            _dummyList.Clear();
        }
    }

    private void OnEnable()
    {
        if (_leftPortalCamera != null)
        {
            _leftPortalCamera.gameObject.SetActive(true);
        }
        if (_rightPortalCamera != null)
        {
            _rightPortalCamera.gameObject.SetActive(true);
        }

        if (_isInitialized && !FrontOfPortal(_transitionManager.MainCamera.transform.position))
        {
            transform.localRotation *= Quaternion.AngleAxis(180f,Vector3.up);
            _transition.Destination.localRotation *= Quaternion.AngleAxis(180f,Vector3.up);
        }
    }
    
    private void OnDisable()
    {
        if (_leftPortalCamera != null)
        {
            _leftPortalCamera.gameObject.SetActive(false);
        }
        if (_rightPortalCamera != null)
        {
            _rightPortalCamera.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_leftPortalCamera)
        {
            Destroy(_leftPortalCamera.gameObject);
        }
        if (_rightPortalCamera)
        {
            Destroy(_rightPortalCamera.gameObject);
        }
        
        foreach (var (_, dummyTransform) in _dummyList)
        {
            Destroy(dummyTransform.gameObject);
        }
        _dummyList.Clear();
    }
}