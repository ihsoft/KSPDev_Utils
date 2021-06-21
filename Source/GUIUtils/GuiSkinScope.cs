// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace KSPDev.GUIUtils {

/// <summary>A utility class to render windows with an alternative skin.</summary>
/// <remarks>
/// When the scope starts, it changes global setting <c>GUI.skin</c>. On the scope end, the original value is restored.
/// It can be used from <c>OnGUI</c> method to replace skin for the entire window.
/// </remarks>
/// <seealso cref="GuiScaledSkin"/>
public class GuiSkinScope : IDisposable {
  readonly GUISkin _oldSkin;

  /// <summary>Stores the old skin and sets a new one.</summary>
  /// <param name="newSkin">The new skin to set.</param>
  public GuiSkinScope(GUISkin newSkin) {
    _oldSkin = GUI.skin;
    GUI.skin = newSkin;
  }

  /// <summary>Restores the skin that was set before the scope started.</summary>
  public void Dispose() {
    GUI.skin = _oldSkin;
  }
}

}
