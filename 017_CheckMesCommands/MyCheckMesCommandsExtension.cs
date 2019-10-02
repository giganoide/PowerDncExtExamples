using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;

namespace TeamSystem.Customizations
{
    [ExtensionData("MyCheckMesCommandsExtension", "Gestore trasmissioni", "1.0",
        "TeamSystem", "TeamSystem")]
    public class MyCheckMesCommandsExtension : IDncExtension
    {
        #region Fields

        private IDncManager _DncManager;

        private const string LOGGERSOURCE = @"MyCheckMesCommandsExtension";

        #endregion

        #region Iterface Implementations

        public void Initialize(IDncManager dncManager)
        {
#if DEBUG
            Debugger.Launch();
#endif
            _DncManager = dncManager;
        }

        public void Run()
        {
            _DncManager.SerialCommEngine.BeforeLoadCommand += SerialCommEngine_BeforeLoadCommand;
        }

        public void Shutdown()
        {
            _DncManager.SerialCommEngine.BeforeLoadCommand -= SerialCommEngine_BeforeLoadCommand;
            _DncManager = null;
        }

        #endregion

        #region Event Handling
        
        private void SerialCommEngine_BeforeLoadCommand(object sender, FileShortOnChannelCancelEventArgs e)
        {
            //discrimino se devo gestire l'evento di valutazione comandoi MES

            var attributes = e.Channel.Settings.Customization.Attributes;

            var evaluateMesCmd = attributes.GetValue<bool>("EVALUATE_MESCMD");
            if (!evaluateMesCmd)
                return;

            var mesType = attributes.GetString("MES_TYPE");
            if (string.IsNullOrWhiteSpace(mesType))
                return;

            //var mustProcessAttribute =
            //    e.Channel.Settings.Customization.Attributes.FirstOrDefault(a => a.Name == "EVALUATE_MESCMD");

            //var mustProcess = mustProcessAttribute != null
            //                  && mustProcessAttribute.IsValid
            //                  && !mustProcessAttribute.Disabled
            //                  && mustProcessAttribute.Type == ValueContainerType.Boolean
            //                  && mustProcessAttribute.GetConvertedValueToType<bool>();

            //if (!mustProcess)
            //    return;

            ////Verifico il tipo di comando MES da valutare
            //var mesTypeAttribute =
            //    e.Channel.Settings.Customization.Attributes.FirstOrDefault(a => a.Name == "MES_TYPE");

            //mustProcess = mesTypeAttribute != null
            //              && mesTypeAttribute.IsValid
            //              && !mesTypeAttribute.Disabled
            //              && mesTypeAttribute.Type == ValueContainerType.String;

            //if (!mustProcess)
            //    return;

            //var mesType = mesTypeAttribute.Value;

            var fileToLoad = Path.Combine(e.Channel.GetTxPath(), e.ShortName) + ".cnc";
            var fileContent = File.ReadAllText(fileToLoad);
            var WordMatch = Regex.Matches(fileContent, mesType);

            //Se ho almeno due comandi MES procedo, diversamente cancello l'evento ed invio messaggio all'operatore.
            if (WordMatch.Count < 2)
            {
                e.Cancel = true;
                e.Channel.SendMessage("(COMANDI MES NON PRESENTI");
            }
        }

        #endregion
    }

    public static class AttributeValueContainerExtensionMethods
    {
        public static T GetValue<T>(this IEnumerable<AttributeValueContainer> attributes, string name)
        {
            var attribute = attributes.FirstOrDefault(a => a.Name == name);
            if (attribute == null)
                return default(T);
            if (!attribute.IsValid)
                return default(T);
            if (attribute.Disabled)
                return default(T);
            return attribute.GetConvertedValueToType<T>();
        }

        public static string GetString(this IEnumerable<AttributeValueContainer> attributes, string name)
        {
            var attribute = attributes.FirstOrDefault(a => a.Name == name);
            if (attribute == null)
                return null;
            if (!attribute.IsValid)
                return null;
            if (attribute.Disabled)
                return null;
            if (attribute.Type != ValueContainerType.String)
                return null;
            return attribute.Value;
        }
    }
}