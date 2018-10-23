using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using Inside_MMA.ViewModels;

namespace Inside_MMA
{
	static class MarshalUTF8
	{
		private static UTF8Encoding _utf8;

		//--------------------------------------------------------------------------------
		static MarshalUTF8()
		{
			_utf8 = new UTF8Encoding();
		}

		//--------------------------------------------------------------------------------
		public static IntPtr StringToHGlobalUTF8(String data)
		{
            Byte[] dataEncoded = _utf8.GetBytes(data + "\0");

			int size = Marshal.SizeOf(dataEncoded[0]) * dataEncoded.Length;

			IntPtr pData = Marshal.AllocHGlobal(size);

			Marshal.Copy(dataEncoded, 0, pData, dataEncoded.Length);

			return pData;
		}

		//--------------------------------------------------------------------------------
		public static String PtrToStringUTF8(IntPtr pData)
		{
			// this is just to get buffer length in bytes
			String errStr = Marshal.PtrToStringAnsi(pData);
			int length = errStr.Length;

			Byte[] data = new byte[length];
			Marshal.Copy(pData, data, 0, length);

			return _utf8.GetString(data);
		}
		//--------------------------------------------------------------------------------
	}

    delegate void NewStringDataHandler(string data);
    delegate void NewBoolDataHandler(bool data);

    static class TXmlConnector
	{
		const String EX_SETTING_CALLBACK = "Не смог установить функцию обратного вызова";

        delegate bool CallBackDelegate(IntPtr pData);
        //delegate bool CallBackExDelegate(IntPtr pData, IntPtr userData);
		
		static readonly CallBackDelegate MyCallbackDelegate =  MyCallBack;
        //static readonly GCHandle CallbackHandle = GCHandle.Alloc(MyCallbackDelegate);

        //static readonly CallBackExDelegate myCallbackExDelegate = new CallBackExDelegate(myCallBackEx);
        //static readonly GCHandle callbacExkHandle = GCHandle.Alloc(myCallbackExDelegate);

        private static bool _bConnected; // флаг наличия подключения к серверу
        
        public static AutoResetEvent StatusDisconnected = new AutoResetEvent(true);
        public static int StatusTimeout;

        public static bool FormReady;
        public static event NewStringDataHandler SendNewFormData;
        public static event NewStringDataHandler SendNewSecurity;
        public static event NewStringDataHandler SendNewTimeframe;
        public static event NewBoolDataHandler SendNewStatus;
        public static event NewStringDataHandler SendNewQuotes;
        public static event NewStringDataHandler SendNewAllTrades;
        public static event NewStringDataHandler SendNewTicks;
        public static event NewStringDataHandler SendNewCandles;
        public static event NewStringDataHandler SendNewTrades;
        public static event NewStringDataHandler SendNewOrders;
        public static event NewStringDataHandler SendNewClientInfo;
        public static event NewStringDataHandler SendNewUnitedPortfolio;
	    public static event NewStringDataHandler SendPortfolioMCT;
        public static event NewStringDataHandler SendNews;
        public static event NewStringDataHandler SendStatus;

	    private static void OnSendPortfolioMct(string data)
	    {
	        SendPortfolioMCT?.Invoke(data);
	    }
        private static void OnSendNews(string data)
	    {
	        SendNews?.Invoke(data);
	    }
        private static void OnSendNewClientInfo(string data)
        {
            SendNewClientInfo?.Invoke(data);
        }
        private static void OnSendNewOrders(string data)
        {
            SendNewOrders?.Invoke(data);
        }
        private static void OnSendNewCandles(string data)
        {
            SendNewCandles?.Invoke(data);
        }
        private static void OnSendNewTrades(string data)
        {
            SendNewTrades?.Invoke(data);
        }
        private static void OnSendNewSecurity(string data)
        {
            SendNewSecurity?.Invoke(data);
        }
        private static void OnSendNewFormData(string data)
        {
            SendNewFormData?.Invoke(data);
        }
        private static void OnSendNewStatus(bool data)
        {
            SendNewStatus?.Invoke(data);
        }

