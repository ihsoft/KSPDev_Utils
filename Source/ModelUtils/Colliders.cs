// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using System;
using System.Linq;
using UnityEngine;

namespace KSPDev.ModelUtils {

/// <summary>Various tools to deal with procedural colliders.</summary>
public static class Colliders {
  /// <summary>Defines how collisions should be checked on a primitive.</summary>
  public enum PrimitiveCollider {
    /// <summary>No collisions check.</summary>
    None,
    /// <summary>Check collisions basing on the mesh. It's performance expensive.</summary>
    /// <seealso href="https://docs.unity3d.com/ScriptReference/MeshCollider.html">
    /// Unity3D: MeshCollider</seealso>
    Mesh,
    /// <summary>Simple collider which fits the primitive type. It's performance optimized.</summary>
    /// <seealso href="https://docs.unity3d.com/ScriptReference/PrimitiveType.html">
    /// Unity3D: PrimitiveType</seealso>
    Shape,
    /// <summary>Simple collider which wraps all mesh vertexes. It's performance optimized.</summary>
    Bounds,
  }

  /// <summary>
  /// Drops the colliders in all the children objects, and adds one big collider to the parent.
  /// </summary>
  /// <remarks>
  /// The main purpose of this method is to create one fast collider at the cost of precision. All
  /// the meshes in the object (the parent and the children) are processed to produce a single
  /// boundary box. Then, this box is applied to the requested primitive type that defines the shape
  /// of the final collider.
  /// <para>
  /// Note, that radius of the sphere and the capsule colliders is the same on both X and Y axis.
  /// If the combined boundary box has any of the dimensions significantly different then it makes
  /// sense to choose a different collider type. Or break down the hierarchy into more colliders.
  /// </para>
  /// </remarks>
  /// <param name="parent">Parent object.</param>
  /// <param name="type">
  /// Type of the primitive which describes the parent object most precise in terms of the shape.
  /// Only <see cref="PrimitiveType.Cube"/>, <see cref="PrimitiveType.Sphere"/>,
  /// <see cref="PrimitiveType.Cylinder"/>, and <see cref="PrimitiveType.Capsule"/> are supported.
  /// The two latter types produce in the same collider type - the capsule.
  /// </param>
  /// <param name="inscribeBoundaryIntoCollider">
  /// When calculating the total volume of the object, all its meshes produce a single box boundary.
  /// Then, the collider either wraps this box entirely, or sits inside it entirely. If this
  /// parameter is <c>true</c> then the collider will cover the boundary box.
  /// </param>
  /// <returns>The collider created or <c>null</c>.</returns>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/GameObject.html">
  /// Unity 3D: GameObject</seealso>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/PrimitiveType.html">
  /// Unity 3D: PrimitiveType</seealso>
  public static Collider SetSimpleCollider(GameObject parent, PrimitiveType type,
                                           bool inscribeBoundaryIntoCollider = true) {
    parent.GetComponentsInChildren<Collider>().ToList()
        .ForEach(SafeDestroy);

    // Get bounds of all renderers in the parent. The bounds come in world's coordinates, so
    // translate them into parent's local space before encapsulating. 
    var renderers = parent.GetComponentsInChildren<Renderer>();
    var combinedBounds = default(Bounds);
    foreach (var renderer in renderers) {
      var bounds = renderer.bounds;
      bounds.center = parent.transform.InverseTransformPoint(bounds.center);
      bounds.size = parent.transform.rotation.Inverse() * bounds.size;
      combinedBounds.Encapsulate(bounds);
    }

    // Add collider basing on the requested type.
    if (type == PrimitiveType.Cube) {
      var collider = GetColliderOfType<BoxCollider>(parent);
      collider.center = combinedBounds.center;
      collider.size = combinedBounds.size;
      return collider;
    }
    if (type == PrimitiveType.Capsule || type == PrimitiveType.Cylinder) {
      // TODO(ihsoft): Choose direction so what the volume is minimized.
      var collider = GetColliderOfType<CapsuleCollider>(parent);
      collider.center = combinedBounds.center;
      collider.direction = 2;  // Z axis
      collider.height = combinedBounds.size.z;
      collider.radius = inscribeBoundaryIntoCollider
          ? Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.y)
          : Mathf.Min(combinedBounds.extents.x, combinedBounds.extents.y);
      return collider;
    }
    if (type == PrimitiveType.Sphere) {
      var collider = GetColliderOfType<SphereCollider>(parent);
      collider.center = combinedBounds.center;
      collider.radius = inscribeBoundaryIntoCollider
          ? Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.y)
          : Mathf.Min(combinedBounds.extents.x, combinedBounds.extents.y);
      return collider;
    }
    DebugEx.Error("Unsupported collider: {0}. Ignoring", type);
    return null;
  }

  /// <summary>Sets the specified values to colliders of all the objects in the part's model.
  /// </summary>
  /// <param name="parent">Game object to start searching for renderers from.</param>
  /// <param name="isPhysical">
  /// If <c>true</c> then collider will trigger physical effects. If <c>false</c> then it will only
  /// trigger collision events. When it's <c>null</c> the collider setting won't be changed.
  /// </param>
  /// <param name="isEnabled">
  /// Defines if colliders should be enabled or disabled. When it's <c>null</c> the collider setting
  /// won't be changed.
  /// </param>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Collider.html">Unity3D: Collider
  /// </seealso>
  public static void UpdateColliders(GameObject parent, bool? isPhysical = null, bool? isEnabled = null) {
    foreach (var collider in parent.GetComponentsInChildren<Collider>()) {
      if (isPhysical.HasValue) {
        collider.isTrigger = !isPhysical.Value;
      }
      if (isEnabled.HasValue) {
        collider.enabled = isEnabled.Value;
      }
    }
  }

  /// <summary>Adds or adjusts a primitive collider on the mesh.</summary>
  /// <remarks>
  /// Type of the primitive collider is chosen basing on the primitive type.
  /// </remarks>
  /// <param name="primitive">The primitive game object to adjust.</param>
  /// <param name="meshSize">
  /// The size of the collider in local units. Depending on <paramref name="shapeType"/> the meaning
  /// of the components is different. If the shape has a "round" component, then it's a "diameter"
  /// in this vector.
  /// </param>
  /// <param name="colliderType">Determines how a collider type should be selected.</param>
  /// <param name="shapeType">
  /// The type of the primitive when <paramref name="colliderType"/> is
  /// <see cref="PrimitiveCollider.Shape"/>. It will determine the type of the collider. Only
  /// <see cref="PrimitiveType.Cylinder"/>, <see cref="PrimitiveType.Sphere"/>, and
  /// <see cref="PrimitiveType.Cube"/> are supported.
  /// </param>
  /// FIXME: it's not working with asymmetric meshes
  /// FIXME: use GameObject.GetRendererBounds?
  public static Collider AdjustCollider(
      GameObject primitive, Vector3 meshSize, PrimitiveCollider colliderType,
      PrimitiveType? shapeType = null) {
    if (colliderType == PrimitiveCollider.None) {
      SafeDestroy(GetActiveCollider<Collider>(primitive));
      return null;
    }
    if (colliderType == PrimitiveCollider.Mesh) {
      var collider = GetColliderOfType<MeshCollider>(primitive);
      collider.convex = true;
      return collider;
    }
    if (colliderType == PrimitiveCollider.Shape) {
      switch (shapeType) {
        // FIXME: non trivial scales doesn't fit simple colliders. Fix it.
        case null:
          SafeDestroy(GetActiveCollider<Collider>(primitive));
          DebugEx.Warning("Primitive type not set. Dropping collider.");
          return null;
        case PrimitiveType.Cylinder: {
          // TODO(ihsoft): Choose direction so what the volume is minimized.
          var collider = GetColliderOfType<CapsuleCollider>(primitive);
          collider.direction = 2;  // Z axis
          collider.height = meshSize.z;  // It's now length.
          collider.radius = meshSize.x / 2.0f;
          return collider;
        }
        case PrimitiveType.Sphere: {
          var collider = GetColliderOfType<SphereCollider>(primitive);
          collider.radius = meshSize.x / 2.0f;
          return collider;
        }
        case PrimitiveType.Cube: {
          var collider = GetColliderOfType<BoxCollider>(primitive);
          collider.size = meshSize;
          return collider;
        }
        default:
          SafeDestroy(GetActiveCollider<Collider>(primitive));
          DebugEx.Warning("Unknown primitive type {0}. Dropping collider.", shapeType.Value);
          return null;
      }
    }
    if (colliderType == PrimitiveCollider.Bounds) {
      return SetSimpleCollider(primitive, PrimitiveType.Cube, inscribeBoundaryIntoCollider: true);
    }
    SafeDestroy(GetActiveCollider<Collider>(primitive));
    DebugEx.Warning(
        "Unsupported collider type {0}. Dropping whatever collider part had", colliderType);
    return null;
  }

  /// <summary>Returns a collider of the requested type or creates one if needed.</summary>
  /// <remarks>
  /// This method returns the exact collider component if it exist in the <paramref name="obj"/>. However, if the object
  /// doesn't have a collider, then a new one will be created. If the game object has a collider but its type is
  /// incompatible, it will be deleted and a new compatible collider will be created. Note that the old collider, if
  /// present, will be removed using the <see cref="Colliders.SafeDestroy"/> method.
  /// </remarks>
  /// <param name="obj"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns>The found or created collider.</returns>
  public static T GetColliderOfType<T>(GameObject obj) where T: Collider {
    var currentCollider = GetActiveCollider<Collider>(obj);
    var collider = currentCollider as T;
    if (collider == null) {
      SafeDestroy(currentCollider);
      collider = obj.AddComponent<T>();
    }
    return collider;
  }

  /// <summary>Disables/enables all the colliders between the objects.</summary>
  /// <remarks>
  /// All colliders in all the children of the both objects are explicitly adjusted. The ignore
  /// state is reset to <c>false</c> on every scene load.
  /// </remarks>
  /// <param name="obj1">Source object.</param>
  /// <param name="obj2">Target object.</param>
  /// <param name="ignore">
  /// If <c>true</c> then the collisions between the objects will be ignored. Otherwise, the
  /// collisions will be handled.
  /// </param>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Collider.html">
  /// Unity3D: Collider</seealso>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html">
  /// Unity3D: Physics.IgnoreCollision</seealso>
  public static void SetCollisionIgnores(Transform obj1, Transform obj2, bool ignore) {
    foreach (var collider1 in obj1.GetComponentsInChildren<Collider>()) {
      foreach (var collider2 in obj2.GetComponentsInChildren<Collider>()) {
        Physics.IgnoreCollision(collider1, collider2, ignore);
      }
    }
  }

  /// <summary>Disables/enables all the colliders between the parts.</summary>
  /// <remarks>The ignore state is reset to <c>false</c> on every scene load.</remarks>
  /// <param name="part1">Source part.</param>
  /// <param name="part2">Target part.</param>
  /// <param name="ignore">
  /// If <c>true</c> then the collisions between the parts will be ignored. Otherwise, the
  /// collisions will be handled.
  /// </param>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Collider.html">
  /// Unity3D: Collider</seealso>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Physics.IgnoreCollision.html">
  /// Unity3D: Physics.IgnoreCollision</seealso>
  public static void SetCollisionIgnores(Part part1, Part part2, bool ignore) {
    DebugEx.Fine("Set collision ignores between {0} and {1} to {2}", part1, part2, ignore);
    SetCollisionIgnores(
        Hierarchy.GetPartModelTransform(part1), Hierarchy.GetPartModelTransform(part2), ignore);
  }

  /// <summary>Disables/enables all the collidres between the part and a vessel.</summary>
  /// <param name="part">The part to adjust colliders for.</param>
  /// <param name="vessel">The vessel to start/stop colliding with.</param>
  /// <param name="ignore">The desired state of the collision check.</param>
  public static void SetCollisionIgnores(Part part, Vessel vessel, bool ignore) {
    DebugEx.Fine("Set collision ignores between {0} and {1} to {2}", part, vessel, ignore);
    var modelRoot = Hierarchy.GetPartModelTransform(part);
    vessel.parts.ForEach(
        p => SetCollisionIgnores(modelRoot, Hierarchy.GetPartModelTransform(p), ignore));
  }

  /// <summary>
  /// Returns the minimum square distance to the nearest point on the part's collider surface.
  /// </summary>
  /// <remarks>This method skips triggers and inactive colliders.</remarks>
  /// <param name="point">The reference point to find distance for.</param>
  /// <param name="part">The part to check for.</param>
  /// <param name="filterFn">
  /// The filter function to apply to every collider. Return <c>false</c> from it to sklip the
  /// collider in the following checks.
  /// </param>
  /// <returns>The square distance or <c>null</c> if no colliders found.</returns>
  public static float? GetSqrDistanceToPart(
      Vector3 point, Part part, Func<Collider, bool> filterFn = null) {
    float? minDistance = null;
    var colliders = part.transform.GetComponentsInChildren<Collider>()
        .Where(c => !c.isTrigger && c.enabled && c.gameObject.activeInHierarchy
               && (filterFn == null || filterFn(c)));
    foreach (var collider in colliders) {
      Vector3 closetsPoint;
      if (collider is WheelCollider) {
        // Wheel colliders don't support closets point check.
        closetsPoint = collider.ClosestPointOnBounds(point);
      } else {
        closetsPoint = collider.ClosestPoint(point);
        if (closetsPoint == collider.transform.position) {
          // The point it inside the collider or on the boundary.
          return 0.0f;
        }
      }
      var sqrMagnitude = (closetsPoint - point).sqrMagnitude;
      minDistance = Mathf.Min(minDistance ?? float.PositiveInfinity, sqrMagnitude);
    }
    return minDistance;
  }

  /// <summary>
  /// Returns the minimum square distance to the nearest point on the part's collider surface.
  /// </summary>
  /// <remarks>This method skips triggers and inactive colliders.</remarks>
  /// <param name="point">The reference point to find distance for.</param>
  /// <param name="part">The part to check for.</param>
  /// <param name="defaultValue">The value to return if no suitable colliders found.</param>
  /// <param name="filterFn">
  /// The filter function to apply to every collider. Return <c>false</c> from it to skip the
  /// collider in the following checks.
  /// </param>
  /// <returns>
  /// The square distance or <paramref name="defaultValue"/> if no colliders found.
  /// </returns>
  public static float GetSqrDistanceToPartOrDefault(
      Vector3 point, Part part,
      float defaultValue = float.PositiveInfinity, Func<Collider, bool> filterFn = null) {
    return GetSqrDistanceToPart(point, part) ?? defaultValue;
  }

  /// <summary>Destroys the collider in a way which is safe for physical callback methods.</summary>
  /// <remarks>
  /// Uses <c>DestroyImmediate</c> to drop the collider when possible. If called during the <c>FixedUpdate</c> cycle,
  /// the <c>Destroy</c> is used, and the collider gets explicitly disabled. In the latter case, the collider won't
  /// actually be deleted after the call (it will on the next frame update), but it won't be executing any behavior and
  /// another collider of the same type could be be added. The caller must always assume that the collider is not
  /// immediately removed from the <c>GameObject</c>. Thus, the <c>Collider.enabled</c> property must be checked when
  /// fetching the active collider.
  /// </remarks>
  /// <param name="obj">The object to destroy. Can be <c>null</c>.</param>
  public static void SafeDestroy(Collider obj) {
    if (obj != null) {
      if (Time.inFixedTimeStep) {
        UnityEngine.Object.Destroy(obj);
        obj.enabled = false;
      } else {
        UnityEngine.Object.DestroyImmediate(obj);
      }
    }
  }

  /// <summary>Returns the first active collider of the requested type.</summary>
  /// <summary>
  /// Use this method in conjunction with <see cref="SafeDestroy"/> to properly handle colliders operations within one
  /// frame.
  /// </summary>
  /// <param name="obj">The object to get a collider from.</param>
  /// <typeparam name="T">The type of the collider to request.</typeparam>
  /// <returns>The collider or <c>null</c> if no active colliders of that type were found.</returns>
  public static T GetActiveCollider<T>(GameObject obj) where T : Collider {
    return obj.GetComponents<T>().FirstOrDefault(c => c.enabled);
  }
}

}  // namespace
