using System;
using System.Threading;


namespace GameServer
{
    public class Driver
    {
        private static bool _isRunning = false;

        public static void Main(string[] args)
        {
            Console.Title = "Game Server";
            _isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(60, 26951); // Find unused port 
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main Thrad had started. Running at {Constants.TICKS_PER_SECOND} ticks per second");
            DateTime nextLoop = DateTime.Now;

            while(_isRunning)
            {
                while(nextLoop<DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    //Save on CPU usage
                    if(nextLoop > DateTime.Now)
                       Thread.Sleep(nextLoop - DateTime.Now);
                }
            }
        }
    }




}
