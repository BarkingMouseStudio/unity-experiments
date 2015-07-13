using UnityEngine;

public class TransformLayout {

  private readonly float horizontalSpacing;
  private readonly float verticalSpacing;
  private readonly int stride;

  private int i = 0;

  private Vector3 offset;

  public TransformLayout(float horizontalSpacing, float verticalSpacing, int count, int stride) {
    this.horizontalSpacing = horizontalSpacing;
    this.verticalSpacing = verticalSpacing;
    this.stride = stride != 0 ? stride : count;

    var width = (stride - 1) * horizontalSpacing;
    var height = ((count / stride) - 1) * verticalSpacing;
    this.offset = new Vector3(-width / 2, -height / 2, 0);
  }

  public Vector3 NextPosition() {
    var x = i % stride;
    var y = Mathf.Floor(i / stride);
    i++;

    return offset + new Vector3(x * horizontalSpacing, y * verticalSpacing, 0);
  }
}
