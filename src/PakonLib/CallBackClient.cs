// Pakon.CallBackClient
using System.Threading;
using PakonLib;

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
        WorkerThreadOperation operation = (WorkerThreadOperation)lOperation;
        m_csScanner.TLXAwake(operation, lStatus);
    }
}
