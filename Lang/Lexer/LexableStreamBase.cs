using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang
{
    public class LexableStreamBase<T> where T : class
    {
        public LexableStreamBase(Func<List<T>> extractor)
        {
            Index = 0;

            Items = extractor();

            SnapshotIndexes = new Stack<int>();
        } 

        private List<T> Items { get; set; }

        protected int Index { get; set; }

        private Stack<int> SnapshotIndexes { get; set; }

        public T Current
        {
            get
            {
                if (EOF(0))
                {
                    return null;
                }

                return Items[Index];
            }
        }

        public void Consume()
        {
            Index++;
        }

        private Boolean EOF(int lookahead)
        {
            if (Index + lookahead >= Items.Count)
            {
                return true;
            }

            return false;
        }

        public Boolean End()
        {
            return EOF(0);
        }

        public virtual T Peek(int lookahead)
        {
            if (EOF(lookahead))
            {
                return null;
            }

            return Items[Index + lookahead];
        }

        public void TakeSnapshot()
        {
            SnapshotIndexes.Push(Index);
        }

        public void RollbackSnapshot()
        {
            Index = SnapshotIndexes.Pop();
        }

        public void CommitSnapshot()
        {
            SnapshotIndexes.Pop();
        }
    }
}
