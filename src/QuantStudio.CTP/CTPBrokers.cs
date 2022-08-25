using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP
{
    /// <summary>
    /// 前置机地址设置
    /// </summary>
    [Serializable]
    public class BrokerFront
    {
        public string Name { get; set; }

        public string TradeFront { get; set; }

        public string MarketFront { get; set; }
    }

    /// <summary>
    /// CTP代理商
    /// </summary>
    [Serializable]
    public class CTPBroker
    {
        public string BrokerID { get; set; }

        public string BrokerName { get; set; }

        public List<BrokerFront> Fronts { get; set; } = new List<BrokerFront>() { };
    }

    /// <summary>
    /// CTP支持的Broker代理商
    /// </summary>
    public class CTPBrokers
    {
        public static IReadOnlyList<CTPBroker> Default { get; set; } = new List<CTPBroker>() {
            // SimNoew
            new CTPBroker(){ BrokerID = "9999",BrokerName = "SimNow" 
                ,Fronts = new List<BrokerFront>(){ new BrokerFront() {Name = "电信", TradeFront= "tcp://180.168.146.187:10201", MarketFront= "tcp://180.168.146.187:10211" } } 
            } 
            ,
            // 东海期货
            new CTPBroker(){ BrokerID = "4700",BrokerName = "东海期货" 
                ,Fronts = new List<BrokerFront>(){ new BrokerFront() {Name = "电信",TradeFront= "tcp://61.132.99.125:41205", MarketFront= "tcp://61.132.99.125:41213" } } 
            } 
        };
    }
}
