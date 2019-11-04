using Photon.Pun;
using UnityEngine;

// MonoBehaviourPunCallbacksを継承すると、photonViewプロパティが使えるようになる
public class GamePlayer : MonoBehaviourPunCallbacks
{
    private ProjectileManager projectileManager;
    private int projectileId = 0;

    private void Awake()
    {
        projectileManager = GameObject.FindWithTag("ProjectileManager").GetComponent<ProjectileManager>();
    }

    private void Update() {
        if (photonView.IsMine) {
            // 入力方向（ベクトル）を正規化する
            var direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            // 移動速度を時間に依存させて、移動量を求める
            var dv = 6f * Time.deltaTime * direction;
            transform.Translate(dv.x, dv.y, 0f);

            if (Input.GetMouseButtonDown(0))
            {
                var playerWorldPosition = transform.position;
                Vector3 Clickpos = Input.mousePosition;
                Clickpos.z = 100;
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Clickpos);
                
                var dp = mouseWorldPosition - playerWorldPosition;
                float angle = Mathf.Atan2(dp.y, dp.x);

                photonView.RPC(nameof(FireProjectile), RpcTarget.All, transform.position, angle);
            }
        }
    }

    [PunRPC]
    private void FireProjectile(Vector3 origin, float angle, PhotonMessageInfo info)
    {
        int timestamp = info.SentServerTimestamp;
        projectileManager.Fire(timestamp, photonView.OwnerActorNr, origin, angle, timestamp);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(photonView.IsMine)
        {
            var projectile = collision.GetComponent<Projectile>();
            if(projectile != null && projectile.OwnerId != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                photonView.RPC(nameof(HitByProjectile), RpcTarget.All, projectile.Id, projectile.OwnerId);
            }
        }
    }

    [PunRPC]
    private void HitByProjectile(int projectileId, int ownerId)
    {
        projectileManager.Remove(projectileId, ownerId);
        Debug.Log("Hit!!");
    }
}