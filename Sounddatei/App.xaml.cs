using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Un4seen.Bass;

namespace Sounddatei
{
    /// <summary>
    /// App
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class App : System.Windows.Application
    {

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent()
        {

#line 4 "..\..\..\App.xaml"
            this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);

#line default
#line hidden
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            ///Initialize Bass
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            ///Loading all of the BASS plugins...
            int pluginWebm = Bass.BASS_PluginLoad(@"lib\basswebm.dll");
            int pluginWV = Bass.BASS_PluginLoad(@"lib\basswv.dll");
            int pluginOpus = Bass.BASS_PluginLoad(@"lib\bassopus.dll");
            int pluginFlac = Bass.BASS_PluginLoad(@"lib\bassflac.dll");
            int pluginAix = Bass.BASS_PluginLoad(@"lib\bass_aix.dll");
            int pluginAdx = Bass.BASS_PluginLoad(@"lib\bass_adx.dll");
            int pluginAc3 = Bass.BASS_PluginLoad(@"lib\bass_ac3.dll");
            int pluginAcc = Bass.BASS_PluginLoad(@"lib\bass_aac.dll");


            Sounddatei.App app = new Sounddatei.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
