using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Encryption;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CTP;
using Microsoft.Extensions.Hosting;

namespace QuantStudio.CTP.Data
{
    /// <summary>
    /// CTP行情数据接收器
    /// </summary>
    public class CTPMdReceiver : ISingletonDependency
    {
        #region private / property

        private CTPSettings _cTPSettings;
        private FtdcMdAdapter DataApi = null;
        private int iRequestID = 0;
        private bool _isConnected = false;
        private List<string> _subscribeInstrumentIDs = new List<string>() { };

        #endregion

        public ILogger<CTPMdReceiver> Logger { get; set; }

        private Action<CTPSettings> _doSettings = null;

        public event EventHandler<HeartBeatEventArgs> OnHeartBeatEvent;
        public event EventHandler<DepthMarketDataArgs> OnDepthMarketDataEvent;
        public event EventHandler<CloseTradingArgs> OnCloseTradingEvent;

        #region ctor

        public CTPMdReceiver()
        {
            Logger = NullLogger<CTPMdReceiver>.Instance;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="futureSettings"></param>
        public void Initialize(CTPSettings cTPSettings, Action<CTPSettings> actionDoSettings)
        {
            // 初始化数据目录

            _cTPSettings = cTPSettings;
            _doSettings = actionDoSettings;
        }

        #endregion

        private async Task _defaultHandlerHeartBeatEvent(object? sender, HeartBeatEventArgs e)
        {
            //while (true)
            //{
            //    await Task.Delay(TimeSpan.FromMinutes(3));

            //    if (!_isConnected)
            //    {
            //        await DoConnect();
            //    }
            //}
        }

        private bool IsError(ThostFtdcRspInfoField rspinfo, string source)
        {
            if (rspinfo != null && rspinfo.ErrorID != 0)
            {
                Logger.LogInformation(rspinfo.ErrorMsg + ", 来源 " + source);
                return true;
            }
            return false;
        }

        private void DataApi_OnFrontEvent(object sender, OnFrontEventArgs e)
        {
            switch (e.EventType)
            {
                case EnumOnFrontType.OnFrontConnected:
                    {
                        var req = new ThostFtdcReqUserLoginField();
                        req.BrokerID = _cTPSettings.Investor.BrokerID;
                        req.UserID = _cTPSettings.Investor.UserID;
                        req.Password = _cTPSettings.Investor.Password; 
                        int iResult = DataApi.ReqUserLogin(req, ++iRequestID);
                    }
                    break;
                case EnumOnFrontType.OnFrontDisconnected:
                    {
                        _isConnected = false;
                        Logger.LogInformation("CTP OnFrontDisconnected");
                    }
                    break;
            }
        }

        private void DataApi_OnRtnEvent(object sender, OnRtnEventArgs e)
        {
            var fld = Conv.P2S<ThostFtdcDepthMarketDataField>(e.Param);
            if (fld != null)
            {
                if(!_isConnected)
                {
                    _isConnected = true;
                }

                Logger.LogInformation("{0}.{1:D3} {2} {3}", fld.UpdateTime, fld.UpdateMillisec, fld.InstrumentID, fld.LastPrice);
                OnDepthMarketDataEvent?.Invoke(this, new DepthMarketDataArgs(fld));
            }
        }

        private async void DataApi_OnRspEvent(object sender, OnRspEventArgs e)
        {
            bool err = IsError(e.RspInfo, e.EventType.ToString());
            switch (e.EventType)
            {
                case EnumOnRspType.OnRspUserLogin:
                    if (!err)
                    {
                        Logger.LogInformation("登录成功");

                        // 心跳事件
                        //HeartBeatEventArgs eventArgs = new HeartBeatEventArgs();
                        //if (OnHeartBeatEvent != null)
                        //{
                        //    // Call to raise the event.
                        //    OnHeartBeatEvent(this, eventArgs);
                        //}
                        //else
                        //{
                        //    await _defaultHandlerHeartBeatEvent(this, eventArgs);
                        //}

                        await SubscribeMarketData(_subscribeInstrumentIDs);
                    }
                    break;
                case EnumOnRspType.OnRspSubMarketData:
                    {
                        var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                        if (f != null && !f.InstrumentID.IsNullOrEmpty())
                        {
                            Logger.LogInformation("订阅成功:" + f.InstrumentID);
                        }
                    }
                    break;
                case EnumOnRspType.OnRspUnSubMarketData:
                    {
                        var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                        if (f != null)
                        {
                            Logger.LogInformation("退订成功:" + f.InstrumentID);
                        }
                    }
                    break;

            }
        }

        private async Task SubscribeMarketData(List<string> instrumentIDs)
        {
            if (DataApi != null)
            {
                if (instrumentIDs.Any())
                {
                    DataApi.SubscribeMarketData(instrumentIDs.ToArray());
                }
                else
                {
                    DataApi.SubscribeMarketData(_subscribeInstrumentIDs.ToArray());
                }
            }
            await Task.CompletedTask;
        }

        private async Task UnSubscribeMarketData(List<string> instrumentIDs)
        {
            if (DataApi != null)
            {
                if (instrumentIDs.Any())
                {
                    DataApi.UnSubscribeMarketData(instrumentIDs.ToArray());
                }
                else
                {
                    DataApi.UnSubscribeMarketData(_subscribeInstrumentIDs.ToArray());
                }
            }
            await Task.CompletedTask;
        }

        private async Task DoConnect()
        {
            if (DataApi == null)
            {
                DataApi = new FtdcMdAdapter("", false, false);
            }

            DataApi.OnFrontEvent += DataApi_OnFrontEvent;
            DataApi.OnRspEvent += DataApi_OnRspEvent;
            DataApi.OnRtnEvent += DataApi_OnRtnEvent;

            DataApi.RegisterFront(_cTPSettings.Investor.MdFrontAddr);
            DataApi.Init();

            await Task.CompletedTask;
        }

        private async Task DoDisconnect()
        {
            if (DataApi != null)
            {
                DataApi.Dispose();
                DataApi = null;
                Logger.LogInformation("Disconnected.");
            }

            await Task.CompletedTask;
        }

        #region public

        public bool IsConnected { get { return DataApi != null && _isConnected; } }

        public async Task Connect()
        {
            _doSettings?.Invoke(_cTPSettings);

            await DoConnect();
        }

        public async Task Disconnect()
        {
            await DoDisconnect();

            OnCloseTradingEvent?.Invoke(this, new CloseTradingArgs() { });
        }

        public void SetsubscribeInstrumentID(List<string> subscribeInstrumentIDs)
        {
            _subscribeInstrumentIDs = subscribeInstrumentIDs.ToList();
        }

        #endregion
    }
}
