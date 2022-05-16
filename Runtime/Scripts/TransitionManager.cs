using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{

    [SerializeReference]
    public List<Transition> Transitions;

    public XROrigin XROrigin => _xrOrigin;
    public Camera MainCamera => _mainCamera;
    public Transform LeftEyeTransform => _leftEyeTransform;
    public Transform RightEyeTransform => _rightEyeTransform;
    public Context CurrentContext => _currentContext;
    
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private Transform _leftEyeTransform;
    [SerializeField]
    private Transform _rightEyeTransform;
    [SerializeReference]
    private Context _currentContext;
    private XROrigin _xrOrigin;

    private void Awake()
    {
        if (_currentContext == null)
        {
            Debug.LogError("No Current Context was defined. Please assign Current Context at your TransitionManager");
        }

        _xrOrigin = FindObjectOfType<XROrigin>();
        
        Context.OnExit += context =>
        {
            if (_currentContext == context)
            {
                _currentContext = null;
            }
        };
        
        Context.OnEnter += context =>
        {
            _currentContext = context;
        };
    }
    
    private void Start()
    {
        Initialization();
    }

    private async void Initialization()
    {
        var tasks = Transitions.Select(transition => transition.Initialization());
        await Task.WhenAll(tasks);
    }
}
