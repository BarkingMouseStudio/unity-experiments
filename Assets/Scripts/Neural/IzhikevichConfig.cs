using System;
using System.Collections;
using System.Collections.Generic;
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

    [MarshalAs(UnmanagedType.I1)]
    public bool is_accomodation;

    [DllImport("libneural")]
    private static extern IzhikevichConfig GetIzhikevichConfig();

    public static IzhikevichConfig Of(double a, double b, double c, double d) {
      var config = GetIzhikevichConfig();
      config.a = a;
      config.b = b;
      config.c = c;
      config.d = d;
      config.v = config.c;
      config.u = config.b * config.c;
      config.e = 5.0;
      config.f = 140.0;
      config.is_accomodation = false;
      return config;
    }

    public static IzhikevichConfig Default() {
      return GetIzhikevichConfig();
    }
  }
}
