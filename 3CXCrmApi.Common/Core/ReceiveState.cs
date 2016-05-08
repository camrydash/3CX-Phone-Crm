namespace Crm.Integration.Common
{
    internal class ReceiveState
    {
        public byte[] buffer = new byte[1024];
        public int workerSocketIndex;
    }
}
