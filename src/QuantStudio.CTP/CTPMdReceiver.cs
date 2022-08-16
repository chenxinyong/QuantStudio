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

namespace QuantConnect.Studio.CTP
{
    /// <summary>
    /// CTP行情数据接收器
    /// </summary>
    public class CTPMdReceiver : ISingletonDependency
    {
        #region private / property

        private IHostEnvironment _hostEnvironment;
        private IConfiguration _configuration;
        private CTPSettings _cTPSettings;
        private IStringEncryptionService _stringEncryptionService;
        private Dictionary<string, ConcurrentQueue<ThostFtdcDepthMarketDataField>> _dictMdQueue = new Dictionary<string, ConcurrentQueue<ThostFtdcDepthMarketDataField>>();
        private FutureSettings _futureSettings = new FutureSettings() { };
        private Dictionary<string, FutureInfo> _dictFutures = new Dictionary<string, FutureInfo>();

        private FtdcMdAdapter DataApi = null;
        private FtdcTdAdapter TraderApi = null;

        private int iRequestID = 0;

        private bool _isConnected = false;

        private List<string> _subscribeInstrumentIDs = new List<string>();
        private string _tickDataPath;


        #endregion

        public ILogger<CTPMdReceiver> Logger { get; set; }

        #region ctor

        public CTPMdReceiver(IConfiguration configuration, IStringEncryptionService stringEncryptionService, IHostEnvironment hostEnvironment)
        {
            Logger = NullLogger<CTPMdReceiver>.Instance;

            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _stringEncryptionService = stringEncryptionService;

            _cTPSettings = _configuration.GetSection("CTPSettings").Get<CTPSettings>();

            // 解密敏感信息
            _cTPSettings.UserID = _stringEncryptionService.Decrypt(_cTPSettings.UserID);
            _cTPSettings.Password = _stringEncryptionService.Decrypt(_cTPSettings.Password);

            // 期货品种配置
            FutureSettings futureSettings = _configuration.GetSection("FutureSettings").Get<FutureSettings>();

            // 初始化配置
            Init(futureSettings);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="futureSettings"></param>
        public void Init(FutureSettings futureSettings)
        {
            string appDataPath = Path.Combine(_hostEnvironment.ContentRootPath, "App_Data");
            if(!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            // Itcks
            string itcksPath = Path.Combine(appDataPath,"Ticks");
            if (!Directory.Exists(itcksPath))
            {
                Directory.CreateDirectory(itcksPath);
            }

            _tickDataPath = itcksPath;

            _dictFutures.Clear();
            _subscribeInstrumentIDs.Clear();

            int currentYear = DateTime.Today.Year;
            int currentMonth = DateTime.Today.Month;
            int currentDay = DateTime.Today.Day;

            List<FutureInfo> futureInfoList = new List<FutureInfo>();

            futureInfoList.AddRange(futureSettings.CFFEX);
            futureInfoList.AddRange(futureSettings.SHFE);
            futureInfoList.AddRange(futureSettings.DCE);
            futureInfoList.AddRange(futureSettings.INE);

            // SHFE上海期货
            foreach (FutureInfo item in futureInfoList)
            {
                if (!_dictFutures.ContainsKey(item.Symbol))
                {
                    bool isTradingTime = item.IsTradingTimeFrame(TimeOnly.FromDateTime(DateTime.Now));
                    _dictFutures.Add(item.Symbol, item);

                    // 订阅合约
                    item.ContractMonths.ForEach(month =>
                    {
                        string insKey = "";
                        string strYear = "";
                        if (month > currentMonth)
                        {
                            // 当年
                            strYear = (currentYear).ToString().Substring(2, 2);
                            insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                        }
                        else if (month == currentMonth)
                        {
                            if (currentDay >= item.LastTradingDay)
                            {
                                // 下一年
                                strYear = (currentYear + 1).ToString().Substring(2, 2);
                                insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                            }
                            else
                            {
                                // 当年
                                strYear = (currentYear).ToString().Substring(2, 2);
                                insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                            }
                        }
                        else
                        {
                            // 下一年
                            strYear = (currentYear + 1).ToString().Substring(2, 2);
                            insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                        }

                        if (!_subscribeInstrumentIDs.Contains(insKey))
                        {
                            _subscribeInstrumentIDs.Add(insKey);
                        }
                    });
                }
            }

            List<FutureInfo> futureInfoCZCEList = new List<FutureInfo>();
            futureInfoCZCEList.AddRange(futureSettings.CZCE);

            // CZCE-郑州
            foreach (FutureInfo item in futureInfoCZCEList)
            {
                if (!_dictFutures.ContainsKey(item.Symbol))
                {
                    bool isTradingTime = item.IsTradingTimeFrame(TimeOnly.FromDateTime(DateTime.Now));
                    _dictFutures.Add(item.Symbol, item);

                    // 订阅合约
                    item.ContractMonths.ForEach(month =>
                    {
                        string insKey = "";
                        string strYear = "";
                        if (month > currentMonth)
                        {
                            // 当年
                            strYear = (currentYear).ToString().Substring(3, 1);
                            insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                        }
                        else if (month == currentMonth)
                        {
                            if (currentDay >= item.LastTradingDay)
                            {
                                // 下一年
                                strYear = (currentYear + 1).ToString().Substring(3, 1);
                                insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                            }
                            else
                            {
                                // 当年
                                strYear = (currentYear).ToString().Substring(3, 1);
                                insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                            }
                        }
                        else
                        {
                            // 下一年
                            strYear = (currentYear + 1).ToString().Substring(3, 1);
                            insKey = $"{item.Symbol}{strYear}{SeqToCode(month)}";
                        }

                        if (!_subscribeInstrumentIDs.Contains(insKey))
                        {
                            _subscribeInstrumentIDs.Add(insKey);
                        }
                    });
                }
            }
        }

        #endregion

        private string SeqToCode(int seq, int maxLenth = 2)
        {
            if (seq <= 0)
            {
                return "00";
            }
            else if (seq > 0 && seq < 10)
            {
                return $"0{seq.ToString()}";
            }
            else if (seq >= 10 && seq <= 12)
            {
                return $"{seq.ToString()}";
            }
            else
            {
                return seq.ToString();
            }
        }

        private bool IsError(ThostFtdcRspInfoField rspinfo, string source)
        {
            if (rspinfo != null && rspinfo.ErrorID != 0)
            {
                Console.WriteLine(rspinfo.ErrorMsg + ", 来源 " + source);
                return true;
            }
            return false;
        }

        private void TradeApiConnect()
        {
            if(TraderApi == null)
            {
                TraderApi = new FtdcTdAdapter("");
            }
            
            TraderApi.OnFrontEvent += TraderApi_OnFrontEvent;
            TraderApi.OnRspEvent += TraderApi_OnRspEvent;
            TraderApi.OnRtnEvent += TraderApi_OnRtnEvent;
            TraderApi.OnErrRtnEvent += TraderApi_OnErrRtnEvent;

            TraderApi.SubscribePublicTopic(EnumTeResumeType.THOST_TERT_QUICK);
            TraderApi.SubscribePrivateTopic(EnumTeResumeType.THOST_TERT_QUICK);
            TraderApi.RegisterFront(_cTPSettings.TradeFrontAddr);

            TraderApi.Init();
        }

        private void DataApi_OnFrontEvent(object sender, OnFrontEventArgs e)
        {
            switch (e.EventType)
            {
                case EnumOnFrontType.OnFrontConnected:
                    {
                        var req = new ThostFtdcReqUserLoginField();
                        req.BrokerID = _cTPSettings.BrokerID;
                        req.UserID = _cTPSettings.UserID;
                        req.Password = _cTPSettings.Password; 
                        int iResult = DataApi.ReqUserLogin(req, ++iRequestID);
                    }
                    break;
            }
        }

        private void DataApi_OnRtnEvent(object sender, OnRtnEventArgs e)
        {
            Console.WriteLine("DataApi_OnRtnEvent " + e.EventType.ToString());

            var fld = Conv.P2S<ThostFtdcDepthMarketDataField>(e.Param);
            if(fld != null)
            {
                Console.WriteLine("{0}.{1:D3} {2} {3}", fld.UpdateTime, fld.UpdateMillisec, fld.InstrumentID, fld.LastPrice);

                // 深度行情数据加入队列的尾部
                if(_dictMdQueue.ContainsKey(fld.InstrumentID))
                {
                    _dictMdQueue[fld.InstrumentID].Enqueue(fld);
                }
                else
                {
                    _dictMdQueue.Add(fld.InstrumentID, new ConcurrentQueue<ThostFtdcDepthMarketDataField>());
                }
            }
        }

        private async void DataApi_OnRspEvent(object sender, OnRspEventArgs e)
        {
            Console.WriteLine("DataApi_OnRspEvent " + e.EventType.ToString());
            bool err = IsError(e.RspInfo, e.EventType.ToString());

            switch (e.EventType)
            {
                case EnumOnRspType.OnRspUserLogin:
                    if (!err)
                    {
                        _isConnected = true;
                        Console.WriteLine("登录成功");

                        await SubscribeMarketData(_subscribeInstrumentIDs);
                    }
                    break;
                case EnumOnRspType.OnRspSubMarketData:
                    {
                        var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                        Console.WriteLine("订阅成功:" + f.InstrumentID);

                        // 创建一个深度行情数据队列
                        if(!_dictMdQueue.ContainsKey(f.InstrumentID))
                        {
                            _dictMdQueue.Add(f.InstrumentID, new ConcurrentQueue<ThostFtdcDepthMarketDataField>());
                        }
                    }
                    break;
                case EnumOnRspType.OnRspUnSubMarketData:
                    {
                        var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                        Console.WriteLine("退订成功:" + f.InstrumentID);
                    }
                    break;
            }
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
                        Console.WriteLine("认证失败!!!");
                    }
                    else
                    {
                        Console.WriteLine("认证成功!!!");

                        //if (chkSubmitUserSystemInfo.Checked)
                        //{
                        //    // RegSystemInfo();
                        //}

                        ReqUserLogin();
                    }

                    break;
                case EnumOnRspType.OnRspUserLogin:
                    if (err)
                    {
                        Console.WriteLine("登录失败");
                    }
                    else
                    {
                        Console.WriteLine("登录成功");
                        var fld = Conv.P2S<ThostFtdcRspUserLoginField>(e.Param);
                        Console.WriteLine("TradingDay is " + fld.TradingDay);
                        Console.WriteLine("CTP Version " + FtdcTdAdapter.GetApiVersion());

                        ThostFtdcSettlementInfoConfirmField req = new ThostFtdcSettlementInfoConfirmField();
                        req.BrokerID = _cTPSettings.BrokerID;
                        req.InvestorID = _cTPSettings.UserID;
                        TraderApi.ReqSettlementInfoConfirm(req, ++this.iRequestID);
                    }
                    break;
                case EnumOnRspType.OnRspQryInstrument:
                    if (e.Param != IntPtr.Zero)
                    {
                        var fld = Conv.P2S<ThostFtdcInstrumentField>(e.Param);
                        Console.WriteLine("=====> {0}, {1},  isLast {2}", e.EventType, fld.InstrumentID, e.IsLast);
                    }
                    break;
                case EnumOnRspType.OnRspQryDepthMarketData:
                    if (e.Param != IntPtr.Zero)
                    {
                        var fld = Conv.P2S<ThostFtdcDepthMarketDataField>(e.Param);
                        Console.WriteLine("=====> {0}, {1},  isLast {2}", e.EventType, fld.InstrumentID, e.IsLast);
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
                        // 默认不启用客户端认证
                        if (false)
                        {
                            var req = new ThostFtdcReqAuthenticateField();
                            req.BrokerID = _cTPSettings.BrokerID;
                            req.UserID = _cTPSettings.UserID;
                            req.AppID = _cTPSettings.AppID;
                            req.AuthCode = _cTPSettings.AuthCode;

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

        private void ReqUserLogin()
        {
            var req = new ThostFtdcReqUserLoginField();
            req.BrokerID = _cTPSettings.BrokerID;
            req.UserID = _cTPSettings.UserID;
            req.Password = _cTPSettings.Password;

            int iResult = TraderApi.ReqUserLogin(req, ++iRequestID);
        }

        private void RegSystemInfo()
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
            field.BrokerID = _cTPSettings.BrokerID;
            field.UserID = _cTPSettings.UserID;
            Array.Copy(buffer, 0, field.ClientSystemInfo, 0, nLen);

            field.ClientPublicIP = "127.0.0.1";
            field.ClientIPPort = 65535;
            field.ClientLoginTime = "11:28:28";
            field.ClientAppID = "Q7";

            int result = TraderApi.RegisterUserSystemInfo(field);
            if (result == 0)
                Console.WriteLine("RegisterUserSystemInfo() 成功");
            else
                Console.WriteLine("RegisterUserSystemInfo() 失败, 错误代码 {0}", result);

            /*  RegisterUserSystemInfo 错误代码 
             0 正确
            -1 字段长度不对
            -2 非CTP采集的终端信息
            -3 当前终端类型非多对多中继
            -5 字段中存在非法字符或者长度超限
            -6 采集结果字段错误
             */
        }

        private void TraderApi_Disconnect()
        {
            if (TraderApi != null)
            {
                TraderApi.Dispose();
                TraderApi = null;
                Console.WriteLine("Disconnected.");
            }
        }

        private void TraderApi_ReqQryInstrument()
        {
            if (TraderApi != null)
            {
                var req = new ThostFtdcQryInstrumentField();
                req.InstrumentID = "";
                req.ExchangeID = "";
                TraderApi.ReqQryInstrument(req, ++this.iRequestID);
            }
        }

        private void TraderApi_ReqQryDepthMarketData(string instrumentID)
        {
            if (TraderApi != null && !string.IsNullOrEmpty(instrumentID))
            {
                var req = new ThostFtdcQryDepthMarketDataField();
                req.InstrumentID = instrumentID;
                req.ExchangeID = "";
                TraderApi.ReqQryDepthMarketData(req, ++this.iRequestID);
            }
        }

        #region public

        public async Task Connect()
        {
            if (DataApi == null)
            {
                DataApi = new FtdcMdAdapter("", false, false);
            }

            DataApi.OnFrontEvent += DataApi_OnFrontEvent;
            DataApi.OnRspEvent += DataApi_OnRspEvent;
            DataApi.OnRtnEvent += DataApi_OnRtnEvent;

            DataApi.RegisterFront(_cTPSettings.MdFrontAddr);
            DataApi.Init();

            await Task.CompletedTask;
        }

        public async Task Disconnect()
        {
            if (DataApi != null)
            {
                DataApi.Dispose();
                DataApi = null;
                Console.WriteLine("Disconnected.");
            }

            await Task.CompletedTask;
        }

        public async Task SubscribeMarketData(List<string> instrumentIDs = null)
        {
            if (DataApi != null)
            {
                if(instrumentIDs.Any())
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

        public async Task UnSubscribeMarketData(List<string> instrumentIDs = null)
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

        public async Task SaveTickDataToCsv()
        {
            while(true)
            {
                // 保存数据
                TimeOnly currentTiem = TimeOnly.FromDateTime(DateTime.Now);
                if(currentTiem.Second == 1)
                {
                    int hour = currentTiem.AddMinutes(-1).Hour;
                    int minute = currentTiem.AddMinutes(-1).Minute;
                    int second = 0;

                    TimeOnly beginTime = new TimeOnly(hour, minute, second);
                    TimeOnly endTime = new TimeOnly(hour, currentTiem.Minute, second);
                }

                await Task.Delay(1000);
            }
        }

        #endregion
    }
}