        private static void OnSendNewQuotes(string data)
        {
            SendNewQuotes?.Invoke(data);
        }

        private static void OnSendNewAllTrades(string data)
        {
            SendNewAllTrades?.Invoke(data);
        }

        private static void OnSendNewTicks(string data)
        {
            SendNewTicks?.Invoke(data);
        }

        private static void OnSendNewUnitedPortfolio(string data)
        {
            SendNewUnitedPortfolio?.Invoke(data);
        }

        public static void ConnectorSetCallback()
        { 
            if (!SetCallback(MyCallbackDelegate))
            {
                throw (new Exception(EX_SETTING_CALLBACK));
            }
        }

        //--------------------------------------------------------------------------------
        static bool MyCallBack(IntPtr pData)
		{
            string res;
			String data = MarshalUTF8.PtrToStringUTF8(pData);
			FreeMemory(pData);

            res = Transaq_HandleData(data);
            if (res == "server_status") New_Status();

            return true;
		}

        //static bool myCallBackEx(IntPtr pData, IntPtr userData)
        //{
        //    String data = MarshalUTF8.PtrToStringUTF8(pData);
        //    FreeMemory(pData);

        //    Transaq_HandleData(data);
        //    //	DataList.Add(data);
        //    return true;
        //}

		//--------------------------------------------------------------------------------

        static void New_Status()
        {
            OnSendNewStatus(_bConnected);
            if (_bConnected)
            {
                StatusDisconnected.Reset();
            }
            else
            {
                StatusDisconnected.Set();
            }
        }



		//--------------------------------------------------------------------------------
        public static String ConnectorSendCommand(String command)
        {
            if (MainWindowViewModel.IsReconnecting) return null;
			IntPtr pData = MarshalUTF8.StringToHGlobalUTF8(command);
		    var pResult = SendCommand(pData);
		    String result = MarshalUTF8.PtrToStringUTF8(pResult);

			Marshal.FreeHGlobal(pData);
			FreeMemory(pResult);

			return result;

		}
        public static bool ConnectorInitialize(String path, Int16 logLevel)
        {
            IntPtr pPath= MarshalUTF8.StringToHGlobalUTF8(path);
            IntPtr pResult = Initialize(pPath, logLevel);

            if (!pResult.Equals(IntPtr.Zero))
            {
                String result = MarshalUTF8.PtrToStringUTF8(pResult);
                Marshal.FreeHGlobal(pPath);
                FreeMemory(pResult);
                //log.WriteLog(result);
                return false;
            }
            else
            {
                Marshal.FreeHGlobal(pPath);
                //log.WriteLog("Initialize() OK");
                return true;
            }

        }


        public static void ConnectorUnInitialize()
        {
            var pResult = UnInitialize();
            

            if (!pResult.Equals(IntPtr.Zero))
            {
                String result = MarshalUTF8.PtrToStringUTF8(pResult);
                FreeMemory(pResult);
                //log.WriteLog(result);
            }
            else
            {
                //log.WriteLog("UnInitialize() OK");
            }

        }


