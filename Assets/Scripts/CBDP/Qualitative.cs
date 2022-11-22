using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.CBDP
{
    public class Qualitative
    {
        public readonly Distance distance;
        public readonly Direction direction;
        public readonly double? angle;
        public readonly double? numericDistance;
        public readonly bool isNull = false;
        public readonly char splitter = '-';

        Dictionary<string, float> DicDistance = new Dictionary<string, float>()
        {
            {"VC-VC", 0f},
            {"VC-C", 0.125f},
            {"VC-A", 0.25f},
            {"VC-F", 0.375f},
            {"VC-VF", 0.5f},
            {"C-VC", 0.125f},
            {"C-C", 0.0f},
            {"C-A", 0.125f},
            {"C-F", 0.25f},
            {"C-VF", 0.375f},
            {"A-VC", 0.25f},
            {"A-C", 0.125f},
            {"A-A", 0.0f},
            {"A-F", 0.125f},
            {"A-VF", 0.25f},
            {"F-VC", 0.375f},
            {"F-C", 0.25f},
            {"F-A", 0.125f},
            {"F-F", 0.0f},
            {"F-VF", 0.125f},
            {"VF-VC", 0.5f},
            {"VF-C", 0.375f},
            {"VF-A", 0.25f},
            {"VF-F", 0.125f},
            {"VF-VF", 0f}
        };

        Dictionary<string, Vector2> DicPoint = new Dictionary<string, Vector2>() {
            {"VC-F", new Vector2(0,1)},
            {"VC-RF", new Vector2(1,1)},
            {"VC-LF", new Vector2(-1,1)},
            {"VC-R", new Vector2(1,0)},
            {"VC-L", new Vector2(-1,0)},
            {"VC-B", new Vector2(0,-1)},
            {"VC-RB", new Vector2(1,-1)},
            {"VC-LB", new Vector2(-1,-1)},
            {"C-F", new Vector2(0,2)},
            {"C-RF", new Vector2(2,2)},
            {"C-LF", new Vector2(-2,2)},
            {"C-R", new Vector2(2,0)},
            {"C-L", new Vector2(-2,0)},
            {"C-B", new Vector2(0,-2)},
            {"C-RB", new Vector2(2,-2)},
            {"C-LB", new Vector2(-2,-2)},
            {"A-F", new Vector2(0,3)},
            {"A-RF", new Vector2(3,3)},
            {"A-LF", new Vector2(-3,3)},
            {"A-R", new Vector2(3,0)},
            {"A-L", new Vector2(-3,0)},
            {"A-B", new Vector2(0,-3)},
            {"A-RB", new Vector2(3,-3)},
            {"A-LB", new Vector2(-3,-3)},
            {"F-F", new Vector2(0,4)},
            {"F-RF", new Vector2(4,4)},
            {"F-LF", new Vector2(-4,4)},
            {"F-R", new Vector2(4,0)},
            {"F-L", new Vector2(-4,0)},
            {"F-B", new Vector2(0,-4)},
            {"F-RB", new Vector2(4,-4)},
            {"F-LB", new Vector2(-4,-4)},
            {"VF-F", new Vector2(0,5)},
            {"VF-RF", new Vector2(5,5)},
            {"VF-LF", new Vector2(-5,5)},
            {"VF-R", new Vector2(5,0)},
            {"VF-L", new Vector2(-5,0)},
            {"VF-B", new Vector2(0,-5)},
            {"VF-RB", new Vector2(5,-5)},
            {"VF-LB", new Vector2(-5,-5)}
        };

        Dictionary<string, float> DicDirection = new Dictionary<string, float>()
        {
            {"F-F", 0},
            {"F-RF", 0.125f},
            {"F-LF", 0.125f},
            {"F-R", 0.25f},
            {"F-L", 0.25f},
            {"F-B", 0.5f},
            {"F-RB", 0.375f},
            {"F-LB", 0.375f},
            {"RF-F", 0.125f},
            {"RF-RF", 0},
            {"RF-LF", 0.25f},
            {"RF-R", 0.125f},
            {"RF-L", 0.375f},
            {"RF-B", 0.375f},
            {"RF-RB", 0.25f},
            {"RF-LB", 0.5f},
            {"LF-F", 0.125f},
            {"LF-RF", 0.25f},
            {"LF-LF", 0},
            {"LF-R", 0.375f},
            {"LF-L", 0.125f},
            {"LF-B", 0.375f},
            {"LF-RB", 0.5f},
            {"LF-LB", 0.25f},
            {"R-F", 0.25f},
            {"R-RF", 0.125f},
            {"R-LF", 0.375f},
            {"R-R", 0},
            {"R-L", 0.5f},
            {"R-B", 0.25f},
            {"R-RB", 0.125f},
            {"R-LB", 0.375f},
            {"L-F", 0.25f},
            {"L-RF", 0.375f},
            {"L-LF", 0.125f},
            {"L-R", 0.5f},
            {"L-L", 0},
            {"L-B", 0.25f},
            {"L-RB", 0.375f},
            {"L-LB", 0.125f},
            {"B-F", 0.5f},
            {"B-RF", 0.375f},
            {"B-LF", 0.375f},
            {"B-R", 0.25f},
            {"B-L", 0.25f},
            {"B-B", 0},
            {"B-RB", 0.125f},
            {"B-LB", 0.125f},
            {"RB-F", 0.375f},
            {"RB-RF", 0.25f},
            {"RB-LF", 0.5f},
            {"RB-R", 0.125f},
            {"RB-L", 0.375f},
            {"RB-B", 0.125f},
            {"RB-RB", 0},
            {"RB-LB", 0.25f},
            {"LB-F", 0.375f},
            {"LB-RF",0.5f},
            {"LB-LF", 0.25f},
            {"LB-R", 0.375f},
            {"LB-L", 0.125f},
            {"LB-B", 0.125f},
            {"LB-RB", 0.25f},
            {"LB-LB", 0}
        };

        public Qualitative()
        {
            isNull = true;
        }

        public Qualitative(Distance distance, Direction direction)
        {
            this.distance = distance;
            this.direction = direction;
        }

        public Qualitative(double angle, double numericDistance)
        {
            this.angle = angle;
            this.numericDistance = numericDistance;
        }

        public Qualitative(string str)
        {
            var aux = str.Split(splitter);

            if(Double.TryParse(aux[0].Replace(".", ","), out double a))
            {
                this.angle = a;
                this.numericDistance = Double.Parse(aux[1].Replace(".", ","));
            }
            else
            {
                Enum.TryParse(aux[0], out this.distance);
                Enum.TryParse(aux[1], out this.direction);
                this.angle = null;
                this.numericDistance = null;
            }
        }

       

        public override string ToString()
        {
            if (isNull)
                return "";

            return distance.ToString() + splitter + direction.ToString();
        }

        public float GetValue(string a, string b)
        {
            DicDirection.TryGetValue(a, out float v1);
            DicDistance.TryGetValue(b, out float v2);

            return v1 + v2;
        }

        public Vector2 GetPoint(string a, string b)
        {
            DicPoint.TryGetValue(a + splitter + b, out Vector2 p);
            return p;
        }

        public Vector2 GetPoint()
        {
            DicPoint.TryGetValue(this.ToString(), out Vector2 p);
            return p;
        }

        public string ToString(bool num)
        {
            if (isNull)
                return "";
            if (num)
            {
                return angle.ToString().Replace(",", ".") + splitter + numericDistance.ToString().Replace(",", ".");
            }
                
            else
                return ToString();
        }
    }
}
