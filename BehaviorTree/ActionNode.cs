using System;

namespace GGJ.AI.BehaviorTree
{
    /// <summary>
    /// とにかく処理を実行するだけ
    /// </summary>
    public sealed class ActionNode : Node
    {
        private readonly Func<NodeStatusEnum, NodeStatusEnum> _actionFunc;

        public ActionNode(Func<NodeStatusEnum, NodeStatusEnum> actionFunc)
        {
            NodeName = "ActionNode";
            _actionFunc = actionFunc;
        }

        public ActionNode(string name ,Func<NodeStatusEnum, NodeStatusEnum> actionFunc)
        {
            NodeName = name;
            _actionFunc = actionFunc;
        }

        public override NodeStatusEnum Action()
        {
            NodeStatus = _actionFunc(NodeStatus);
            return NodeStatus;
        }
    }
}
