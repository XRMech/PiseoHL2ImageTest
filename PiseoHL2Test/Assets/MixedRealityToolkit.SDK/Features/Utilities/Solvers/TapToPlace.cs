﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Microsoft.MixedReality.Toolkit.Utilities.Editor.Solvers
{
    /// <summary>
    /// Tap to place is a far interaction component used to place objects on a surface.
    /// </summary>
    public class TapToPlace : Solver, IMixedRealityPointerHandler
    {
        [Space(10)]
        [SerializeField]
        [Tooltip("If true, the game object to place will start selected.  The object will immediately start" +
                " following the TrackedTargetType (Head or Controller Ray) and then a tap is required to place the object." +
                " This value must be modified before Start() is invoked in order to have any effect")]
        private bool autoStart = false;

        /// <summary>
        /// If true, the game object to place will start out selected.  The object will immediately start
        /// following the TrackedTargetType (Head or Controller Ray) and then a tap is required to place the object.  
        /// This value must be modified before Start() is invoked in order to have any effect.
        /// </summary>
        public bool AutoStart
        {
            get => autoStart;
            set => autoStart = value;
        }

        [SerializeField]
        [Tooltip("The default distance (in meters) an object will be placed relative to the TrackedTargetType forward in the SolverHandler." +
                " The GameObjectToPlace will be placed at the default placement distance if a surface is not hit by the raycast.")]
        private float defaultPlacementDistance = 1.5f;

        /// <summary>
        /// The default distance (in meters) an object will be placed relative to the TrackedTargetType forward in the SolverHandler.
        /// The GameObjectToPlace will be placed at the default placement distance if a surface is not hit by the raycast.
        /// </summary>
        public float DefaultPlacementDistance
        {
            get => defaultPlacementDistance;
            set => defaultPlacementDistance = value;    
        }

        [SerializeField]
        [Tooltip("Max distance (in meters) to place an object if there is a raycast hit on a surface.")]
        private float maxRaycastDistance = 20.0f;

        /// <summary>
        /// The max distance (in meters) to place an object if there is a raycast hit on a surface
        /// </summary>
        public float MaxRaycastDistance
        {
            get => maxRaycastDistance;
            set => maxRaycastDistance = value;
        }

        /// <summary>
        /// If true, the game object to place is selected.
        /// </summary>
        public bool IsBeingPlaced { get; protected set; }

        [SerializeField]
        [Tooltip("The distance between the center of the game object to place and a surface along the surface normal, if the raycast hits a surface")]
        private float surfaceNormalOffset = 0.0f;

        /// <summary>
        /// The distance between the center of the game object to place and a surface along the surface normal, if the raycast hits a surface.
        /// </summary>
        public float SurfaceNormalOffset
        {
            get => surfaceNormalOffset;
            set => surfaceNormalOffset = value;
        }

        [SerializeField]
        [Tooltip("If true, the game object to place will remain upright and in line with Vector3.up")]
        private bool keepOrientationVertical = false;

        /// <summary>
        /// If true, the game object to place will remain upright and in line with Vector3.up
        /// </summary>
        public bool KeepOrientationVertical
        {
            get => keepOrientationVertical;
            set => keepOrientationVertical = value;
        }

        [SerializeField]
        [Tooltip("If false, the game object to place will not change its rotation according to the surface hit.  The object will" +
                " remain facing the camera while IsBeingPlaced is true.  If true, the object will rotate according to the surface normal" +
                " if there is a hit.")]
        private bool rotateAccordingToSurface = false;

        /// <summary>
        /// If false, the game object to place will not change its rotation according to the surface hit.  The object will
        /// remain facing the camera while IsBeingPlaced is true.  If true, the object will rotate according to the surface normal
        /// if there is a hit.
        /// </summary>
        public bool RotateAccordingToSurface
        {
            get => rotateAccordingToSurface;
            set => rotateAccordingToSurface = value;
        }

        [SerializeField]
        [Tooltip("Array of LayerMask to execute from highest to lowest priority. First layermask to provide a raycast hit will be used by component.")]
        private LayerMask[] magneticSurfaces = { UnityEngine.Physics.DefaultRaycastLayers };

        /// <summary>
        /// Array of LayerMask to execute from highest to lowest priority. First layermask to provide a raycast hit will be used by component.
        /// </summary>
        public LayerMask[] MagneticSurfaces
        {
            get => magneticSurfaces;
            set => magneticSurfaces = value;
        }

        [SerializeField]
        [Tooltip("If true and in the Unity Editor, the normal of the raycast hit will be drawn in yellow.")]
        private bool debugEnabled = true;

        /// <summary>
        /// If true and in the Unity Editor, the normal of the raycast hit will be drawn in yellow.
        /// </summary>
        public bool DebugEnabled
        {
            get => debugEnabled;
            set => debugEnabled = value;
        }

        [SerializeField]
        [Tooltip("This event is triggered once when the game object to place is selected.")]
        private UnityEvent onPlacingStarted = new UnityEvent();

        /// <summary>
        /// This event is triggered once when the game object to place is selected.
        /// </summary>
        public UnityEvent OnPlacingStarted
        {
            get => onPlacingStarted;
            set => onPlacingStarted = value;
        }

        [SerializeField]
        [Tooltip("This event is triggered once when the game object to place is unselected, placed.")]
        private UnityEvent onPlacingStopped = new UnityEvent();

        /// <summary>
        /// This event is triggered once when the game object to place is unselected, placed.
        /// </summary>
        public UnityEvent OnPlacingStopped
        {
            get => onPlacingStopped;
            set => onPlacingStopped = value;
        }

        /// <summary>
        /// The current game object layer before it is temporarily switched to IgnoreRaycast while placing the game object.
        /// </summary>
        protected internal int GameObjectLayer { get; protected set; }

        protected internal bool IsColliderPresent => gameObject != null ? gameObject.GetComponent<Collider>() != null : false;

        private int ignoreRaycastLayer;

        // The current ray is based on the TrackedTargetType (Controller Ray, Head, Hand Joint).
        // The following properties are updated each frame while the game object is selected to determine
        // object placement if there is a hit on a surface.
        protected RayStep CurrentRay;

        protected bool DidHitSurface;

        protected RaycastHit CurrentHit;

        // Used to record the time (seconds) between OnPointerClicked calls to avoid two calls in a row.
        protected float LastTimeClicked = 0;

        protected float DoubleClickTimeout = 0.5f;

        #region MonoBehaviour Implementation
        protected override void Start()
        {
            base.Start();

            Debug.Assert(IsColliderPresent, $"The game object {gameObject.name} does not have a collider attached, please attach a collider to use Tap to Place");

            SurfaceNormalOffset = gameObject.GetComponent<Collider>().bounds.extents.z;

            ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

            if (AutoStart)
            {
                StartPlacement();  
            }
            else
            {
                SolverHandler.UpdateSolvers = false;
            } 
        }

        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        }

        #endregion

        /// <summary>
        /// Start the placement of a game object without the need of the OnPointerClicked event. The game object will begin to follow the 
        /// TrackedTargetType (Head by default) at a default distance. StopPlacement() must be called after StartPlacement() to stop the 
        /// game object from following the TrackedTargetType.  The game object layer is changed to IgnoreRaycast temporarily and then
        /// restored to its original layer in StopPlacement().
        /// </summary>
        public void StartPlacement()
        {
            // Added for code configurability to avoid multiple calls to StartPlacement in a row
            if (!IsBeingPlaced)
            {
                // Store the initial game object layer
                GameObjectLayer = gameObject.layer;

                // Temporarily change the game object layer to IgnoreRaycastLayer to enable a surface hit beyond the game object
                //gameObject.layer = ignoreRaycastLayer;
				gameObject.SetLayerRecursively(ignoreRaycastLayer);

                SolverHandler.UpdateSolvers = true;

                IsBeingPlaced = true;

                OnPlacingStarted?.Invoke();

                // A global pointer handler is needed to enable object placement without the need for focus.
                // The object's layer is changed to IgnoreRaycast in this method, which means the game object cannot receive focus.
                // Without a global handler, the game object would not receive pointer events.
                CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
            }
        }

        /// <summary>
        /// Stop the placement of a game object without the need of the OnPointerClicked event. 
        /// </summary>
        public void StopPlacement()
        {
            // Added for code configurability to avoid multiple calls to StopPlacement in a row
            if (IsBeingPlaced)
            {
				// Change the game object layer back to the game object's layer on start
				//gameObject.layer = GameObjectLayer;
				gameObject.SetLayerRecursively(GameObjectLayer);

                SolverHandler.UpdateSolvers = false;

                IsBeingPlaced = false;

                OnPlacingStopped?.Invoke();

                CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
            }
        }

        /// <inheritdoc/>
        public override void SolverUpdate()
        {
			// Make sure the Transform target is not null, added for the case where auto start is true 
			// and the tracked target type is the controller ray, if the hand is not in the frame we cannot
			// calculate the position of the object
			//Debug.Log("place obj 1");
            if (SolverHandler.TransformTarget != null)
            {
				//Debug.Log("place obj 2");
				PerformRaycast();
                SetPosition();
                SetRotation();
            }
        }

        protected virtual void PerformRaycast()
        {
            // The transform target is the transform of the TrackedTargetType, i.e. Controller Ray, Head or Hand Joint
            var transform = SolverHandler.TransformTarget;

            Vector3 origin = transform.position;
            Vector3 endpoint = transform.position + transform.forward;
            CurrentRay.UpdateRayStep(ref origin, ref endpoint);

            // Check if the current ray hits a magnetic surface
            DidHitSurface = MixedRealityRaycaster.RaycastSimplePhysicsStep(CurrentRay, MaxRaycastDistance, MagneticSurfaces, false, out CurrentHit);  
        }

        /// <summary>
        /// Change the position of the game object if there was a hit, if not then place the object at the default distance
        /// relative to the TrackedTargetType origin position
        /// </summary>
        protected virtual void SetPosition()
        {
            if (DidHitSurface)
            {
                // Take the current hit point and add an offset relative to the surface to avoid half of the object in the surface
                GoalPosition = CurrentHit.point;  
                AddOffset(CurrentHit.normal * SurfaceNormalOffset);

#if UNITY_EDITOR
                if (DebugEnabled)
                {
                    // Draw the normal of the raycast hit for debugging 
                    Debug.DrawRay(CurrentHit.point, CurrentHit.normal * 0.5f, Color.yellow);
                }
#endif // UNITY_EDITOR
            }
            else
            {
				GoalPosition = SolverHandler.TransformTarget.position + (SolverHandler.TransformTarget.forward * DefaultPlacementDistance);
            }
        }

        protected virtual void SetRotation()
        {
            Vector3 direction = CurrentRay.Direction;
            Vector3 surfaceNormal = CurrentHit.normal;
            
            if (KeepOrientationVertical)
            {
                direction.y = 0;
                surfaceNormal.y = 0;
            }

            // If the object is on a surface then change the rotation according to the normal of the hit point
            if (DidHitSurface && rotateAccordingToSurface)
            {
                GoalRotation = Quaternion.LookRotation(-surfaceNormal, Vector3.up);
            }
            else 
            {
                GoalRotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        #region IMixedRealityPointerHandler

        /// <inheritdoc/>
        public void OnPointerDown(MixedRealityPointerEventData eventData) { }

        /// <inheritdoc/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

        /// <inheritdoc/>
        public void OnPointerUp(MixedRealityPointerEventData eventData) { }

        /// <inheritdoc/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Checking the amount of time passed between OnPointerClicked calls is handling the case when OnPointerClicked is called
            // twice after one click.  If OnPointerClicked is called twice after one click, the object will be selected and then immediately
            // unselected. If OnPointerClicked calls are within 0.5 secs of each other, then return to prevent an immediate object state switch.
            if ((Time.time - LastTimeClicked) < DoubleClickTimeout)
            {
                return;
            }

            if (!IsBeingPlaced)
            {
                StartPlacement();
            }
            else
            {
                StopPlacement();
            }

            // Get the time of this click action
            LastTimeClicked = Time.time;
        }

        #endregion
    }
}