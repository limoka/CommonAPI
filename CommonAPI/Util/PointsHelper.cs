using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonAPI
{
    public interface IPointsAssignable
    {
        void SetPoints(Vector3[] points);
    }

    public enum PointsType
    {
        Land,
        Water,
        Custom
    }
    
    /// <summary>
    /// Helper class to assign position data to classes such as <see cref="BuildConditionConfig"/> or that implement <see cref="IPointsAssignable"/>
    /// </summary>
    public class PointsHelper : MonoBehaviour
    {
        public Component target;
        public Transform searchPoint;
        public PointsType pointsType;

        public void Assign()
        {
            Transform[] transforms = searchPoint.Cast<Transform>().ToArray();
            Vector3[] points = transforms.Select(trs => trs.position).ToArray();
            if (target == null)
            {
                target = GetComponent<BuildConditionConfig>();
            }else if (pointsType == PointsType.Land || pointsType == PointsType.Water)
            {
                target = target.gameObject.GetComponent<BuildConditionConfig>();
            }

            if (target == null) return;
            
            if (target is IPointsAssignable trg && pointsType == PointsType.Custom)
            {
                trg.SetPoints(points);
                return;
            }

            if (target is BuildConditionConfig config)
            {
                if (pointsType == PointsType.Land)
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