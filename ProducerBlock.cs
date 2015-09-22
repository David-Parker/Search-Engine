using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchBackend
{
    public static class ProducerBlock
    {
        private static Object block;
        public static int high = 100000;
        public static int low = 90000;

        // Singleton
        public static Object GetReference()
        {
            if(block == null)
            {
                block = new object();
            }

            return block;
        }
    }
}
