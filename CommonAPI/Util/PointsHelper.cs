using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonAPI
{
    public interface IPointsAssignable
    {
        void SetPoints(Vector3[] points);
    }
    
    public class PointsHelper : MonoBehaviour
    {
        public Component target;
        public Transform searchPoint;
        public bool landPoints;

        public void Assign()
        {
            Transform[] transforms = searchPoint.Cast<Transform>().ToArray();
            Vector3[] points = transforms.Select(trs => trs.position).ToArray();
            if (target == null)
            {
                target = GetComponent<BuildConditionConfig>();
            }

            if (target == null) return;
            
            if (target is IPointsAssignable trg)
            {
                trg.SetPoints(points);
                return;
            }

            if (target is BuildConditionConfig config)
            {
                if (landPoints)
                {
                    config.landPoints = points;
                }
                else
                {
                    config.waterPoints = points;
                }
            }
        }
    }
}