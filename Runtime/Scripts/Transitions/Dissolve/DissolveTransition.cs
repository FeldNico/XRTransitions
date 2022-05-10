using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    
    [Serializable]
    public class DissolveTransition: Transition
    {
        [SerializeField] private GameObject DissolvePrefab;
        
        private Camera _camera;
        public Camera Camera => _camera;
        
        private Transform _eyeLeftTransform;
        public Transform EyeLeftTransform => _eyeLeftTransform;
        
        private Transform _eyeRightTransform;
        public Transform EyeRightTransform => _eyeRightTransform;
        
        [SerializeField] private float _duration;
        public float Duration => _duration;

        private Dissolve _dissolve;
        
        private TransitionManager _transitionManager;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private void Awake()
        {
            
        }
        
        public override bool IsTransitioning { get; protected set; }

        public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (traveller.IsPlayer())
            {
                IsTransitioning = true;
                OnTransition?.Invoke();
            }

            _dissolve = Object.Instantiate(DissolvePrefab).GetComponent<Dissolve>();
            _dissolve.transform.parent = _camera.transform;
            _dissolve.transform.localPosition = new Vector3(0f, 0.5f, 0);
            _dissolve.transform.localRotation = Quaternion.identity;
            _dissolve.Initialize(this);

            var startTime = Time.time;
            while (Time.time <= startTime + Duration)
            {
                await Task.Yield();
                _dissolve.PlaneRenderer.material.SetFloat(Alpha,(Time.time - startTime)/Duration);
            }
            _dissolve.PlaneRenderer.material.SetFloat(Alpha,1);
            
            traveller.Origin.position = (traveller.Origin.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Origin.RotateAround(traveller.transform.position,axis,angle);
            Physics.SyncTransforms();

            if (traveller.IsPlayer())
            {
                IsTransitioning = false;
                OnTransitionEnd?.Invoke();
            }

            //Destroy(_dissolve.gameObject);
            //_dissolve = null;
        }

        public override async Task Initialization(Camera mainCamera, Transform leftEyeTransform, Transform rightEyeTransform)
        {
            _camera = mainCamera;
            _eyeLeftTransform = leftEyeTransform;
            _eyeRightTransform = rightEyeTransform;
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
        }

        [MenuItem("Dissolve/Trigger")]
        public static void Trigger()
        {
            
        }
    }
}