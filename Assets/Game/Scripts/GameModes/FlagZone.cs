using UnityEngine;

public class FlagZone : MonoBehaviour
{
    public string team;
    public GameObject flagVisual;
    
    private bool isOccupied;
    private GameObject occupyingPlayer;
    
    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health != null && health.Team == team)
        {
            isOccupied = true;
            occupyingPlayer = other.gameObject;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == occupyingPlayer)
        {
            isOccupied = false;
            occupyingPlayer = null;
        }
    }
    
    public bool IsOccupied() { return isOccupied; }
    public GameObject GetOccupyingPlayer() { return occupyingPlayer; }
}