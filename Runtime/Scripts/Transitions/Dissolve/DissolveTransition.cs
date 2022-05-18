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
            _dissolve = Object.Instantiate(DissolvePrefab).GetComponent<Dissolve>();
            _dissolve.Initialize(this);
            
            await _dissolve.BlendForSeconds(Duration);
            
            var localToWorldMatrix = Destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _dissolve.LocalDummy.transform.worldToLocalMatrix * _transitionManager.MainCamera.transform.localToWorldMatrix;
            _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
            Quaternion targetRotation = localToWorldMatrix.rotation *
                                        Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            targetRotation.ToAngleAxis(out var angle,out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);

            Object.Destroy(_dissolve.gameObject);
            _dissolve = null;
        }

        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !_transitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }
            _initiateAction.EnableDirectAction();
            InputSystem.onAfterUpdate += HandleInput;
        }

        private void HandleInput()
        {
            if (_initiateAction.action.WasPressedThisFrame())
            {
                TriggerTransition();
            }
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Dissolve")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }
            
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(DissolveTransition));
            if (transition != null)
            {
                await transition.Initialization();
                transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No DissolveTransition found");
            }
        }
    }
}