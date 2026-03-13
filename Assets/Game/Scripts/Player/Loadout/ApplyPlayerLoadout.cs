using UnityEngine;

/// <summary>
/// Reads PlayerLoadout singleton at spawn and applies skills + weapon to this player.
/// Attach to the Player prefab alongside PlayerController + TeleportAbility.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class ApplyPlayerLoadout : MonoBehaviour
{
    public TeleportAbility        teleportAbility;
    public PlayerWeaponController weaponController;
    public Transform              weaponHolder;

    private void Start()
    {
        if (PlayerLoadout.Instance == null) return;
        var ld = PlayerLoadout.Instance;

        // ── skills ────────────────────────────────────────────────────
        GetComponent<PlayerController>().jumpSkillLevel =
            Mathf.Clamp(ld.jumpSkillPoints, 0, 2);

        if (teleportAbility != null)
            teleportAbility.teleportSkillLevel = ld.teleportSkillPoints;

        // ── weapon ───────────────────────────────────────────────────
        if (weaponController != null && weaponHolder != null)
        {
            if (ld.primaryWeaponPrefab != null)
            {
                foreach (Transform child in weaponHolder)
                    Destroy(child.gameObject);

                var weaponGO = Instantiate(ld.primaryWeaponPrefab, weaponHolder);
                weaponGO.transform.localPosition = Vector3.zero;
                weaponGO.transform.localRotation = Quaternion.identity;

                var weapon = weaponGO.GetComponent<Weapon>();
                if (weapon != null)
                    weaponController.EquipWeapon(weapon);
            }
        }

        // Note: grenade spawning is handled by GrenadeController (reads loadout directly).
    }
}
