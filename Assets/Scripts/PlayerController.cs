using UnityEngine;

namespace BrickBreaker.Assets.Scripts
{
    public class PlayerController: MonoBehaviour
    {
        [SerializeField]
        private float limitX = 7.5f;

        void Update()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float clampedX = Mathf.Clamp(mouseWorldPos.x, -limitX, limitX);

            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }
    }
}