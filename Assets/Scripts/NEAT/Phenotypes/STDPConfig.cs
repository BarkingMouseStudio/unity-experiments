using System;
using System.Runtime.InteropServices;

namespace Neural {

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
    public bool continuous;
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
