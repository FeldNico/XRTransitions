using System;
using System.Threading.Tasks;
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
        private TransitionManager _transitionManager;
        private Context _targetContext;
        
        public abstract Task Initialization();
        internal abstract Task  OnTriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation);
        public abstract Context GetStartContext();
        public virtual Context GetTargetContext()
        {
            if (_targetContext == null)
            {
                _targetContext = Destination.GetComponentInParent<Context>();
            }

            return _targetContext;
        }
        
        public async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (_transitionManager == null)
            {
                _transitionManager = Object.FindObjectOfType<TransitionManager>();
            }

            if (_transitionManager.CurrentContext != GetStartContext())
            {
                return;
            }
            
            Context.OnExit?.Invoke(GetStartContext());

            await OnTriggerTransition(traveller, targetPosition, targetRotation);

            Physics.SyncTransforms();
            
            Context.OnEnter?.Invoke(GetTargetContext());
        }
    }
}