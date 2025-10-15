// Pakon.CallBackClient
using System;
using System.Threading;
using PakonLib;
using TLXLib;
using System.Runtime.InteropServices;

namespace PakonLib
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class CallBackClient : StandardOleMarshalObject, ICallBackClient
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
}
