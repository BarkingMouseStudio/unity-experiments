using System;
using System.Runtime.InteropServices;

namespace Neural {

  public struct SymConfig {
    public double weight;
    public double a_sym;
    public double tau_a;
    public double tau_b;
    public UIntPtr delay;
    public double min;
    public double max;

    [DllImport("libneural")]
    private static extern SymConfig GetSymConfig();

    public static SymConfig Of(double weight, double min, double max) {
      var config = GetSymConfig();
      config.weight = weight;
      config.min = min;
      config.max = max;
      return config;
    }

    public static SymConfig Default {
      get {
        return GetSymConfig();
      }
    }
  }
}
