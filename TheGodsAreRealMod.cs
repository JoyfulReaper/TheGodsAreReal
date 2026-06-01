using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheGodsAreReal
{
    [StaticConstructorOnStartup]
    public static class GodsChecker
    {
        static GodsChecker()
        {
            Log.Message("The Gods Are Real: Successfully loaded and initialized.");
        }
    }
}
