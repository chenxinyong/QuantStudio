using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Studio.CTP
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

        public CTPSettings(string brokerID, string tradeFrontAddr, string mdFrontAddr, string investorID, string password, string appId = "",string authCode = "")
        {
            BrokerID = brokerID;
            TradeFrontAddr = tradeFrontAddr;
            MdFrontAddr = mdFrontAddr;
            UserID = investorID;
            Password = password;
            AppID = appId;
            AuthCode = authCode;
        }

        #endregion

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
