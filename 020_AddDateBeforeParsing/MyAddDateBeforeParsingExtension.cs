using System;
using System.Diagnostics;
using System.IO;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;

namespace TeamSystem.Customizations
{
    [ExtensionData("MyAddDateBeforeParsingExtension", "Gestore trasmissioni", "1.0",
        "TeamSystem", "TeamSystem")]
    public class MyAddDateBeforeParsingExtension : IDncExtension
    {
        #region Event Handling

        private void SerialCommEngine_BeforeCommandParsing(object sender, FileOnChannelEventArgs e)
        {
            var fullPath = e.FullPath;
            string line = null;
            var finefound = 0;
            var savefound = 0;
            var loadfound = 0;
            InsertFileDate(fullPath);
            var file = new StreamReader(fullPath);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("#S")) savefound++;

                if (line.Contains("#C")) loadfound++;

                if (line.Contains("M30") || line.Contains("M02") || line.Contains("M2")) finefound++;
            }

            file.Close();
            if (finefound == 0 && savefound > 0)
            {
                var file_read = new StreamReader(fullPath);
                var file_write = new StreamWriter(fullPath + "123");
                while ((line = file_read.ReadLine()) != null)
                {
                    if (line.Contains("#S"))
                    {
                        line = null;
                        line = "(#S" + " " + DateTime.Today + " #)";
                    }

                    file_write.WriteLine(line);
                }

                file_write.Close();
                file_read.Close();
                File.Delete(fullPath);
                File.Copy(fullPath + "123", fullPath);
            }
        }

        #endregion

        #region Fields

        private IDncManager _DncManager;

        private const string LOGGERSOURCE = @"MyAddDateBeforeParsingExtension";

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
            _DncManager.SerialCommEngine.BeforeCommandParsing += SerialCommEngine_BeforeCommandParsing;
        }

        public void Shutdown()
        {
            _DncManager.SerialCommEngine.BeforeCommandParsing -= SerialCommEngine_BeforeCommandParsing;
            _DncManager = null;
        }

        #endregion

        #region Elaboration

        public void InsertFileDate(string fullPath)
        {
            try
            {
                var fileSaved = new StreamReader(fullPath);
                var line = string.Empty;
                var newFile = string.Empty;

                //Leggo e scrivo i primi tre blocchi
                for (var i = 1; i <= 5; ++i)
                {
                    line = fileSaved.ReadLine();
                    newFile = newFile + line + Environment.NewLine;
                    if (line.Contains("#S")) i = 7;
                }

                //Inserisco la data
                newFile = newFile + "(DATA ULTIMO SALVATAGGIO) (" + GetFileDate(fullPath) + ")" +
                          Environment.NewLine;

                //Leggo e scrivo fino in fondo al file saltando l'eventuale blocco con la data
                //che è già stata inserita.
                while (!fileSaved.EndOfStream)
                {
                    line = fileSaved.ReadLine();
                    if (!line.StartsWith("(DATA ULTIMO SALVATAGGIO) ("))
                        newFile = newFile + line + Environment.NewLine;
                }

                fileSaved.Close();
                fileSaved.Dispose();
                //scrivo la nuova stringa sul file salvato
                File.WriteAllText(fullPath, newFile);
            }
            catch (Exception ex)
            {
                var message = $"Sub InsertFileDate : {DateTime.Now} {ex}";
                _DncManager.AppendMessageToLog(MessageLevel.Error, LOGGERSOURCE, message);
            }
        }

        /// <summary>
        ///     Ricava la data in  formato stringa GG-MM-AA del file passato
        /// </summary>
        /// <param name="fullPath">Percorso completo file</param>
        /// <returns></returns>
        private string GetFileDate(string fullPath)
        {
            var result = string.Empty;

            try
            {
                var lastDate = File.GetLastWriteTime(fullPath);

                result = lastDate.Date.Day + "." +
                         lastDate.Month + "." +
                         lastDate.Year;
            }
            catch (Exception ex)
            {
                //    this.AppendTextToLog(this._LogFileFullPath, "Sub GetFileDate : " + DateTime.Now + " " + ex);
            }


            return result;
        }

        #endregion
    }
}