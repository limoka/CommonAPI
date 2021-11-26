using UnityEngine;

namespace CommonAPI.ShotScene
{
    public class TPCameraController : MonoBehaviour
    {
        public Transform lookAt;
        public Camera camera;

        private float currentX = 70;
        private float currentY = 40;

        public float yAngleMin = 10f;
        public float yAngleMax = 85f;
        public float cameraDistance = 100f;
        public float minCameraDistance = 1f;
        public float scrollSensitivity = 20f;
        public float sensitivity = 3;
        public Vector3 cameraOffset = new Vector3(0,2,0);
        
        private float currentDistance;


        private void Start()
        {
            currentY = Mathf.Clamp(currentY, yAngleMin, yAngleMax);
            currentDistance = 10;
            camera = GetComponent<Camera>();

        }

        public void RecalculatePosition()
        {
            if (lookAt == null) return;
            
            Vector3 dir = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            var position = lookAt.position + cameraOffset;

            transform.position = position + rotation * dir;

            transform.LookAt(position);
        }

        private void Update()
        {
            if (GeneratorSceneController.pointerInside) return;
            
            float mouseScroll = -Input.mouseScrollDelta.y * scrollSensitivity;
            currentDistance += mouseScroll * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minCameraDistance, cameraDistance);

            if (!Input.GetMouseButton(0)) return;
            
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY += -Input.GetAxis("Mouse Y") * sensitivity;

            currentY = Mathf.Clamp(currentY, yAngleMin, yAngleMax);
        }

        private void LateUpdate()
        {
            RecalculatePosition();
        }
    }
}