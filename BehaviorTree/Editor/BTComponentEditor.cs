using UnityEditor;
using UnityEngine;

namespace GGJ.AI.BehaviorTree
{
    [CustomEditor(typeof(BehaviorTreeComponent))]
    internal class BTComponentEditor : Editor
    {
        private BehaviorTreeComponent Target
        {
            get { return EditorApplication.isPlaying ? (BehaviorTreeComponent) target : null; }
        }

        public override void OnInspectorGUI()
        {

            EditorGUI.BeginChangeCheck();

            if (Target != null &&
                Target.RootNode != null)
            {
                switch (Target.RootNode.NodeStatus)
                {
                    case NodeStatusEnum.Ready:
                        GUILayout.BeginVertical(GetBoxColor(Color.cyan));
                        break;

                    case NodeStatusEnum.Running:
                        GUILayout.BeginVertical(GetBoxColor(Color.yellow));
                        break;

                    case NodeStatusEnum.Success:
                        GUILayout.BeginVertical(GetBoxColor(Color.green));
                        break;

                    case NodeStatusEnum.Failure:
                        GUILayout.BeginVertical(GetBoxColor(Color.red));
                        break;
                }

                GUILayout.Label(string.Format("Status:{0} {1}", Target.RootNode.NodeStatus,
                    Target.RootNode.GetRunNodeName()));

                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box");

                GUILayout.Label("Status:None");

                GUILayout.EndVertical();
            }

            if (Target != null)
                EditorUtility.SetDirty(Target);
        }

        private GUIStyle GetBoxColor(Color col)
        {
            var pix = new Color[2 * 2];
            for (var i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            var tex = new Texture2D(2, 2);
            tex.SetPixels(pix);
            tex.Apply();

            var result = new GUIStyle(GUI.skin.box) { normal = { background = tex } };

            return result;
        }
    }
}
