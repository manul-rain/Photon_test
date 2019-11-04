using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    private Vector3 origin;
    private Vector3 velocity;
    private int timestamp;

    public int Id{get; private set; }
    public int OwnerId{ get; private set; }
    public bool Equals(int id, int ownerId) => id == Id && ownerId == OwnerId;
    public bool IsActive => gameObject.activeSelf;

    public void Activate(int id, int ownerId, Vector3 origin, float angle, int timestamp)
    {
        Id = id;
        OwnerId = ownerId;
        this.origin = origin;
        transform.position = origin;
        velocity = 9f * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
        this.timestamp = timestamp;

        OnUpdate();
        gameObject.SetActive(true);
    }

    public void OnUpdate()
    {
        float elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
        
        transform.position = origin + velocity * elapsedTime;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnBecameInvisible()
    {
        Deactivate();
    }
}
