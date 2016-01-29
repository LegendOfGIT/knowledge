# fb.services.extensions.biprobehavior

## <a>Inhalt</a>
1. [Was tut dieses Behavior?](#purpose "Was tut dieses Behavior?")
1. [Wo wird das Behavior eingesetzt?](#location "Wo wird das Behavior eingesetzt?")
1. [Voraussetzungen](#requirements "Voraussetzungen")
1. [Wie wird dieses Behavior installiert?](#installation "Wie wird dieses Behavior installiert?")
1. [Wie wird das Behavior für einen Endpunkt eingerichtet?](#endpointconfiguration "Wie wird das Behavior für einen Endpunkt eingerichtet?")
1. [Wie kann das Behavior konfiguriert werden?](#configuration "Wie kann das Behavior konfiguriert werden?")

### <a name='purpose'>Was tut dieses Behavior?</a>
Dieses Behavior kann über die Web.-/App.config eines Programm/Webservice für einen Endpunkt konfiguriert werden.
Ist das Behavior aktiv, wird vor jedem Absenden eines Request an den konfigurierten Endpunkt der Header der abgehenden
Soap-Nachricht überschrieben und ein BiPRO SecurityToken in dem Header eingefügt.
Dieses wird für die zur Zeit übliche Form der BiPRO-Authentifizierung benötigt.
Das BiPRO Security-Token wird hierbei von dem Behavior über einen für das Behavior konfigurierten BiPRO Token Webservice angefragt.

Nach der Modifikation des BiProBehavior, sieht der Soap-Header in etwa so aus:
&lt;Header&gt;
&nbsp;&nbsp;&lt;Security xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;SecurityContextToken xmlns="http://schemas.xmlsoap.org/ws/2005/02/sc"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;Identifier&gt;bipro:MTQ1MzQ0OTE5Mzg3MTE0NTM0NDkxOTM4NzExNDUzND...&lt;/Identifier&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/SecurityContextToken&gt;
&nbsp;&nbsp;&lt;/Security&gt;
&lt;/Header&gt;

### <a name='location'>Wo wird das Behavior eingesetzt?</a>
Das Behavior kann in jedem .NET Projekt - zum Beispiel in Webservices oder Konsolenanwendungen - eingesetzt werden.

### <a name='requirements'>Voraussetzungen</a>
Für den Einsatz wird mindestens das .NET-Framework 4.0 benötigt, da innerhalb des Behaviorcode das Element Tuple&lt;T, T&gt; eingesetzt wird.
Des Weiteren muss das jeweilige Programm in einem .NET Projekt liegen, damit das Bevaior als Package installiert werden kann.

### <a name='installation'>Wie wird dieses Behavior installiert?</a>
1. Zu erweiterndes Projekt öffnen
1. Paket-Manager-Konsole öffnen
1. Befehl 'install-package fb.services.extensions.behaviors.biprobehavior' ausführen

### <a name='endpointconfiguration'>Wie wird das Behavior für einen Endpunkt eingerichtet?</a>
Als erstes wird die Behavior-Extension innerhalb der App-/Web.Config bekannt gemacht.
Hierfür wird folgender Konfigurationsabschnitt im Abschnitt "system.serviceModel" 
zwischen den Unterabschnitten "behaviors" und "bindings" platziert:

&lt;configuration&gt;
&nbsp;&nbsp;...
&nbsp;&nbsp;&lt;system.serviceModel&gt;
&nbsp;&nbsp;&nbsp;&nbsp;...
&nbsp;&nbsp;&nbsp;&nbsp;&lt;behaviors&gt;...&lt;/behaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;extensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;behaviorExtensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;add 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;name="BiProBehavior" 
**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;type="FB.Services.Extensions.Behaviors.BiProBehavior, FB.Services.Extensions.Behaviors.BiProBehavior, Version=1.0.7.0"**
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/behaviorExtensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/extensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;bindings&gt;...&lt;/bindings&gt;
&nbsp;&nbsp;&lt;/system.serviceModel&gt;
&nbsp;&nbsp;...
&lt;/configuration&gt;	  


Ist die Behavior-Extension bekannt, kann eine Behavior Abschnitt für die Behavior-Extension hinzukonfiguriert werden:

&lt;configuration&gt;
&nbsp;&nbsp;...
&nbsp;&nbsp;&lt;system.serviceModel&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;behaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;endpointBehaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;behavior name="BiPROBehavior"&gt;
**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;BiProBehavior**
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TokenURL="https://ws0.barmenia24.de/ibis/services/UsernamePasswordLogin_2.1.3.1.0" 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TokenUsername="frankebornberg" 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TokenPassword="fh6vcp0fg" 
**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/&gt;**
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/behavior&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/endpointBehaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/behaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;extensions&gt;...&lt;/extensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;...
&nbsp;&nbsp;&lt;/system.serviceModel&gt;
&nbsp;&nbsp;...
&lt;configuration&gt;


Um das konfigurierte Behavior letzendlich bei der Ansprache eines Endpunkt auszulösen, 
muss dem Endpunkt das konfigurierte Behavior zugeordnet werden:

&lt;configuration&gt;
&nbsp;&nbsp;&lt;system.serviceModel&gt;
&nbsp;&nbsp;&nbsp;&nbsp;...  
&nbsp;&nbsp;&nbsp;&nbsp;&lt;behaviors&gt;...&lt;/behaviors&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;extensions&gt;...&lt;/extensions&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;client&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;endpoint
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;address="http://83.133.158.53/BBVN_RC/tarife.asmx"
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;binding="basicHttpBinding"
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;bindingConfiguration="RechenkernDieBayerischeSoap"
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;contract="BBVN.RechenkernDieBayerischeSoap"
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;name="RechenkernDieBayerischeSoap"
**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;behaviorConfiguration="BiPROBehavior"**
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/client&gt;
&nbsp;&nbsp;&nbsp;&nbsp;...  
&nbsp;&nbsp;&lt;/system.serviceModel&gt;
&lt;/configuration&gt;

### <a name='configuration'>Wie kann das Behavior konfiguriert werden?</a>
**TokenURL / TokenUsername / TokenPassword**
Diese Attribute sind bei der Erstellung einer Konfiguration für die Behavior-Extension immer Pflicht.
Mit diesen Parametern werden die URL und die Zugangsdaten des BiPro SecurityToken Webservice festgelegt, 
mit dem das BiPRO Security-Token für den Soap-Header geholt wird.

#### UseToken
Insofern das Attribut "UseToken" nicht angeben, oder mit "true" belegt wurde, holt die BehaviorExtension
bei jeder Anfrage an den Endpunkt ein BiPRO-Security Token von dem konfigurierten BiPro Security-Token Service
und ersetzt den bestehenden Header mit einem Header der den geholten BiPRO-Security Token enthält.

Wird das Attribut "UseToken" mit "false" konfiguriert, bleibt der Soap-Header wie er ist.
In diesem Fall werden nur die anderen Soap Manipulationen wie das Aufräumen von Namespaces durchgeführt.

#### TokenPath
Mit dem Attribut "TokenPath" kann der Entwickler einen Dateipfad angeben, in dem ein, über den
BiPro Security-Token Service bezogenes BiPRO-Security Token zwischengespeichert werden kann.
Ist der Dateipfad (z.B. c:\temp\temptoken.txt) konfiguriert, wird ein bezogenes BiPRO-Security Token aus der zwischengespeicherten Datei
wiederverwendet, wenn dieses nicht älter als zwei Minuten ist.
Sind die zwei Minuten überschritten, fordert die Behavior-Extension ein neues Token an und speicher dieses zwischen.

**Achtung**
Für das konfigurierte Verzeichnis, müssen natürlich Lese- und Schreibberechtigungen gesetzt sein.

#### TokenTemplatepath
Mit diesem Attribut kann man eine Vorlagendatei für die Anfrage eines BiPro SecurityToken über den BiPro SecurityToken Webservice festlegen.
Bei der autmatischen Anfrage BiPro SecurityToken, wird dann die Soap-Anfrage aus der Vorlagendatei verwendet.
Folgende Ausdrücke, werden in der Vorlage dabei bei einer Anfrage automatisch ersetzt:
{0} = TokenUsername
{1} = TokenPassword

#### Namespaces
Über das Attribut "Namespaces" können mit einem | getrennt mehrere Pärchen 
in der Form "{Namespacepräfix};{Namespaceurl}" angegeben werden.
Bei der Anfrage an den Endpunkt wird dann jeder der festgelegten Namespaces mit dem jeweils festgelegten Präfix
in den Wurzelknoten des Body-Teil gelegt und anschließend alle separaten Definitionen des Namespace
in den XML-Knoten des Bodys entfernt und durch das vorgegebene Präfix ersetzt.

**Ein Beispiel**
Konfiguration 
"barmdaten|http://www.barmenia.de/namespace/datentypen;daten|http://www.bipro.net/namespace/datentypen;partner|http://www.bipro.net/namespace/partner;leben"

**Soap-Body vor der Manipulation**
&lt;s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"&gt;
&nbsp;&nbsp;&lt;getQuote xmlns="http://www.bipro.net/namespace/tarifierung"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;Request xmlns:ns0="http://www.bipro.net/namespace/allgemein"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns0:BiPROVersion&gt;2.1.5.1.2&lt;/ns0:BiPROVersion&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;Tarifierung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;Partner xmlns:ns1="http://www.bipro.net/namespace/partner" xsi:type="ns1:CT_Person"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns0:Erweiterung xmlns:ns2="http://www.barmenia.de/namespace/leben" xsi:type="ns2:CT_PersonErweiterung"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns2:IstPolitischExponiertePerson&gt;false&lt;/ns2:IstPolitischExponiertePerson&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/ns0:Erweiterung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:PartnerID&gt;Versicherungsnehmer&lt;/ns1:PartnerID&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Anschrift/&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Geburtsdatum&gt;1986-02-24&lt;/ns1:Geburtsdatum&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Geschlecht&gt;1&lt;/ns1:Geschlecht&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Berufstaetigkeit&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Beruf&gt;Bankkaufmann/-kauffrau&lt;/ns1:Beruf&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:StellungImBeruf xmlns:ns3="http://www.barmenia.de/namespace/datentypen" xsi:type="ns3:STE_StellungImBerufErweitert"&gt;11&lt;/ns3:StellungImBeruf&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;ns1:Berufsschluessel&gt;691009&lt;/ns1:Berufsschluessel&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/ns1:Berufstaetigkeit&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Partner&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;...
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Tarifierung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Request&gt;
&nbsp;&nbsp;&lt;/getQuote&gt;
&lt;/s:Body&gt;
		  
**Soap-Body nach der Manipulation**
&lt;s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"&gt;
&nbsp;&nbsp;&lt;getQuote xmlns="http://www.bipro.net/namespace/tarifierung" xmlns:barmdaten="http://www.barmenia.de/namespace/datentypen" xmlns:daten="http://www.bipro.net/namespace/datentypen" xmlns:partner="http://www.bipro.net/namespace/partner" xmlns:leben="http://www.bipro.net/namespace/leben" xmlns:barmleben="http://www.barmenia.de/namespace/leben" xmlns:allgemein="http://www.bipro.net/namespace/allgemein" xmlns:barmtransfer="http://www.barmenia.de/namespace/transfer"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;Request&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;allgemein:BiPROVersion&gt;2.1.5.1.2&lt;/allgemein:BiPROVersion&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;Tarifierung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;Partner xsi:type="partner:CT_Person"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;allgemein:Erweiterung xsi:type="barmleben:CT_PersonErweiterung"&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;barmleben:IstPolitischExponiertePerson&gt;false&lt;/barmleben:IstPolitischExponiertePerson&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/allgemein:Erweiterung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:PartnerID&gt;Versicherungsnehmer&lt;/partner:PartnerID&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Anschrift/&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Geburtsdatum&gt;1986-02-24&lt;/partner:Geburtsdatum&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Geschlecht&gt;1&lt;/partner:Geschlecht&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Berufstaetigkeit&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Beruf&gt;Bankkaufmann/-kauffrau&lt;/partner:Beruf&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:StellungImBeruf xsi:type="barmdaten:STE_StellungImBerufErweitert"&gt;11&lt;/partner:StellungImBeruf&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Berufsschluessel&gt;691009&lt;/partner:Berufsschluessel&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/partner:Berufstaetigkeit&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Partner&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;...
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Tarifierung&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;/Request&gt;
&nbsp;&nbsp;&lt;/getQuote&gt;
&lt;/s:Body&gt;		  


#### RepairAttributePrefixes
Wenn das Attribut "RepairAttributePrefixes" mit "true" konfiguriert wird, werden alle Werte in allen Xml-Attributen
mit dem Namespacepräfix des Elternelement versehen, wenn die Attributwerte vorher keinen eigenen Namespace verwendet haben.

**Ein Beispiel**

**Vor der Manipulation**
&lt;Partner xsi:type="partner:CT_Person"&gt;
&nbsp;&nbsp;&lt;partner:Anschrift&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Strasse beispiel="wert"&gt;&lt;/partner:Strasse&gt;
&nbsp;&nbsp;&lt;/partner:Anschrift&gt;
&lt;/Partner&gt;

**Nach der Manipulation**
&lt;Partner xsi:type="partner:CT_Person"&gt;
&nbsp;&nbsp;&lt;partner:Anschrift&gt;
&nbsp;&nbsp;&nbsp;&nbsp;&lt;partner:Strasse beispiel="partner:wert"&gt;&lt;/partner:Strasse&gt;
&nbsp;&nbsp;&lt;/partner:Anschrift&gt;
&lt;/Partner&gt;


#### NuernbergerBiPROWorkaround
Wenn das Attribut "NuernbergerBiPROWorkaround" mit "true" konfiguriert wird, 
wird in der Anfrage die Information "&lt;.*?ArtID.*?VorgabeBerechnungErweitert.*?&lt;/.*?ArtID&gt;"
analysiert und bei bestimmten Vorgabearten der Namespacepräfix der ArtID angepasst.


