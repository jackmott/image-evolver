using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    public static class Settings
    {
        public const int MIN_GEN_SIZE = 1;
        public const int MAX_GEN_SIZE = 20;
        public const int POP_SIZE = 36;

        //note min gradients cannot be less than 2
        public const int MIN_GRADIENTS = 2;
        public const int MAX_GRADIENTS = 20;
        public const float HORIZONTAL_SPACING = .01f;
        public const float VERTICAL_SPACING = .01f;
    }
}
