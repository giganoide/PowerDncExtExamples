using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;

namespace TeamSystem.Customizations
{
    [ExtensionData("MyCheckBeforeLoadExtension", "Gestore trasmissioni", "1.0",
        "TeamSystem", "TeamSystem")]
    public class MyCheckBeforeLoadExtension : IDncExtension
    {
        #region Fields

        private IDncManager _DncManager = null;

        private const string LOGGERSOURCE = @"MyCheckBeforeLoadExtension";

        #endregion

        public MyCheckBeforeLoadExtension() { }

        #region Iterface Implementations

        public void Initialize(IDncManager dncManager)
        {
#if DEBUG
            Debugger.Launch();
#endif
            this._DncManager = dncManager;
        }

        public void Run()
        {
            this._DncManager.SerialCommEngine.BeforeLoadCommand += SerialCommEngine_BeforeLoadCommand;
        }

        public void Shutdown()
        {
            this._DncManager.SerialCommEngine.BeforeLoadCommand -= SerialCommEngine_BeforeLoadCommand;
            this._DncManager = null;
        }

        #endregion

        #region Event Handling

        void SerialCommEngine_BeforeLoadCommand(object sender, FileShortOnChannelCancelEventArgs e)
        {
            //Se il file esiste anche in RX, interrompo la trasmissione ed avviso l'operatore
            var rxPath = e.Channel.GetRxPath();
            var fullPath = Path.Combine(rxPath, e.ShortName) + ".cnc";
            if (File.Exists(fullPath))
            {
                e.Cancel = true;
                e.Channel.SendMessage("(FILE " + e.ShortName + " PRESENTE ANCHE IN RX)");
            }
            else
            {
                var startCmd = GetTextFromAsciiCodes(e.Channel.Settings.Machine.CommandStartCharCodes);
                var isSaveAdded = AddSaveCmd(fullPath, e.ShortName, startCmd);
                if (!isSaveAdded)
                    this._DncManager.AppendMessageToLog(MessageLevel.Error, LOGGERSOURCE, "Save not added");
            }
        }

        #endregion

        #region Elaboration

        private string GetTextFromAsciiCodes(string inStr)
        {
            if (string.IsNullOrWhiteSpace(inStr))
                return string.Empty;

            var codes = inStr.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();

            foreach (var code in codes)
            {
                var numCode = Convert.ToInt32(code); //se non numerico ho l'eccezione di cast

                if (numCode >= 0 && numCode <= 255)
                    sb.Append((char)numCode);
                else
                    sb.Append("[NOTASCII]"); //oppure trow exception
            }

            return sb.ToString();
        }

        private bool AddSaveCmd(string fullPath, string shortName, string startCmd)
        {
            var result = false;

            //Leggo il file e lo scrivo in una stringa
            var strfileToLoad = File.ReadAllText(fullPath);

            //Verifico se ho due "%"
            var WordMatch = Regex.Matches(strfileToLoad, "%");
            var countPercent = WordMatch.Count;

            //Verifico se contiene il comando salva
            if (!strfileToLoad.Contains(startCmd + "S") && !strfileToLoad.Contains(startCmd + " S"))
            {
                strfileToLoad = string.Empty;
                var fileToload = new StreamReader(fullPath);

                try
                {
                    //Leggo e scrivo i primi due blocchi
                    var line = fileToload.ReadLine();
                    strfileToLoad = strfileToLoad + line + Environment.NewLine;
                    line = fileToload.ReadLine();
                    strfileToLoad = strfileToLoad + line + Environment.NewLine;

                    //Scrivo il blocco con il comando salva
                    strfileToLoad = strfileToLoad + "(X00 S" + shortName.TrimStart() + " X11)" + Environment.NewLine;
                    result = true;

                    //Leggo e scrivo fino alla fine del file
                    while (!fileToload.EndOfStream)
                    {
                        line = fileToload.ReadLine();
                        strfileToLoad = strfileToLoad + line + Environment.NewLine;
                    }

                    //Verifico se aggiungere il % finale
                    if (countPercent < 2)
                        strfileToLoad = strfileToLoad + "%" + Environment.NewLine;

                    //Chiudo il file aperto
                    fileToload.Close();
                    fileToload.Dispose();

                    //Sovrascrivo il file
                    File.WriteAllText(fullPath, strfileToLoad);
                }
                catch (Exception ex)
                {
                    var errorMessage = "AddSaveCmd : " + DateTime.Now + " " + ex.Message + Environment.NewLine;
                    this._DncManager.AppendMessageToLog(MessageLevel.Error, LOGGERSOURCE, errorMessage);
                }

            }
            else
            {
                //Verifico se aggiungere il % finale
                if (countPercent < 2)
                    File.AppendAllText(fullPath, Environment.NewLine + "%");
            }

            return result;
        }
        
        #endregion
    }
}