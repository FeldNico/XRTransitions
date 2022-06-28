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
        
        private bool _wasPressed;

        private Fade _fade;
        
        internal override async Task OnInitialization()
        {
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

            if (_fade != null)
            {
                Object.Destroy(_fade.gameObject);
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
            _fade = Object.Instantiate(_fadePrefab).GetComponent<Fade>();
            _fade.Initialize(this);

            await _fade.FadeOut(_duration / 2f, _color);

            TransitionManager.XROrigin.transform.position = Destination.position;

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
                await transition.OnInitialization();
                transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No FadeTransition found");
            }
        }
    }