using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantStudio.CTP
{

    /// <summary>
    /// 全局定义公共对象
    /// </summary>
    public class CTPGlobals
    {
        #region 静态方法或字段
        private static volatile CTPGlobals _defaultInstance = null;
        private static readonly object lockObject = new object();

        private void Initialize()
        {

        }

        private CTPGlobals()
        {
            Initialize();
        }

        public static CTPGlobals Instance {
            get {
                if (CTPGlobals._defaultInstance == null)
                {
                    lock (CTPGlobals.lockObject)
                    {
                        if (CTPGlobals._defaultInstance == null)
                        {
                            CTPGlobals._defaultInstance = new CTPGlobals();
                        }
                    }
                }

                return _defaultInstance; 
            } 
        }

        #endregion

    }


}
