using System;
using System.Runtime.InteropServices;

namespace Neural {

  [StructLayout(LayoutKind.Sequential)]
  public struct STDPConfig {
    public double weight;
    public double min;
    public double max;
    public double n_pos;
    public double n_neg;
    public double tau_pos;
    public double tau_neg;
    public double a_pos;
    public double a_neg;

    [MarshalAs(UnmanagedType.I1)]
    public bool continuous;

    [MarshalAs(UnmanagedType.I1)]
    public bool scale;

    public UIntPtr delay;

    [DllImport("libneural")]
    private static extern STDPConfig GetSTDPConfig();

    public static STDPConfig Of(double weight, double min, double max) {
      var config = GetSTDPConfig();
      config.weight = weight;
      config.min = min;
      config.max = max;
      return config;
    }

    public static STDPConfig Default {
      get {
        return GetSTDPConfig();
      }
    }
  }
}
