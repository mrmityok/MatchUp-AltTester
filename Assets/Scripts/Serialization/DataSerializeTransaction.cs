using System;
using System.Collections.Generic;

namespace Serialization
{
    public class DataSerializeTransaction : IDataSerializeTransaction
    {
        private List<Action> commitCallbacks = new List<Action>();
        private List<Action> rollbackCallbacks = new List<Action>();
        private List<Action> commitedCallbacks = new List<Action>();
        private List<Action> rollbackedCallbacks = new List<Action>();

        private bool closed = false;
        private bool applyed = false;

        ~DataSerializeTransaction()
        {
            Rollback();
        }

        bool IDataSerializeTransaction.Commit()
        {
			for (int i = 0; i < commitCallbacks.Count; i++)
			{
				var handler = commitCallbacks[i];
				if (handler != null)
					handler();
			}

			for (int i = 0; i < commitedCallbacks.Count; i++)
			{
				var handler = commitedCallbacks[i];
				if (handler != null)
					handler();
			}

            commitCallbacks.Clear();
            rollbackCallbacks.Clear();
            commitedCallbacks.Clear();
            rollbackedCallbacks.Clear();

            bool wasClosed = closed;
            closed = true;
            return !wasClosed;
        }

        bool IDataSerializeTransaction.Rollback()
        {
            return Rollback();
        }

        private bool Rollback()
        {
			for (int i = 0; i < rollbackCallbacks.Count; i++)
			{
				var handler = rollbackCallbacks[i];
				if (handler != null)
					handler();
			}

			for (int i = 0; i < rollbackedCallbacks.Count; i++)
			{
				var handler = rollbackedCallbacks[i];
				if (handler != null)
					handler();
			}

            commitCallbacks.Clear();
            rollbackCallbacks.Clear();
            commitedCallbacks.Clear();
            rollbackedCallbacks.Clear();

            bool wasClosed = closed;
            closed = true;
            return !wasClosed;
        }

        public bool AddCommitCallback(Action onCommit)
        {
            if (!closed && !applyed && onCommit != null)
            {
				if (!commitCallbacks.Contains(onCommit))
               		commitCallbacks.Add(onCommit);
				
                return true;
            }

            return false;
        }

        public bool AddRollbackCallback(Action onRollback)
        {
            if (!closed && !applyed && onRollback != null)
            {
				if (!rollbackCallbacks.Contains(onRollback))
                	rollbackCallbacks.Add(onRollback);
				
                return true;
            }

            return false;
        }

        public bool AddCommittedCallback(Action onCommitted)
        {
			if (!closed && !applyed && onCommitted != null)
            {
				if (!commitedCallbacks.Contains(onCommitted))
                	commitedCallbacks.Add(onCommitted);
				
                return true;
            }

            return false;
        }

        public bool AddRollbackedCallback(Action onRollbacked)
        {
            if (!closed && !applyed && rollbackedCallbacks != null)
            {
				if (!rollbackedCallbacks.Contains(onRollbacked))
                	rollbackedCallbacks.Add(onRollbacked);

                return true;
            }

            return false;
        }
    }
}