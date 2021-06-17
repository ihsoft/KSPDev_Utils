// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityEngine;

namespace KSPDev.GUIUtils {

/// <summary>A wrapper class to present a simple overlay window with some text.</summary>
/// <remarks>
/// <p>The overlay windows don't have a border or title. The main purpose of such windows is
/// present the "hints". I.e. short a lived piece of information presented for the current context.
/// The hint won't be shown in UI until explicitly requested via a call to the <c>ShowAt*</c> method.
/// </p>
/// <p>
/// Keep in mind that this window resources will be destroyed when the scene is re-loaded. I.e. the
/// hint window must be re-created on every scene change.
/// </p>
/// </remarks>
/// <example>
/// In a common case, the initialization of the hint window is done on the game object awakening,
/// and it's either shown or hidden in the <c>OnGUI</c> method.
/// <code>
/// class MyMod : MonoBehaviour {
///   HintOverlay hint;
///
///   void Awake() {
///     hint = new HintOverlay(() => GUI.skin, () => GUI.skin.label, adjustGuiScale: true);
///   }
///
///   void OnGUI() {
///     hint.text = string.Format("Current frame is: {0}", Time.frameCount);
///     hint.ShowAtCursor();
///   }
/// }
/// </code>
/// <p>In the example above text of the hint is set on every frame update since the frame count is updated
/// this frequently. However, if your data is updated less frequently you may save some performance
/// by updating text in the methods different from <c>OnGUI</c>.</p>
/// </example>
public class HintOverlay {
  // ReSharper disable MemberCanBePrivate.Global
  // ReSharper disable FieldCanBeMadeReadOnly.Global
  // ReSharper disable ConvertToConstant.Global
  #region Public properties and fields
  /// <summary>Padding when showing hint on the right side of the mouse cursor.</summary>
  public int rightSideMousePadding = 24;
 
  /// <summary>Padding when showing hint on the left side of the mouse cursor.</summary>
  public int leftSideMousePadding = 4;

  /// <summary>The hint overlay text.</summary>
  /// <remarks>
  /// Linefeed symbols are correctly handled. Use them to make multiline content. Setting text is an
  /// expensive operation since it results in window size recalculation. Don't update it more
  /// frequently than the underlying data does.
  /// </remarks>
  /// <value>The text to be show as a hint.</value>
  public string text {
    get => _text;
    set {
      _text = value;
      _textSize = hintWindowStyle.CalcSize(new GUIContent(text)); 
    }
  }
  string _text;

  // ReSharper enable MemberCanBePrivate.Global
  // ReSharper enable FieldCanBeMadeReadOnly.Global
  // ReSharper enable ConvertToConstant.Global
  #endregion

  /// <summary>Precalculated UI text size for the currently assigned text.</summary>
  Vector2 _textSize;

  /// <summary>Precalculated style for the hint overlay window.</summary>
  GUIStyle hintWindowStyle {
    get {
      if (_hintWindowStyle != null &&
          (!_adjustGuiScale || !(Mathf.Abs(_lastGuiScale - GameSettings.UI_SCALE) > float.Epsilon))) {
        return _hintWindowStyle;
      }
      _lastGuiScale = GameSettings.UI_SCALE;
      _hintWindowStyle = _adjustGuiScale
          ? GuiScaledSkin.ScaleGuiStyle(_skinProvider.Invoke(), _styleProvider.Invoke(), _lastGuiScale)
          : _styleProvider.Invoke();
      return _hintWindowStyle;
    }
  }
  GUIStyle _hintWindowStyle;

  readonly bool _adjustGuiScale;
  readonly Func<GUIStyle> _styleProvider;
  readonly Func<GUISkin> _skinProvider;

  /// <summary>The GUI scale for which <see cref="_hintWindowStyle"/> was last updated.</summary>
  /// <remarks>Used to detect when the style needs to be updated.</remarks>
  float _lastGuiScale;

  /// <summary>Constructs an overlay.</summary>
  /// <param name="styleProvider">A function that returns the reference skin.</param>
  /// <param name="skinProvider">A function that returns the style.</param>
  /// <param name="adjustGuiScale">
  /// If set to <c>true</c>, then the overlay will consider the current UI scale setting in the game. The text style
  /// will be adjusted accordingly.
  /// </param>
  public HintOverlay(Func<GUISkin> skinProvider, Func<GUIStyle> styleProvider, bool adjustGuiScale = false) {
    _styleProvider = styleProvider;
    _skinProvider = skinProvider;
    _adjustGuiScale = adjustGuiScale;
  }

  /// <summary>Shows hint text at the current mouse pointer.</summary>
  /// <remarks>When possible the window is shown on the right side of the cursor. Though, if part of
  /// the window goes out of the screen then it will be shown on the left side. If bottom boundary
  /// of the window hits bottom boundary of the screen then hint is aligned vertically so what the
  /// full content is visible. </remarks>
  public void ShowAtCursor() {
    var xPos = Mouse.screenPos.x + rightSideMousePadding;
    if (xPos + _textSize.x > Screen.width) {
      xPos = Mouse.screenPos.x - leftSideMousePadding - _textSize.x;
    }
    var yPos = Mouse.screenPos.y;
    if (yPos + _textSize.y > Screen.height) {
      yPos = Screen.height - _textSize.y;
    }
    ShowAtPosition(xPos, yPos);
  }

  /// <summary>Shows hint at the absolute screen position.</summary>
  /// <remarks>
  /// If hint content goes out of the screen it's clipped.
  /// <para>This method must be called from the <c>OnGUI()</c> method.</para>
  /// </remarks>
  /// <param name="x">X position is screen coordinates.</param>
  /// <param name="y">Y position is screen coordinates.</param>
  public void ShowAtPosition(float x, float y) {
    var hintLabelRect = new Rect(x, y, _textSize.x, _textSize.y);
    GUI.Label(hintLabelRect, text, hintWindowStyle);
  }
}

}  // namespace
