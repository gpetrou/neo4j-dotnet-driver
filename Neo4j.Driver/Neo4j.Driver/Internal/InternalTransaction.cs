﻿using System;
using System.Collections.Generic;
using Neo4j.Driver.Exceptions;
using Neo4j.Driver.Internal.result;

namespace Neo4j.Driver
{
    public class InternalTransaction : ITransaction
    {
        private State _state = State.Active;
        private readonly IConnection _connection;
        public bool Finished { get; private set; }

        public InternalTransaction(IConnection connection)
        {
            _connection = connection;
            Finished = false;

            _connection.Run(null, "BEGIN");
            _connection.DiscardAll();
        }

        private enum State
        {
            /** The transaction is running with no explicit success or failure marked */
            Active,

            /** Running, user marked for success, meaning it'll value committed */
            MarkedSuccess,

            /** User marked as failed, meaning it'll be rolled back. */
            MarkedFailed,

            /**
             * An error has occurred, transaction can no longer be used and no more messages will be sent for this
             * transaction.
             */
            Failed,

            /** This transaction has successfully committed */
            Succeeded,

            /** This transaction has been rolled back */
            RolledBack
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
            {
                return;
            }
            try
            {
                if (_state == State.MarkedSuccess)
                {
                    _connection.Run(null, "COMMIT");
                    _connection.DiscardAll();
                    _connection.Sync();
                    _state = State.Succeeded;
                }
                else if (_state == State.MarkedFailed || _state == State.Active)
                {
                    // If alwaysValid of the things we've put in the queue have been sent off, there is no need to
                    // do this, we could just clear the queue. Future optimization.
                    _connection.Run(null, "ROLLBACK");
                    _connection.DiscardAll();
                    _state = State.RolledBack;
                }
            }
            finally
            {
                Finished = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ResultCursor Run(string statement, IDictionary<string, object> statementParameters = null)
        {
            EnsureNotFailed();

            try
            {
                ResultBuilder resultBuilder = new ResultBuilder();
                _connection.Run(resultBuilder, statement, statementParameters);
                _connection.PullAll(resultBuilder);
                _connection.Sync();
                return resultBuilder.Build();
            }
            catch (Neo4jException e)
            {
                _state = State.Failed;
                throw;
            }
        }

        private void EnsureNotFailed()
        {
            if (_state == State.Failed)
            {
                throw new ClientException(
                    "Cannot run more statements in this transaction, because previous statements in the " +
                    "transaction has failed and the transaction has been rolled back. Please start a new" +
                    " transaction to run another statement."
                );
            }
        }

        public void Success()
        {
            if (_state == State.Active)
            {
                _state = State.MarkedSuccess;
            }
        }

        public void Failure()
        {
            if (_state == State.Active || _state == State.MarkedSuccess)
            {
                _state = State.MarkedFailed;
            }
        }
    }
}