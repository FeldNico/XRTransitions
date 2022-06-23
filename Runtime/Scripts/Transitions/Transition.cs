using System;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Scripts
{
    [Serializable]
    public abstract class Transition
    {
        public Transform Destination => _destination;
        public bool IsTransitioning { get; private set; }
        [field: SerializeField, DisableProperty]
        public bool IsInitialized { get; private set; }
        
        [SerializeField]
        private Transform _destination;
        private TransitionManager _manager;
        private Context _targetContext;
        internal abstract Task OnInitialization();
        internal abstract Task OnDeinitialization();
        internal abstract Task  OnTriggerTransition();
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
            await OnInitialization();
            IsInitialized = true;
        }

        public async Task Deinitialize()
        {
            if (!IsInitialized)
            {
                return;
            }
            await OnDeinitialization();
            IsInitialized = false;
        }
        
        public async Task TriggerTransition()
        {
            if (_manager == null)
            {
                _manager = Object.FindObjectOfType<TransitionManager>();
            }

            if (_manager.CurrentContext != GetStartContext() || IsTransitioning)
            {
                return;
            }

            IsTransitioning = true;
            Context.OnExit?.Invoke(GetStartContext());

            Debug.Log("Transition");
            
            await OnTriggerTransition();

            Physics.SyncTransforms();
            
            Context.OnEnter?.Invoke(GetTargetContext());
            IsTransitioning = false;
        }
    }
}