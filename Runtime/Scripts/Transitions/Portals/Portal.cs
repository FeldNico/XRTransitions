using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts;
using Scripts.Utils;
using UnityEngine;

public class Portal : MonoBehaviour
{

    private bool _isInitialized = false;
    
    [SerializeField]
    private Renderer _planeRenderer;
    public Renderer PlaneRenderer => _planeRenderer;

    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;

    private PortalTransition _transition;
    private Transform _destination;
    private Dictionary<TransitionTraveller,List<(Transform,Transform)>> _travellersDict = new();

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = transform.Find("RenderPlane").GetComponent<MeshRenderer>();
        }
    }

    public void Initialize(PortalTransition transition)
    {
        _transition = transition;
        
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
        _isInitialized = true;
    }

    public bool FrontOfPortal(Vector3 pos)
    {
        Transform t = transform;
        return Math.Sign(Vector3.Dot(pos - t.position, t.forward)) > 0;
    }

    private void Update()
    {
        HandleTravellers();
    }
    
    private async void HandleTravellers()
    {
        if (!_isInitialized)
        {
            return;
        }

        var travellers = _travellersDict.Keys.ToArray();
        
        for (int i = 0; i < _travellersDict.Count; i++)
        {
            TransitionTraveller transitionTraveller = travellers[i];

            if (FrontOfPortal(transitionTraveller.LastPosition) && !FrontOfPortal(transitionTraveller.transform.position))
            {
                var m = _destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix *
                        transitionTraveller.transform.localToWorldMatrix;
                Vector3 targetPosition = m.GetColumn(3);
                Quaternion targetRotation =
                    Quaternion.FromToRotation(transform.forward, _destination.forward) *
                    Quaternion.AngleAxis(180f, Vector3.up);
                await _transition.TriggerTransition(transitionTraveller,targetPosition,targetRotation);
                foreach (var (_, dummyTransform) in _travellersDict[transitionTraveller])
                {
                    Destroy(dummyTransform.gameObject);
                }
                _travellersDict[transitionTraveller].Clear();
                _travellersDict.Remove(transitionTraveller);
                i--;
            }
            else
            {
                foreach (var (originalTransform, dummyTransform) in _travellersDict[transitionTraveller])
                {
                    var localToWorldMatrix = _destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix * originalTransform.localToWorldMatrix;
                    dummyTransform.SetPositionAndRotation(localToWorldMatrix.GetColumn(3),localToWorldMatrix.rotation);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TransitionTraveller traveller = other.GetComponent<TransitionTraveller>();
        if (traveller && !_travellersDict.Keys.Contains(traveller))
        {
            _travellersDict.Add(traveller,new List<(Transform, Transform)>());
            MeshFilter[] filters = traveller.Origin.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null).ToArray();
            foreach (MeshFilter filter in filters)
            {
                GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
                MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
                MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
                dummyFilter.GetCopyOf(filter);
                dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
                dummy.transform.localScale = filter.transform.lossyScale;
                dummy.transform.parent = transform;
                _travellersDict[traveller].Add((filter.transform,dummy.transform));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TransitionTraveller traveller = other.GetComponent<TransitionTraveller>();
        if (traveller && _travellersDict.Keys.Contains(traveller))
        {
            foreach (var (_, dummyTransform) in _travellersDict[traveller])
            {
                Destroy(dummyTransform.gameObject);
            }
            _travellersDict[traveller].Clear();
            _travellersDict.Remove(traveller);
        }
    }
}