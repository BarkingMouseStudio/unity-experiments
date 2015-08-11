using System;
using System.Runtime.InteropServices;

namespace Neural {

  [StructLayout(LayoutKind.Sequential)]
  public struct IzhikevichConfig {
    public double v;
    public double u;
    public double a;
    public double b;
    public double c;
    public double d;
    public double e;
    public double f;

    [MarshalAs(UnmanagedType.I1)]
    public bool is_accomodation;

    [DllImport("libneural")]
    private static extern IzhikevichConfig GetIzhikevichConfig();

    public static IzhikevichConfig Of(float a, float b, float c, float d) {
      var config = GetIzhikevichConfig();
      config.a = a;
      config.b = b;
      config.c = c;
      config.d = d;
      config.v = c;
      config.u = b * c;
      return config;
    }

    public static IzhikevichConfig Default {
      get {
        return GetIzhikevichConfig();
      }
    }
  }
}
