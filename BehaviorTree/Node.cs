namespace GGJ.AI.BehaviorTree
{
    public abstract class Node
    {
        protected virtual string NodeName { get; set; }

        public virtual NodeStatusEnum NodeStatus { get; protected set; }

        public abstract NodeStatusEnum Action();

        /// <summary>
        /// 状態リセット
        /// </summary>
        public virtual void Reset()
        {
            NodeStatus = NodeStatusEnum.Ready;
        }

        /// <summary>
        /// 現在実行しているNode名を取得
        /// </summary>
        /// <returns>ノード名</returns>
        public virtual string GetRunNodeName()
        {
            return NodeName;
        }
    }
}
