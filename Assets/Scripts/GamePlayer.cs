using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// MonoBehaviourPunCallbacksを継承すると、photonViewプロパティが使えるようになる
[RequireComponent(typeof(SpriteRenderer))]
public class GamePlayer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshPro nameLabel = default;

    private ProjectileManager projectileManager;
    private SpriteRenderer spriteRenderer;
    private int projectileId = 0;

    private void Awake()
    {
        projectileManager = GameObject.FindWithTag("ProjectileManager").GetComponent<ProjectileManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        PhotonNetwork.LocalPlayer.NickName ="Player";
    }

    private void Start()
    {

        var customProperties = photonView.Owner.CustomProperties;
        bool hasScore = customProperties.TryGetValue("Score", out object scoreObject);
        int score = (hasScore)? (int)scoreObject : 0;
        nameLabel.text = $"{photonView.Owner.NickName}({score.ToString()})";

        if(customProperties.TryGetValue("Hue", out object hueObject))
        {
            spriteRenderer.color = Color.HSVToRGB((float)hueObject, 1f, 1f);
        }
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
        
        if (photonView.IsMine)
        {
            var hashtable = new Hashtable();
            hashtable["Hue"] = Random.value;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        } else if (ownerId == PhotonNetwork.LocalPlayer.ActorNumber){
            var customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
            bool hasScore = customProperties.TryGetValue("Score", out object scoreObject);
            int score = (hasScore)? (int)scoreObject : 0;

            var hashtable = new Hashtable();
            hashtable["Score"] = score + 100;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (target.ActorNumber != photonView.OwnerActorNr){ return; }

        if(changedProps.TryGetValue("Score", out object scoreObject))
        {
            int score = (int)scoreObject;
            nameLabel.text = $"{photonView.Owner.NickName}({score.ToString()})";
        }

        if (changedProps.TryGetValue("Hue", out object hueObject))
        {
            spriteRenderer.color = Color.HSVToRGB((float)hueObject, 1f,1f);
        }
        
    }
}