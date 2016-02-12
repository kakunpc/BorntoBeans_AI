using System.Collections.Generic;

namespace GGJ.AI.BehaviorTree
{
    /// <summary>
    /// リストを順番に実行していく、次のノードへの移動は子がSUCCESS以外を返したとき時
    /// 子供が1こでも完了したら終了
    /// </summary>
    public sealed class SelectorNode : Node
    {
        private readonly List<Node> _nodeList;
        private int _runIndex;
        
        public SelectorNode(params Node[] nodes)
        {
            NodeName = "SelectorNode";
            _runIndex = 0;
            _nodeList = new List<Node>(nodes);
        }

        public SelectorNode(string name, params Node[] nodes)
        {
            NodeName = name;
            _runIndex = 0;
            _nodeList = new List<Node>(nodes);
        }

        public override NodeStatusEnum Action()
        {
            if (NodeStatus == NodeStatusEnum.Success ||
                NodeStatus == NodeStatusEnum.Failure)
                return NodeStatus;

            var node = _nodeList[_runIndex];

            if (node.NodeStatus == NodeStatusEnum.Ready)
            {
                var status = node.Action();
                if (status == NodeStatusEnum.Success)
                {
                    NodeStatus = NodeStatusEnum.Success;
                    return NodeStatus;
                }
                NodeStatus = NodeStatusEnum.Running;
                return NodeStatus;
            }
            else if (node.NodeStatus == NodeStatusEnum.Running)
            {
                var state = node.Action();
                if (state == NodeStatusEnum.Success)
                {
                    NodeStatus = NodeStatusEnum.Success;
                    return NodeStatus;
                }
                else
                {
                    NodeStatus = NodeStatusEnum.Running;
                    return NodeStatus;
                }
            }
            else
            {
                ++_runIndex;
                if (_runIndex >= _nodeList.Count)
                {
                    NodeStatus = NodeStatusEnum.Failure;
                    return NodeStatus;
                }
                NodeStatus = NodeStatusEnum.Running;
                return Action();
            }

        }

        public override void Reset()
        {
            base.Reset();
            _runIndex = 0;
            foreach (var node in _nodeList)
            {
                node.Reset();
            }
        }

        public override string GetRunNodeName()
        {
            if ((NodeStatus == NodeStatusEnum.Running ||
                 NodeStatus == NodeStatusEnum.Success) &&
                _runIndex < _nodeList.Count)
                return _nodeList[_runIndex].GetRunNodeName();
            return base.GetRunNodeName();
        }
    }
}
