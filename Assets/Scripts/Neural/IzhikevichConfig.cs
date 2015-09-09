using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Neural {

  public struct IzhikevichConfig {
    public float v;
    public float u;
    public float a;
    public float b;
    public float c;
    public float d;
    public float e;
    public float f;

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
      config.v = config.c;
      config.u = config.b * config.c;
      config.e = 5.0f;
      config.f = 140.0f;
      config.is_accomodation = false;
      return config;
    }

    public static IzhikevichConfig Default() {
      return GetIzhikevichConfig();
    }
  }
}
