using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Transformation
{
    public class TransformationTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _transformationPrefab;
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