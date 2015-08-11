using System;
using System.Collections;
using System.Collections.Generic;

public class Slicer<T> {

  private readonly T[] array;
  private int offset = 0;

  public Slicer(T[] array) {
    this.array = array;
  }

  public Slice<T> NextSlice(int count) {
    var slice = new Slice<T>(this.array, this.offset, count);
    this.offset += count;
    return slice;
  }
}
