using System;
using System.Threading;

namespace KrolStudio
{
    public class CtsHelper : IDisposable
    {
        private CancellationTokenSource _cts;

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        /// <summary>
        /// Returns true if the token is already canceled.
        /// </summary>
        public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

        /// <summary>
        /// Cancels the current token and creates a new one.
        /// </summary>
        public void Restart()
        {
            Cancel();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancels and disposes the current token.
        /// </summary>
        public void Cancel()
        {
            if (_cts == null)
                return;

            if (!_cts.IsCancellationRequested)
                _cts.Cancel();

            _cts.Dispose();
            _cts = null;
        }

        /// <summary>
        /// Simply creates the token if it has not been created yet.
        /// </summary>
        public void EnsureCreated()
        {
            if (_cts == null)
                _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Deletes and cleans up the token.
        /// </summary>
        public void Dispose()
        {
            Cancel();
        }
    }
}
