using UnityEngine;

public class RopeVisual : MonoBehaviour
{
    private const float PLAYER_OFFSET = 5f;
    [SerializeField] private float visualOffsetX = 0f;

    void Update()
    {
        if (GameManager.Instance == null) return;

        float ropeX = GameManager.Instance.ropePosition.Value;

        transform.position = new Vector3(ropeX + visualOffsetX, transform.position.y, transform.position.z);
        transform.localScale = new Vector3(PLAYER_OFFSET * 2f, transform.localScale.y, transform.localScale.z);
        transform.rotation = Quaternion.identity;
    }
}