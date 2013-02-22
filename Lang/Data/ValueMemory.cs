using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Symbols;

namespace Lang.Data
{
    class ValueMemory
    {
        public dynamic Value { get; set; }
        public MemorySpace Memory { get; set; }

        public ValueMemory(dynamic value, MemorySpace memory)
        {
            Value = value;
            Memory = memory;
        }
    }
}
