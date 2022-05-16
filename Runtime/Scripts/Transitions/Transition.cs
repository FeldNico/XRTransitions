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
        
        [SerializeField]
        private Transform _destination;
        private TransitionManager _manager;
        private Context _targetContext;
        
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

            if (_manager.CurrentContext != GetStartContext())
            {
                return;
            }
            
            Context.OnExit?.Invoke(GetStartContext());

            await OnTriggerTransition();

            Physics.SyncTransforms();
            
            Context.OnEnter?.Invoke(GetTargetContext());
        }
    }
}