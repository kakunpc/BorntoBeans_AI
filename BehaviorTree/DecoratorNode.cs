using System;

namespace GGJ.AI.BehaviorTree
{
    /// <summary>
    /// 評価を行い、trueなら個のノードを実行する、その時はSucces
    /// 実行を行えない場合はFailureを返却する
    /// </summary>
    public sealed class DecoratorNode : Node
    {
        private readonly Node _actionNode;
        private readonly Func<bool> _whereFunc;

        public DecoratorNode(Node node, Func<bool> whereAction )
        {
            NodeName = "DecoratorNode";
            _actionNode = node;
            _whereFunc = whereAction;
        }
        public DecoratorNode(string name,Node node, Func<bool> whereAction)
        {
            NodeName = name;
            _actionNode = node;
            _whereFunc = whereAction;
        }

        public override NodeStatusEnum Action()
        {
            if (NodeStatus == NodeStatusEnum.Ready)
            {
                if (_whereFunc())
                {
                    Reset();
                    NodeStatus = _actionNode.Action();
                    return NodeStatus;
                }
                else
                {
                    NodeStatus = NodeStatusEnum.Failure;
                    return NodeStatus;
                }
            }

            if (NodeStatus == NodeStatusEnum.Running)
            {
                NodeStatus = _actionNode.Action();
                return NodeStatus;
            }

            return NodeStatus;
        }

        public override void Reset()
        {
            base.Reset();
            _actionNode.Reset();
        }

        public override string GetRunNodeName()
        {
            if (_actionNode.NodeStatus == NodeStatusEnum.Running ||
                _actionNode.NodeStatus == NodeStatusEnum.Success)
                return _actionNode.GetRunNodeName();
            return base.GetRunNodeName();
        }
    }
}
