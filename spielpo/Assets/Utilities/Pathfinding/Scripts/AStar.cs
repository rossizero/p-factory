using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utilities
{
    namespace Pathfinding
    {
        public class AStar<TNodeType> where TNodeType : IAStarNode<TNodeType>
        {

            // Function defining the used heuristic.
            private readonly Func<TNodeType, TNodeType, float> heuristic;

            public AStar(Func<TNodeType, TNodeType, float> heuristic)
            {
                this.heuristic = heuristic;
            }

            public List<TNodeType> calculatePath(TNodeType startNode, TNodeType endNode)
            {
                // List of all unchecked nodes
                IList<TNodeType> openNodes = new List<TNodeType>();
                openNodes.Add(startNode);

                // List of all nodes discovered and checked
                IList<TNodeType> closedNodes = new List<TNodeType>();

                // This will later determine the path by backtracking through the dict.
                IDictionary<TNodeType, TNodeType> parents = new Dictionary<TNodeType, TNodeType>();
                parents[startNode] = default(TNodeType);

                // Costs to every node already discovered.
                IDictionary<TNodeType, float> actualCost = new Dictionary<TNodeType, float>();
                actualCost[startNode] = 0;

                // Heuristic estimation of cost to target node.
                IDictionary<TNodeType, float> estimatedTotalCost = new Dictionary<TNodeType, float>();
                estimatedTotalCost[startNode] = heuristic(startNode, endNode);

                while (openNodes.Count != 0)
                {
                    // Get open node with smallest heuristically estimated cost
                    TNodeType minNode = openNodes[0];
                    foreach (TNodeType node in openNodes)
                    {
                        if (estimatedTotalCost[node] < estimatedTotalCost[minNode])
                            minNode = node;
                    }

                    // Add that node to the already discovered nodes.
                    openNodes.Remove(minNode);
                    closedNodes.Add(minNode);

                    // We are finished if that node is the our target node, we can now backtrack.
                    if (minNode.Equals(endNode))
                    {
                        return BackTrackPath(parents, endNode);
                    }

                    foreach (TNodeType neighbour in minNode.neighbours)
                    {
                        if (!(closedNodes.Contains(neighbour)))
                        {
                            // Calculate actual cost to every neighbor that hasn't already been closed.
                            float actualNeighborCost = actualCost[minNode] + minNode.weights[neighbour];

                            // If we either haven't discovered that neighbor as a node yet, or the newly calculated actual cost is lower, that what we found before, replace it.
                            if (!(openNodes.Contains(neighbour)) || actualNeighborCost < actualCost[neighbour])
                            {
                                actualCost[neighbour] = actualNeighborCost;
                                estimatedTotalCost[neighbour] = actualCost[neighbour] + heuristic(neighbour, endNode);
                                parents[neighbour] = minNode;
                                openNodes.Add(neighbour);
                            }
                        }
                    }
                }
                return null;
            }

            // Helper function for backtracing the path through a dictionary of node parents.
            private List<TNodeType> BackTrackPath(IDictionary<TNodeType, TNodeType> parents, TNodeType endNode)
            {
                TNodeType parentNode = parents[endNode];
                Stack<TNodeType> pathStack = new Stack<TNodeType>();

                // Traversing the path from the back and pushing each node to a stack.
                while (!EqualityComparer<TNodeType>.Default.Equals(parentNode, default(TNodeType)))
                {
                    pathStack.Push(parentNode);
                    parentNode = parents[parentNode];
                }

                //Convert the stack into a list. Now the path is in the correct order, not backwards.
                List<TNodeType> path = new List<TNodeType>(pathStack.ToArray());
                return path;
            }

        }
    }
}