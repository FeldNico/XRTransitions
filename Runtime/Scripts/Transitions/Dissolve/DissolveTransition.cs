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

        internal override async Task OnTriggerTransition()
        {
            Destination.transform.position = new Vector3(Destination.transform.position.x,
                _transitionManager.MainCamera.transform.position.y, Destination.transform.position.z);
            
            _dissolve = Object.Instantiate(DissolvePrefab).GetComponent<Dissolve>();
            _dissolve.transform.parent = _transitionManager.MainCamera.transform;
            _dissolve.transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane+0.01f);
            _dissolve.transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
            _dissolve.Initialize(this);
            _dissolve.PlaneRenderer.material.SetFloat(Alpha,0f);

            var startTime = Time.time;
            while (Time.time <= startTime + Duration)
            {
                await Task.Yield();
                _dissolve.PlaneRenderer.material.SetFloat(Alpha,(Time.time - startTime)/Duration);
            }
            _dissolve.PlaneRenderer.material.SetFloat(Alpha,1);
            
            _transitionManager.XROrigin.MoveCameraToWorldLocation(Destination.position);
            var rotDiff = Destination.rotation * Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            rotDiff.ToAngleAxis(out var angle, out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);

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
                TriggerTransition();
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
            transition.TriggerTransition();
        }
    }
}