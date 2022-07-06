using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Object = UnityEngine.Object;

namespace Scripts
{
    [Serializable]
    public abstract class Transition
    {
        [field: SerializeField] public Transform Destination { get; private set; }

        [field: SerializeField, DisableProperty]
        public bool IsInitialized { get; private set; }

        [SerializeField] private InputActionProperty _initiateAction;

        public TransitionManager TransitionManager { get; private set; }

        private Context _targetContext;
        private InputDevice _currentPressedDevice = null;
        internal abstract Task OnInitialization();
        internal abstract Task OnDeinitialization();
        internal abstract Task OnTriggerTransition();
        internal abstract Task OnActionPressed();
        internal abstract Task OnActionRelease();
        public abstract Context GetStartContext();

        public virtual Context GetTargetContext()
        {
            if (_targetContext == null)
            {
                _targetContext = Destination.GetComponentInParent<Context>();
            }

            return _targetContext;
        }

        public async Task Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            TransitionManager = Object.FindObjectOfType<TransitionManager>();
            await OnInitialization();
            _initiateAction.EnableDirectAction();
            InputSystem.onAfterUpdate += HandleInput;
            IsInitialized = true;
        }

        public async Task Deinitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            _initiateAction.DisableDirectAction();
            InputSystem.onAfterUpdate -= HandleInput;
            _currentPressedDevice = null;
            await OnDeinitialization();
            IsInitialized = false;
        }

        private async void HandleInput()
        {
            if (!TransitionManager.IsTransitioning && GetStartContext() == TransitionManager.CurrentContext && _initiateAction.action.ReadValue<float>() > 0.7f && _currentPressedDevice == null)
            {
                _currentPressedDevice = _initiateAction.action.activeControl.device;
                await OnActionPressed();
            }

            if (_initiateAction.action.ReadValue<float>() < 0.3f && _initiateAction.action.activeControl != null && _initiateAction.action.activeControl.device == _currentPressedDevice)
            {
                _currentPressedDevice = null;
                await OnActionRelease();
            }
        }

        public async Task TriggerTransition()
        {
            if (!IsInitialized || TransitionManager.CurrentContext != GetStartContext() ||
                TransitionManager.IsTransitioning)
            {
                return;
            }

            TransitionManager.OnStartTransition?.Invoke(this);
            Context.OnExit?.Invoke(GetStartContext());

            Debug.Log("Transition");

            await OnTriggerTransition();

            Physics.SyncTransforms();

            await Task.Yield();

            Context.OnEnter?.Invoke(GetTargetContext());
            TransitionManager.OnEndTransition?.Invoke(this);
        }
    }
}