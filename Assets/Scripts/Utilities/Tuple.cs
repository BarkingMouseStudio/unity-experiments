public class Tuple<T1, T2> {

  public T1 First { get; private set; }
  public T2 Second { get; private set; }

  public Tuple(T1 first, T2 second) {
    First = first;
    Second = second;
  }
}

public class Tuple<T1, T2, T3> {

  public T1 First { get; private set; }
  public T2 Second { get; private set; }
  public T3 Third { get; private set; }

  public Tuple(T1 first, T2 second, T3 third) {
    First = first;
    Second = second;
    Third = third;
  }
}

public static class Tuple {

  public static Tuple<T1, T2> Of<T1, T2>(T1 first, T2 second) {
    return new Tuple<T1, T2>(first, second);
  }

  public static Tuple<T1, T2, T3> Of<T1, T2, T3>(T1 first, T2 second, T3 third) {
    return new Tuple<T1, T2, T3>(first, second, third);
  }
}
