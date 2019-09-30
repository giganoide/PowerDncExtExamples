using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atys.PowerDNC.Extensibility;
using Atys.PowerDNC.Foundation;

namespace TeamSystem.Customizations
{
    [ExtensionData("MyPowerDncExtension", "Put here an extension description.", "1.0",
        Author = "Author Name", EditorCompany = "Company Name")]
    public class MyEmptyExtension : IDncExtension
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
             * Your custom implementation here...
             * (Attach to application events, if needed)
             * --
             * Fornire di seguito l'implementazione del metodo...
             * (Se necessario creare qui i gestori eventi applicazione)
             */
        }

        /// <summary>
        /// Deve contenere il codice di cleanup da eseguire prima della disattivazione
        /// dell'estensione o comunque alla chiusura di PowerDNC
        /// -
        /// Must contain the clean-up code, to be executed at PowerDNC shutdown
        /// </summary>
        public void Shutdown()
        {
            /*
             * Your custom implementation here...
             * (Detach from application events)
             * --
             * Fornire di seguito l'implementazione del metodo...
             * (Rilasciare eventuale gestione eventi applicazione)
             */
        }

        #endregion
    }
}