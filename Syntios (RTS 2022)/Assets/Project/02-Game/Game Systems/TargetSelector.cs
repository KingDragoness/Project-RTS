using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Entities;

namespace ProtoRTS.Game
{
	public class TargetSelector : MonoBehaviour
	{
        [System.Serializable]
        public class QuadNode
        {
            public int x, y;
            public List<GameUnit> units = new List<GameUnit>();

            public QuadNode(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }


        [Range(0,30)] public int ChecksPerSecond = 2; //if 10 checks, check every 3 ticks
        public int MaximumAgentPerCheck = 100;
        public int quadLength = 8;

        public float max_x, max_y;
        public List<QuadNode> list_Quads = new List<QuadNode>();
        public QuadNode[,] array2d_Quads = new QuadNode[0, 0];

        private float _cooldownCheck = 0.4f;
        private int _indexCheck = 0;

        public static TargetSelector instance;

        private int length_X
        {
            get { return (int)max_x / quadLength; }
        }
        private int length_Y
        {
            get { return (int)max_y / quadLength; }
        }
        private int MaxLimitIndexCheck
        {
            get
            {
                return Mathf.CeilToInt((float)SyntiosEngine.Instance.ListedGameUnits.Count / (float)MaximumAgentPerCheck);
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            SyntiosEvents.Game_ReloadMap += RefreshMap;
        }
        private void OnDisable()
        {
            SyntiosEvents.Game_ReloadMap -= RefreshMap;
        }

        private void RefreshMap()
        {
            max_x = Map.TerrainData.size_x;
            max_y = Map.TerrainData.size_y;

            int totalX = (int)max_x / quadLength;
            int totalY = (int)max_y / quadLength;

            array2d_Quads = new QuadNode[totalX, totalY];

            for (int x = 0; x < totalX; x++)
            {
                for (int y = 0; y < totalY; y++)
                {
                    QuadNode quad = new QuadNode(x, y);
                    list_Quads.Add(quad);
                    array2d_Quads[x, y] = quad;
                }
            }

            _cooldownCheck = 1f / (float)ChecksPerSecond;
        }

        private void Update()
        {
            _cooldownCheck -= Time.deltaTime;

            if (_cooldownCheck < 0f)
            {
                QuadGrouping();
                AssignEveryUnit_ClosestTarget();
                _cooldownCheck = 1f / (float)ChecksPerSecond;
            }

            if (MaxLimitIndexCheck <= _indexCheck)
            {
                _indexCheck = 0;
            }
        }

        public QuadNode GetQuadNode(Transform unit)
        {
            int x = Mathf.FloorToInt(unit.transform.position.x / quadLength);
            int y = Mathf.FloorToInt(unit.transform.position.z / quadLength);

            if (x >= length_X) return null;
            if (y >= length_Y) return null;
            if (x < 0) return null;
            if (y < 0) return null;

            return array2d_Quads[x, y];
        }

        public QuadNode GetQuadNode(int posx, int posy)
        {
            if (posx >= length_X) return null;
            if (posy >= length_Y) return null;
            if (posx < 0) return null;
            if (posy < 0) return null;

            return array2d_Quads[posx, posy];
        }


        private void QuadGrouping()
        {
            foreach (var quad in list_Quads)
            {
                quad.units.Clear();
            }

            foreach (var unit in SyntiosEngine.Instance.ListedGameUnits)
            {
                if (unit == null) continue;
                int x = Mathf.FloorToInt(unit.transform.position.x / quadLength);
                int y = Mathf.FloorToInt(unit.transform.position.z / quadLength);

                var quad = GetQuadNode(x, y);

                if (quad == null) continue;

                quad.units.Add(unit);
            }
        }

        /// <summary>
        /// It always check by WeaponHandler.CurrentWeapon's criteria
        /// 
        /// </summary>
        private void AssignEveryUnit_ClosestTarget()
        {
            //Every distance check, every unit will be grouped into regions (either region03, region11, region63, etc...)
            List<QuadNode> allQuadNeighborsCheck = new List<QuadNode>();
            int limit = (_indexCheck + 1) * MaximumAgentPerCheck;
            if (limit > SyntiosEngine.Instance.ListedGameUnits.Count) limit = SyntiosEngine.Instance.ListedGameUnits.Count;

            for (int currIndex = _indexCheck * MaximumAgentPerCheck; currIndex < limit; currIndex++)
            {
                var unit = SyntiosEngine.Instance.ListedGameUnits[currIndex];
                if (unit == null) continue;
                if (unit.weaponHandler == null)
                {
                    if (SyntiosEngine.Instance.engine_PrintErrorMissingBehavior)
                    {
                        Debug.LogError("Missing weaponHandler");
                        continue;
                    }
                }
                
                allQuadNeighborsCheck.Clear();
                float closest = float.MaxValue;
                GameUnit closestUnit = null;

                int unitPos_x = Mathf.FloorToInt(unit.transform.position.x / quadLength);
                int unitPos_y = Mathf.FloorToInt(unit.transform.position.z / quadLength);


                QuadNode currentQuadNode = GetQuadNode(unit.transform);

                //MAXIMUM closest check is within box of 8x2 = 16 units
                int lengthQuad = 2;

                int n = lengthQuad + lengthQuad;

                int quadToScan = (n + 3) * (n + 3);
                int b = lengthQuad + 1;

                //Debug.Log(quadToScan);
                for (int x = -b; x <= b; x++)
                {
                    for (int y = -b; y <= b; y++)
                    {
                        int pos_x = unitPos_x + x;
                        int pos_y = unitPos_y + y;

                        QuadNode newQuad = GetQuadNode(pos_x, pos_y);

                        if (newQuad != null)
                        {
                            if (allQuadNeighborsCheck.Contains(newQuad) == false) allQuadNeighborsCheck.Add(newQuad);
                        }
                    }
                }

                //Debug.Log($"{allQuadNeighborsCheck.Count} |  Pos: ({unitPos_x},{unitPos_y})");

                for (int q = 0; q < allQuadNeighborsCheck.Count; q++)
                {
                    var quad = allQuadNeighborsCheck[q];

                    foreach (GameUnit unitToCompare in quad.units)
                    {
                        if (unit == unitToCompare) { continue; }
                        SO_Weapon usingWeapon = null;

                        if (unit.IsAir())
                        {
                            if (unit.weaponHandler.AirWeapon() == null) continue;
                            usingWeapon = unit.weaponHandler.AirWeapon();
                        }
                        else
                        {
                            if (unit.weaponHandler.GroundWeapon() == null) continue;
                            usingWeapon = unit.weaponHandler.GroundWeapon();
                        }

                        float dist = Vector3.Distance(unitToCompare.transform.position, unit.transform.position);

                        if (dist <= closest)
                        {
                            closest = dist;
                            //begin checks

                            //if (targetCriteria.Contains())
                            

                            closestUnit = unitToCompare;
                        }
                    }
                }

                //if (closestUnit != null) unit.closestUnit = closestUnit.transform;
                unit.closest_attackableUnit = closestUnit;
            }

            _indexCheck++;
        }
    }
}