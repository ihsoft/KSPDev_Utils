﻿// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSP.Localization;

namespace KSPDev.ResourceUtils {

/// <summary>
/// A helper class that holds string and ID defintions for all the game stock resources. 
/// </summary>
/// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Resource">KSP Wiki: Resource</seealso>
/// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
public static class StockResourceNames {
  /// <summary>Electric charge resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Electric_charge">
  /// KSP Wiki: Electric charge</seealso>
  public const string ElectricCharge = "ElectricCharge";

  /// <summary>Liquid fuel resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Liquid_fuel">
  /// KSP Wiki: Liquid fuel</seealso>
  public const string LiquidFuel = "LiquidFuel";

  /// <summary>Oxidizer resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Oxidizer">
  /// KSP Wiki: Oxidizer</seealso>
  public const string Oxidizer = "Oxidizer";

  /// <summary>Intake air resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Intake_air">
  /// KSP Wiki: Intake air</seealso>
  public const string IntakeAir = "IntakeAir";

  /// <summary>Solid fuel resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Solid_fuel">
  /// KSP Wiki: Solid fuel</seealso>
  public const string SolidFuel = "SolidFuel";

  /// <summary>Monopropellant resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Monopropellant">
  /// KSP Wiki: Monopropellant</seealso>
  public const string MonoPropellant = "MonoPropellant";

  /// <summary>EVA Propellant resource name.</summary>
  /// <remarks>It's the fuel that powers the EVA jetpack.</remarks>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Extra-Vehicular_Activity">
  /// KSP Wiki: Extra-Vehicular Activity</seealso>
  public const string EvaPropellant = "EVA Propellant";

  /// <summary>Xenon gas resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Xenon_gas">
  /// KSP Wiki: Xenon gas</seealso>
  public const string XenonGas = "XenonGas";

  /// <summary>Ore resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Ore">
  /// KSP Wiki: Ore</seealso>
  public const string Ore = "Ore";

  /// <summary>Ablator resource name.</summary>
  /// <seealso href="http://wiki.kerbalspaceprogram.com/wiki/Ablator">
  /// KSP Wiki: Ablator</seealso>
  public const string Ablator = "Ablator";

  /// <summary>Returns an ID for the specified resource name.</summary>
  /// <remarks>This ID can be used in the methods that can only work with IDs.</remarks>
  /// <param name="resourceName">The name of the stock resource.</param>
  /// <returns>An ID of the resource.</returns>
  /// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
  public static int GetId(string resourceName) {
    return resourceName.GetHashCode();
  }

  /// <summary>Returns a user friendly name of the resource.</summary>
  /// <remarks>
  /// This could be a rather expensive call. Cach the result if the timing is critical.
  /// </remarks>
  /// <param name="resourceName">The resource common name.</param>
  /// <param name="removeLingoonaTags">
  /// Specifies if any Lingoona tags on the name must be removed. Keep it default if the name is
  /// intended to be used 'as-is". If the name is to be used as a parameter to localizable phrase,
  /// then the tags should be kept.
  /// </param>
  /// <returns>A user friendly string that identifies the resource.</returns>
  /// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
  public static string GetResourceTitle(string resourceName, bool removeLingoonaTags = true) {
    return GetResourceTitle(GetId(resourceName), removeLingoonaTags);
  }

  /// <summary>Returns a user friendly name of the resource.</summary>
  /// <remarks>
  /// This could be a rather expensive call. Cach the result if the timing is critical.
  /// </remarks>
  /// <param name="resourceId">The resource ID.</param>
  /// <param name="removeLingoonaTags">
  /// Specifies if any Lingoona tags on the name must be removed. Keep it default if the name is
  /// intended to be used 'as-is". If the name is to be used as a parameter to localizable phrase,
  /// then the tags should be kept.
  /// </param>
  /// <returns>A user friendly string that identifies the resource.</returns>
  /// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
  public static string GetResourceTitle(int resourceId, bool removeLingoonaTags = true) {
    var res = PartResourceLibrary.Instance.GetDefinition(resourceId);
    return res == null
        ? "Res#" + resourceId
        : removeLingoonaTags ? Localizer.Format("<<1>>", res.displayName) : res.displayName;
  }

  /// <summary>Returns a user friendly name of the resource bsort name (abbreviation).</summary>
  /// <remarks>
  /// If the abbreviation is not set for the resource, then the first 3 letters of its display name
  /// are returned.
  /// </remarks>
  /// <param name="resourceName">The resource common name.</param>
  /// <param name="removeLingoonaTags">
  /// Specifies if any Lingoona tags on the name must be removed. Keep it default if the name is
  /// intended to be used 'as-is". If the name is to be used as a parameter to localizable phrase,
  /// then the tags should be kept.
  /// </param>
  /// <returns>A user friendly string that identifies the resource.</returns>
  /// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
  public static string GetResourceAbbreviation(
      string resourceName, bool removeLingoonaTags = true) {
    return GetResourceAbbreviation(GetId(resourceName));
  }

  /// <summary>Returns a user friendly name of the resource bsort name (abbreviation).</summary>
  /// <remarks>
  /// If the abbreviation is not set for the resource, then the first 3 letters of its display name
  /// are returned.  This could be a rather expensive call. Cach the result if the timing is
  /// critical.
  /// </remarks>
  /// <param name="resourceId">The resource ID.</param>
  /// <param name="removeLingoonaTags">
  /// Specifies if any Lingoona tags on the name must be removed. Keep it default if the name is
  /// intended to be used 'as-is". If the name is to be used as a parameter to localizable phrase,
  /// then the tags should be kept.
  /// </param>
  /// <returns>A user friendly string that identifies the resource.</returns>
  /// <code source="Examples/ResourceUtils/StockResourceNames-Examples.cs" region="StockResourceNames1"/>
  public static string GetResourceAbbreviation(int resourceId, bool removeLingoonaTags = true) {
    var res = PartResourceLibrary.Instance.GetDefinition(resourceId);
    if (res == null) {
      return "Res#" + resourceId;
    } else {
      var str = res.abbreviation.Length > 0 ? res.abbreviation : res.displayName.Substring(0, 3);
      return removeLingoonaTags ? Localizer.Format("<<1>>", str) : str;
    }
  }
}

}  // namepsace
