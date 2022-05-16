using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Fade
{
    public class FadeTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private float _duration;
        
        public override Task Initialization()
        {
            throw new System.NotImplementedException();
        }

        internal override Task OnTriggerTransition()
        {
            throw new System.NotImplementedException();
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
    }
}