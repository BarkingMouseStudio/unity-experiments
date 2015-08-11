using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using size_t = System.UIntPtr;

namespace Neural {

  public class Network : IDisposable {

    [DllImport("libneural")]
    private static extern IntPtr CreateNetwork(size_t  maxDelay);

    [DllImport("libneural")]
    private static extern void DestroyNetwork(IntPtr network);

    [DllImport("libneural")]
    private static extern size_t GetNeuronCount(IntPtr network);

    [DllImport("libneural")]
    private static extern size_t GetSynapseCount(IntPtr network);

    [DllImport("libneural")]
    private static extern size_t AddNeuron(IntPtr network, IzhikevichConfig config);

    [DllImport("libneural")]
    private static extern size_t AddSynapse(IntPtr network, size_t sendrId, size_t recvrId, STDPConfig config);

    [DllImport("libneural")]
    private static extern double TickNetwork(IntPtr network, size_t ticks, IntPtr inputsPtr, IntPtr spikesPtr);

    private IntPtr ptr;
    private bool disposed;

    public Network(ulong maxDelay) {
      ptr = CreateNetwork(new size_t(maxDelay));
      Assert.AreNotEqual(ptr, IntPtr.Zero, "Failed to initialize the neural network.");
    }

    ~Network() {
      Dispose(false);
    }

    public void Dispose() {
      if (!disposed) {
        Dispose(true);
        GC.SuppressFinalize(this);
        disposed = true;
      }
    }

    protected virtual void Dispose(bool disposing) {
      // if (disposing) { } // Clean up managed resources
      DestroyNetwork(ptr); // Clean up unmanaged resources
    }

    public ulong NeuronCount {
      get {
        return (ulong)GetNeuronCount(ptr);
      }
    }

    public ulong SynapseCount {
      get {
        return (ulong)GetSynapseCount(ptr);
      }
    }

    public ulong AddNeuron(IzhikevichConfig config) {
      return (ulong)AddNeuron(ptr, config);
    }

    public ulong AddSynapse(ulong sendrId, ulong recvrId, STDPConfig config) {
      return (ulong)AddSynapse(ptr, new size_t(sendrId), new size_t(recvrId), config);
    }

    public unsafe double Tick(ulong ticks, double[] inputs, ref double[] outputs) {
      // Grab a pointer to the first element
      fixed (double* inputsPtr = inputs, outputsPtr = outputs) {
        // Convert double* to IntPtr to avoid `unsafe`
        return TickNetwork(ptr, new size_t(ticks), (IntPtr)inputsPtr, (IntPtr)outputsPtr);
      }
    }
  }
}
