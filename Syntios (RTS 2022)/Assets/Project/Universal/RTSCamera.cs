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
        public int minimumHeight = 20;
        public int maximumHeight = 100;
        public float ySpeed = 25f;
        public LayerMask layer_Terrain;
        public float mapBorder = 8f;
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
        float scrollWheel_targetY = 0f;
        float terrain_targetY = 0f;
        float original_yPos = 0;

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
            scrollWheel_targetY = MainCamera.transform.position.y;
            original_yPos = scrollWheel_targetY;
            //transform.position = Center;

            if (SyntiosEngine.CurrentMode == Gamemode.Game)
            {
                maximumHeight = 70;
            }
        }

        void OnDrawGizmosSelected()
        {

        }

        public static void RestoreHeight()
        {
            instance.scrollWheel_targetY = instance.original_yPos;
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
            UpdateFogColor();

            if (Input.GetMouseButtonUp(2))
            {
                isDisablePanning = false;
            }

        }

        private void UpdateFogColor()
        {
            //MainCamera.backgroundColor = RenderSettings.fogColor;
        }

        private void GetCameraPositionY()
        {

        }


        private void Scrollwheel()
        {
            var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            var deltaZoom = Vector3.up * mouseScroll * speedZoom;

            Vector3 result = MainCamera.transform.position + deltaZoom;
            if (result.y < minimumHeight)
            {
                result.y = minimumHeight;
            }
            if (result.y > maximumHeight)
            {
                result.y = maximumHeight;
            }

            scrollWheel_targetY += deltaZoom.y;
            if (scrollWheel_targetY < minimumHeight) scrollWheel_targetY = minimumHeight;
            if (scrollWheel_targetY > maximumHeight) scrollWheel_targetY = maximumHeight;

            Vector3 targeted = MainCamera.transform.position;
            targeted.y = scrollWheel_targetY + terrain_targetY;

            MainCamera.transform.position = Vector3.MoveTowards(MainCamera.transform.position, targeted, ySpeed * Time.deltaTime);
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
                pos.y = y_position;

                if (SyntiosEngine.CurrentMode == Gamemode.Game)
                {
                    if (pos.y >= 8) pos.y = 8f;
                }

                delta.y = gameObject.transform.position.y - pos.y;
                delta.y = -delta.y * ySpeed * 0.0025f;


                terrain_targetY += (pos.y - terrain_targetY) * Time.deltaTime * ySpeed;
            }


            transform.position += delta * speedPan * (MainCamera.transform.position.y / minimumHeight) * Time.deltaTime;
        }

        private void ClampPosition()
        {
            Vector3 pos = transform.position;

            if (pos.x > Map.MapSize.x * 2f - (mapBorder))
            {
                pos.x = Map.MapSize.x * 2f - (mapBorder);
            }

            if (pos.x < mapBorder)
            {
                pos.x = mapBorder;
            }

            if (pos.z > Map.MapSize.y * 2f - (mapBorder))
            {
                pos.z = Map.MapSize.y * 2f - (mapBorder);
            }

            if (pos.z < mapBorder - 15)
            {
                pos.z = mapBorder - 15;
            }

            transform.position = pos;

        }

    }
}
