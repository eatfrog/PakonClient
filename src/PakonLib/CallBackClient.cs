// Pakon.CallBackClient
using System.Threading;
using PakonLib;

public class CallBackClient : ICallBackClient
{
    private Scanner scanner;

    public CallBackClient(Scanner scannerInstance)
    {
        scanner = scannerInstance;
    }

    public void Awake(int operationValue, int status)
    {
        if (status == 3000)
        {
            Thread.Sleep(300);
        }
        WorkerThreadOperation operation = (WorkerThreadOperation)operationValue;
        scanner.TLXAwake(operation, status);
    }
}
