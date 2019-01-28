// Kerbal Development tools.
// Author: igor.zavoychinskiy@gmail.com
// This software is distributed under Public domain license.

using System;
using System.Diagnostics;

namespace KSPDev.DebugUtils {

/// <summary>Simple performance counter to measure CPU cost of the code branch.</summary>
/// <remarks>
/// The counter itself can have a performance impact, but it's not counted towards the methods or
/// function cost being measured. Don't profile branches that have other performance counters, since
/// such measurements will also count the overhead of thouse counters.
/// </remarks>
public sealed class PerfCounter {
  readonly Stopwatch watch = new Stopwatch();

  /// <summary>Total number of readings captured.</summary>
  /// <value>The number of times <see cref="MeasureAction"/> was called.</value>
  public int numSamples { get; private set; }

  /// <summary>Total elapsed milleseconds in all readings.</summary>
  /// <value>The total number of milliseconds, spent in the measured code.</value>
  public double totalDurationMs { get { return watch.ElapsedMilliseconds; } }

  /// <summary>Average time spent per one reading.</summary>
  /// <value>The average time in milliseconds.</value>
  public double avgDurationPerCallMs { get; private set; }

  /// <summary>Maximum time spent in one reading during the life of the counter.</summary>
  /// <value>The maximum duration, taken by the measured code.</value>
  public double maxDurationPerCallMs { get; private set; }

  /// <summary>Minimum time spent in one reading during the life of the counter.</summary>
  /// <value>The minimum duration, taken by the measured code.</value>
  public double minDurationPerCallMs { get; private set; }

  /// <summary>Measures timing in a simple action that doesn't return result.</summary>
  /// <param name="fn">The action to measure.</param>
  public void MeasureAction(Action fn) {
    numSamples += 1;
    watch.Start();
    fn();
    watch.Stop();
    avgDurationPerCallMs = (double)watch.ElapsedMilliseconds / numSamples;
    maxDurationPerCallMs = Math.Max(maxDurationPerCallMs, avgDurationPerCallMs);
    minDurationPerCallMs = Math.Min(minDurationPerCallMs, avgDurationPerCallMs);
  }

  /// <summary>Measures timing in a function that returns result.</summary>
  /// <param name="fn">The function to measure.</param>
  /// <typeparam name="RetVal">The return parameter of the measured function.</typeparam>
  /// <returns>The value from the measured function.</returns>
  public RetVal MeasureFunction<RetVal>(Func<RetVal> fn) {
    numSamples += 1;
    watch.Start();
    var res = fn();
    watch.Stop();
    avgDurationPerCallMs = (double)watch.ElapsedMilliseconds / numSamples;
    maxDurationPerCallMs = Math.Max(maxDurationPerCallMs, avgDurationPerCallMs);
    minDurationPerCallMs = Math.Min(minDurationPerCallMs, avgDurationPerCallMs);
    return res;
  }
}

}  // namespace
