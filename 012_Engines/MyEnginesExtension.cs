using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;

namespace TeamSystem.Customizations
{
    [ExtensionData("MyEnginesExtension", "Put here an extension description.", "1.0",
        Author = "Author Name", EditorCompany = "Company Name")]
    public class MyEnginesExtension : IDncExtension
    {
        private IDncManager _DncManager = null; //riferimento a PowerDNC


        #region IDncExtension Members

        /// <summary>
        /// Inizializzazione estensione e collegamento all'oggetto principale PowerDNC
        /// (eseguito al caricamento in memoria dell'estensione)
        /// -
        /// Extension's initialization code
        /// </summary>
        /// <param name="dncManager">Riferimento all'oggetto principale PowerDNC</param>
        /// <remarks>In questo punto gli engine di comunicazione NON sono
        /// ancora caricati ed inizializzati</remarks>
        public void Initialize(IDncManager dncManager)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            //memorizzo il riferimento all'oggetto principale di PowerDNC
            this._DncManager = dncManager;

            //questa istruzione inserisce un messaggio nel file di log di PowerDNC
            this._DncManager.AppendMessageToLog(MessageLevel.Diagnostics, "MyEmptyExtension", "Estensione creata!");
            //mentre la successiva invia un messaggio ai Remote Panels
            this._DncManager.SendMessageToUI(MessageLevel.Diagnostics, "MyEmptyExtension", "Estensione creata!");

            /*
             * Your custom implementation here...
             * --
             * Fornire di seguito l'implementazione del metodo...
             */
        }

        /// <summary>
        /// Esegue/avvia l'estensione
        /// -
        /// Extension execution
        /// </summary>
        /// <remarks>In questo punto gli engine di comunicazione sono
        /// caricati ed inizializzati con gli EndPoints assegnati</remarks>
        public void Run()
        {
            /*
             *in questo punto posso agganciarmi agli eventi dei motori di comunicazione
             * oppure a quello di notifica inizializzazione generale
             * completata e da qui utilizzare ciò che serve
             *
             */

            this._DncManager.Initialized += this._DncManager_Initialized;
        }

        /// <summary>
        /// Deve contenere il codice di cleanup da eseguire prima della disattivazione
        /// dell'estensione o comunque alla chiusura di PowerDNC
        /// -
        /// Must contain the clean-up code, to be executed at PowerDNC shutdown
        /// </summary>
        public void Shutdown()
        {

        }

        #endregion

        /// <summary>
        /// Gestione evento inizializzazione generale servizio:
        /// a questo punto tutti i sistemi sono attivi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DncManager_Initialized(object sender, EventArgs e)
        {
            //aggancio a eventi motori di comunicazione
            var serialCommEngine = this._DncManager.SerialCommEngine;
            var ftpEngine = this._DncManager.FtpServerEngine;
            var fileWatcherEngine = this._DncManager.WatcherEngine;

            //motore di comunicazione seriale sempre abilitato
            serialCommEngine.BeforeEnqueueFile += this.SerialCommEngine_BeforeEnqueueFile;
            serialCommEngine.BeforeSend += this.SerialCommEngine_BeforeSend;
            serialCommEngine.CommandParseCompleted += this.SerialCommEngine_CommandParseCompleted;
            serialCommEngine.BeforeSaveCommand += this.SerialCommEngine_BeforeSaveCommand;
            serialCommEngine.SaveCommandRaised += this.SerialCommEngine_SaveCommandRaised;
            serialCommEngine.SaveCommandFailed += this.SerialCommEngine_SaveCommandFailed;
            
            if (ftpEngine != null && ftpEngine.IsInitialized && ftpEngine.FtpServerRunning)
            {
                ftpEngine.QueryCanSendFile += this.FtpEngine_QueryCanSendFile;
                ftpEngine.BeforeStoreFileCustom += this.FtpEngine_BeforeStoreFileCustom;
                ftpEngine.StoreFileCustom += this.FtpEngine_StoreFileCustom;
                ftpEngine.StoreFileCustomCompleted += this.FtpEngine_StoreFileCustomCompleted;
            }

            if (fileWatcherEngine != null && fileWatcherEngine.IsInitialized)
            {
                fileWatcherEngine.BeforeWatcherItemEvaluation += this.FileWatcherEngine_BeforeWatcherItemEvaluation;
                fileWatcherEngine.WatcherItemEvaluated += this.FileWatcherEngine_WatcherItemEvaluated;
                fileWatcherEngine.WatcherItemCreated += this.FileWatcherEngine_WatcherItemCreated;
                fileWatcherEngine.WatcherItemDeleted += this.FileWatcherEngine_WatcherItemDeleted;
            }

        }

        #region eventi motore di comunicazione seriale

        private void SerialCommEngine_BeforeEnqueueFile(object sender, EnqueueCancelEventArgs e)
        {
            /*
             * ogni operazione di trasmissione è asincrona e viene messa in una coda
             * questo evento permette di cancellare l'inserimento di un file
             * in coda di trasmissione
             * CANCELLABILE
             */

            //ESEMPI DI PARAMETRI E COMPONENTI RAGGIUNGIBILI

            //e.Cancel = true: cancella l'operazione
            //e.FullPath: percorso completo del file da inviare
            //e.Channel.Status: stato del canale di comunicazione seriale
            //e.Channel.Connected: se la porta seriale è aperta
            //e.Channel.EndPoint: dati relativi all'EndPoint su cui è stato aperto il canale di comunicazione
        }

