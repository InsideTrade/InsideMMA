using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.HockeyApp;
using SciChart.Charting.Visuals;

namespace Inside_MMA
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Ensure SetLicenseKey is called once, before any SciChartSurface instance is created 
            // Check this code into your version-control and it will enable SciChart 
            // for end-users of your application. 
            // 
            // You can test the Runtime Key is installed correctly by Running your application 
            // OUTSIDE Of Visual Studio (no debugger attached). Trial watermarks should be removed. 
            SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
                                                  <Customer>Russian State Vocational Pedagogical University</Customer>
                                                  <OrderId>EDUCATIONAL-USE 0013</OrderId>
                                                  <LicenseCount>1</LicenseCount>
                                                  <IsTrialLicense>false</IsTrialLicense>
                                                  <SupportExpires>05/11/2017 00:00:00</SupportExpires>
                                                  <ProductCode>SC-WPF-SDK-PRO</ProductCode>
                                                  <KeyCode>lwAAAAEAAADNfrRLu4PSAZAAQ3VzdG9tZXI9UnVzc2lhbiBTdGF0ZSBWb2NhdGlvbmFsIFBlZGFnb2dpY2FsIFVuaXZlcnNpdHk7T3JkZXJJZD1FRFVDQVRJT05BTC1VU0UgMDAxMztTdWJzY3JpcHRpb25WYWxpZFRvPTExLU1heS0yMDE3O1Byb2R1Y3RDb2RlPVNDLVdQRi1TREstUFJPZeiYKeB0uPiMOQvdtbYwKsGZnGqx1G6H4p1hAXZRHW23KckVkxI2Erp+Xvmr8q96</KeyCode>
                                                </LicenseContract>");
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            HockeyClient.Current.Configure("a536ac2e8cba464aa30c8836643aea7d");
            await HockeyClient.Current.SendCrashesAsync(true);
        }
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    PresentationTraceSources.Refresh();
        //    PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
        //    PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
        //    PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
        //    base.OnStartup(e);
        //}
    }
    //public class DebugTraceListener : TraceListener
    //{
    //    public override void Write(string message)
    //    {
    //    }

    //    public override void WriteLine(string message)
    //    {
    //        Debugger.Break();
    //    }
    //}
}
    
