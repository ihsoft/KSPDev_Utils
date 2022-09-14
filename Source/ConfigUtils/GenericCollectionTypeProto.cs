// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using System;
using System.Reflection;
using System.Collections;
using UnityEngine;

namespace KSPDev.ConfigUtils {

/// <summary>A proto handler for a mutable generic collection.</summary>
/// <remarks>
/// The generic must have exactly one argument. The target type must provide two methods: <c>Add</c> and <c>Clear</c>.
/// If any of them is missing, the type is considered ineligible. If they are present, then they will be used to fill
/// the collection on a deserialization.
/// </remarks>
/// <seealso cref="PersistentFieldAttribute"/>
public sealed class GenericCollectionTypeProto : AbstractCollectionTypeProto {
  readonly Type itemType;
  readonly MethodInfo addMethod;
  readonly MethodInfo clearMethod;

  /// <inheritdoc/>
  public GenericCollectionTypeProto(Type containerType) : base(containerType) {
    if (!containerType.IsGenericType || containerType.GetGenericArguments().Length != 1) {
      DebugEx.Error(
          "{0} requires generic container with one type parameter but found: {1}",
          GetType().FullName, containerType.FullName);
      throw new ArgumentException("Invalid container type");
    }
    itemType = containerType.GetGenericArguments()[0];

    addMethod = containerType.GetMethod("Add");
    if (addMethod == null) {
      DebugEx.Error("Type {0} doesn't have Add() method", containerType.FullName);
      throw new ArgumentException("Invalid container type");
    }
    clearMethod = containerType.GetMethod("Clear");
    if (clearMethod == null) {
      DebugEx.Error("Type {0} doesn't have Clear() method", containerType.FullName);
      throw new ArgumentException("Invalid container type");
    }
  }

  /// <inheritdoc/>
  public override Type GetItemType() {
    return itemType;
  }
  
  /// <inheritdoc/>
  public override IEnumerable GetEnumerator(object instance) {
    return instance as IEnumerable;
  }
  
  /// <inheritdoc/>
  public override void AddItem(object instance, object item) {
    addMethod.Invoke(instance, new[] {item});
  }

  /// <inheritdoc/>
  public override void ClearItems(object instance) {
    clearMethod.Invoke(instance, new object[0]);
  }
}

}  // namespace
