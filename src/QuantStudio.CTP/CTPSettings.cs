using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP
{
    /// <summary>
    /// CTP配置
    /// </summary>
    [Serializable]
    public class CTPSettings
    {
        #region ctor

        public CTPSettings()
        {

        }

        public InvestorSetting Investor { get; set; }

        public List<string> SubInstruments { get; set; }

        #endregion

    }

    [Serializable]
    public class InvestorSetting
    {
        public InvestorSetting()
        {

        }
        public InvestorSetting(string brokerID, string tradeFrontAddr, string mdFrontAddr, string investorID, string password, string appId = "", string authCode = "")
        {
            BrokerID = brokerID;
            TradeFrontAddr = tradeFrontAddr;
            MdFrontAddr = mdFrontAddr;
            UserID = investorID;
            Password = password;
            AppID = appId;
            AuthCode = authCode;
        }

        /// <summary>
        /// 期货公司经纪商代码
        /// </summary>
        public string BrokerID { get; set; }

        /// <summary>
        /// 交易前置地址
        /// </summary>
        public string TradeFrontAddr { get; set; }

        /// <summary>
        /// 行情前置地址
        /// </summary>
        public string MdFrontAddr { get; set; }

        /// <summary>
        /// 投资者ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 交易密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// AppID
        /// </summary>
        public string AppID { get; set; }

        /// <summary>
        /// 认证码
        /// </summary>
        public string AuthCode { get; set; }
    }

}
