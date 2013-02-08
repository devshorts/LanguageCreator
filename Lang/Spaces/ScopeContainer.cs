namespace Lang.Spaces
{
    public enum ScopeType
    {
        Class,
        Global
    }
    public class ScopeContainer
    {
        public ScopeStack<Scope> Global { get; set; }

        public ScopeStack<Scope> Class { get; set; } 

        public ScopeContainer()
        {
            Global = new ScopeStack<Scope>();

            Class = new ScopeStack<Scope>();

            CurrentScopeType = ScopeType.Global;
        }

        public ScopeType CurrentScopeType { get; set; } 

        public ScopeStack<Scope> CurrentScopeStack
        {
            get
            {
                switch (CurrentScopeType)
                {
                    case ScopeType.Class:
                        return Class;
                    case ScopeType.Global:
                        return Global;
                }

                return null;
            }
        } 
    }
}
