using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace ProtoRTS
{
    public class CircleIndexCounter : MonoBehaviour
    {



        public List<Texture2D> allCircles = new List<Texture2D>();
        public List<CirclePixels> DataCircles1 = new List<CirclePixels>();

        [Button("Calculate Indexes")]
        public void CalculateCircleIndexes()
        {
            foreach (var circle in allCircles)
            {
                CirclePixels cpDat = new CirclePixels();
                string str = "[";
                List<Vector2Int> coordOff = new List<Vector2Int>();

                var pixels = circle.GetPixels32(0);
                int _index = 0;
                foreach (var pixel in pixels)
                {
                    if (pixel.r > 245 && pixel.g > 245)
                    {
                        int x = _index % circle.width;
                        int y = _index / circle.height;
                        coordOff.Add(new Vector2Int(x, y));
                    }

                    _index++;
                }

                foreach (var index1 in coordOff)
                {
                    str += $"{index1}, ";
                }

                str += "]";
                //Debug.Log($"{circle.name}: {str}");
                cpDat.ID = circle.name;
                cpDat.LineOfSight = circle.width / 2;
                cpDat.coordToDraw = coordOff;

                DataCircles1.Add(cpDat);
            }
        }

    }
}