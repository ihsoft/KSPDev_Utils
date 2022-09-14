// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using System;
using System.Reflection;
using System.Collections;

namespace KSPDev.ConfigUtils {

/// <summary>A proto handler for a mutable generic collection.</summary>
/// <remarks>
/// The generic must have exactly one argument. The target type must provide two methods: <c>Add</c> and <c>Clear</c>.
/// If any of them is missing, the type is considered ineligible. If they are present, then they will be used to fill
/// the collection on a deserialization.
/// </remarks>
/// <seealso cref="PersistentFieldAttribute"/>
/// <seealso cref="IsSupportedType"/>
public sealed class GenericCollectionTypeProto : AbstractCollectionTypeProto {
  readonly Type _itemType;
  readonly MethodInfo _addMethod;
  readonly MethodInfo _clearMethod;

  /// <inheritdoc/>
  public GenericCollectionTypeProto(Type containerType) : base(containerType) {
    if (!IsSupportedType(containerType, logFailedChecks: true)) {
      throw new ArgumentException("Invalid container type");
    }
    _itemType = containerType.GetGenericArguments()[0];
    _addMethod = containerType.GetMethod("Add");
    _clearMethod = containerType.GetMethod("Clear");
  }

  /// <inheritdoc/>
  public override Type GetItemType() {
    return _itemType;
  }
  
  /// <inheritdoc/>
  public override IEnumerable GetEnumerator(object instance) {
    return instance as IEnumerable;
  }
  
  /// <inheritdoc/>
  public override void AddItem(object instance, object item) {
    _addMethod.Invoke(instance, new[] {item});
  }

  /// <inheritdoc/>
  public override void ClearItems(object instance) {
    _clearMethod.Invoke(instance, new object[0]);
  }

  /// <summary>Verifies if this proto can handle the provided collection type.</summary>
  /// <param name="type">The type to check.</param>
  /// <param name="logFailedChecks">Indicates that the failed checks must be logged.</param>
  /// <returns><c>true</c> if the collection type can be handled by this proto.</returns>
  public static bool IsSupportedType(Type type, bool logFailedChecks = false) {
    if (!type.IsGenericType || type.GetGenericArguments().Length != 1) {
      if (logFailedChecks) {
        DebugEx.Error(
            "{0} requires a generic container with one type parameter but found: {1}",
            nameof(GenericCollectionTypeProto), type.FullName);
      }
      return false;
    }
    if (type.GetMethod("Add") == null) {
      if (logFailedChecks) {
        DebugEx.Error("Type {0} doesn't have Add() method", type.FullName);
      }
      return false;
    }
    if (type.GetMethod("Clear") == null) {
      if (logFailedChecks) {
        DebugEx.Error("Type {0} doesn't have Clear() method", type.FullName);
      }
      return false;
    }
    return true;
  }
}

}  // namespace
