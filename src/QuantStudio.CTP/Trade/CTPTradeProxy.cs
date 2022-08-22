using CTP;
using Microsoft.Extensions.Logging;
using QLNet;
using QuantStudio.CTP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using static QuantStudio.CTP.CTPConsts;

namespace QuantStudio.CTP.Trade
{
    /// <summary>
    /// CTP
    /// </summary>
    public class CTPTradeProxy : ISingletonDependency
    {
        #region private / property

        private CTPSettings _cTPSettings;
        private FtdcTdAdapter TraderApi = null;
        private int iRequestID = 0;
        private bool _isConnected = false;

        private Action<CTPSettings> _doSettings = null;

        #endregion

        public ILogger<CTPTradeProxy> Logger { get; set; }

        public bool IsConnected { get { return _isConnected; } }

        #region TradeApi

        private bool IsError(ThostFtdcRspInfoField rspinfo, string source)
        {
            if (rspinfo != null && rspinfo.ErrorID != 0)
            {
                Logger.LogInformation(rspinfo.ErrorMsg + ", 来源 " + source);
                return true;
            }
            return false;
        }

        private async Task DoConnect()
        {
            TraderApi = new FtdcTdAdapter("");
            TraderApi.OnFrontEvent += TraderApi_OnFrontEvent;
            TraderApi.OnRspEvent += TraderApi_OnRspEvent;
            TraderApi.OnRtnEvent += TraderApi_OnRtnEvent; ;
            TraderApi.OnErrRtnEvent += TraderApi_OnErrRtnEvent;
            TraderApi.SubscribePublicTopic(EnumTeResumeType.THOST_TERT_QUICK);
            TraderApi.SubscribePrivateTopic(EnumTeResumeType.THOST_TERT_QUICK);
            TraderApi.RegisterFront(_cTPSettings.Investor.TradeFrontAddr);
            TraderApi.Init();

            await Task.CompletedTask;
        }

        void ReqUserLogin()
        {
            var req = new ThostFtdcReqUserLoginField();
            req.BrokerID = _cTPSettings.Investor.BrokerID;
            req.UserID = _cTPSettings.Investor.UserID;
            req.Password = _cTPSettings.Investor.Password;
            int iResult = TraderApi.ReqUserLogin(req, ++iRequestID);
        }

        void RegSystemInfo()
        {
            byte[] buffer = new byte[512];
            int nLen = 0;
            int res = FtdcTdAdapter.CTP_GetSystemInfo(buffer, ref nLen);
            if (res != 0 || nLen == 0)
            {
                Console.WriteLine("CTP_GetSystemInfo() 失败, 错误代码 {0}, nLen = {1}", res, nLen);
                return;
            }

            var field = new ThostFtdcUserSystemInfoField();
            field.BrokerID = _cTPSettings.Investor.BrokerID;
            field.UserID = _cTPSettings.Investor.UserID;
            Array.Copy(buffer, 0, field.ClientSystemInfo, 0, nLen);

            field.ClientPublicIP = "127.0.0.1";
            field.ClientIPPort = 65535;
            field.ClientLoginTime = DateOnly.FromDateTime(DateTime.Now).ToString();
            field.ClientAppID = "QuantStuido.Shell";
            int result = TraderApi.RegisterUserSystemInfo(field);

            if (result == 0)
                Logger.LogInformation("RegisterUserSystemInfo() 成功");
            else
                Logger.LogInformation("RegisterUserSystemInfo() 失败, 错误代码 {0}", result);

            /*  RegisterUserSystemInfo 错误代码 
             0 正确
            -1 字段长度不对
            -2 非CTP采集的终端信息
            -3 当前终端类型非多对多中继
            -5 字段中存在非法字符或者长度超限
            -6 采集结果字段错误
             */
        }

        private void TraderApi_OnErrRtnEvent(object sender, OnErrRtnEventArgs e)
        {
            Console.WriteLine("=====> " + e.EventType);
        }

        private void TraderApi_OnRtnEvent(object sender, OnRtnEventArgs e)
        {
            Console.WriteLine("=====> " + e.EventType);
        }

