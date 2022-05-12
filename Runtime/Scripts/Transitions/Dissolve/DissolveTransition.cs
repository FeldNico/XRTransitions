using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    
    [Serializable]
    public class DissolveTransition: Transition
    {
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        
        public float Duration => _duration;

        [SerializeField]
        private Context _startContext;
        
        [SerializeField] 
        private GameObject DissolvePrefab;

        [SerializeField] 
        private float _duration;
        
        [SerializeField]
        private InputActionProperty _initiateAction;
        
        private Dissolve _dissolve;
        private TransitionManager _transitionManager;

        internal override async Task OnTriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
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

            Object.Destroy(_dissolve.gameObject);
            _dissolve = null;
        }

        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Delay(1);
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _initiateAction.EnableDirectAction();
            _initiateAction.action.performed += _ =>
            {
                TriggerTransition(Traveller.GetPlayerTraveller(), Destination.position,
                    Quaternion.identity* Quaternion.AngleAxis(180f,Vector3.up));
            };
        }
        
        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Dissolve")]
        public static async void Trigger()
        {
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.First(transition => transition.GetType() == typeof(DissolveTransition));
            await transition.Initialization();
            transition.TriggerTransition(Traveller.GetPlayerTraveller(), transition.Destination.position,
                Quaternion.identity* Quaternion.AngleAxis(180f,Vector3.up));
        }
    }
}