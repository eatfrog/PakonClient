// Pakon.CallBackClient
using System.Threading;
using Pakon;
using TLXLib;

public class CallBackClient : ICallBackClient
{
    private Scanner m_csScanner;

    public CallBackClient(Scanner csScanner)
    {
        m_csScanner = csScanner;
    }

    public void Awake(int lOperation, int lStatus)
    {
        if (lStatus == 3000)
        {
            Thread.Sleep(300);
        }
        m_csScanner.TLXAwake((WORKER_THREAD_OPERATION_000 )lOperation, lStatus);
    }
}
