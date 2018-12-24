﻿// Kerbal Development tools - Examples
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using KSPDev.GUIUtils;
using KSPDev.GUIUtils.TypeFormatters;
using UnityEngine;

namespace Examples {

#region DistanceTypeDemo1
public class DistanceTypeDemo1 : PartModule {
  static readonly Message<DistanceType> msg1 = new Message<DistanceType>(
      "#TypeDemo_msg1", defaultTemplate: "Distance is: <<1>>");

  // Depending on the current language in the system, this method will present different unit names. 
  void Show() {
    Debug.Log(msg1.Format(0.051));
    // Prints: "Distance is: 0.051 m"
    Debug.Log(msg1.Format(0.45));
    // Prints: "Distance is: 0.45 m"
    Debug.Log(msg1.Format(95.45));
    // Prints: "Distance is: 95.5 m"
    Debug.Log(msg1.Format(120.45));
    // Prints: "Distance is: 121 m"
    Debug.Log(msg1.Format(9535.45));
    // Prints: "Distance is: 9536 m"
    Debug.Log(msg1.Format(12345.45));
    // Prints: "Distance is: 12.5 km"
    Debug.Log(msg1.Format(123456.45));
    // Prints: "Distance is: 123457 km"
  }
}
#endregion

public class DistanceTypeDemo2 {
  void FormatDefault() {
    #region DistanceTypeDemo2_FormatDefault
    Debug.Log(DistanceType.Format(0.051));
    // Prints: "0.051 m"
    Debug.Log(DistanceType.Format(0.45));
    // Prints: "0.45 m"
    Debug.Log(DistanceType.Format(95.45));
    // Prints: "95.5 m"
    Debug.Log(DistanceType.Format(120.45));
    // Prints: "121 m"
    Debug.Log(DistanceType.Format(9535.45));
    // Prints: "9536 m"
    Debug.Log(DistanceType.Format(12345.45));
    // Prints: "12.5 km"
    Debug.Log(DistanceType.Format(123456.45));
    // Prints: "123457 km"
    #endregion
  }

  void FormatWithScale() {
    #region DistanceTypeDemo2_FormatWithScale
    Debug.Log(DistanceType.Format(123456.56, scale: 1000));
    // Prints: "123.6 km"
    Debug.Log(DistanceType.Format(123456.56, scale: 1));
    // Prints: "123456.6 m"
    Debug.Log(DistanceType.Format(123456.56, scale: 10));
    // Scale 10 is not known, so it's rounded down to 1.
    // Prints: "123456.6 m"
    Debug.Log(DistanceType.Format(123.56, scale: 1000));
    // Prints: "0.1 km"
    #endregion
  }

  void FormatFixed() {
    #region DistanceTypeDemo2_FormatFixed
    Debug.Log(DistanceType.Format(1234.5678, format: "0.0000"));
    // Prints: "1234.5678 m"
    Debug.Log(DistanceType.Format(1234.5678, format: "0.00"));
    // Prints: "1234.57 m"
    Debug.Log(DistanceType.Format(1234.5678, format: "0.0000", scale: 1000));
    // Prints: "1.2346 km"
    Debug.Log(DistanceType.Format(1234.5678, format: "0.00", scale: 1000));
    // Prints: "1.24 km"
    #endregion
  }
}

}  // namespace
