// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.ModelUtils;
using System;
using System.Linq;
using KSPDev.LogUtils;
using UnityEngine;

namespace KSPDev.PartUtils {

/// <summary>Helper methods to deal with the part models.</summary>
public static class PartModel {
  /// <summary>Refreshes the highlighters on the part that owns the provided model.</summary>
  /// <remarks>
  /// When a part is highlighted (e.g. due to the mouse hover event), it highlights its models via a
  /// pre-cached set of the highlighter components. This cache is constructed on the part creation.
  /// If a model is added or removed from the part in runtime, the cache needs to be updated. This
  /// method does it by finding the part from the game objects hierarchy. If there is a part found,
  /// then its highlighters are updated.
  /// </remarks>
  /// <param name="modelObj">The game object which needs an update. It can be <c>null</c>.</param>
  /// <param name="exclude">If set, then this object and all its children will be excluded from highlighting.</param>
  public static void UpdateHighlighters(Transform modelObj, Transform exclude = null) {
    if (modelObj == null) {
      return;
    }
    var ownerPart = modelObj.GetComponentInParent<Part>();
    if (ownerPart != null) {
      UpdateHighlighters(ownerPart, exclude);
    }
  }

  /// <summary>Refreshes the highlighters on the part.</summary>
  /// <remarks>
  /// It goes through the highlighters cache and drops all the renderers that are no more in the part's
  /// model hierarchy. Then, it gets all the renderers in the hierarchy and ensures all of them are
  /// in the cache. It's not a cheap operation performance wise.
  /// </remarks>
  /// <param name="part">The part to refresh the highlighters for. It can be <c>null</c>.</param>
  /// <param name="exclude">
  /// If set, then this object and all its children will be excluded from highlighting. Note, that the stock game logic
  /// doesn't assume a case when some renderers from the model are not subject to highlighting. The client must track
  /// events which result to the stock update logic to run and repeat the custom update to maintain the excluded state. 
  /// </param>
  public static void UpdateHighlighters(Part part, Transform exclude = null) {
    if (part == null) {
      return;
    }
    if (part != null && part.HighlightRenderer != null) {
      part.ResetModelRenderersCache();
      var partModel = Hierarchy.GetPartModelTransform(part);
      part.HighlightRenderer = partModel.GetComponentsInChildren<Renderer>()
          .Where(r => exclude == null || !r.transform.IsChildOf(exclude)).ToList();
      part.HighlightRenderer.ForEach(r => r.SetPropertyBlock(part.mpb));
      part.RefreshHighlighter();
      // Refresh active highlighting to apply it on the new renderers. 
      if (part.HighlightActive) {
        var recursive = part.RecurseHighlight;
        part.SetHighlight(false, recursive);  // Need to reset the state first.
        part.SetHighlight(true, recursive);
      }
    }
  }
}
  
}  // namespace
