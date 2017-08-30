using System;
using System.Threading.Tasks;

namespace WetPicsTelegramBot.Helpers
{
    static class FlowExtensions
    {
        public static void Repeat(Action action, 
                                  Action<Exception> exceptionStepHandler = null, 
                                  Action<Exception> exceptionLastHandler = null, 
                                  int repeatTimes = 5)
        {
            int counter = 0;

            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception e)
                {
                    counter++;

                    if (counter == repeatTimes)
                    {
                        exceptionLastHandler?.Invoke(e);
                        return;
                    }

                    exceptionStepHandler?.Invoke(e);
                }
            }
        }

        public static async Task RepeatAsync(Func<Task> action,
                                             Action<Exception> exceptionStepHandler = null,
                                             Action<Exception> exceptionLastHandler = null,
                                             int repeatTimes = 5)
        {
            int counter = 0;

            while (true)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception e)
                {
                    counter++;

                    if (counter == repeatTimes)
                    {
                        exceptionLastHandler?.Invoke(e);
                        return;
                    }

                    exceptionStepHandler?.Invoke(e);
                }
            }
        }
    }
}
