using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProtoRTS.MapEditor
{
	public class MapToolBrush : MonoBehaviour
	{

        public enum Shape
        {
            Square,
            Circle,
            Diagonal
        }

        public float targetBrushSize = 2;
        public Shape targetShape;

        [Header("References")]
        public Material mat_projectorSquare;
        public Material mat_projectorCircle;
        public Transform brushProjector;
        public Transform DEBUG_originBrush;
        public Projector projector;

        private Vector3 brushPosition = new Vector3();

        public Vector3 BrushPosition { get => brushPosition; set => brushPosition = value; }


        public void EnableBrush()
        {
            if (gameObject.activeSelf == false) gameObject.SetActive(true);
        }

        public void DisableBrush()
        {
            if (gameObject.activeSelf == true) gameObject.SetActive(false);
        }

        private void Update()
        {
            projector.orthographicSize = targetBrushSize / 2f;
            Update_BrushProjector();
        }


        private void Update_BrushProjector()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Vector3 pos = hit.point;
                pos.y = 50f;

                brushPosition = hit.point;
                brushProjector.transform.position = pos;
            }

            if (targetShape == Shape.Circle)
            {
                projector.material = mat_projectorCircle;
            }
            else if (targetShape == Shape.Square)
            {
                projector.material = mat_projectorSquare;
            }
        }

    }
}