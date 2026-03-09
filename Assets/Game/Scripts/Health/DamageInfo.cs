using UnityEngine;

public struct DamageInfo
{
    public Vector3 hitPoint;
    public Vector3 hitNormal;
    public Vector3 direction;
    public GameObject attacker;
    public GameObject weapon;
    public float time;

    public DamageInfo(
        Vector3 hitPoint,
        Vector3 hitNormal,
        Vector3 direction,
        GameObject attacker,
        GameObject weapon)
    {
        this.hitPoint = hitPoint;
        this.hitNormal = hitNormal;
        this.direction = direction;
        this.attacker = attacker;
        this.weapon = weapon;
        this.time = Time.time;
    }
}