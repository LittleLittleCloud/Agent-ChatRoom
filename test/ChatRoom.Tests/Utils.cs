using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.Tests;

internal static class Utils
{
    public static Task WaitUntilTrue(Func<bool> condition, int maxSeconds = 10)
    {
        var timeout = TimeSpan.FromSeconds(maxSeconds);
        return Task.Run(() =>
        {
            while (!condition())
            {
                Task.Delay(100).Wait();
                timeout -= TimeSpan.FromMilliseconds(100);

                if (timeout <= TimeSpan.Zero)
                {
                    throw new TimeoutException("Condition was not met within the timeout");
                }
            }
        });
    }
}
