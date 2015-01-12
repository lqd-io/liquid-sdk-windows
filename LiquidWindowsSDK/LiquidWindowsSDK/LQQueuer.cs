using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using LiquidWindowsSDK.Model;

namespace LiquidWindowsSDK
{
    public class LQQueuer
    {
        internal const int LIQUID_QUEUE_SIZE_LIMIT = 500;
        internal const int LIQUID_DEFAULT_FLUSH_INTERVAL = 15;
        internal const int LIQUID_MAX_NUMBER_OF_TRIES = 10;

        internal int _flushInterval;
        internal List<LQNetworkRequest> _httpQueue;
        internal DispatcherTimer _timer;
        internal String _apiToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="LQQueuer"/> class.
        /// </summary>
        /// <param name="token">The api token.</param>
        public LQQueuer(String token)
            : this(token, new List<LQNetworkRequest>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LQQueuer"/> class.
        /// </summary>
        /// <param name="token">The api token.</param>
        /// <param name="queue">The network requests queue.</param>
        public LQQueuer(String token, List<LQNetworkRequest> queue)
        {
            _httpQueue = queue;
            _apiToken = token;
        }

        /// <summary>
        /// Adds an event to the HTTP queue.
        /// </summary>
        /// <param name="queuedEvent">The event to be queued.</param>
        /// <returns>Whether an event had to be removed from the queue.</returns>
        public bool AddToHttpQueue(LQNetworkRequest queuedEvent)
        {
            _httpQueue.Add(queuedEvent);
            if (_httpQueue.Count > LIQUID_QUEUE_SIZE_LIMIT)
            {
                _httpQueue.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the flush timer interval.
        /// </summary>
        /// <param name="seconds">The interval time in seconds.</param>
        public void SetFlushTimer(int seconds)
        {
            StopFlushTimer();
            _flushInterval = seconds;
            StartFlushTimer();
        }

        /// <summary>
        /// Gets the flush timer.
        /// </summary>
        /// <value>
        /// The flush timer.
        /// </value>
        public int FlushTimer
        {
            get { return _flushInterval; }
        }

        /// <summary>
        /// Gets the Network Requests queue.
        /// </summary>
        /// <value>
        /// The queue.
        /// </value>
        public List<LQNetworkRequest> Queue
        {
            get { return _httpQueue; }
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public async Task Flush()
        {
            try
            {
                if (LiquidTools.IsNetworkAvailable())
                {
                    DateTime now = DateTime.Now;
                    var failedQueue = new List<LQNetworkRequest>();
                    while (_httpQueue.Count > 0)
                    {
                        LQNetworkRequest queuedHttp = _httpQueue[0];
                        _httpQueue.RemoveAt(0);
                        if (queuedHttp.CanFlush(now))
                        {
                            LQLog.InfoVerbose("Flushing " + queuedHttp);
                            LQNetworkResponse result = await queuedHttp.SendRequest(_apiToken);
                            if (!result.HasSucceeded())
                            {
                                LQLog.Error("HTTP (" + result.HttpCode + ") " + queuedHttp);
                                if (queuedHttp.NumberOfTries < LIQUID_MAX_NUMBER_OF_TRIES)
                                {
                                    if (!result.HasForbidden())
                                    {
                                        queuedHttp.LastTry = now;
                                    }
                                    queuedHttp.IncrementNumberOfTries();
                                    failedQueue.Add(queuedHttp);
                                }
                            }
                        }
                        else
                        {
                            failedQueue.Add(queuedHttp);
                        }
                    }
                    _httpQueue.AddRange(failedQueue);
                    await LQNetworkRequest.SaveQueue(_httpQueue, _apiToken);
                }
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error flushing data", ex.ToString());
            }
        }

        /// <summary>
        /// Starts the flush timer.
        /// </summary>
        public void StartFlushTimer()
        {
            try
            {
                if (_timer != null)
                {
                    return;
                }
                if (_flushInterval <= 0)
                {
                    _flushInterval = LIQUID_DEFAULT_FLUSH_INTERVAL;
                }
                _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, _flushInterval) };
                _timer.Tick += async (sender, o) =>
                {
                    await Flush();
                };
                _timer.Start();
                LQLog.InfoVerbose("Started Flush timer");
            }
            catch (Exception ex)
            {
                LiquidTools.LogUnexpectedException("Unexpected error starting flush timer", ex.ToString());
            }
        }

        /// <summary>
        /// Stops the flush timer.
        /// </summary>
        public void StopFlushTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
                LQLog.InfoVerbose("Stopped Flush timer");
            }
        }
    }
}