        //================================================================================
        public static string Transaq_HandleData(string data)
        {
            // обработка данных, полученных коннектором от сервера Транзак
            string sTime = DateTime.Now.ToString("HH:mm:ss.fff");
            string info = "";

            // включить полученные данные в строку вывода в лог-файл
            string textForWindow = data;
            //log.WriteLog("ServerData: " + data);
            XmlReaderSettings xs = new XmlReaderSettings();
            xs.IgnoreWhitespace = true;
            xs.ConformanceLevel = ConformanceLevel.Fragment;
            xs.DtdProcessing = DtdProcessing.Ignore;
            
            var xr = XmlReader.Create(new System.IO.StringReader(data), xs);
            
            string section = "";
            string line = "";
            string str = "";
            string ename = "";
            string evalue = "";
            string attr = "";
            //string values = "";

            // обработка "узлов" 
            while (xr.Read())
            {
                switch (xr.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                        ename = xr.Name; break;
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Comment:
                    case XmlNodeType.XmlDeclaration:
                        evalue = xr.Value; break;
                    case XmlNodeType.DocumentType:
                        ename = xr.Name; evalue = xr.Value; break;
                    default: break;
                }

                //................................................................................
                // определяем узел верхнего уровня - "секцию"
                if (xr.Depth == 0)
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        section = ename;

                        if ((section != "boards") && (section != "securities") && (section != "pits") &&
                            (section != "sec_info_upd") && (section != "quotes") && (section != "alltrades") &&
                            (section != "ticks") && (textForWindow.Length > 0) && (section != "united_portfolio"))
                        {
                            textForWindow = "";
                            //OnSendNews(data);
                        }

                        line = "";
                        str = "";
                        for (int i = 0; i < xr.AttributeCount; i++)
                        {
                            str = str + xr.GetAttribute(i) + ";";
                        }
                    }
                    if (xr.NodeType == XmlNodeType.EndElement)
                    {
                        //line = "";
                        //section = "";
                    }
                    if (xr.NodeType == XmlNodeType.Text)
                    {
                        str = str + evalue + ";";
                    }
                }
                //................................................................................
                // данные для рынков
                if (section == "markets")
                {
                    //xe = (XElement)XNode.ReadFrom(xr);

                    if (ename == "market")
                    {
                        if (xr.NodeType == XmlNodeType.Element)
                        {
                            line = "";
                            str = "";
                            for (int i = 0; i < xr.AttributeCount; i++)
                            {
                                str = str + xr.GetAttribute(i) + ";";
                            }
                        }
                        if (xr.NodeType == XmlNodeType.EndElement)
                        {
                            line = "add market: " + str;
                            str = "";
                        }
                        if (xr.NodeType == XmlNodeType.Text)
                        {
                            str = str + evalue + ";";
                        }
                    }
                }
                //................................................................................
                // данные для таймфреймов
                if (section == "candlekinds")
                {
                    
                    break;
                    //if (ename == "kind")
                    //{
                    //    if (xr.NodeType == XmlNodeType.Element)
                    //    {
                    //        line = "";
                    //        str = "";
                    //    }
                    //    if (xr.NodeType == XmlNodeType.EndElement)
                    //    {
                    //        line = "add kind: " + str;
                    //        On_New_Timeframe(str);
                    //        str = "";
                    //    }
                    //}
                    //else
                    //{
                    //    if (xr.NodeType == XmlNodeType.Text)
                    //    {
                    //        str = str + evalue + ";";
                    //    }
                    //}
                }
                //................................................................................
                // данные для инструментов
                if (section == "securities")
                {
                    OnSendNewSecurity(data);
                    break;
                    //if (ename == "security")
                    //{
                    //    if (xr.NodeType == XmlNodeType.Element)
                    //    {
                    //        line = "";
                    //        str = "";
                    //        for (int i = 0; i < xr.AttributeCount; i++)
                    //        {
                    //            str = str + xr.GetAttribute(i) + ";";
                    //        }
                    //    }
                    //    if (xr.NodeType == XmlNodeType.EndElement)
                    //    {
                    //        line = "add security: " + str;
                    //        On_New_Security(str);
                    //        str = "";
                    //    }
                    //}
                    //else
                    //{
                    //    if (xr.NodeType == XmlNodeType.Element)
                    //    {
                    //        for (int i = 0; i < xr.AttributeCount; i++)
                    //        {
                    //            str = str + xr.GetAttribute(i) + ";";
                    //        }
                    //    }
                    //    if (xr.NodeType == XmlNodeType.Text)
                    //    {
                    //        str = str + evalue + ";";
                    //    }
                    //}
                }
                //................................................................................
                // данные по свечам
                if (section == "candles")
                {
                    OnSendNewCandles(data);
                    break;
                }
                //................................................................................
                // данные по клиенту
                if (section == "client")
                {
                    //if (ename == "client")
                    //{
                    //    if (xr.NodeType == XmlNodeType.Element)
                    //    {
                    //        line = "";
                    //        str = "";
                    //        for (int i = 0; i < xr.AttributeCount; i++)
                    //        {
                    //            str = str + xr.GetAttribute(i) + ";";
                    //        }
                    //        // определение параметров клиента
                    //        //string[] с_attrs = str.Split(';');
                    //        //if (с_attrs.Length > 0)
                    //        //{
                    //        //    ClientCode = с_attrs[0];
                    //        //}
                    //        line = "add client: " + str;
                    //    }

                    //    else
                    //    {
                    //        line = "";
                    //        if (xr.NodeType == XmlNodeType.Text)
                    //        {
                    //            str = str + evalue + ";";
                    //            line = "set: " + ename + "=" + evalue;
                    //        }
                    //    }
                    //}
                    OnSendNewClientInfo(data);
                    break;
                }
                //................................................................................
                // данные для позиций
                if (section == "positions")
                {
                   

                }
                //................................................................................
                if (section == "overnight")
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        line = "";
                        str = "";
                        for (int i = 0; i < xr.AttributeCount; i++)
                        {
                            str = str + "<" + xr.GetAttribute(i) + ">;";
                        }
                        line = "set overnight status: " + str;
                    }
                }
                //................................................................................
                // данные о статусе соединения с сервером
                if (section == "server_status")
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        string attr_connected = xr.GetAttribute("connected");
                        if (attr_connected == "true") _bConnected = true;
                        if (attr_connected == "false")
                        {
                            _bConnected = false;
                        }
                        if (attr_connected == "error")
                        {
                            _bConnected = false;
                            xr.Read();
                            OnSendNewFormData(xr.Value);
                        }
                    }
                    SendStatus?.Invoke(data);
                }

                if (section == "orders") //обрабатываем заявки
                {
                    OnSendNewOrders(data);
                    break;
                }

                if (section == "stoporders") //обрабатываем заявки
                {
                    OnSendNewOrders(data);
                    break;
                }
                if (section == "ticks")
                {
                    OnSendNewTicks(data);
                    break;
                }

                if (section == "quotes")
                {
                    OnSendNewQuotes(data);
                    break;
                }
                
                if(section == "alltrades")
                {
                    OnSendNewAllTrades(data);
                    break;
                }
                if (section == "trades")
                {
                    OnSendNewTrades(data);
                    break;
                }

                if (section == "united_portfolio")
                {
                    OnSendNewUnitedPortfolio(data);
                    break;
                }
                if (section == "portfolio_mct")
                {
                    OnSendPortfolioMct(data);
                    break;
                }
                if (section == "news_header" || section == "news_body")
                {
                    OnSendNews(data);
                    break;
                }
                //................................................................................
                if (line.Length > 0)
                {
                    //line = new string(' ',xr.Depth*2) + line;
                    if (info.Length > 0) info = info + (char)13 + (char)10;
                    info = info + line;
                }

            }


            //if (info.Length > 0) log.WriteLog(info);

            return section;
            // вывод дополнительной информации для удобства отладки
        }


 
		//--------------------------------------------------------------------------------
		// файл библиотеки TXmlConnector.dll должен находиться в одной папке с программой

        [DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetCallback(CallBackDelegate pCallback);

        //[DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
        //private static extern bool SetCallbackEx(CallBackExDelegate pCallbackEx, IntPtr userData);

        [DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
		private static extern IntPtr SendCommand(IntPtr pData);

        [DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
		private static extern bool FreeMemory(IntPtr pData);

        [DllImport("txmlconnector.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr Initialize(IntPtr pPath, Int32 logLevel);

	    [DllImport("txmlconnector.dll", CallingConvention = CallingConvention.StdCall)]
	    private static extern IntPtr InitializeEx(IntPtr data);

        [DllImport("TXmlConnector.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr UnInitialize();

        [DllImport("TXmlConnector.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr SetLogLevel(Int32 logLevel);
        //--------------------------------------------------------------------------------


	    
	}
	

	//================================================================================


}
