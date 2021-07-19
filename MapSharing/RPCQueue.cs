using System.Collections.Generic;
using BepInEx;

namespace VMP_Mod.MapSharing
{
    public class RpcData
    {
        public string Name;
        public object[] Payload;
        public long Target = ZRoutedRpc.Everybody;
    }

    public static class RpcQueue
    {
        private static readonly Queue<RpcData> RPCQueue = new Queue<RpcData>();
        private static bool _ack = true;

        public static void Enqueue(RpcData rpc)
        {
            RPCQueue.Enqueue(rpc);
        }

        public static bool SendNextRpc()
        {
            if (RPCQueue.Count == 0 || !_ack) return false;

            var rpc = RPCQueue.Dequeue();

            if (rpc.Name.IsNullOrWhiteSpace() ||
                rpc.Payload == null)
                return false;

            ZRoutedRpc.instance.InvokeRoutedRPC(rpc.Target, rpc.Name, rpc.Payload);
            _ack = false;

            return true;
        }

        public static void GotAck()
        {
            _ack = true;
        }
    }
}