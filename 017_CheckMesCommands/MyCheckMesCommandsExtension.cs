using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Atys.PowerDNC.Commands;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;
using Atys.PowerDNC.Settings;

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
            this._DncManager.SerialCommEngine.BeforeLoadCommand += this.SerialCommEngine_BeforeLoadCommand;
            this._DncManager.SerialCommEngine.BeforeDncCommand += this.SerialCommEngine_BeforeDncCommand;
            this._DncManager.SerialCommEngine.DncCommandRaised += this.SerialCommEngine_DncCommandRaised;
            this._DncManager.SerialCommEngine.CommandParseCompleted += this.SerialCommEngine_CommandParseCompleted;
        }
        public void Shutdown()
        {
            this._DncManager.SerialCommEngine.BeforeLoadCommand -= this.SerialCommEngine_BeforeLoadCommand;
            this._DncManager.SerialCommEngine.BeforeDncCommand -= this.SerialCommEngine_BeforeDncCommand;
            this._DncManager.SerialCommEngine.DncCommandRaised -= this.SerialCommEngine_DncCommandRaised;
        }

        #endregion

        #region verifica presenza comandi MES su caricamento file

        private void SerialCommEngine_BeforeLoadCommand(object sender, FileShortOnChannelCancelEventArgs e)
        {
            //discrimino se devo gestire l'evento di valutazione comandi MES

            var attributes = e.Channel.Settings.Customization.Attributes;

            var evaluateMesCmd = attributes.GetValue<bool>("EVALUATE_MESCMD");
            if (!evaluateMesCmd)
                return;

            var mesType = attributes.GetString("MES_TYPE"); //in un attributo ho la regex per la ricerca dei comandi
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
            var wordMatch = Regex.Matches(fileContent, mesType);

            //Se ho almeno due comandi MES procedo, diversamente cancello l'evento ed invio messaggio all'operatore.
            if (wordMatch.Count < 2)
            {
                e.Cancel = true;
                e.Channel.SendMessage("(COMANDI MES NON PRESENTI");
            }
        }

        #endregion

        #region utilizzo comandi custom su ricezione file

        /*
         * se utilizzo comandi custom (e ad esempio me li aspetto sempre),
         * può essere utile fare una verifica della presenza degli stessi
         * sulla ricezione al termine della ricerca di tutti i comandi.
         * La srtinga identificativa di un comando non viene memorizzata
         * in testo semplice, ma come stringa che contiene la sequenza dei codici ascii
         * separati da spazi
         *
         * NB: gli eventi sono generati in modo centralizzato
         *     dall'engine per tutti i suoi EndPoint:
         *     testare Id EndPoint e valutare se usare lock
         */

        private void SerialCommEngine_CommandParseCompleted(object sender, CommandParsingEventArgs e)
        {
            /*
             * L'elenco dei comandi estratti contiene solo comandi validi
             * (definiti nelle opzioni)
             */

            if (e.Channel.EndPoint.Id == 0)
            {
                //comandi custom che mi aspetto
                var endPointCustomCommands = e.Channel.Settings.Commands.Commands
                                              .Where(c => c.Command == CommandType.Custom).Select(c => c.TextCharCodes)
                                              .ToList();

                //comandi custom trovati nel file ricevuto
                var extractedCommands = e.Commands?.ToList() ?? new List<ExtractedCommand>();
                var myCommandsCount = extractedCommands.Select(c => c.Command.TextCharCodes).Count();

                //ESEMPIO 1: mi servono tutti i comandi
                var foundAll = myCommandsCount == endPointCustomCommands.Count;
                if (!foundAll)
                {
                    //TODO: notifico qualcuno?
                }

                //ESEMPIO 2: mi basta trovare almeno un comando custom 
                var foundOne = myCommandsCount > 0;
                if (!foundOne)
                {
                    //TODO: notifico qualcuno?
                }

                //ESEMPIO 3: ne voglio uno specifico
                const string lookingForCommand = @"67 85 83 84 79 77 49"; //= CUSTOM1
                var foundSpecific = extractedCommands.Any(c => c.Command.TextCharCodes == lookingForCommand);
                if (!foundSpecific)
                {
                    //TODO: notifico qualcuno?
                }
            }
        }


        /*
         * per ogni singolo comando custom rilevato nel programma, vengono
         * generati i due eventi seguenti
         */

        private void SerialCommEngine_BeforeDncCommand(object sender, DncCancelCommandEventArgs e)
        {
            if (e.Channel.EndPoint.Id == 0 && e.Command.DisplayName == "CUSTOM1")
            {
                //eventuale verifica dei parametri del comando
                //come previsti da settings EndPoint
                var arguments = e.Arguments?.ToList() ?? new List<string>();
                //AD ESEMPIO se mi aspetto esattamente due parametri:
                e.Cancel = arguments.Count != 2;
            }

            //questo per tutti i comandi custom da gestire...
        }

        private void SerialCommEngine_DncCommandRaised(object sender, DncCommandEventArgs e)
        {
            if (e.Channel.EndPoint.Id == 0 && e.Command.DisplayName == "CUSTOM1")
            {
                var arguments = e.Arguments?.ToList() ?? new List<string>();

                //TODO: codie per gestire il comando custom

            }

            //questo per tutti i comandi custom da gestire...
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