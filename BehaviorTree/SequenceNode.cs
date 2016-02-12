using System.Collections.Generic;

namespace GGJ.AI.BehaviorTree
{
    /// <summary>
    /// リストを順番に実行していく、次のノードへの移動は子がSUCCESSを返したとき時
    /// 子供が1こでも失敗したら終了
    /// </summary>
    public sealed class SequenceNode : Node
    {
        private readonly List<Node> _nodeList;
        private int _runIndex;

        public SequenceNode(params Node[] nodes)
        {
            NodeName = "SequenceNode";
            _runIndex = 0;
            _nodeList = new List<Node>(nodes);
        }

        public SequenceNode(string name,params Node[] nodes)
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
                if (status == NodeStatusEnum.Failure)
                {
                    NodeStatus = NodeStatusEnum.Failure;
                    return NodeStatus;
                }
                NodeStatus = NodeStatusEnum.Running;
                return NodeStatus;
            }
            else if (node.NodeStatus == NodeStatusEnum.Running)
            {
                var state = node.Action();
                if (state == NodeStatusEnum.Failure)
                {
                    NodeStatus = NodeStatusEnum.Failure;
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
                    NodeStatus = NodeStatusEnum.Success;
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
            if (NodeStatus == NodeStatusEnum.Running &&
                _runIndex < _nodeList.Count)
                return _nodeList[_runIndex].GetRunNodeName();
            return base.GetRunNodeName();
        }
    }
}
