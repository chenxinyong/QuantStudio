using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using CTP;
using Microsoft.Extensions.Hosting;
using QuantStudio.CTP.Data;
using Volo.Abp.Security.Encryption;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using static QuantStudio.CTP.CTPConsts;
using Microsoft.Extensions.Logging;
using QuantStudio.CTP.Trade;

namespace QuantStudio.CTP.Trade
{
    /// <summary>
    /// CTP交易管理类
    /// </summary>
    public class TradeManager : ISingletonDependency
    {
        #region private 

        private CTPSettings _ctpSettings;
        private IConfiguration _configuration;
        private IHostEnvironment _hostEnvironment;
        private IStringEncryptionService _stringEncryptionService;

        private CTPTradeProxy _cTPTradeProxy;
        private Action<CTPSettings> _doSettings = null;

        private void DoTradeProxySettings(CTPSettings settings)
        {
        }

        #endregion

        public ILogger<TradeManager> Logger { get; set; }

        #region Ctor / 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mdReceiver"></param>
        public TradeManager(IConfiguration configuration, IStringEncryptionService stringEncryptionService, IHostEnvironment hostEnvironment,CTPTradeProxy cTPTradeProxy)
        {
            Logger = NullLogger<TradeManager>.Instance;

            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _stringEncryptionService = stringEncryptionService;

            _ctpSettings = _configuration.GetSection("CTPSettings").Get<CTPSettings>();

            // 解密敏感信息
            _ctpSettings.Investor.UserID = _stringEncryptionService.Decrypt(_ctpSettings.Investor.UserID);
            _ctpSettings.Investor.Password = _stringEncryptionService.Decrypt(_ctpSettings.Investor.Password);


            _cTPTradeProxy = cTPTradeProxy;

            _cTPTradeProxy.Initialize(_ctpSettings, DoTradeProxySettings);

            // Initialize();
        }


        public async void Initialize()
        {
            // await _cTPTradeProxy.Connect();
        }

        #endregion
    }
}
