using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;

namespace Scripts.Transformation
{
    public class TransformationTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _transformationPrefab;
        [SerializeField] private float _duration = 1.3f;
        [SerializeField]
        private InputActionProperty _initiateAction;
        
        private Transformation _transformation;
        private TransitionManager _transitionManager;
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
        
        internal override async Task OnTriggerTransition()
        {
            _transformation = Object.Instantiate(_transformationPrefab).GetComponent<Transformation>();
            _transformation.Initialize(this);
            
            await _transformation.BlendForSeconds(_duration);
            
            var localToWorldMatrix = Destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _transformation.LocalDummy.transform.worldToLocalMatrix * _transitionManager.MainCamera.transform.localToWorldMatrix;
            _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
            Quaternion targetRotation = localToWorldMatrix.rotation *
                                        Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            targetRotation.ToAngleAxis(out var angle,out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);

            Object.Destroy(_transformation.gameObject);
            _transformation = null;
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Transformation")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }
            
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(TransformationTransition) && transition.GetStartContext() == transitionManager.CurrentContext);
            if (transition != null)
            {
                await transition.Initialization();
                await transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No TransformationTransition found");
            }
        }
    }
}