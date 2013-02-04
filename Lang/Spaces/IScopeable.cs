using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Spaces
{
    public interface IScopeable<T> where T : class, new()
    {
        void SetParentScope(T scope);
        List<IScopeable<T>> ChildScopes { get; } 
    }
}
