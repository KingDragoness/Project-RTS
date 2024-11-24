using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS
{

    /// <summary>
    /// Is a modular script that requires "BaseRTSEngine" prefab
    /// </summary>
	public class RTSCamera : MonoBehaviour
	{

        public Camera MainCamera;
        public Transform bait_noX;
        public float ySpeed = 25f;
        public LayerMask layer_Terrain;
        public Vector3 Offset;
        [Space]
        public float speedPan = 10f;
        public float speedZoom = 10f;
        public float rotateSpeed = 20f;
        public float screenDistX = 48f;
        public float screenDistY = 32f;

        [ReadOnly] [SerializeField] private bool isDisablePanning = false;
        private Vector3 deltaOriginMouse = new Vector3();
        private Vector3 lastMouseMiddleClick = new Vector3();
        private Vector3 lastMouseMiddle_delta = new Vector3();

        private Transform originalParent;

        private static RTSCamera instance;

        public Vector3 Center
        {
            get
            {
                return new Vector3(Map.MapSize.x / 2f, 0f, Map.MapSize.y / 2f);
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            originalParent = transform.parent;
            //transform.position = Center;
        }

        void OnDrawGizmosSelected()
        {

        }

        private void Update()
        {

            if (Input.GetKeyUp(KeyCode.Z)) isDisablePanning = !isDisablePanning;
            if (Input.GetMouseButton(2)) isDisablePanning = true;

            if (isDisablePanning == false)
            {
                ScreenPan();

            }

            Scrollwheel();
            ClampPosition();

            if (Input.GetMouseButtonUp(2))
            {
                isDisablePanning = false;
            }

        }

        private void GetCameraPositionY()
        {

        }

        private void Scrollwheel()
        {
            var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            var deltaZoom = transform.forward * mouseScroll * speedZoom;


        }

        public static void SetPositionByMinimap(Vector3 position)
        {
            instance.transform.position = position;
        }

        private void ScreenPan()
        {
            if (MainUI.GetEventSystemRaycastResults().Count > 0) return; //cancelled if hit any UI

            Vector3 mousePos = Input.mousePosition;
            Vector3 delta = new Vector3();

            if (mousePos.x < screenDistX)
            {
                delta -= bait_noX.transform.right;
            }
            else if (mousePos.x > (Screen.width - screenDistX))
            {
                delta += bait_noX.transform.right;
            }

            if (mousePos.y < screenDistY)
            {
                delta -= bait_noX.transform.forward;

            }
            else if (mousePos.y > (Screen.height - screenDistY))
            {
                delta += bait_noX.transform.forward;

            }

            {
                RaycastHit hit;
                float y_position = 0f;

                Vector3 posOrigin = MainCamera.gameObject.transform.position + Offset;

                if (Physics.Raycast(posOrigin, Vector3.down, out hit, 1000f, layer_Terrain))
                {
                    y_position = hit.point.y;
                }

                Vector3 pos = transform.position;
                pos.y = y_position / 2f;

                //if (pos.y >= 8) pos.y = 8f;

                delta.y = gameObject.transform.position.y - pos.y;
                delta.y = -delta.y * ySpeed * 0.0025f;
            }


            transform.position += delta * speedPan * Time.deltaTime;
        }

        private void ClampPosition()
        {
            Vector3 pos = transform.position;

            if (pos.x > Map.MapSize.x * 2f)
            {
                pos.x = Map.MapSize.x * 2f;
            }

            if (pos.x < 0)
            {
                pos.x = 0;
            }

            if (pos.z > Map.MapSize.y * 2f)
            {
                pos.z = Map.MapSize.y * 2f;
            }

            if (pos.z < -10)
            {
                pos.z = -10;
            }

            transform.position = pos;

        }

    }
}
