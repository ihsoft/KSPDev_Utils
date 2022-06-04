// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using KSPDev.LogUtils;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace KSPDev.GUIUtils {

/// <summary>Component for GUI dialog rescaling, based on the current game's GUI scale setting.</summary>
/// <remarks>
/// <p>
/// The scale is applied to the dialog controls by adjusting <c>GUI.matrix</c>, which is a global variable. It must be
/// restored to the original value before exiting <c>OnGUI</c>, or else the other controls in the game will also be
/// affected.
/// </p>
/// <p>
/// This control simply magnifies the dialog. The good side of it is that the original code doesn't need to be changed.
/// The bad side is that the low-res graphics (like the controls backgrounds) may not look nice when stretched up.
/// </p>
/// </remarks>
/// <example>
/// <code>
/// <![CDATA[
/// GuiScale _guiScale;
/// Rect _windowRect;
///
/// void OnAwake() {
///   _guiScale = new GuiScale(getPivotFn: () => new Vector2(_windowRect.x, _windowRect.y));
/// }
///
/// void OnGUI() {
///   using (new GuiMatrixScope()) {
///     _guiScale.UpdateMatrix();
///     _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, WindowFunc, WindowTitle);
///   }
/// }
/// ]]>
/// </code>
/// </example>
/// <seealso cref="GuiMatrixScope"/>
/// <seealso cref="GuiSkinScope"/>
public class GuiScale {
  /// <summary>The pivot point to scale the dialog at.</summary>
  /// <remarks>In a simple c case it's the position of the top-left corner of the dialog.</remarks>
  /// <value>The pivot point to make the matrix at.</value>
  public Vector2 pivot => _getPivotFn.Invoke();

  /// <summary>The current scale which this control is tracking.</summary>
  /// <value>The scale for which the last <c>onScaleUpdatedFn</c> was called.</value>
  public Vector2 scale { get; private set; }

  /// <summary>Tells if the scale must be recalculated on the next access to <see cref="scale"/>.</summary>
  public bool scaleIsDirty;

  readonly Action _onScaleUpdatedFn;
  readonly Func<Vector2> _getPivotFn;
  readonly bool _highFps;

  /// <summary>Creates a scaled skin instance.</summary>
  /// <param name="getPivotFn">
  /// A function that returns the pivot point for the scale transformation. In a regular usual case, this method returns
  /// the position top-left corner of the last known dialog position. 
  /// </param>
  /// <param name="onScaleUpdatedFn">
  /// A callback which will be called every time the game's UI scale is updated. The mods can react on it to
  /// update their internal cached styles. This callback is processed from the <see cref="UpdateMatrix"/> method.
  /// </param>
  /// <param name="highFps">
  /// Indicates if this object is created frequently. Even though it's not recommended to create this object frequently,
  /// this setting would tell the code to limit any logging or checks.
  /// </param>
  public GuiScale(Func<Vector2> getPivotFn, Action onScaleUpdatedFn = null, bool highFps = false) {
    _onScaleUpdatedFn = onScaleUpdatedFn;
    _getPivotFn = getPivotFn;
    _highFps = highFps;
    scaleIsDirty = true;
    GameEvents.onUIScaleChange.Add(OnUIScaleChangeGameEvent);
  }

  /// <summary>Updates the current GUI matrix to adjust the scale.</summary>
  /// <remarks>
  /// This method can trigger the <c>onScaleUpdatedFn</c> callback. So it's best to call it at the very beginning of
  /// the <c>OnGUI</c> method.
  /// </remarks>
  public void UpdateMatrix() {
    if (scaleIsDirty) {
      UpdateScale();
    }
    GUIUtility.ScaleAroundPivot(scale, pivot);
  }

  /// <summary>Unregisters any game's callbacks. </summary>
  ~GuiScale() {
    if (!_highFps) {
      DebugEx.Fine("Destroying GUI scale object...");
    }
    GameEvents.onUIScaleChange.Remove(OnUIScaleChangeGameEvent);
  }

  #region Local utility methods
  /// <summary>Makes a scaled skin based on the origin one.</summary>
  void UpdateScale() {
    scaleIsDirty = false;
    scale = new Vector2(GameSettings.UI_SCALE, GameSettings.UI_SCALE);
    if (!_highFps) {
      DebugEx.Fine("GUI scale updated: scale={0}", GameSettings.UI_SCALE);
    }
    _onScaleUpdatedFn?.Invoke();
  }

  /// <summary>Reacts on the GUI scale change event and recalculates the skin.</summary>
  void OnUIScaleChangeGameEvent() {
    scaleIsDirty = true;
  }
  #endregion
}
}
