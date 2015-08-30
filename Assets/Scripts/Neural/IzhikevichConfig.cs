using System;
using System.Runtime.InteropServices;

namespace Neural {

  public struct IzhikevichConfig {
    public double v;
    public double u;
    public double a;
    public double b;
    public double c;
    public double d;
    public double e;
    public double f;
    public bool is_accomodation;

    [DllImport("libneural")]
    private static extern IzhikevichConfig GetIzhikevichConfig();

    public static IzhikevichConfig Of(float a, float b, float c, float d) {
      var config = GetIzhikevichConfig();
      config.a = a;
      config.b = b;
      config.c = c;
      config.d = d;
      config.v = config.c;
      config.u = config.b * config.c;
      return config;
    }

    public static IzhikevichConfig Default() {
      return GetIzhikevichConfig();
    }
  }
}
