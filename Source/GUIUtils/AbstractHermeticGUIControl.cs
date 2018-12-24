﻿// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.LogUtils;
using System;
using System.Reflection;
using UnityEngine;

namespace KSPDev.GUIUtils {

/// <summary>Base class for a GUI element that incapsulates a class instance member.</summary>
/// <remarks>
/// This kind of controls are used to represent and/or edit the values of various types, but not in
/// the usual <c>OnGUI</c> approach. The usual Unity approach assumes that the value is passed and
/// returned in each rendering call. However, for the hermetic control, the source of the value is
/// defined only once, at the creation time. Once it's done, the rendering is only called to have
/// the logic handled. That said, the caller rendering method doesn't care about the underlying
/// data, and that's why it's "hermetic".
/// <para>
/// The value source is either a field or a property of the class. Due to the need of using
/// reflections to access the member, these controls are not performance optimized. They are fine
/// for the debugging and settings dialogs, but for the performance demanding applications a regular
/// Unity approach is suggested.
/// </para>
/// </remarks>
public abstract class AbstractHermeticGUIControl : IRenderableGUIControl {

  #region Initialization settings.
  readonly object instance;
  readonly FieldInfo fieldInfo;
  readonly PropertyInfo propertyInfo;
  readonly Action onBeforeUpdate;
  readonly Action onAfterUpdate;
  #endregion

  #region IRenderableGUIControl implementation
  /// <inheritdoc/>
  public abstract void RenderControl(
      GuiActionsList actionsList, GUIStyle layoutStyle, GUILayoutOption[] layoutOptions);
  #endregion

  /// <summary>Creates a control, bound to a member.</summary>
  /// <param name="instance">The class instance that owns the member to manage.</param>
  /// <param name="fieldInfo">
  /// The field to manage. It must be <c>null</c> if <paramref name="propertyInfo"/> is set.
  /// </param>
  /// <param name="propertyInfo">
  /// The property to manage. It's ignored if <paramref name="fieldInfo"/> is not <c>null</c>.
  /// </param>
  /// <param name="onBeforeUpdate">
  /// The callback to call before changing the value. <see cref="InvalidOperationException"/> can be
  /// throws form this action to prevent the change.
  /// </param>
  /// <param name="onAfterUpdate">The callback to call when the value is changed.</param>
  protected AbstractHermeticGUIControl(
      object instance,
      FieldInfo fieldInfo,
      PropertyInfo propertyInfo,
      Action onBeforeUpdate,
      Action onAfterUpdate) {
    this.instance = instance;
    this.fieldInfo = fieldInfo;
    this.propertyInfo = propertyInfo;
    this.onBeforeUpdate = onBeforeUpdate;
    this.onAfterUpdate = onAfterUpdate;
    if (propertyInfo != null && !propertyInfo.CanRead) {
      throw new ArgumentException(string.Format(
          "Property not readable: {0}.{1} => {2}",
          propertyInfo.DeclaringType.FullName, propertyInfo.Name, propertyInfo.PropertyType));
    }
  }

  /// <summary>Returns the type of the member value.</summary>
  /// <returns>The type object of the member.</returns>
  protected Type GetMemberType() {
    return fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
  }

  /// <summary>Get the value from the controlled member.</summary>
  /// <typeparam name="T">The type of the member.</typeparam>
  /// <returns>The value of the member.</returns>
  protected T GetMemberValue<T>() {
    return (T) (fieldInfo != null
        ? fieldInfo.GetValue(instance)
        : propertyInfo.GetValue(instance, null));
  }

  /// <summary>Sets value to the controlled member.</summary>
  /// <typeparam name="T">The type of the member.</typeparam>
  /// <param name="value">The new value to set.</param>
  /// <param name="actionsList">
  /// The actions list to submit the update actions into. If it's <c>null</c>, then the update and
  /// the notification will happen immediately.
  /// </param>
  protected void SetMemberValue<T>(T value, GuiActionsList actionsList = null) {
    if (actionsList != null) {
      actionsList.Add(() => SetMemberValueInternal(value));
    } else {
      SetMemberValueInternal(value);
    }
  }

  /// <summary>Calls the customized update method.</summary>
  /// <remarks>It's fail proof.</remarks>
  protected void UpdateMemberInstance() {
    if (onAfterUpdate != null) {
      try {
        onAfterUpdate();
      } catch (Exception ex) {
        DebugEx.Error("Exception in the update method: {0}", ex);
      }
    }
  }

  /// <summary>Implemnet actual setting logic.</summary>
  /// <param name="value">The value to set.</param>
  void SetMemberValueInternal<T>(T value) {
    if (onBeforeUpdate != null) {
      try {
        onBeforeUpdate();
      } catch (InvalidOperationException ex) {
        ScreenMessaging.ShowErrorScreenMessage(ex.Message);
        return;
      }
    }
    if (fieldInfo != null) {
      fieldInfo.SetValue(instance, value);
    } else {
      if (propertyInfo.CanWrite) {
        propertyInfo.SetValue(instance, value, null);
      } else {
        DebugEx.Error(
            "Property not writable: {0}.{1} => {2}",
            propertyInfo.DeclaringType.FullName, propertyInfo.Name, propertyInfo.PropertyType);
      }
    }
    UpdateMemberInstance();
  }
}

}  // namespace
