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
        private bool _wasPressed;
        internal override async Task OnInitialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !TransitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }
            _initiateAction.EnableDirectAction();
            InputSystem.onAfterUpdate += HandleInput;
        }

        internal override async Task OnDeinitialization()
        {
            _initiateAction.DisableDirectAction();
            InputSystem.onAfterUpdate -= HandleInput;
            while (TransitionManager.IsTransitioning)
            {
                await Task.Delay(1);
            }
            
            if (_transformation != null)
            {
                Object.Destroy(_transformation);
            }
            _wasPressed = false;
        }

        private async void HandleInput()
        {
            if (_initiateAction.action.ReadValue<float>() > 0.7f && !_wasPressed)
            {
                _wasPressed = true;
                TriggerTransition();
            }
            if (_initiateAction.action.ReadValue<float>() < 0.3f && _wasPressed)
            {
                _wasPressed = false;
            }
        }
        
        internal override async Task OnTriggerTransition()
        {
            _transformation = Object.Instantiate(_transformationPrefab).GetComponent<Transformation>();
            _transformation.Initialize(this);
            
            await _transformation.BlendForSeconds(_duration);

            TransitionManager.XROrigin.transform.position = Destination.position;

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
                await transition.OnInitialization();
                await transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No TransformationTransition found");
            }
        }
    }
}