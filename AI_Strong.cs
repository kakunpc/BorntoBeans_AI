using GGJ.AI.BehaviorTree;
using GGJ.GameManager;
using GGJ.Player;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GGJ.AI
{
    public class AI_Strong : MonoBehaviour, IPlayerInput
    {
        private const float LevelMaxGettingUpToTime = 30f; // 最大のレベルになるまでの時間
        private const float AttackDistance = 3f;  // 攻撃開始までの距離
        private const float StopDistance = 1.5f;  // 止まるまでの距離
        private const float GetItemDistance = 10f;  // アイテムを取り行く距離


        private Subject<bool> onAttackButtonSubject = new Subject<bool>();

        /// <summary>
        /// 攻撃ボタンが押されているかどうか
        /// </summary>
        public IObservable<bool> OnAttackButtonObservable
        {
            get { return onAttackButtonSubject.AsObservable(); }
        }

        private Subject<Vector3> moveDirectionSubject = new Subject<Vector3>();

        /// <summary>
        /// プレイヤの移動方向
        /// </summary>
        public ReadOnlyReactiveProperty<Vector3> MoveDirection
        {
            get { return moveDirectionSubject.ToReadOnlyReactiveProperty(); }
        }

        // Use this for initialization
        private void Start()
        {
            var waitPositonTime = 0f;
            GameObject attackPlayer = null;
            var isMoveTerget = false;
            var moveTerget = Vector3.zero;
            GameObject itemObject = null;
            var coolTime = 0f;

            // 行き先に向かって移動する
            this.UpdateAsObservable()
                .Where(_ => isMoveTerget)
                .Select(_ =>
                {
                    var path = new NavMeshPath();
                    isMoveTerget = NavMesh.CalculatePath(transform.position, moveTerget, NavMesh.AllAreas, path);
                    return path;
                })
                .Where(x => x.corners.Length >= 2)
                .Do(_ => waitPositonTime = 0f) // 行き先を見つけた
                .Select(x => (x.corners[1] - x.corners[0]).normalized)
                .Select(x => AI_Extension.CalcDoc(x))
                .Subscribe(moveDirectionSubject);

            // 止まっている時間のカウント
            this.UpdateAsObservable()
                .Where(_ => isMoveTerget == false)
                .Subscribe(_ => { waitPositonTime += Time.deltaTime; });

            // 止まっている時間が一定数立ってしまったらとりあえずターゲットに向かって歩く
            this.ObserveEveryValueChanged(_ => waitPositonTime)
                .Where(_ => isMoveTerget == false)
                .Where(x => x > 1f)
                .Where(_ => (Vector3.Distance(moveTerget, transform.position) > StopDistance * (1f - LevelMaxGettingUpToTimeRate)))  // 近くにすでにいるなら動かない
                .Select(_ => (moveTerget - transform.position).normalized)
                .Select(x => AI_Extension.CalcDoc(x))
                .Subscribe(moveDirectionSubject);

            // 行きたい位置についたら移動処理を停止させる
            this.UpdateAsObservable()
                .Where(_ => isMoveTerget)
                .Select(_ => Mathf.Abs(Vector3.Distance(moveTerget, transform.position)))
                .Where(x => x < StopDistance * (1f - LevelMaxGettingUpToTimeRate))
                .Subscribe(_ => isMoveTerget = false);

            // 敵の近くによったら攻撃する
            this.UpdateAsObservable()
                .Where(_ => attackPlayer != null)
                .Where(_ => (Vector3.Distance(attackPlayer.transform.position, this.transform.position) <= AttackDistance))
                .Subscribe(_ =>
                {
                    onAttackButtonSubject.OnNext(true);
                    onAttackButtonSubject.OnNext(false);
                });

            // アイテム生成通知
            StageSpawner.Instance.OnSpowenItemAsObservable()
                .Where(x => Vector3.Distance(x.transform.position, this.transform.position) < GetItemDistance) // 自分の近くに生成された
                .Subscribe(x =>
                {
                    // アイテムが存在していることを入れる
                    itemObject = x;
                    // 交戦中のプレイヤーを消しておく（取りに行くため）
                    attackPlayer = null;
                });

            // アイテム存在チェック
            var checkNearItemState =
                new ActionNode("CheckItem", _ =>
                {
                    if (itemObject != null)
                        return NodeStatusEnum.Success;
                    return NodeStatusEnum.Failure;
                });

            // アイテムに近づくAI
            var goItemObject = new DecoratorNode("GoItemCheck",
                new ActionNode("GoItem", _ =>
                {
                    // 取れた取れてないにかかわらず終了
                    if (itemObject == null)
                    {
                        isMoveTerget = false;
                        return NodeStatusEnum.Success;
                    }
                    // ターゲットを設定
                    moveTerget = itemObject.transform.position;
                    moveTerget.y = transform.position.y;
                    isMoveTerget = true;
                    return NodeStatusEnum.Running;
                })
                , () => (itemObject != null));

            // 近くにアイテムがスポーンしたなら優先的に取り行くAI
            var checkItem = new SequenceNode("SearchItem",
                checkNearItemState // 生成してる
                , goItemObject // 取りに行く
                );

            // 近くのプレイヤーを選択するAI
            var nearPlayer = new ActionNode("SearchPlayer",
                _ =>
                {
                    // 生きているプレイヤー取得
                    var players = PlayerManager.Instance.GetAlivePlayers();
                    if (players.Count <= 1)
                    {
                        isMoveTerget = false;
                        return NodeStatusEnum.Failure;
                    }

                    // ソート
                    players.Sort((x, y) =>
                    {
                        var xDist = Vector3.Distance(x.transform.position, this.transform.position);
                        var yDist = Vector3.Distance(y.transform.position, this.transform.position);
                        if (xDist == yDist)
                            return 0;
                        if (xDist > yDist)
                            return 1;
                        else
                            return -1;
                    });
                    // 一番近いプレイヤーを選択
                    attackPlayer = players[1].gameObject;
                    moveTerget = AI_Extension.RandomPosition(attackPlayer.transform.position,
                        (1f - LevelMaxGettingUpToTimeRate)); // 時間経過で性格射撃になる
                    moveTerget.y = transform.position.y;
                    isMoveTerget = true;
                    // 思考を停止するクールタイム
                    coolTime = Random.Range(0.5f, 2f);
                    coolTime *= (1f - LevelMaxGettingUpToTimeRate);// 時間経過で思考停止クールタイムがなくなる
                    return NodeStatusEnum.Success;
                }
                );

            var aiCoolTime = new ActionNode("CoolTime", _ =>
            {
                coolTime -= Time.deltaTime;
                if (coolTime < 0f)
                {
                    return NodeStatusEnum.Failure;
                }
                return NodeStatusEnum.Running;
            });

            // AI初期化
            BehaviorTreeComponent.RegsterComponent(this.gameObject, new SelectorNode("name",
                checkItem,
                aiCoolTime,
                nearPlayer));
        }

        /// <summary>
        /// 時間経過によるレベルの割合
        /// </summary>
        /// <returns>0～1 1Max</returns>
        private float LevelMaxGettingUpToTimeRate
        {
            get
            {
                if (LevelMaxGettingUpToTime <= 0f) return 1f;
                return TimerManager.Instance.OnGameTimer.Value / LevelMaxGettingUpToTime;
            }
        }
    }
}
