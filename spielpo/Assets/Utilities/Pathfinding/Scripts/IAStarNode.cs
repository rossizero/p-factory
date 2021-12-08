using System.Collections.Generic;

namespace Utilities
{
    namespace Pathfinding
    {
        public interface IAStarNode<TNodeType> where TNodeType : IAStarNode<TNodeType>
        {
            ISet<TNodeType> neighbours { get; }
            IDictionary<TNodeType, float> weights { get; }
        }
    }
}
