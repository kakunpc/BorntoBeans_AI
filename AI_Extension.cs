using UnityEngine;

namespace GGJ.AI
{
    public static class AI_Extension
    {
        // 入力の8方向
        private static readonly Vector3[] EntryDirections = new Vector3[]
        {
            Vector3.forward,
            Vector3.right,
            -Vector3.forward,
            -Vector3.right,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward - Vector3.right).normalized,
            (-Vector3.forward + Vector3.right).normalized,
            (-Vector3.forward - Vector3.right).normalized
        };

        /// <summary>
        /// 進行方向を８方向へ変更
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector3 CalcDoc(Vector3 val)
        {

            var index = 0;
            var dec = Vector3.Dot(EntryDirections[0], val);
            for (var i = 1; i < EntryDirections.Length; ++i)
            {
                var cal = Vector3.Dot(EntryDirections[i], val);
                if (!(Mathf.Abs(1 - cal) < Mathf.Abs(1 - dec))) continue;
                index = i;
                dec = cal;
            }
            return EntryDirections[index];
        }

        /// <summary>
        /// ターゲとの位置から少しずれた位置を返す
        /// </summary>
        /// <param name="terget"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Vector3 RandomPosition(Vector3 terget, float power = 3f)
        {
            return terget +
                   new Vector3(Random.Range(-power, power), Random.Range(-power, power), Random.Range(-power, power));
        }
    }
}
