# PowerDncExtExamples
Esempi di codice per creare extension PowerDnc

## Extensions PowerMES
* Progetto di tipo class library
* Estensione DLL: «.DncExt.dll»
* Una sola extension per DLL
* Referenziare la libreria: 
    * Atys.PowerDNC.Contracts
    * Atys.PowerDNC.Foundation
* Deve essere creata una classe pubblica
    * Deve implementare l’interfaccia «IDncExtension»
    * Deve essere decorata con l’attributo «ExtensionData»
    * Aggiungere using: Atys.PowerDNC.Extensibility, Atys.PowerDNC.Foundation

La DLL deve essere posizionata in una sotto-cartella del percorso di installazione «C:\Program Files (x86)\Atys\PowerDNC\Extensions», una DLL per ogni sotto-cartella

## Esempi
1. 010_Empty: Struttura di base per la creazione di un extension
2. 012_Engines: Motori di comunicazione ed eventi principali
3. 015_CheckBeforeLoad: Controlli pre-trasmissione con eventuale annullamento
4. 017_CheckMesCommands: Verifica comandi mes
5. 020_AddDateBeforeParsing: Aggiunta comando di salvataggio e aggiunta data all'interno del part program