using System;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class FadeTransition : Transition
    {

        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _fadePrefab;
        [SerializeField] private Color _color = Color.black;
        [SerializeField] private float _duration;
        [SerializeField]
        private InputActionProperty _initiateAction;

        private TransitionManager _transitionManager;

        private Fade _fade;
        
        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
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
            _fade = Object.Instantiate(_fadePrefab).GetComponent<Fade>();
            _fade.Initialize(this);

            await _fade.FadeOut(_duration / 2f, _color);
            
            var localToWorldMatrix = Destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _transitionManager.XROrigin.transform.worldToLocalMatrix * _transitionManager.MainCamera.transform.localToWorldMatrix;
            _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
            Quaternion targetRotation = localToWorldMatrix.rotation *
                                        Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            targetRotation.ToAngleAxis(out var angle,out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);

            await _fade.FadeIn(_duration / 2f);
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Fade")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }
            
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(FadeTransition));
            if (transition != null)
            {
                await transition.Initialization();
                transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No FadeTransition found");
            }
        }
    }