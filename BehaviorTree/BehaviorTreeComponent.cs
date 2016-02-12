#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Diagnostics;
using UnityEngine;

namespace GGJ.AI.BehaviorTree
{
    public class BehaviorTreeComponent : MonoBehaviour
    {
        private Node _rootNode;
        private bool _autoReset;

        public  Node RootNode { get { return _rootNode; } }

        public void Initialize(Node rootNode, bool autoReset)
        {
            _rootNode = rootNode;
            _autoReset = autoReset;
        }

        void Start()
        {
            if (_rootNode != null)
                _rootNode.Reset();
        }


        void Update()
        {
            if(_rootNode == null)
                return;

            if (_rootNode.NodeStatus != NodeStatusEnum.Ready &&
                _rootNode.NodeStatus != NodeStatusEnum.Running &&
                _autoReset)
                Reset();
            if (_rootNode.NodeStatus == NodeStatusEnum.Success ||
                _rootNode.NodeStatus == NodeStatusEnum.Failure)
                return;
            _rootNode.Action();
        }

        /// <summary>
        /// 処理をリセットする
        /// </summary>
        public void Reset()
        {
            if (_rootNode == null) return;

            _rootNode.Reset();

        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="gameObject">設定したいゲームオブジェクト</param>
        /// <param name="rootNode">親ノード</param>
        /// <param name="autoReset">自動的にリセットして再開するか</param>
        /// <returns></returns>
        public static BehaviorTreeComponent RegsterComponent(GameObject gameObject, Node rootNode, bool autoReset = true)
        {
           var behaviorTreeComponent =  gameObject.AddComponent<BehaviorTreeComponent>();

            behaviorTreeComponent.Initialize(rootNode,autoReset);

            return behaviorTreeComponent;
        }

        [Conditional("DEBUG")]
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if(EditorApplication.isPlaying && 
                _rootNode != null)
            UnityEditor.Handles.Label(transform.position, _rootNode.GetRunNodeName());
#endif
        }
    }
}
