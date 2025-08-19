using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace AnalizadorDivisioDeAcometidas.Classes
{
    internal class Test04Bootstrapper
    {
        public Test04AppSettings appSettings;
        public Test04Bootstrapper() {
            CarregarTest04AppSettings();
        }

        private void CarregarTest04AppSettings() {
            appSettings = new Test04AppSettings();
            string appSettings_txt = File.ReadAllText("../../../JSONS/AppSettings.json");
            JObject appSettings_JObject = JObject.Parse(appSettings_txt);

            string conexioBD = (string)appSettings_JObject["ConexionBD"];
            string rutaPDF = (string)appSettings_JObject["RutaPDF"];

            appSettings.connexionBD = conexioBD;
            appSettings.rutaPDF = rutaPDF;
        }
    }
}
