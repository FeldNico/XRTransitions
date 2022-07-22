using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using Scripts.Transformation;
using Scripts.Transitions.Cut;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class TransitionManager : MonoBehaviour
{

    [SerializeReference]
    public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;
    
    [field: SerializeField]
    public Context CurrentContext { set; get; }

    public bool IsTransitioning { get; private set;  }
    
    public Transition CurrentTransition { get; private set; }

    public UnityAction<Transition> OnStartTransition;
    public UnityAction<Transition> OnEndTransition;

    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private Transform _leftEyeTransform;
    [SerializeField]
    private Transform _rightEyeTransform;
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
        
        Context.OnEnter += context =>
        {
            CurrentContext = context;
        };

        OnStartTransition += t =>
        {
            IsTransitioning = true;
            CurrentTransition = t;
        };

        OnEndTransition += t =>
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
    
    private async void HandleInput()
    {
        var transition = Transitions.FirstOrDefault(t => t.IsInitialized && t.GetStartContext() == CurrentContext);

        if (transition != null && !IsTransitioning && _initiateAction.action.ReadValue<float>() > 0.7f && _currentPressedDevice == null)
        {
            _currentPressedDevice = _initiateAction.action.activeControl.device;
            await transition.OnActionPressed();
        }

        if (_initiateAction.action.ReadValue<float>() < 0.3f && _initiateAction.action.activeControl != null && _initiateAction.action.activeControl.device == _currentPressedDevice)
        {
            _currentPressedDevice = null;
            if (transition != null)
            {
                await transition?.OnActionRelease();
            }
        }
    }

    public async Task InitializeTransitionType(Type type)
    {
        _currentPressedDevice = null;
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() != type && transition.IsInitialized).Select(transition => transition.Deinitialize()));
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() == type).Select(transition => transition.Initialize()));
    }
}
