using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLXLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
namespace pakontest3
{
    class Program
    {
        public delegate void Callback();

        static void Timeout()
        {
            Console.WriteLine("Timeout!!");
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MyFunctionDelegate(IntPtr frame);

        [STAThread]
        static void Main(string[] args)
        {

            unsafe
            {
                //var tlx = ActivateClass<TLXMainClass>(Guid.NewGuid(), new Guid("0x79205da0"));
                //var tlx = new TLXMainClass();
                INITIALIZE_CONTROL_000 ctrl = INITIALIZE_CONTROL_000.INITIALIZE_CSharpClient;
                Callback myAction = new Callback(Timeout);

                IntPtr timeoutPtr = Marshal.GetFunctionPointerForDelegate(myAction);


                Type comType = Type.GetTypeFromCLSID(new Guid("EA82986B-E47C-4C0F-97EA-FB50ED216D2E"), true);
                ITLXMain instance = (ITLXMain)Activator.CreateInstance(comType);
                try
                {
                    Console.WriteLine("Press key");
                    Console.ReadKey();
                    IntPtr errors = Marshal.AllocHGlobal(1024);
                    int errorPointer = (int) errors;
                    instance.InitializeScanner((int)ctrl, (int)timeoutPtr);
                    instance.GetInitializeWarnings(ref errorPointer);

                    Console.WriteLine("Scanner inited");
                    IScanPictures scan = (IScanPictures)Activator.CreateInstance(comType);
                    scan.AdvanceFilm(3000, 1016);
                    Thread.Sleep(1000);
                    //tlx.AdvanceFilm(1000, 100);

                }
                catch (Exception ex)
                {
                    var errorCode = Enum.Parse(typeof(ERROR_CODES_000), ex.Message);
                    Console.WriteLine(errorCode);
                }
            }
        }

        public static I ActivateClass<I>(Guid clsid, Guid iid)
        {
            Debug.Assert(iid == typeof(I).GUID);
            object temp;
            int hr = CoCreateInstance(ref clsid, IntPtr.Zero, /*CLSCTX_INPROC_SERVER*/ 5, ref iid, out temp);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return (I)temp;
        }

        [DllImport("Ole32")]
        private static extern int CoCreateInstance(
            ref Guid rclsid,
            IntPtr pUnkOuter,
            int dwClsContext,
            ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out object ppObj);
    }
}