        private void SerialCommEngine_BeforeSend(object sender, FileOnChannelEventArgs e)
        {
            /*
             * evento di notifica inizio trasmissione di un file
             */
            
        }

        /*
         *una operazione di ricezione termina con il salvataggio di un file in un file
         * temporaneo di ricezione, oppure in un file specifico se è possibile
         * determinare qual è il suo nome. In genere questo viene fatto
         * tramite un comando di SALVA presente nel file ricevuto
         *
         */

        private void SerialCommEngine_CommandParseCompleted(object sender, CommandParsingEventArgs e)
        {
            /*
             * al termine della ricezione di un file, prima del salvataggio,
             * questo viene analizzato per capire se al suo interno sono presenti dei "comandi",
             * cioè segnaposti che permettono l'esecuzione automatica di azioni
             * da parte di PowerDNC
             */

            foreach (var command in e.Commands)
            {
                Debug.Write($"COMANDO: {command.Command.Command.ToString()} VALIDO: {command.IsValid}");
            }
            
        }

        private void SerialCommEngine_BeforeSaveCommand(object sender, FileSaveOnChannelCancelEventArgs e)
        {
            /*
             *evento di notifica dell'inizio dell'operazione di salvataggio
             * CANCELLABILE
             */

            //e.Cancel = true: cancella l'operazione
            //e.SaveFilename: percorso completo e definitivo del file da salvare
            //e.ShortName: nome breve e senza estensione del file
        }

        private void SerialCommEngine_SaveCommandRaised(object sender, FileSaveOnChannelEventArgs e)
        {
            /*
             * evento di notifica operazione di salvataggio
             * completata con successo
             */

        }

        private void SerialCommEngine_SaveCommandFailed(object sender, FileSaveOnChannelEventArgs e)
        {
            /*
             * evento di notifica operazione di salvataggio
             * NON completata
             */
        }

        #endregion

        #region motore di comunicazione FTP

        /*
         *il protocollo FTP ha vincoli sulle operazioni che possono essere fatte
         * che sono insiti nel protocollo stesso. Ad esempio l'operazione di
         * salvataggio è gestita dal protocollo (inclusa l'assegnazione del nome),
         * quindi non si può semplicemente cambiare nome al file, ma bisogna
         * gestire un salvataggio completamente personalizzato
         * se non si vuole usare il nome originale
         *
         */

        private void FtpEngine_BeforeStoreFileCustom(object sender, BeforeStoreFileCustomCancelEventArgs e)
        {
            /*
             *inizio dell'operazione di salvataggio personalizzato
             * CANCELLABILE
             *
             */
            
        }

        private void FtpEngine_StoreFileCustom(object sender, StoreFileCustomEventArgs e)
        {
            /*
             *gestione del salvataggio personalizzato
             *
             */
             
            //prima dell'uscita dal gestore evento impostare le due proprietà:
            //e.Handled = true\false: se l'evento di salvataggio personalizzato è stato usato
            //e.Saved: esito del salvataggio
            //NB: potrei avere Handled a true (salvataggio custom), ma Saved a false (salvataggio fallito)
        }

        private void FtpEngine_StoreFileCustomCompleted(object sender, StoreFileCustomCompletedEventArgs e)
        {
            /*
             * salvataggio personalizzato completato
             */

            //e.TargetShortFilename: nome definitivo
        }

        private void FtpEngine_QueryCanSendFile(object sender, QueryCanSendFileCancelEventArgs e)
        {
            /*
             * evento con il quale si può
             * confermare od annullare l'invio di un file
             * CANCELLABILE
             */

            //e.Cancel = true: cancella l'operazione
            //e.FullPath: percorso completo del file da inviare
        }

        #endregion

        #region motore di comunicazione monitoraggio cartelle

        private void FileWatcherEngine_BeforeWatcherItemEvaluation(object sender, WatcherItemBeforeEvaluationEventArgs e)
        {
            /*
             * evento di inizio valutazione di un file
             * CANCELLABILE
             */
             
            //e.Item: elemento\file a cui si riferisce l'evento
        }

        private void FileWatcherEngine_WatcherItemEvaluated(object sender, WatcherEvaluationEventArgs e)
        {
            /*
             *valutazione di un file completata
             */

            //e.DetectedResult (== WatcherAction.Modified): risultato della valutazione
        }

        private void FileWatcherEngine_WatcherItemCreated(object sender, WatcherItemActionCompletedEventArgs e)
        {
            /*
             * evento specifico di elemento\file creato
             */
        }

        private void FileWatcherEngine_WatcherItemDeleted(object sender, WatcherItemActionCompletedEventArgs e)
        {
            /*
             * evento specifico di elemento\file elimonato
             */
        }

        #endregion

    }

}
