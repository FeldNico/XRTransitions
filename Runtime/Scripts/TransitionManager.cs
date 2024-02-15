using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class TransitionManager : MonoBehaviour
{
    [SerializeReference] public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Vector3 CenterEyePosition => (_leftEyeTransform.position + _rightEyeTransform.position) / 2f; // Can't use TrackedPoseDriver for CenterEye, as Varjo does not update the MR Offset on the CenterEye, only on the Left and Right Eye.
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;

    [field: SerializeField] public Context CurrentContext { set; get; }

    public bool IsTransitioning { get; private set; }

    public Transition CurrentTransition { get; private set; }

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _leftEyeTransform;
    [SerializeField] private Transform _rightEyeTransform;
    [SerializeField] private InputActionProperty _initiateAction;
    private XROrigin _xrOrigin;
    private InputDevice _currentPressedDevice = null;

    private void Awake()
    {
        _xrOrigin = FindObjectOfType<XROrigin>();

        Context.OnExit += context =>
        {
            if (CurrentContext == context)
            {
                CurrentContext = null;
            }
        };

        Context.OnEnter += context => { CurrentContext = context; };

        Transition.OnStartTransition += t =>
        {
            IsTransitioning = true;
            CurrentTransition = t;
        };

        Transition.OnEndTransition += t =>
        {
            IsTransitioning = false;
            if (t == CurrentTransition)
            {
                CurrentTransition = null;
            }
        };

        _initiateAction.EnableDirectAction();
        InputSystem.onAfterUpdate += HandleInput;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += async change =>
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var t in Transitions.Where(transition => transition.IsInitialized))
                {
                    await t.Deinitialize();
                }
            }
        };
#endif
    }

    private Dictionary<InputDevice, float> _inputDict = new();

    private async void HandleInput()
    {
        if (_initiateAction.action.activeControl == null)
        {
            return;
        }

        var device = _initiateAction.action.activeControl.device;
        var currentValue = _initiateAction.action.ReadValue<float>();
        if (!_inputDict.ContainsKey(device))
        {
            _inputDict[device] = currentValue;
        }
        var lastValue = _inputDict[device];

        var transition = Transitions.FirstOrDefault(t => t.IsInitialized && t.GetStartContext() == CurrentContext);

        if (currentValue > 0.7f && lastValue <= 0.7f && transition != null && !IsTransitioning &&
            _currentPressedDevice == null)
        {
            _currentPressedDevice = device;
            var controller = FindObjectsOfType<ActionBasedController>().FirstOrDefault(controller =>
                controller.hapticDeviceAction.action.activeControl.device == _currentPressedDevice);
            controller.SendHapticImpulse(0.3f, 0.1f);
            var isRight = controller.name.ToLower().Contains("right");
            Transition.OnActionPressed?.Invoke(transition, isRight);

            await transition.OnActionDown(isRight);
        }

        if (currentValue < 0.4f && device == _currentPressedDevice)
        {
            if (transition != null)
            {
                await transition?.OnActionUp();
                Transition.OnActionReleased?.Invoke(transition);
            }

            _currentPressedDevice = null;
        }
    }

    public List<Transition> GetActiveTransitions()
    {
        return Transitions.Where(transition => transition.IsInitialized).ToList();
    }

    public async Task InitializeTransitionType(Type type)
    {
        _currentPressedDevice = null;
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() != type && transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() == type)
            .Select(transition => transition.Initialize()));
    }

    public async Task DisableTransitions()
    {
        _currentPressedDevice = null;
        await Task.WhenAll(Transitions.Where(transition => transition.IsInitialized)
            .Select(transition => transition.Deinitialize()));
    }

    /*
    [MenuItem("Transition/Trigger")]
    public static void TriggerAction()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        var tManager = FindObjectOfType<TransitionManager>();
        var transition = tManager.GetActiveTransitions().FirstOrDefault(transition => transition.GetStartContext() == tManager.CurrentContext);
        if (transition != null)
        {
            transition.OnActionDown(true);
        }
    }
    */
}