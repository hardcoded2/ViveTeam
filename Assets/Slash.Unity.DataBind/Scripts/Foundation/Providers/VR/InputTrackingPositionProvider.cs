namespace Slash.Unity.DataBind.Foundation.Providers.VR
{
    using System;
    using Slash.Unity.DataBind.Core.Presentation;

    using UnityEngine;
    using UnityEngine.VR;

    /// <summary>
    ///   Provides the world position of a specific vr node.
    ///   See https://forum.unity3d.com/threads/world-positions-of-left-right-eye.350076/
    /// </summary>
    [AddComponentMenu("Data Bind/Foundation/Providers/VR/[DB] Input Tracking Position Provider")]
    public class InputTrackingPositionProvider : DataProvider
    {
        /// <summary>
        ///   Node to get local position for.
        /// </summary>
        public VRNode VRNode;

        private Vector3 position;

        /// <inheritdoc />
        public override object Value
        {
            get
            {
                return this.GetPosition();
            }
        }

        /// <summary>
        ///   Unity callback.
        /// </summary>
        protected void Update()
        {
            var newPosition = this.GetPosition();
            if (this.position != newPosition)
            {
                this.OnValueChanged(newPosition);
                this.position = newPosition;
            }
        }

        /// <inheritdoc />
        protected override void UpdateValue()
        {
            this.OnValueChanged(this.Value);
        }

        private Vector3 GetPosition()
        {
#if UNITY_EDITOR
            switch (this.VRNode)
            {
                case VRNode.LeftEye:
                    return new Vector3(-0.0315f, 0);
                case VRNode.RightEye:
                    return new Vector3(0.0315f, 0);
                case VRNode.CenterEye:
                    return new Vector3(0, 0);
                case VRNode.Head:
                    return new Vector3(0, 0);
#if UNITY_5_5_OR_NEWER
                case VRNode.LeftHand:
                    return new Vector3(0, 0);
                case VRNode.RightHand:
                    return new Vector3(0, 0);
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }
#else
                    return Quaternion.Inverse(InputTracking.GetLocalRotation(this.VRNode)) * InputTracking.GetLocalPosition(this.VRNode);
#endif
        }
    }
}