using UnityEngine;

// ============================================================
//  PLAYER MODEL HOOK  — Neural Strike
//
//  Keeps a 3-D character mesh aligned with the Player capsule.
//  The mesh yaws with the player but does NOT pitch (avoiding
//  broken-neck syndrome when looking up/down).
//
//  HOW TO WIRE IN UNITY
//  1. Import your GLB/FBX into Assets/Art/Characters/.
//  2. Create empty "ModelRoot" as child of Player.
//  3. Drag the imported mesh under ModelRoot, reset local pos/rot.
//  4. Attach THIS script to ModelRoot.
//  5. Assign playerRoot = Player, cameraTransform = Camera child.
//  6. Set feetOffset so the model's feet sit at the capsule bottom
//     (typically -1.0 for a 2 m capsule).
// ============================================================

public class PlayerModelHook : MonoBehaviour
{
    [Header("References")]
    public Transform playerRoot;        // Player (has CharacterController)
    public Transform cameraTransform;   // Camera child

    [Header("Foot offset (negative = down)  usually -1")]
    public float feetOffset = -1f;

    void LateUpdate()
    {
        if (playerRoot == null) return;

        // Position: follow capsule centre, adjust feet
        transform.position = playerRoot.position + Vector3.up * feetOffset;

        // Rotation: yaw only — no pitch
        float yaw = playerRoot.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
