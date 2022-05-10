using System;
using System.Linq;
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

        [SerializeField] private float _duration;
        public float Duration => _duration;

        private Dissolve _dissolve;
        
        private TransitionManager _transitionManager;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        public override bool IsTransitioning { get; protected set; }

        public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (traveller.IsPlayer())
            {
                IsTransitioning = true;
                OnTransition?.Invoke();
            }

            _dissolve = Object.Instantiate(DissolvePrefab).GetComponent<Dissolve>();
            _dissolve.transform.parent = _transitionManager.MainCamera.transform;
            _dissolve.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            _dissolve.transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
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

            Object.Destroy(_dissolve.gameObject);
            _dissolve = null;
        }

        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
        }

        [MenuItem("Dissolve/Trigger")]
        public static async void Trigger()
        {
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.First(transition => transition.GetType() == typeof(DissolveTransition));
            await transition.Initialization();
            transition.TriggerTransition(Object.FindObjectOfType<DissolveTraveller>(), transition.Destination.position,
                Quaternion.identity);
        }
    }
}