        private void TraderApi_OnRspEvent(object sender, OnRspEventArgs e)
        {
            bool err = IsError(e.RspInfo, e.EventType.ToString());

            switch (e.EventType)
            {
                case EnumOnRspType.OnRspAuthenticate:
                    if (err)
                    {
                        Logger.LogInformation("认证失败!!!");
                    }
                    else
                    {
                        Logger.LogInformation("认证成功!!!");
                        bool isAuth = false;
                        if (isAuth)
                        {
                            RegSystemInfo();
                        }
                        ReqUserLogin();
                    }

                    break;
                case EnumOnRspType.OnRspUserLogin:
                    if (err)
                    {
                        Logger.LogInformation("登录失败");
                    }
                    else
                    {
                        Logger.LogInformation("登录成功");

                        var fld = Conv.P2S<ThostFtdcRspUserLoginField>(e.Param);
                        Logger.LogInformation("TradingDay is " + fld.TradingDay);
                        Logger.LogInformation("CTP Version " + FtdcTdAdapter.GetApiVersion());

                        ThostFtdcSettlementInfoConfirmField req = new ThostFtdcSettlementInfoConfirmField();
                        req.BrokerID = _cTPSettings.Investor.BrokerID;
                        req.InvestorID = _cTPSettings.Investor.UserID;
                        TraderApi.ReqSettlementInfoConfirm(req, ++this.iRequestID);
                    }
                    break;
                case EnumOnRspType.OnRspQryInstrument:
                    if (e.Param != IntPtr.Zero)
                    {
                        var fld = Conv.P2S<ThostFtdcInstrumentField>(e.Param);
                        if(fld != null && !fld.InstrumentID.IsNullOrEmpty())
                        {
                            Logger.LogInformation("=====> {0}, {1},  isLast {2}", e.EventType, fld.InstrumentID, e.IsLast);
                        }
                    }
                    break;
                case EnumOnRspType.OnRspQryDepthMarketData:
                    if (e.Param != IntPtr.Zero)
                    {
                        var fld = Conv.P2S<ThostFtdcDepthMarketDataField>(e.Param);
                        if(fld != null && !fld.InstrumentID.IsNullOrEmpty())
                        {
                            Logger.LogInformation("=====> {0}, {1},  isLast {2}", e.EventType, fld.InstrumentID, e.IsLast);
                        }   
                    }
                    break;
            }
        }

        private void TraderApi_OnFrontEvent(object sender, OnFrontEventArgs e)
        {
            switch (e.EventType)
            {
                case EnumOnFrontType.OnFrontConnected:
                    {
                        bool isAuthenticate = false;
                        if (isAuthenticate)
                        {
                            var req = new ThostFtdcReqAuthenticateField();
                            req.BrokerID = _cTPSettings.Investor.BrokerID;
                            req.UserID = _cTPSettings.Investor.UserID;
                            req.AppID = _cTPSettings.Investor.AppID;
                            req.AuthCode = _cTPSettings.Investor.AuthCode;

                            TraderApi.ReqAuthenticate(req, ++iRequestID);
                        }
                        else
                        {
                            ReqUserLogin();
                        }
                    }
                    break;
            }
        }

        private async Task DoAutoConnection()
        {
            while (true)
            {
                // 每分钟间隔
                await Task.Delay(TimeSpan.FromMinutes(1));

                // CTP在线的时间段内 , 不在线，自动重连
                if (CTPOnlineTradingTimeFrames.IsCTPOnlineTradingTime(DateTime.Now) && !_isConnected)
                {
                    await DoConnect();
                }
            }

        }

        private async Task DoDisconnect()
        {
            if (TraderApi != null)
            {
                TraderApi.Dispose();
                TraderApi = null;
                Logger.LogInformation("Disconnected.");
            }
        }

        #endregion

        #region ctor

        public CTPTradeProxy()
        {

        }

        public void Initialize(CTPSettings cTPSettings, Action<CTPSettings> actionDoSettings)
        {
            // 初始化数据目录

            _cTPSettings = cTPSettings;
            _doSettings = actionDoSettings;
        }

        #endregion

        public async Task Connect()
        {
            _doSettings?.Invoke(_cTPSettings);

            await DoConnect();

            await DoAutoConnection();
        }

        public async Task Disconnect()
        {
            await DoDisconnect();
        }
    }
}
