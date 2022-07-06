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
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{

    [SerializeReference]
    public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;
    
    [field: SerializeField]
    public Context CurrentContext { private set; get; }

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
    private XROrigin _xrOrigin;

    
    
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
        
#if UNITY_EDITOR
        CurrentContext = FindObjectsOfType<Context>().First(context => context.name == "Context 1");
        
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
    
    
    public async Task InitializeTransitionType(Type type)
    {
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() != type && transition.IsInitialized).Select(transition => transition.Deinitialize()));
        await Task.WhenAll(Transitions.Where(transition => transition.GetType() == type).Select(transition => transition.Initialize()));
    }

    [MenuItem("Transition/Initialize/Transformation")]
    public static void InitializeTransformations()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(TransformationTransition));
    }
    [MenuItem("Transition/Initialize/Portal")]
    public static void InitializePortals()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(PortalTransition));
    }
    [MenuItem("Transition/Initialize/Cut")]
    public static void InitializeCuts()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(CutTransition));
    }
    [MenuItem("Transition/Initialize/Fade")]
    public static void InitializeFades()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(FadeTransition));
    }
    [MenuItem("Transition/Initialize/Dissolve")]
    public static void InitializeDissolve()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(DissolveTransition));
    }
    [MenuItem("Transition/Initialize/Orbs")]
    public static void InitializeOrbs()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        var tm = FindObjectOfType<TransitionManager>();
        tm.InitializeTransitionType(typeof(OrbTransition));
    }
}
