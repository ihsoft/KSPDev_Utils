﻿// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KSPDev.DebugUtils {

/// <summary>Helper class to deal with the debug GUI functionality.</summary>
/// <seealso cref="DebugAdjustableAttribute"/>
public static class DebugGui {

  /// <summary>Game object to use to host the debug dialogs.</summary>
  /// <seealso cref="MakePartDebugDialog"/>
  static GameObject dialogsRoot {
    get {
      if (_dialogsRoot == null) {
        _dialogsRoot = new GameObject("dialogsRoot-" + LibraryLoader.assemblyVersionStr);
      }
      return _dialogsRoot;
    }
  }
  static GameObject _dialogsRoot;

  /// <summary>Metadata about the member that is available for debugging.</summary>
  public struct DebugMemberInfo {
    /// <summary>Attribute, that describes the member.</summary>
    public DebugAdjustableAttribute attr;

    /// <summary>Field info for the field member.</summary>
    public FieldInfo fieldInfo;

    /// <summary>Property info for the property member.</summary>
    public PropertyInfo propertyInfo;

    /// <summary>Method info for the method member.</summary>
    public MethodInfo methodInfo;
  }

  /// <summary>Dumps the objects hierarchy to the logs.</summary>
  /// <remarks>
  /// This method recirsively goes down to the all decendants, starting from
  /// <paramref name="child"/>. The path, however, is shown realtive to <paramref name="root"/>.
  /// </remarks>
  /// <param name="root">The root object to make the path from.</param>
  /// <param name="child">The descendant of <paramref name="root"/> to dump descendants for.</param>
  public static void DumpHierarchy(Transform  root, Transform child) {
    if (root != child) {
      DebugEx.Warning("{0} (localPos: {1}, localRot: {2}, localEuler: {3})",
                      Hierarchy.MakePath(Hierarchy.GetFullPath(child, parent: root)),
                      child.localPosition, child.localRotation, child.localRotation.eulerAngles);
    }
    for (var i = 0; i < child.childCount; i++) {
      DumpHierarchy(root, child.GetChild(i));
    }
  }

  /// <summary>Gets the fields, available for debugging.</summary>
  /// <param name="obj">The instance to get the fields from.</param>
  /// <param name="group">
  /// The group to get the controls for. If empty, then all the controls are returned.
  /// </param>
  /// <returns>The member meta info for all the available fields.</returns>
  /// <seealso cref="DebugAdjustableAttribute"/>
  public static List<DebugMemberInfo> GetAdjustableFields(object obj, string group = "") {
    var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
    var attrType = typeof(DebugAdjustableAttribute);
    return obj.GetType()
        .GetFields(flags)
        .Where(m => m.GetCustomAttributes(attrType, true).Length > 0)
        .Select(m => new DebugMemberInfo() {
            attr = m.GetCustomAttributes(attrType, true)[0] as DebugAdjustableAttribute,
            fieldInfo = m
        })
        .Where(m => group == "" || m.attr.group == group)
        .ToList();
  }

  /// <summary>Gets the properties, available for debugging.</summary>
  /// <param name="obj">The instance to get the properties from.</param>
  /// <param name="group">
  /// The group to get the controls for. If empty, then all the controls are returned.
  /// </param>
  /// <returns>The member meta info for all the available properties.</returns>
  /// <seealso cref="DebugAdjustableAttribute"/>
  public static List<DebugMemberInfo> GetAdjustableProperties(object obj, string group = "") {
    var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
    var attrType = typeof(DebugAdjustableAttribute);
    return obj.GetType()
        .GetProperties(flags)
        .Where(m => m.GetCustomAttributes(attrType, true).Length > 0)
        .Select(m => new DebugMemberInfo() {
            attr = m.GetCustomAttributes(attrType, true)[0] as DebugAdjustableAttribute,
            propertyInfo = m
        })
        .ToList();
  }

  /// <summary>Gets the methods, available for calling from the debugging GUI.</summary>
  /// <remarks>The atributed method must have zero parameters.</remarks>
  /// <param name="obj">The instance to get the methods from.</param>
  /// <param name="group">
  /// The group to get the controls for. If empty, then all the controls are returned.
  /// </param>
  /// <returns>The member meta info for all the available methods.</returns>
  /// <seealso cref="DebugAdjustableAttribute"/>
  public static List<DebugMemberInfo> GetAdjustableActions(object obj, string group = "") {
    var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                | BindingFlags.Instance;
    var attrType = typeof(DebugAdjustableAttribute);
    return obj.GetType()
        .GetMethods(flags)
        .Where(m => m.GetCustomAttributes(attrType, true).Length > 0)
        .Select(m => new DebugMemberInfo() {
            attr = m.GetCustomAttributes(attrType, true)[0] as DebugAdjustableAttribute,
            methodInfo = m
        })
        .ToList();
  }

  /// <summary>Creates a debug dialog for the parts.</summary>
  /// <param name="title">The titile of the dialog.</param>
  /// <param name="dialogWidth">
  /// The width of the dialog. If omitted, then the code will decide.
  /// </param>
  /// <param name="valueColumnWidth">
  /// The width of the value changing controls. If omitted, then the code will decide.
  /// </param>
  /// <param name="group">
  /// The group of the controls to present. If empty, then all the controls are shown.
  /// </param>
  /// <returns>The created dialog.</returns>
  /// <seealso cref="DestroyPartDebugDialog"/>
  /// <seealso cref="DebugAdjustableAttribute"/>
  public static PartDebugAdjustmentDialog MakePartDebugDialog(
      string title, float? dialogWidth = null, float? valueColumnWidth = null, string group = "") {
    var dlg = dialogsRoot.AddComponent<PartDebugAdjustmentDialog>();
    dlg.dialogTitle = title;
    dlg.dialogWidth = dialogWidth ?? dlg.dialogWidth;
    dlg.dialogValueSize = valueColumnWidth ?? dlg.dialogValueSize;
    dlg.controlsGroup = group;
    DebugEx.Info("Created debug dialog: {0}", title);
    return dlg;
  }

  /// <summary>Destroys the debug dialog.</summary>
  /// <param name="dlg">The dialog to destroy.</param>
  /// <seealso cref="MakePartDebugDialog"/>
  public static void DestroyPartDebugDialog(PartDebugAdjustmentDialog dlg) {
    UnityEngine.Object.Destroy(dlg);
  }
}

}  // namespace
