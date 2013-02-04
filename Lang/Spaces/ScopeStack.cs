using System.Collections.Generic;

namespace Lang.Spaces
{
    public class ScopeStack<T> where T : class, IScopeable<T>, new()
    {
        private Stack<T> Stack { get; set; }

        public T Current { get; private set; }

        public ScopeStack()
        {
            Stack = new Stack<T>();
        }

        public void CreateScope()
        {
            var parentScope = Current;

            if (Current != null)
            {
                Stack.Push(Current);
            }

            Current = new T();

            Current.SetParentScope(parentScope);

            if (parentScope != null)
            {
                parentScope.ChildScopes.Add(Current);
            }
        }

        public void PopScope()
        {
            if (Stack.Count > 0)
            {
                Current = Stack.Pop();
            }
        }
    }
}
