using System;
using System.Runtime.InteropServices;

namespace Neural {

  public struct SymConfig {
    public float weight;
    public float a_sym;
    public float tau_a;
    public float tau_b;
    public UIntPtr delay;
    public float min;
    public float max;

    [DllImport("libneural")]
    private static extern SymConfig GetSymConfig();

    public static SymConfig Of(float weight, float min, float max) {
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
