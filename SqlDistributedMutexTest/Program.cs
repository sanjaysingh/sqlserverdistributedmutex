using SqlDistributedMutexPattern;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SqlDistributedMutexTest
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (var distLock = SqlDistributedMutex.TryAcquire(ConfigurationManager.ConnectionStrings["Test"].ConnectionString, "MyLock"))
                {
                    if (distLock != null)
                    {
                        while (true)
                        {
                            Console.WriteLine("Doing job after acquiring lock...");
                            Task.Delay(1000).Wait();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Waiting for the lock to be available..just chilling out intil then..");
                    }
                }
                Task.Delay(1000).Wait();
            }
        }

    }
}
