using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Scripts;
using Scripts.Utils;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Dissolve : MonoBehaviour
{
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    public Renderer PlaneRenderer => _planeRenderer;
    public Transform LocalDummy => _localDummy;
    
    [SerializeField]
    private Renderer _planeRenderer;
    private DissolveCamera _leftPortalCamera;
    private DissolveCamera _rightPortalCamera;
    private TransitionManager _transitionManager;
    private Transform _destination;
    private Transform _localDummy;
    private List<(Transform, Transform)> _dummyList = new();

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = GetComponentInChildren<Renderer>();
        }

        _transitionManager = FindObjectOfType<TransitionManager>();
        _localDummy = new GameObject("DissolveLocalDummy").transform;
        //_localDummy.parent = _transitionManager.XROrigin.transform;
        var camPos = _transitionManager.MainCamera.transform.position;
        camPos.y = _transitionManager.XROrigin.transform.position.y;
        _localDummy.position = camPos;
        _localDummy.rotation = Quaternion.identity;
    }

    public void Initialize(DissolveTransition transition)
    {
        _destination = transition.Destination;
        
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<DissolveCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<DissolveCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);
        
        transform.parent = _transitionManager.MainCamera.transform;
        transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane+0.01f);
        transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
        
        PlaneRenderer.material.SetFloat(Alpha,0f);
    }

    public async Task BlendForSeconds(float seconds)
    {
        MeshFilter[] filters = _transitionManager.XROrigin.GetComponentsInChildren<MeshFilter>().Where(filter => filter.GetComponent<MeshRenderer>() != null && filter.GetComponentInParent<Dissolve>() == null).ToArray();
        foreach (MeshFilter filter in filters)
        {
            GameObject dummy = new GameObject(filter.gameObject.name + "-Dummy");
            MeshFilter dummyFilter = dummy.AddComponent<MeshFilter>();
            MeshRenderer dummyRenderer = dummy.AddComponent<MeshRenderer>();
            dummyFilter.GetCopyOf(filter);
            dummyRenderer.GetCopyOf(filter.GetComponent<MeshRenderer>());
            dummyRenderer.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            dummy.transform.parent = _localDummy;
            dummy.transform.localScale = filter.transform.lossyScale;
            _dummyList.Add((filter.transform,dummy.transform));
        }

        var startTime = Time.time;
        while (Time.time <= startTime + seconds)
        {
            await Task.Yield();
            PlaneRenderer.material.SetFloat(Alpha,(Time.time - startTime)/seconds);
        }
        PlaneRenderer.material.SetFloat(Alpha,1);
        
        foreach (var (_, dummyTransform) in _dummyList)
        {
            Destroy(dummyTransform.gameObject);
        }
        _dummyList.Clear();
    }

    private void Update()
    {
        foreach (var (originalTransform, dummyTransform) in _dummyList)
        {
            dummyTransform.position =
                _destination.transform.TransformPoint(_transitionManager.XROrigin.transform.InverseTransformPoint(originalTransform.position));
            dummyTransform.rotation = _destination.transform.TransformRotation(
                _transitionManager.XROrigin.transform.InverseTransformRotation(originalTransform.rotation));
        }
    }

    private void OnDestroy()
    {
        Destroy(_localDummy.gameObject);
        Destroy(_leftPortalCamera.gameObject);
        Destroy(_rightPortalCamera.gameObject);
    }
}