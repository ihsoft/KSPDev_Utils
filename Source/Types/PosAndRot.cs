﻿// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using KSPDev.ConfigUtils;
using System;
using System.Linq;
using UnityEngine;

namespace KSPDev.Types {

/// <summary>Type to hold position and rotation of a transform. It can be serialized.</summary>
/// <remarks>
/// The value serializes into 6 numbers separated by a comma. They form two triplets:
/// <list type="bullet">
/// <item>The first triplet is a position: x, y, z.</item>
/// <item>
/// The second triplet is a Euler rotation around each axis: x, y, z.
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <code source="Examples/Extensions/PosAndRotExtensions-Examples.cs" region="ToLocal"/>
/// <code source="Examples/Extensions/PosAndRotExtensions-Examples.cs" region="ToWorld"/>
/// </example>
public sealed class PosAndRot : IPersistentField {
  /// <summary>Position of the transform.</summary>
  public Vector3 pos;

  /// <summary>Euler rotation.</summary>
  /// <remarks>
  /// The rotation angles are automatically adjusted to stay within the [0; 360) range.
  /// </remarks>
  /// <value>The Euler angles of the rotation.</value>
  public Vector3 euler {
    get { return _euler; }
    set {
      _euler = value;
      NormlizeAngles();
      rot = Quaternion.Euler(_euler.x, _euler.y, _euler.z);
    }
  }
  Vector3 _euler;

  /// <summary>Orientation of the transform.</summary>
  /// <value>The rotation of the transfrom.</value>
  public Quaternion rot { get; private set; }

  /// <summary>Constructs a default instance.</summary>
  /// <remarks>Required for the persistence to work correctly.</remarks>
  /// <para>
  /// By default position is <c>(0,0,0)</c>, Euler angles are <c>(0,0,0)</c>, and the rotation is
  /// <c>Quaternion.identity</c>.  
  /// </para>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Vector3.html">
  /// Unity3D: Vector3</seealso>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Quaternion-identity.html">
  /// Unity3D: Quaternion</seealso>
  public PosAndRot() {
  }

  /// <summary>Constructs a copy of an object of the same type.</summary>
  /// <param name="from">Source object.</param>
  public PosAndRot(PosAndRot from) {
    pos = from.pos;
    euler = from.euler;
  }

  /// <summary>Constructs an object from a transform properties.</summary>
  /// <param name="pos">Position of the transform.</param>
  /// <param name="euler">Euler rotation of the transform.</param>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Vector3.html">
  /// Unity3D: Vector3</seealso>
  /// <seealso href="https://docs.unity3d.com/ScriptReference/Transform.html">
  /// Unity3D: Transform</seealso>
  public PosAndRot(Vector3 pos, Vector3 euler) {
    this.pos = pos;
    this.euler = euler;
  }

  /// <summary>Gives a deep copy of the object.</summary>
  /// <returns>New object.</returns>
  public PosAndRot Clone() {
    return new PosAndRot(this);
  }

  /// <inheritdoc/>
  public string SerializeToString() {
    return string.Format(
        "{0},{1},{2}, {3},{4},{5}", pos.x, pos.y, pos.z, euler.x, euler.y, euler.z);
  }

  /// <inheritdoc/>
  public void ParseFromString(string value) {
    var elements = value.Split(',');
    if (elements.Length != 6) {
      throw new ArgumentException(
          "PosAndRot type needs exactly 6 elements separated by a comma but found: " + value);
    }
    var args = elements.Select(float.Parse).ToArray();
    pos = new Vector3(args[0], args[1], args[2]);
    euler = new Vector3(args[3], args[4], args[5]);
  }

  /// <summary>Shows a human readable representation.</summary>
  /// <returns>String value.</returns>
  public override string ToString() {
    return string.Format(
        "[PosAndRot Pos={0}, Euler={1}]", DbgFormatter.Vector(pos), DbgFormatter.Vector(euler));
  }

  /// <summary>Creates a new instance from the provided string.</summary>
  /// <param name="strValue">The value to parse.</param>
  /// <param name="failOnError">
  /// If <c>true</c> then a parsing error will fail the creation. Otherwise, a default instance will
  /// be returned.
  /// </param>
  /// <returns>An instance, intialized from the string.</returns>
  public static PosAndRot FromString(string strValue, bool failOnError = false) {
    var res = new PosAndRot();
    try {
      res.ParseFromString(strValue);
    } catch (ArgumentException ex) {
      if (failOnError) {
        throw;
      }
      DebugEx.Warning("Cannot parse PosAndRot, using default: {0}", ex.Message);
    }
    return res;
  }

  /// <summary>
  /// Transforms the object from the world space to the local space of a reference transform.
  /// </summary>
  /// <param name="parent">The transfrom to assume as a parent.</param>
  /// <returns>A new object in the world space of <paramref name="parent"/>.</returns>
  /// <example>
  /// <code source="Examples/Extensions/PosAndRotExtensions-Examples.cs" region="ToWorld"/>
  /// </example>
  public PosAndRot Transform(Transform parent) {
    return new PosAndRot(
        parent.position + parent.rotation * pos, (parent.rotation * rot).eulerAngles);
  }

  /// <summary>
  /// Transforms the object from world space to local space of a reference transform.
  /// </summary>
  /// <param name="parent">The transfrom to assume as a parent.</param>
  /// <returns>A new object in the local space of <paramref name="parent"/>.</returns>
  /// <example>
  /// <code source="Examples/Extensions/PosAndRotExtensions-Examples.cs" region="ToLocal"/>
  /// </example>
  public PosAndRot InverseTransform(Transform parent) {
    var inverseRot = parent.rotation.Inverse();
    return new PosAndRot(inverseRot * (pos - parent.position), (inverseRot * rot).eulerAngles);
  }
  
  /// <summary>
  /// Ensures that all the angles are in the range of <c>[0; 360)</c>. 
  /// </summary>
  void NormlizeAngles() {
    while (_euler.x >= 360) _euler.x -= 360;
    while (_euler.x < 0) _euler.x += 360;
    while (_euler.y >= 360) _euler.y -= 360;
    while (_euler.y < 0) _euler.y += 360;
    while (_euler.z >= 360) _euler.z -= 360;
    while (_euler.z < 0) _euler.z += 360;
  }
}

}  // namespace
