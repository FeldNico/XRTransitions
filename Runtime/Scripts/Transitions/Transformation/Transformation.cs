using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts.Utils;
using UnityEngine;

namespace Scripts.Transformation
{
    public class Transformation : MonoBehaviour
    {
        private static readonly int Progress = Shader.PropertyToID("_Progress");

        public Renderer Renderer => _renderer;

        [SerializeField] private Renderer _renderer;
        private TransformationCamera _leftPortalCamera;
        private TransformationCamera _rightPortalCamera;
        private TransitionManager _transitionManager;
        private Transform _destination;
        private List<(Transform, Transform)> _dummyList = new();

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            _transitionManager = FindObjectOfType<TransitionManager>();
        }

        public void Initialize(TransformationTransition transition)
        {
            _destination = transition.Destination;

            _leftPortalCamera = new GameObject("LeftCamera").AddComponent<TransformationCamera>();
            _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

            _rightPortalCamera = new GameObject("RightCamera").AddComponent<TransformationCamera>();
            _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

            var camTransform = _transitionManager.MainCamera.transform;
            transform.position = camTransform.position;
            transform.localRotation = Quaternion.LookRotation(camTransform.forward,
                camTransform.up);
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
                dummy.transform.localScale = filter.transform.lossyScale;
                _dummyList.Add((filter.transform,dummy.transform));
            }
            
            var startTime = Time.time;
            Renderer.material.SetFloat(Progress, 0);
            while (Time.time <= startTime + seconds)
            {
                await Task.Yield();
                Renderer.material.SetFloat(Progress, (Time.time - startTime) / seconds);
            }

            Renderer.material.SetFloat(Progress, 1);
            
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
            if (_leftPortalCamera != null)
            {
                Destroy(_leftPortalCamera.gameObject);
            }

            if (_rightPortalCamera != null)
            {
                Destroy(_rightPortalCamera.gameObject);
            }
        }
    }
}