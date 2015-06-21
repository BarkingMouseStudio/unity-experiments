using System;
using System.Collections;
using NetMQ;
using NetMQ.zmq;

public static class NetMQUtils {

  public static IEnumerator WaitForResponse(this IReceivingSocket socket, Action<string> callback) {
    string frameString = "";

    while (true) {
      try {
        frameString = socket.ReceiveString(SendReceiveOptions.DontWait);
        break;
      } catch {}

      yield return null;
    }

    callback(frameString);
  }
}
