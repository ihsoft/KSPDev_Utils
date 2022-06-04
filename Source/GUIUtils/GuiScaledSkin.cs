// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using KSPDev.LogUtils;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace
namespace KSPDev.GUIUtils {

/// <summary>Component for automatic <c>GUISkin</c> rescaling, based on the current game's GUI scale setting.</summary>
/// <remarks>
/// <p>
/// The scaling is achieved by proportional increase/decrease of the font size and margin/padding rects. The default
/// font stays unchanged, but all the controls that use default font size will get overrides for the nwe scaled size.
/// Some more elements will not get scaled:
/// </p>
/// <p>
/// <list type="bullet">
/// <item>The background images.</item>
/// <item>The textures, rendered via <c>Label</c> and <c>DrawTexture</c> controls.</item>
/// <item>The <c>GUILayoutOption</c> size settings.</item>
/// <item>The <c>GUILayout.Space()</c> control.</item>
/// </list>
/// </p> 
/// </remarks>
/// <example>
/// <code>
/// <![CDATA[
/// GuiScaledSkin _guiScaledSkin;
///
/// void OnAwake() {
///   _guiScaledSkin = new GuiScaledSkin(() => GUI.skin);
/// }
///
/// void OnGUI() {
///   using (new GuiSkinScope(_guiScaledSkin.scaledSkin)) {
///     _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, WindowFunc, WindowTitle);
///   }
/// }
/// ]]>
/// </code>
/// </example>
/// <seealso cref="GuiSkinScope"/>
public class GuiScaledSkin {
  /// <summary>The skin that is scaled to the current game's settings.</summary>
  /// <remarks>Dop not cache this instance! It changes as the scale setting does.</remarks>
  /// <value>A copy of the original skin that is scaled to the currently selected game's settings.</value>
  /// <seealso cref="guiScale"/>
  public GUISkin scaledSkin {
    get {
      if (scaledSkinIsDirty) {
        UpdateScaledSkin();
      }
      return _scaledSkin;
    }
  }

  /// <summary>Tells if the skin must be recalculated on the next access to <see cref="scaledSkin"/>.</summary>
  public bool scaledSkinIsDirty;

  /// <summary>The scale setting which this instance currently tracks.</summary>
  /// <remarks>It's updated from the <see cref="scaledSkin"/> property getter.</remarks>
  /// <value>The scale of <see cref="scaledSkin"/>.</value>
  public float guiScale { get; private set; }

  /// <summary>The dialog title height with this skin applied.</summary>
  /// <remarks>It's a best guess number. It's <b>NOT</b> the real value to be used in the GUI layout engine.</remarks>
  /// <value>The height of the GUI dialog title.</value>
  public int guiTitleHeight { get; private set; }

  readonly Func<GUISkin> _sourceSkinProvider;
  readonly Action<GUISkin> _onSkinUpdatedFn;
  GUISkin _scaledSkin;

  /// <summary>Creates a scaled skin instance.</summary>
  /// <param name="sourceSkinProvider">
  /// A getter function that returns the <i>base</i> skin. This skin must not be affected by the game's GUI scale!
  /// </param>
  /// <param name="onSkinUpdatedFn">
  /// <p>
  /// A callback which will be called every time the <see cref="scaledSkin"/> is updated. The mods can react on it to
  /// update their internal cached styles. This callback is processed from the <see cref="scaledSkin"/> property getter.
  /// </p>
  /// <p>The very first call will be made in scope of the constructor.</p>
  /// </param>
  public GuiScaledSkin(Func<GUISkin> sourceSkinProvider, Action<GUISkin> onSkinUpdatedFn = null) {
    _sourceSkinProvider = sourceSkinProvider;
    _onSkinUpdatedFn = onSkinUpdatedFn;
    scaledSkinIsDirty = true;
    GameEvents.onUIScaleChange.Add(OnUIScaleChangeGameEvent);
  }

  /// <summary>Unregisters any game's callbacks. </summary>
  ~GuiScaledSkin() {
    DebugEx.Fine("Destroying scaled skin object...");
    UnityEngine.Object.Destroy(_scaledSkin);
    GameEvents.onUIScaleChange.Remove(OnUIScaleChangeGameEvent);
  }

  /// <summary>Makes a style that respects the provided scale factor.</summary>
  /// <remarks>
  /// This method overrides the default font usage. The resulted style will be overriding the font size, even if the
  /// original style wasn't assuming it. It may have performance or UI consequences.
  /// </remarks>
  /// <param name="originalSkin">The skin that is a base for the scaling.</param>
  /// <param name="originalStyle">The style to get values from.</param>
  /// <param name="guiScale">The scale factor, where <c>1.0</c> means "exactly as in the original".</param>
  /// <returns>A scaled style. The default font size will be overwritten.</returns>
  public static GUIStyle ScaleGuiStyle(GUISkin originalSkin, GUIStyle originalStyle, float guiScale) {
    var refFont = originalStyle.font != null ? originalStyle.font : originalSkin.font;
    var fontSize = originalStyle.fontSize > float.Epsilon ? originalStyle.fontSize : refFont.fontSize; 
    var res = new GUIStyle(originalStyle) {
        fontSize = Mathf.CeilToInt(guiScale * fontSize),
        contentOffset = originalStyle.contentOffset * guiScale,
        fixedHeight = originalStyle.fixedHeight * guiScale,
        fixedWidth = originalStyle.fixedWidth * guiScale,
        overflow = ScaleRectOffset(originalStyle.overflow, guiScale),
        margin = ScaleRectOffset(originalStyle.margin, guiScale),
        padding = ScaleRectOffset(originalStyle.padding, guiScale),
    };
    return res;
  }

