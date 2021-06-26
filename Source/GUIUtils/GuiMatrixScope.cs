// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace KSPDev.GUIUtils {

/// <summary>A utility class to render windows with an alternative transformation matrix.</summary>
/// <remarks>This scope will remember the <c>GUI.matrix</c> property and restore it on exit.</remarks>
/// <seealso cref="GuiScaledSkin"/>
public class GuiMatrixScope : IDisposable {
  readonly Matrix4x4 _oldMatrix;

  /// <summary>Stores the old matrix.</summary>
  public GuiMatrixScope() {
    _oldMatrix = GUI.matrix;
  }

  /// <summary>Restores the matrix that was set before the scope started.</summary>
  public void Dispose() {
    GUI.matrix = _oldMatrix;
  }
}

}
