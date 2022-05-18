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
        public bool IsTransitioning => _isTransitioning;
        
        [SerializeField]
        private Transform _destination;
        private TransitionManager _manager;
        private Context _targetContext;
        private bool _isTransitioning;
        public abstract Task Initialization();
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
        
        public async Task TriggerTransition()
        {
            if (_manager == null)
            {
                _manager = Object.FindObjectOfType<TransitionManager>();
            }

            if (_manager.CurrentContext != GetStartContext() || _isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            Context.OnExit?.Invoke(GetStartContext());

            Debug.Log("Transition");
            
            await OnTriggerTransition();

            Physics.SyncTransforms();
            
            Context.OnEnter?.Invoke(GetTargetContext());
            _isTransitioning = false;
        }
    }
}