  /// <summary>Scales the provided <c>RectOffset</c> by the scale factor.</summary>
  /// <remarks>Primarily used to scale the KSP GUI controls.</remarks>
  /// <param name="original">The original value.</param>
  /// <param name="guiScale">The scale factor.</param>
  /// <returns>A scaled <c>RectOffset</c>.</returns>
  public static RectOffset ScaleRectOffset(RectOffset original, float guiScale) {
    var scaleDelta = guiScale - 1.0f;
    return new RectOffset(
        Mathf.CeilToInt(original.left + scaleDelta * original.left),
        Mathf.CeilToInt(original.right + scaleDelta * original.right),
        Mathf.CeilToInt(original.top + scaleDelta * original.top),
        Mathf.CeilToInt(original.bottom + scaleDelta * original.bottom));
  }

  #region Local utility methods
  /// <summary>Makes a scaled skin based on the origin one.</summary>
  void UpdateScaledSkin() {
    scaledSkinIsDirty = false;
    guiScale = GameSettings.UI_SCALE;

    var originalSkin = _sourceSkinProvider.Invoke();
    UnityEngine.Object.Destroy(scaledSkin);
    _scaledSkin = UnityEngine.Object.Instantiate(originalSkin);

    // Basic styles.
    _scaledSkin.box = ScaleGuiStyle(originalSkin, originalSkin.box, guiScale);
    _scaledSkin.button = ScaleGuiStyle(originalSkin, originalSkin.button, guiScale);
    _scaledSkin.horizontalScrollbar = ScaleGuiStyle(originalSkin, originalSkin.horizontalScrollbar, guiScale);
    _scaledSkin.horizontalScrollbarLeftButton =
        ScaleGuiStyle(originalSkin, originalSkin.horizontalScrollbarLeftButton, guiScale);
    _scaledSkin.horizontalScrollbarRightButton =
        ScaleGuiStyle(originalSkin, originalSkin.horizontalScrollbarRightButton, guiScale);
    _scaledSkin.horizontalScrollbarThumb = ScaleGuiStyle(originalSkin, originalSkin.horizontalScrollbarThumb, guiScale);
    _scaledSkin.horizontalSlider = ScaleGuiStyle(originalSkin, originalSkin.horizontalSlider, guiScale);
    _scaledSkin.horizontalSliderThumb = ScaleGuiStyle(originalSkin, originalSkin.horizontalSliderThumb, guiScale);
    _scaledSkin.label = ScaleGuiStyle(originalSkin, originalSkin.label, guiScale);
    _scaledSkin.scrollView = ScaleGuiStyle(originalSkin, originalSkin.scrollView, guiScale);
    _scaledSkin.textArea = ScaleGuiStyle(originalSkin, originalSkin.textArea, guiScale);
    _scaledSkin.textField = ScaleGuiStyle(originalSkin, originalSkin.textField, guiScale);
    _scaledSkin.toggle = ScaleGuiStyle(originalSkin, originalSkin.toggle, guiScale);
    _scaledSkin.verticalScrollbar = ScaleGuiStyle(originalSkin, originalSkin.verticalScrollbar, guiScale);
    _scaledSkin.verticalScrollbarDownButton =
        ScaleGuiStyle(originalSkin, originalSkin.verticalScrollbarDownButton, guiScale);
    _scaledSkin.verticalScrollbarThumb = ScaleGuiStyle(originalSkin, originalSkin.verticalScrollbarThumb, guiScale);
    _scaledSkin.verticalScrollbarUpButton =
        ScaleGuiStyle(originalSkin, originalSkin.verticalScrollbarUpButton, guiScale);
    _scaledSkin.verticalSlider = ScaleGuiStyle(originalSkin, originalSkin.verticalSlider, guiScale);
    _scaledSkin.verticalSliderThumb = ScaleGuiStyle(originalSkin, originalSkin.verticalSliderThumb, guiScale);
    _scaledSkin.window = ScaleGuiStyle(originalSkin, originalSkin.window, guiScale);

    // Custom styles
    var newCustomStyles = new GUIStyle[originalSkin.customStyles.Length];
    for (var i = newCustomStyles.Length - 1; i >= 0; i--) {
      newCustomStyles[i] = ScaleGuiStyle(originalSkin, originalSkin.customStyles[i], guiScale);
    }

    var wndStyle = _scaledSkin.window;
    guiTitleHeight = Mathf.CeilToInt(wndStyle.margin.top + wndStyle.padding.top);

    DebugEx.Info("Created a new scaled skin: scale={0}, guiTitleHeight={1}", guiScale, guiTitleHeight);

    _onSkinUpdatedFn?.Invoke(scaledSkin);
  }

  /// <summary>Reacts on the GUI scale change event and recalculates the skin.</summary>
  void OnUIScaleChangeGameEvent() {
    scaledSkinIsDirty = true;
  }
  #endregion
}
}
