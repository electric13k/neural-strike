using UnityEngine;

/// <summary>
/// Runs at player spawn. Reads PlayerLoadout singleton and applies
/// skill levels to PlayerController, TeleportAbility and weapon.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class ApplyPlayerLoadout : MonoBehaviour
{
    public TeleportAbility        teleportAbility;
    public PlayerWeaponController weaponController;

    private void Start()
    {
        var loadout = PlayerLoadout.Instance;
        if (loadout == null) return;

        // Jump level
        GetComponent<PlayerController>().jumpSkillLevel =
            Mathf.Clamp(loadout.jumpSkillPoints, 0, 2);

        // Teleport level
        if (teleportAbility != null)
            teleportAbility.teleportSkillLevel = loadout.teleportSkillPoints;

        // Weapon
        if (weaponController != null && loadout.primaryWeaponPrefab != null)
        {
            Transform holder = weaponController.weaponHolder;
            if (holder != null)
            {
                foreach (Transform child in holder)
                    Destroy(child.gameObject);

                var go = Instantiate(loadout.primaryWeaponPrefab, holder);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                var w = go.GetComponent<Weapon>();
                if (w != null) weaponController.EquipWeapon(w);
            }
        }
    }
}
