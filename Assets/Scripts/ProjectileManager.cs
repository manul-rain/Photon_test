using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField]
    private Projectile projectilePrefab = default;

    private List<Projectile> activeList = new List<Projectile>();
    private Stack<Projectile> inactivePool = new Stack<Projectile>();

    private void Update()
    {
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            var projectile = activeList[i];
            if (projectile.IsActive)
            {
                projectile.OnUpdate();
            }else{
                Remove(projectile);
            }
        }
    }

    public void Fire(int id, int ownerId, Vector3 origin, float angle)
    {
        var projectile = (inactivePool.Count > 0)
            ? inactivePool.Pop()
            : Instantiate(projectilePrefab, transform);
        projectile.Activate(id, ownerId, origin, angle);
        activeList.Add(projectile);
    }

    public void Remove(Projectile projectile)
    {
        activeList.Remove(projectile);
        projectile.Deactivate();
        inactivePool.Push(projectile);
    }

    public void Remove(int id, int ownerId)
    {
        foreach (var projectile in activeList)
        {
            if(projectile.Equals(id, ownerId))
            {
                Remove(projectile);
                break;
            }
        }
    }
}
